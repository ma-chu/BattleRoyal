using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


// Реализуем модель MVC (Не описывает сетевых протоколов, только логику игры):
// Model = Server.cs = Бизнес-логика
// Controller = Client.cs - обработка ввода/вывода игрока, приведение его к интерфейсу сервера
// View = UI

// 1. В режиме сингл HeroManager создаем сервер и 2 клиентов: player & AI
// 2. В режиме мультисервер HeroManager создаем сервер, адаптер сервера и 1 клиент-player
// 3. В режиме мультиклиент HeroManager создаем 1 клиент-player и адаптер клиента

// Если существуют сетевые клиенты, реализовать для них паттерн адаптер photon->IServer в отдельном классе
// (будет отлавливать события сервера и паковать их в исх. события photon'а, а вх. события photon'а в методы сервера)

[System.Serializable]
public struct MatchInfo
{
    public int numRoundsToWin;                                    // надо выиграть раундов для выигрыша игры
    public int roundNumber;                                       // текущий номер раунда
    public PlayerObject player1;                                  // первый игрок
    public PlayerObject player2;                                  // второй игрок
    public PlayerObject roundWinner;                              // победитель раунда
    public PlayerObject matchWinner;                              // победитель матча
    public MatchInfo(int numRoundsToWin, PlayerObject player1 = null, PlayerObject player2 = null)
    {
        this.numRoundsToWin = numRoundsToWin;
        roundNumber = 1;
        this.player1 = player1;
        this.player2 = player2;
        roundWinner = null;
        matchWinner = null;
    }
}


public class Server : MonoBehaviour, IServer
{
    private static Server _instance;
    public static Server Instance => _instance;
    
    [SerializeField] private List<PlayerObject> players = new List<PlayerObject>();

    [SerializeField] private MatchInfo match = new MatchInfo(4);
   
    /* Раскомментировать при работе с фотоном
    public static Photon.Bolt.BoltEntity myBoltEntity;       // это точно лишнее        
    public static Photon.Bolt.BoltEntity enemyBoltEntity;
    private Photon.Bolt.IEFPlayerState _enemyState;
    public static bool ClientConnected = false;              // а с этим разобраться
    public static bool ClientDisconnected = false;
    [HideInInspector] public bool doServerExchange;
    [HideInInspector] public bool doClientExchange;
*/
    
    public event EventHandler<string> JoinedAction;
    public void Join(string name, EventHandler<string> onJoined)
    {
        // Подключение к турниру
        players.Add(new PlayerObject(name));
        Debug.Log("Локальный сервер: клиент "+ name +" подключился к турниру");
        
        JoinedAction += onJoined;
        JoinedAction?.Invoke(this, name);  // по событию клиент вызывает SubscribeOnStartMatch, SubscribeOnEndRound, ... и SubscribeOnResultsReady
        
        if (players.Count == 2) StartMatch();    // естественно, первый играет со вторым
    }
    
    public void SubscribeOnStartMatch(EventHandler<StartMatchInfo> onStartMatch) => StartMatchAction += onStartMatch;
    public event EventHandler<StartMatchInfo> StartMatchAction;
    
    public void SubscribeOnResultsReady(EventHandler<TurnOutInfo> onResultsReady) => ResultsReadyAction += onResultsReady;
    public event EventHandler<TurnOutInfo> ResultsReadyAction;
    
    public void SubscribeOnEndMatch(EventHandler<EndMatchInfo> onEndMatch) => EndMatchAction += onEndMatch;
    public event EventHandler<EndMatchInfo> EndMatchAction;

    public void SubscribeOnStartRound(EventHandler<StartRoundInfo> onStartRound) => StartRoundAction += onStartRound;
    public event EventHandler<StartRoundInfo> StartRoundAction;
    public void SubscribeOnEndRound(EventHandler<EndRoundInfo> onEndRound) => EndRoundAction += onEndRound;
    public event EventHandler<EndRoundInfo> EndRoundAction;

    private void StartMatch()
    {
        match.roundNumber = 1;
        match.player1 = players[0];
        match.player2 = players[1];

        var player1MatchInfo = new StartMatchInfo();
        var player2MatchInfo = new StartMatchInfo();

        player1MatchInfo.PlayerName = player2MatchInfo.EnemyName = match.player1.name;
        player1MatchInfo.EnemyName = player2MatchInfo.PlayerName = match.player2.name;
        for (var i = 0; i < match.player1.inventoryItems.Length; i++)
            if (match.player1.inventoryItems[i] != null)
                player1MatchInfo.PlayerInventoryItems[i] = player2MatchInfo.EnemyInventoryItems[i] = match.player1.inventoryItems[i].Name;
        for (var i = 0; i < match.player2.inventoryItems.Length; i++)
            if (match.player2.inventoryItems[i] != null)
                player1MatchInfo.EnemyInventoryItems[i] = player2MatchInfo.PlayerInventoryItems[i] = match.player2.inventoryItems[i].Name;

        StartMatchAction?.Invoke(this, player1MatchInfo);    // событие для игрока 1;
        StartMatchAction?.Invoke(this, player2MatchInfo);    // событие для игрока 2;
        
        Debug.Log("Локальный сервер: матч между " + player1MatchInfo.PlayerName +" и "+ player2MatchInfo.PlayerName +" начинается");

        Invoke(nameof(StartNewRound), 3f);
    }
    
    /*private PhotonPlayerObject StartMatch()                            // Вызывать в GameManager'е, вернуться в фотоне
    {
        return PhotonPlayerObjectRegisty.ServerPhotonPlayer;    // пример
    }
    */
    
    public void TakeDecision(string name, TurnInInfo turnInInfo)
    {
        var player = players.Find(pl=> pl.name == name);

        // Принять входые данные соперников
        player.decision = turnInInfo.PlayerDecision;
        switch (player.decision)
        {
            case Decision.ChangeSwordShield:
                player.SetSwordShield();
                break;
            case Decision.ChangeSwordSword:
                player.SetSwordSword();
                break;
            case Decision.ChangeTwoHandedSword:
                player.SetTwoHandedSword();
                break;
        }
        player.defencePart = turnInInfo.PlayerDefencePart * (player.Tweakers.MaxDefencePart + player.Tweakers.ParryChance);
        player.dataTaken = true;
        
        if (!match.player1.dataTaken || !match.player2.dataTaken) return;
        // Посчитать результаты схода (здесь же вычитаем здоровье)
        ExchangeResultsAndDamages();
        // Обновить серии
        AddSeries();
            
        var player1TurnOutInfo = new TurnOutInfo
        {
            PlayerName = match.player1.name,
            EnemyDecision = match.player2.decision,
            PlayerExchangeResults = match.player1.exchangeResults,
            EnemyExchangeResults = match.player2.exchangeResults,
            PlayerDamages = match.player1.gotDamages,
            EnemyDamages = match.player2.gotDamages,
            PlayerHP = (int)match.player1.Hp.Health,
            EnemyHP = (int)match.player2.Hp.Health,
            PlayerSeries = new []{match.player1.Series.StrongStrikesNum, match.player1.Series.SeriesOfBlocksNum, match.player1.Series.SeriesOfStrikesNum} ,
            EnemySeries = new []{match.player2.Series.StrongStrikesNum, match.player2.Series.SeriesOfBlocksNum, match.player2.Series.SeriesOfStrikesNum}
        };
        
        var player2TurnOutInfo = new TurnOutInfo
        {
            PlayerName = match.player2.name,
            EnemyDecision = match.player1.decision,
            PlayerExchangeResults = match.player2.exchangeResults,
            EnemyExchangeResults = match.player1.exchangeResults,
            PlayerDamages = match.player2.gotDamages,
            EnemyDamages = match.player1.gotDamages,
            PlayerHP = (int)match.player2.Hp.Health,
            EnemyHP = (int)match.player1.Hp.Health,
            PlayerSeries = new []{match.player2.Series.StrongStrikesNum, match.player2.Series.SeriesOfBlocksNum, match.player2.Series.SeriesOfStrikesNum},
            EnemySeries = new []{match.player1.Series.StrongStrikesNum, match.player1.Series.SeriesOfBlocksNum, match.player1.Series.SeriesOfStrikesNum}
        };

        match.player1.dataTaken = false;
        match.player2.dataTaken = false;
        
        // Выдать результаты в событии ResultsReadyAction. Именно после обнуления dataTaken, а то события обрабатываются сразу, а не после тела ф-ии
        ResultsReadyAction?.Invoke(this, player2TurnOutInfo);
        ResultsReadyAction?.Invoke(this, player1TurnOutInfo);
        
        // Проверить на конец раунда
        if (OneHeroLeft())
        {
            // Проверить на конец матча. Если да, то (здесь) приза не выдывать, в конце новый раунд не начинать
            match.matchWinner = GameWinner();
            
            Item prize;
            if (match.matchWinner == null)
                prize = match.roundWinner != null ? GiveOutPrize(match.roundWinner) : null;
            else prize = null;
            
            var player1EndRoundInfo = new EndRoundInfo()
            {
                PlayerName = match.player1.name,
                roundWinner = match.roundWinner != null ? match.roundWinner.name : string.Empty,
                prize = (match.roundWinner == match.player1 && prize != null)? prize.Name : string.Empty
            };
            if (match.roundWinner == match.player2 && match.player1.name == "bot" && match.player1.roundsLost == 3) 
                match.player1.AddInventoryItem(AllItems.Instance.items.First(i => i.Name == "ring_of_cunning"));
            EndRoundAction?.Invoke(this, player1EndRoundInfo);
                
            var player2EndRoundInfo = new EndRoundInfo()
            {
                PlayerName = match.player2.name,
                roundWinner = match.roundWinner != null ? match.roundWinner.name : string.Empty,
                prize = (match.roundWinner == match.player2 && prize != null)? prize.Name : string.Empty
            };
            EndRoundAction?.Invoke(this, player2EndRoundInfo);
            
            if (match.matchWinner != null)
            {
                Invoke(nameof(EndMatch), 7);    // задержки согласовать с оными из ViewModel
                return;
            }
            else // Иначе новый раунд
            {
                match.roundNumber++;
                Invoke(nameof(StartNewRound), 9);
            }
        }
    }

    private void StartNewRound()
    {
        match.player1.Reset();
        match.player2.Reset();
        
        Debug.Log("Локальный сервер: раунд " + match.roundNumber +" начинается");

        var player1StartRoundInfo = new StartRoundInfo()
        {
            PlayerName = match.player1.name,
            roundNumber = match.roundNumber,
        };
        var player2StartRoundInfo = new StartRoundInfo()
        {
            PlayerName = match.player2.name,
            roundNumber = match.roundNumber
        };
        player1StartRoundInfo.PlayerStartHealth = player2StartRoundInfo.EnemyStartHealth = match.player1.Tweakers.StartingHealth;
        player1StartRoundInfo.EnemyStartHealth = player2StartRoundInfo.PlayerStartHealth = match.player2.Tweakers.StartingHealth;
        StartRoundAction?.Invoke(this, player1StartRoundInfo);
        StartRoundAction?.Invoke(this, player2StartRoundInfo);
    }

    private void EndMatch()
    {
        var player1EndMatchInfo = new EndMatchInfo()    
        {
            PlayerName = match.player1.name,
            matchWinner = match.matchWinner.name
        };
        EndMatchAction?.Invoke(this, player1EndMatchInfo);
        var player2EndMatchInfo = new EndMatchInfo()
        {
            PlayerName = match.player2.name,
            matchWinner = match.matchWinner.name
        };
        EndMatchAction?.Invoke(this, player2EndMatchInfo);
    }

    private void ExchangeResultsAndDamages()
    {
        if (GameManager.gameType == GameType.Client) return;
        
        // 1. Сперва рассчитаем предварительные коэффициенты на основе текущего набора оружия и решения
        match.player1.CalculatePreCoeffs();
        match.player2.CalculatePreCoeffs();
        // 2. Дорассчитаем предварительные коэффициенты на основе предварительных коэффициентов противника
        match.player1.preCoeffs[0].blockVs2Handed = (match.player1.weaponSet == WeaponSet.SwordShield)
                                                    && (match.player1.preCoeffs[0].block)
                                                    && (match.player2.decision == Decision.Attack)
                                                    && (match.player2.weaponSet == WeaponSet.TwoHandedSword);
        match.player2.preCoeffs[0].blockVs2Handed = (match.player2.weaponSet == WeaponSet.SwordShield)
                                                    && (match.player2.preCoeffs[0].block) 
                                                    && (match.player1.decision == Decision.Attack) 
                                                    && (match.player1.weaponSet == WeaponSet.TwoHandedSword);
        
        // 3.На основе предварительных коэффицентов определяем результат схода и возможный урон
        // Удар 1 
        match.player2.exchangeResults[0] = (match.player1.decision == Decision.Attack)
            ? match.player2.CalculateExchangeResult(1)
            : ExchangeResult.No;
        if (match.player2.exchangeResults[0] == ExchangeResult.BlockVs2Handed)
        {
            match.player1.preCoeffs[0].damage *= match.player1.Tweakers.Part2HandedThroughShield;
        }

        match.player1.exchangeResults[0] = (match.player2.decision == Decision.Attack)
            ? match.player1.CalculateExchangeResult(1)
            : ExchangeResult.No;
        if (match.player1.exchangeResults[0] == ExchangeResult.BlockVs2Handed)
        {
            match.player2.preCoeffs[0].damage *= match.player2.Tweakers.Part2HandedThroughShield;
        }

        match.player2.preCoeffs[0].damage =
            Mathf.Round(match.player2.preCoeffs[0].damage - match.player2.preCoeffs[0].damage * match.player2.defencePart); // уберём часть урона, потраченную на парирование, и округлим
        match.player1.preCoeffs[0].damage =
            Mathf.Round(match.player1.preCoeffs[0].damage - match.player1.preCoeffs[0].damage * match.player1.defencePart); // уберём часть урона, потраченную на парирование, и округлим

        // Удар 2
        match.player2.exchangeResults[1] = ((match.player1.decision == Decision.Attack) && (match.player1.preCoeffs[1].damage != 0f))
            ? match.player2.CalculateExchangeResult(2)
            : ExchangeResult.No;
        match.player1.exchangeResults[1] = ((match.player2.decision == Decision.Attack) && (match.player2.preCoeffs[1].damage != 0f))
            ? match.player1.CalculateExchangeResult(2)
            : ExchangeResult.No;
            
        match.player2.preCoeffs[1].damage =
            Mathf.Round(match.player2.preCoeffs[1].damage - match.player2.preCoeffs[1].damage * match.player2.defencePart); // уберём часть урона, потраченную на парирование, и округлим
        match.player1.preCoeffs[1].damage =
            Mathf.Round(match.player1.preCoeffs[1].damage - match.player1.preCoeffs[1].damage * match.player1.defencePart); // уберём часть урона, потраченную на парирование, и округлим

        // 4. Реальный урон. Еще раз:
        // preCoeffs[i].damage - возможный урон противнику;
        // gotDamages[i] - реально полученный урон, по его значению можно играть звуки и анимации
        // Удар 1
        if (match.player1.exchangeResults[0] == ExchangeResult.GetHit ||
            match.player1.exchangeResults[0] == ExchangeResult.BlockVs2Handed)
        {
            match.player1.gotDamages[0] = (int) match.player2.preCoeffs[0].damage;
            match.player1.dead = match.player1.Hp.TakeDamage(match.player1.gotDamages[0]);
        }
        //else match.player1.gotDamages[0] = 0;
        if (match.player2.exchangeResults[0] == ExchangeResult.GetHit || match.player2.exchangeResults[0] == ExchangeResult.BlockVs2Handed)
        {
            match.player2.gotDamages[0] = (int) match.player1.preCoeffs[0].damage;
            match.player2.dead = match.player2.Hp.TakeDamage(match.player2.gotDamages[0]);
        }
        //else match.player2.gotDamages[0] = 0;
        // Удар 2
        if (match.player1.exchangeResults[1] == ExchangeResult.GetHit)
        {
            match.player1.gotDamages[1] = (int) match.player2.preCoeffs[1].damage;
            match.player1.dead = match.player1.Hp.TakeDamage(match.player1.gotDamages[1]);
        }
        //else match.player1.gotDamages[1] = 0;
        if (match.player2.exchangeResults[1] == ExchangeResult.GetHit)
        {
            match.player2.gotDamages[1] = (int) match.player1.preCoeffs[1].damage;
            match.player2.dead = match.player2.Hp.TakeDamage(match.player2.gotDamages[1]);
        }
        //else match.player2.gotDamages[1] = 0;
    }

    private void AddSeries()
    { 
        // Удар 1. Обновление коэфф. и добавление эффектов серий
        if (match.player2.exchangeResults[0] == ExchangeResult.GetHit || match.player2.exchangeResults[0] == ExchangeResult.BlockVs2Handed)
            match.player1.Series.AddStrongSeries(1);
        if (match.player1.exchangeResults[0] == ExchangeResult.GetHit || match.player1.exchangeResults[0] == ExchangeResult.BlockVs2Handed)
            match.player2.Series.AddStrongSeries(1);

        if (match.player2.exchangeResults[0] == ExchangeResult.GetHit || match.player2.exchangeResults[0] == ExchangeResult.BlockVs2Handed)
        {
            match.player1.Series.AddSeriesOfStrikes();
            match.player2.Series.ResetSeriesOfBlocks();    // ресет серии блоков
        }

        if (match.player1.exchangeResults[0] == ExchangeResult.GetHit || match.player1.exchangeResults[0] == ExchangeResult.BlockVs2Handed)
        {
            match.player2.Series.AddSeriesOfStrikes();
            match.player1.Series.ResetSeriesOfBlocks();    // ресет серии блоков
        }
        
        if (match.player1.exchangeResults[0] == ExchangeResult.Parry || match.player1.exchangeResults[0] == ExchangeResult.Block)
            match.player1.Series.AddSeriesOfBlocks();
        if (match.player2.exchangeResults[0] == ExchangeResult.Parry || match.player2.exchangeResults[0] == ExchangeResult.Block)
            match.player2.Series.AddSeriesOfBlocks();
        
        // Удар 2. Обновление коэфф. и добавление эффектов серий ударов
        if (match.player2.exchangeResults[1] == ExchangeResult.GetHit)
            match.player1.Series.AddStrongSeries(2);
        if (match.player1.exchangeResults[1] == ExchangeResult.GetHit)
            match.player2.Series.AddStrongSeries(2);
                
        if (match.player2.exchangeResults[1] == ExchangeResult.GetHit)
        {
            match.player1.Series.AddSeriesOfStrikes();
            match.player2.Series.ResetSeriesOfBlocks();    // ресет серии блоков
        }
        if (match.player1.exchangeResults[1] == ExchangeResult.GetHit)
        {
            match.player2.Series.AddSeriesOfStrikes();
            match.player1.Series.ResetSeriesOfBlocks();    // ресет серии блоков
        }
        
        if (match.player1.exchangeResults[1] == ExchangeResult.Parry || match.player1.exchangeResults[1] == ExchangeResult.Block)
            match.player1.Series.AddSeriesOfBlocks();
        if (match.player2.exchangeResults[1] == ExchangeResult.Parry || match.player2.exchangeResults[1] == ExchangeResult.Block)
            match.player2.Series.AddSeriesOfBlocks();
        
        // Коэффициенты серий ударов. Ресет.
        if (match.player2.exchangeResults[0] != ExchangeResult.GetHit && 
            match.player2.exchangeResults[1] != ExchangeResult.GetHit && 
            match.player2.exchangeResults[0] != ExchangeResult.BlockVs2Handed)                    
            match.player1.Series.ResetSeriesOfStrikes();
        if (match.player1.exchangeResults[0] != ExchangeResult.GetHit &&
            match.player1.exchangeResults[1] != ExchangeResult.GetHit && 
            match.player1.exchangeResults[0] != ExchangeResult.BlockVs2Handed)                    
            match.player2.Series.ResetSeriesOfStrikes();
    }

    private bool OneHeroLeft()                          // кто-то умер
    {
        if (match.player1.dead)
        {
            if (match.player2.dead)                          // ничья
            {
                match.roundWinner = null;
                return true;
            }
            match.player2.roundsWon++;                 // врагу +1 раунд
            match.player1.roundsLost++;                 
            match.roundWinner = match.player2;          
            return true;
        }
        if (match.player2.dead)
        {
            match.player1.roundsWon++;                   // мне +1 раунд
            match.player2.roundsLost++; 
            match.roundWinner = match.player1;          
            return true;
        }
        return false;
    }
    
    private PlayerObject GameWinner()
    {
        var roundsForEnemy = GameManager.gameType == GameType.Single ? 1 : match.numRoundsToWin;
        if (match.player1.roundsWon >= roundsForEnemy) return match.player1;
        if (match.player2.roundsWon >= match.numRoundsToWin) return match.player2;
        return null;
    }
    
    public Item GiveOutPrize(PlayerObject player)       
    {
        int item;
        do item = player.AddInventoryItem(AllItems.Instance.items[UnityEngine.Random.Range(0, AllItems.Instance.items.Length)]);
        while (item == -2);                                    // добавить уникальный инвентарь
        if (item != -1) return player.inventoryItems[item];    // и чтоб не был полный инвенторий (т.е. мы не выиграли 4 раунд, т.е. игру)
        else return null;
    }

    private void Awake() => _instance ??= this;

    public void Disable() =>  enabled = false;                 

    /* Пока закомментирую, когда вернусь к фотону, разберусь
    private void Update()
    {
        if (ClientDisconnected)
        {
            StopAllCoroutines();
            resultText.text = "Partner Disconnected!";
            // остановить болт
            player.RestartPressed();
        }
    }
    */
}

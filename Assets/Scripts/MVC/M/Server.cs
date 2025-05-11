using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Реализуем модель MVC (Не описывает сетевых протоколов, только логику игры):
///   Model = Server.cs = бизнес-логика
///   Controller = Client.cs - обработка ввода/вывода игрока, приведение его к интерфейсу сервера
///   View = UI
///
/// 1. В режиме сингл создаем сервер и 2 клиентов: player & AI
/// 2. В режиме мультисервер создаем сервер, адаптер сервера и 1 клиент-player
/// 3. В режиме мультиклиент создаем 1 клиент-player и адаптер клиента
///
/// Если существуют сетевые клиенты, реализовать для них паттерн адаптер photon->IServer в отдельном классе
/// (будет отлавливать события сервера и паковать их в исх. события photon'а, а вх. события photon'а в методы сервера)
/// 
/// </summary>

[Serializable]
public class MatchInfo
{
    public int amountRoundsToWin;                                 
    public int roundNumber;                                       
    public PlayerObject player1;                                  
    public PlayerObject player2;                                  
    public PlayerObject roundWinner;                              
    public PlayerObject matchWinner;                              
    
    public MatchInfo(int amountRoundsToWin, PlayerObject player1 = null, PlayerObject player2 = null)
    {
        this.amountRoundsToWin = amountRoundsToWin;
        roundNumber = 1;
        this.player1 = player1;
        this.player2 = player2;
        roundWinner = null;
        matchWinner = null;
    }
}

public class Server : MonoBehaviour, IServer
{
    [SerializeField] private List<PlayerObject> players = new();
    [SerializeField] private MatchInfo match = new(4);
    
    private static Server _instance;

    public event EventHandler<string> JoinedAction;
    public event EventHandler<StartMatchInfo> StartMatchAction;
    public event EventHandler<TurnOutInfo> ResultsReadyAction;
    public event EventHandler<EndMatchInfo> EndMatchAction;
    public event EventHandler<StartRoundInfo> StartRoundAction;
    public event EventHandler<EndRoundInfo> EndRoundAction;

    public static Server Instance => _instance;

    private void Awake() => _instance ??= this;
    
    public void Join(string name, EventHandler<string> onJoined)
    {
        players.Add(new PlayerObject(name));
        Debug.Log("Локальный сервер: клиент "+ name +" подключился к турниру");
        
        JoinedAction += onJoined;
        JoinedAction?.Invoke(this, name); 
        if (players.Count == 2) 
            StartCoroutine(StartMatch());                
    }

    public void SubscribeOnStartMatch(EventHandler<StartMatchInfo> onStartMatch) =>
        StartMatchAction += onStartMatch;
    
    public void SubscribeOnResultsReady(EventHandler<TurnOutInfo> onResultsReady) =>
        ResultsReadyAction += onResultsReady;
    
    public void SubscribeOnEndMatch(EventHandler<EndMatchInfo> onEndMatch) =>
        EndMatchAction += onEndMatch;
    
    public void SubscribeOnStartRound(EventHandler<StartRoundInfo> onStartRound) =>
        StartRoundAction += onStartRound;
    
    public void SubscribeOnEndRound(EventHandler<EndRoundInfo> onEndRound) =>
        EndRoundAction += onEndRound;
    
    private IEnumerator StartMatch()
    {
        match.roundNumber = 1;
        var player1 = match.player1 = players[0];
        var player2 = match.player2 = players[1];

        var player1MatchInfo = new StartMatchInfo();
        var player2MatchInfo = new StartMatchInfo();

        player1MatchInfo.PlayerName = player2MatchInfo.EnemyName = player1.Name;
        player1MatchInfo.EnemyName = player2MatchInfo.PlayerName = player2.Name;
        
        for (var i = 0; i < player1.InventoryItems.Length; i++)
        {
            if (player1.InventoryItems[i] != null)
                player1MatchInfo.PlayerInventoryItems[i] =
                    player2MatchInfo.EnemyInventoryItems[i] = player1.InventoryItems[i].Name;
        }

        for (var i = 0; i < player2.InventoryItems.Length; i++)
        {
            if (player2.InventoryItems[i] != null)
                player1MatchInfo.EnemyInventoryItems[i] =
                    player2MatchInfo.PlayerInventoryItems[i] = player2.InventoryItems[i].Name;
        }

        StartMatchAction?.Invoke(this, player1MatchInfo);
        StartMatchAction?.Invoke(this, player2MatchInfo);
        
        Debug.Log("Локальный сервер: матч между " + player1MatchInfo.PlayerName +" и "+ player2MatchInfo.PlayerName +" начинается");

        yield return new WaitForSeconds(ViewModel.StartDelay);
        StartNewRound();
    }
    
    private void StartNewRound()
    {
        var player1 = match.player1;
        var player2 = match.player2;
        
        player1.Reset();
        player2.Reset();
        
        Debug.Log("Локальный сервер: раунд " + match.roundNumber +" начинается");

        var player1StartRoundInfo = new StartRoundInfo()
        {
            PlayerName = player1.Name,
            RoundNumber = match.roundNumber,
        };
        var player2StartRoundInfo = new StartRoundInfo()
        {
            PlayerName = player2.Name,
            RoundNumber = match.roundNumber
        };
        player1StartRoundInfo.PlayerStartHealth = player2StartRoundInfo.EnemyStartHealth = player1.Tweakers.StartingHealth;
        player1StartRoundInfo.EnemyStartHealth = player2StartRoundInfo.PlayerStartHealth = player2.Tweakers.StartingHealth;
        StartRoundAction?.Invoke(this, player1StartRoundInfo);
        StartRoundAction?.Invoke(this, player2StartRoundInfo);
    }
    
    public void TakeDecision(string playerName, TurnInInfo turnInInfo)
    {
        var player = players.Find(player => player.Name == playerName);

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
        
        if (match.player1.dataTaken && match.player2.dataTaken)
            CalculateTurn();
    }

    private void CalculateTurn()
    {
        CalculateExchangeResultsAndDamages();
        AddSeries();
            
        var player1TurnOutInfo = new TurnOutInfo
        {
            PlayerName = match.player1.Name,
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
            PlayerName = match.player2.Name,
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
        
        ResultsReadyAction?.Invoke(this, player2TurnOutInfo);
        ResultsReadyAction?.Invoke(this, player1TurnOutInfo);
        
        if (OneHeroLeft()) 
            StartCoroutine(EndRound());
    }

    private IEnumerator EndRound()
    {
        match.matchWinner = GameWinner();
            
        Item prize;
        if (match.matchWinner == null)
            prize = match.roundWinner != null ? GiveOutPrize(match.roundWinner) : null;
        else
            prize = null;
        
        var player1EndRoundInfo = new EndRoundInfo()
        {
            PlayerName = match.player1.Name,
            RoundWinner = match.roundWinner != null ? match.roundWinner.Name : string.Empty,
            Prize = prize != null ? prize.Name : string.Empty
        };
        EndRoundAction?.Invoke(this, player1EndRoundInfo);
                
        var player2EndRoundInfo = new EndRoundInfo()
        {
            PlayerName = match.player2.Name,
            RoundWinner = match.roundWinner != null ? match.roundWinner.Name : string.Empty,
            Prize = prize != null ? prize.Name : string.Empty
        };
        EndRoundAction?.Invoke(this, player2EndRoundInfo);

        const float endDelay = ViewModel.DeathDelay + ViewModel.EndDelay + 0.5f;
        
        if (match.matchWinner != null)
        {
            var endMatchDelay = new WaitForSeconds(endDelay);
            yield return endMatchDelay;
            EndMatch();
        }
        else
        {
            match.roundNumber++;
            
            var endRoundDelay = new WaitForSeconds(2 * endDelay);
            yield return endRoundDelay;
            GiveOutRingToBotForFinalRound();
            StartNewRound();
        }
    }
    
    private void EndMatch()
    {
        var player1EndMatchInfo = new EndMatchInfo()    
        {
            PlayerName = match.player1.Name,
            MatchWinner = match.matchWinner.Name
        };
        EndMatchAction?.Invoke(this, player1EndMatchInfo);
        
        var player2EndMatchInfo = new EndMatchInfo()
        {
            PlayerName = match.player2.Name,
            MatchWinner = match.matchWinner.Name
        };
        EndMatchAction?.Invoke(this, player2EndMatchInfo);
        
        ResetMatch();
    }

    private void CalculateExchangeResultsAndDamages()
    {
        if (GameManager.GameType == GameType.Client)
            return;

        CalculatePreCoeffs();
        CalculateExchangeResultAndPossibleDamage();
        CalculateRealDamage();
    }

    private void CalculatePreCoeffs()
    {
        var player1 = match.player1;
        var player2 = match.player2;
        
        player1.CalculatePreCoeffs();
        player2.CalculatePreCoeffs();
        player1.preCoeffs[0].blockVs2Handed = player1.weaponSet == WeaponSet.SwordShield
                                                    && player1.preCoeffs[0].block
                                                    && player2.decision == Decision.Attack
                                                    && player2.weaponSet == WeaponSet.TwoHandedSword;
        player2.preCoeffs[0].blockVs2Handed = player2.weaponSet == WeaponSet.SwordShield
                                                    && player2.preCoeffs[0].block
                                                    && player1.decision == Decision.Attack 
                                                    && player1.weaponSet == WeaponSet.TwoHandedSword;
    }

    private void CalculateExchangeResultAndPossibleDamage()
    {
        var player1 = match.player1;
        var player2 = match.player2;
        
        HandleFirstStrike(player1, player2);
        HandleFirstStrike(player2, player1);
        
        player1.preCoeffs[0].damage = Mathf.Round(player1.preCoeffs[0].damage * (1 - player1.defencePart));
        player2.preCoeffs[0].damage = Mathf.Round(player2.preCoeffs[0].damage * (1 - player2.defencePart));
        
        HandleSecondStrike(player1, player2);
        HandleSecondStrike(player2, player1);
        
        player1.preCoeffs[1].damage = Mathf.Round(player1.preCoeffs[1].damage - (1 - player1.defencePart));
        player2.preCoeffs[1].damage = Mathf.Round(player2.preCoeffs[1].damage - (1 - player2.defencePart));
        
        void HandleFirstStrike(PlayerObject player, PlayerObject enemy)
        {
            player.exchangeResults[0] = enemy.decision == Decision.Attack
                ? player.CalculateExchangeResult(1)
                : ExchangeResult.No;
            if (player.exchangeResults[0] == ExchangeResult.BlockVs2Handed)
                enemy.preCoeffs[0].damage *= enemy.Tweakers.Part2HandedThroughShield;
        }

        void HandleSecondStrike(PlayerObject player, PlayerObject enemy)
        {
            player.exchangeResults[1] = enemy.decision == Decision.Attack && enemy.preCoeffs[1].damage != 0f
                ? player.CalculateExchangeResult(2)
                : ExchangeResult.No;
        }
    }
    
    private void CalculateRealDamage()
    {
        var player1 = match.player1;
        var player2 = match.player2;
        
        HandleStrike(player1, player2, 1);
        HandleStrike(player2, player1, 1);

        HandleStrike(player1, player2, 2);
        HandleStrike(player2, player1, 2);
        
        void HandleStrike(PlayerObject player, PlayerObject enemy, int strike)
        {
            if (player.exchangeResults[strike - 1] != ExchangeResult.GetHit &&
                player.exchangeResults[strike - 1] != ExchangeResult.BlockVs2Handed) 
                return;
            
            player.gotDamages[strike - 1] = (int) enemy.preCoeffs[strike - 1].damage;
            player.isDead = player.Hp.TakeDamage(player.gotDamages[strike - 1]);
        }
    }
    
    private void AddSeries()
    { 
        var player1 = match.player1;
        var player2 = match.player2;
        
        HandleStrike(player1, player2, 1);
        HandleStrike(player2, player1, 1);
        
        HandleStrike(player1, player2, 2);
        HandleStrike(player2, player1, 2);
        
        TryToResetSeriesOfStrikes(player1, player2);
        TryToResetSeriesOfStrikes(player2, player1);

        void HandleStrike(PlayerObject player, PlayerObject enemy, int strike)
        {
            if (enemy.exchangeResults[strike - 1] == ExchangeResult.GetHit ||
                enemy.exchangeResults[strike - 1] == ExchangeResult.BlockVs2Handed)
            {
                player.Series.TryToAddStrongSeries(strike);
                player.Series.AddSeriesOfStrikes();
                enemy.Series.ResetSeriesOfBlocks();
            }

            if (player.exchangeResults[strike - 1] == ExchangeResult.Parry ||
                player.exchangeResults[strike - 1] == ExchangeResult.Block)
                player.Series.AddSeriesOfBlocks();
        }

        void TryToResetSeriesOfStrikes(PlayerObject player, PlayerObject enemy)
        {
            if (enemy.exchangeResults[0] != ExchangeResult.GetHit && 
                enemy.exchangeResults[1] != ExchangeResult.GetHit && 
                enemy.exchangeResults[0] != ExchangeResult.BlockVs2Handed)                    
                player.Series.ResetSeriesOfStrikes();
        }
    }
    
    private bool OneHeroLeft()
    {
        var player1 = match.player1;
        var player2 = match.player2;
        
        if (player1.isDead)
        {
            if (player2.isDead)                   // ничья
            {
                match.roundWinner = null;
                return true;
            }
            
            player2.roundsWon++;                 
            player1.roundsLost++;                 
            match.roundWinner = player2;          
            return true;
        }
        
        if (player2.isDead)
        {
            player1.roundsWon++;                 
            player2.roundsLost++; 
            match.roundWinner = player1;          
            return true;
        }
        
        return false;
    }

    private PlayerObject GameWinner()
    {
        var enemyAmountRoundsToWin = GameManager.GameType == GameType.Single ? 1 : match.amountRoundsToWin;
        if (match.player1.roundsWon >= enemyAmountRoundsToWin)
            return match.player1;
        
        if (match.player2.roundsWon >= match.amountRoundsToWin)
            return match.player2;
        
        return null;
    }
    
    private Item GiveOutPrize(PlayerObject player)       
    {
        int item;
        do item = player.AddInventoryItem(AllItems.Instance.items[UnityEngine.Random.Range(0, AllItems.Instance.items.Length)]);
        while (item == -2);
        if (item != -1) 
            return player.InventoryItems[item];
        
        return null;
    }
    
    private void GiveOutRingToBotForFinalRound()       
    {
        if (match.roundWinner == match.player2 && match.player1.Name == "bot" &&
            match.player1.roundsLost == match.amountRoundsToWin - 1) 
            match.player1.AddInventoryItem(AllItems.Instance.items.First(i => i.Name == "ring_of_cunning"));
    }
    
    public void Disable() => enabled = false;

    private void ResetMatch()
    {
        players.Clear();
        
        JoinedAction = null;
        StartMatchAction = null;
        ResultsReadyAction = null;
        EndMatchAction = null;
        StartRoundAction = null;
        EndRoundAction = null;
    }
}

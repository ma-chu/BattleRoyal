using System.Collections;                                 // для сопрограмм
using UnityEngine;
using UnityEngine.UI;
using System;
using EF.Localization;
using EF.Sounds;
using EF.Tools;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

// #pragma warning disable 0649    // убирает предупреждения компилятора о [SerializeField] private переменных, инициализируемых в редакторе   

public enum WeaponSet : short { SwordShield, SwordSword, TwoHandedSword };                             // варианты сетов оружия у героя
public enum Heroes : short { Player, Enemy, Nobody };                                                  // варианты героев (победителей раундов и игры)

public enum Decision : short {No, Attack, ChangeSwordShield, ChangeSwordSword, ChangeTwoHandedSword};  // варианты действий героя - импульс на 1 такт
public enum ExchangeResult : short { No, Evade, Parry, BlockVs2Handed, Block, GetHit };                // варианты исхода размена ударами для каждого из 2 ударов противника
public enum GameType : short { Single, Server, Client };                                               // тип игры

[System.Serializable]
public struct PreCoeffs
{
    public float damage;                                           // Возможный нанесенный урон
    public bool evade;                                             // Уворот от удара на смене     
    public bool block;                                             // Блок удара
    public bool parry;                                             // Парирование удара   
    public bool blockVs2Handed;                                    // Блок удара двуручником - половина урона
}

public class GameManager : MonoBehaviour {
    // Вообще, статические события лучше не юзать: подписываясь на них, мы создаем ссылки на HeroManager в объекте (статическом), который не будет уничтожен,
    // пока существует класс GameManager, то есть пока не закончится работа программы. Но в данном случае все мои два HeroManager-а тоже вечны
    public static event Action ExchangeEvent1;             // удар1 состоялся
    public static event Action ExchangeEvent2;             // удар2 состоялся
    public static event Action ExchangeEndedEvent;         // весь сход закончен

    private static GameManager _instance;                  // ссылка на себя, сигнализирующая, создан ли (единственный - "singleton") инстанс этого класса или нет. Для Bolt-а// GameObject.FindGameObjectsWithTag() в ServerNetworkCallbacks не срабатывает
    public static GameManager Instance => _instance;                                                       
    
    public  PlayerManager player;                        
    public  EnemyManager enemy;                          

    [SerializeField] private int numRoundsToWin = 4;       // надо выиграть раундов для выигрыша игры
    
    private int _roundNumber = 0;                          // текущий номер раунда
    private Heroes _roundWinner;                           // победитель раунда
    private Heroes _gameWinner;                            // победитель игры

    [SerializeField] private Text resultText;                             // текст для вывода "Игра окончена" и прочего

    [SerializeField] private float startDelay = 3.5f;                     // стартовая задержка в секундах
    [SerializeField] private float endDelay = 5f;                         // конечная задержка в секундах
    [SerializeField] private float deathDelay = 2.5f;                     // задержка на смерть в секундах
    [SerializeField] private float attackDelay = 3f;                      // задержка на анимацию размена ударами
    [SerializeField] private float changeDelay = 7.5f;                    // задержка на анимацию смены оружия

    private WaitForSeconds _deathWait;                    // задержки понятного сопрограмме вида
    private WaitForSeconds _startWait;                    // переведение в него из секунд состоится в ф-ии Start()                 
    private WaitForSeconds _endWait;
    private WaitForSeconds _attackWait;
    private WaitForSeconds _changeWait;

    // Стафф конца игры
    [SerializeField] private Animator gameOverAnimator;
    [SerializeField] private GameObject fireExplodePrefab;                 // ссылка на объект-салют (префаб, состоящий из particle system (уже без звука)
    [SerializeField] private float explodesInterval = 1f;                  // задержка меж выстрелами в секундах

    [SerializeField] private int stupitidyChangeDelay;                     // задержка на тупизну бота перед сменой оружия
    
    // Multiplayer staff
    public static GameType gameType;      
    public static Photon.Bolt.BoltEntity myBoltEntity;           
    public static Photon.Bolt.BoltEntity enemyBoltEntity;
    public static bool ClientConnected = false;
    public static bool ClientDisconnected = false;
    [HideInInspector] public bool doServerExchange;
    [HideInInspector] public bool doClientExchange;
    private Photon.Bolt.IEFPlayerState _enemyState;
    
    [SerializeField] private Text myNameText;  
    [SerializeField] private Text enemyNameText;

    private void Awake()
    {
        if (_instance.IsNull()) _instance = this;
        //if (!SceneManager.GetSceneByName("Start").isLoaded) SceneManager.LoadScene ("Start", LoadSceneMode.Additive);
        if (!SceneManager.GetSceneByBuildIndex(1).isLoaded) SceneManager.LoadScene (1, LoadSceneMode.Additive);
    }

    private void Start()
    {
        myNameText.text = PlayerPrefs.GetString("username");

        _deathWait = new WaitForSeconds(deathDelay);         // инициализируем задержки: переводим секунды в понятный сопрограмме вид. Затем будем использовать их yield-ом
        _startWait = new WaitForSeconds(startDelay);
        _endWait = new WaitForSeconds(endDelay);
        _attackWait = new WaitForSeconds(attackDelay);
        _changeWait = new WaitForSeconds(changeDelay);
        
        StartCoroutine(GameLoop());                     // запускаем сам процесс боя как сопрограмму
                                                                // Почему как сопрограмму? Потому что будем прерывать её директивой yield return
    }

    private IEnumerator GameLoop()                              // основная петля поединка
    {
        // выход из yeild-ов-функций происходит по усоловию - выдаче соотв. функциями true
        if (gameType == GameType.Server) yield return StartCoroutine(WaitForNetworkPartner());

        if (_roundNumber == 0) yield return StartCoroutine(GameStarting());    // начало игры - обозначить цель

        yield return StartCoroutine(RoundStarting());   // начало раунда: вывод номера раунда и количества побед у бойцов. Стартовая пауза
        yield return StartCoroutine(RoundPlaying());    // сам процесс боя
        yield return StartCoroutine(RoundEnding());     // конец раунда: вывод победителя раунда, количества побед у бойцов и имени победителя. Конечная пауза

        _gameWinner = GameWinner();
        
        if (_gameWinner != Heroes.Nobody)
        {
            var winner = _gameWinner.ToString().Localize();
            if (gameType != GameType.Single)
            {
                if (_gameWinner == Heroes.Player) winner = PlayerPrefs.GetString("username");
                else winner = _enemyState.Username;
            }
            resultText.text = "game_over".Localize() + winner + "win".Localize();
            
            gameOverAnimator.SetTrigger("GameOver");
            SoundsManager.Instance.PlaySound(SoundsContainer.GetAudioClip(SoundTypes.GameOver));

            if (_gameWinner == Heroes.Player)
            {
                GameSave.LastLoadedSnapshot.tournamentsWon++;
                yield return StartCoroutine(Salute());
            }
            yield return _endWait;
            
            player.restartButtonObject.SetActive(true);
        }
        else
        {        
            StartCoroutine(GameLoop());         // рекурсия: текущая GameLoop() завершается и начинается новая 
        }
    }

    private IEnumerator WaitForNetworkPartner()         // ожидание сетевого партнера
    {
        while (!ClientConnected)
        {
            yield return null;
            resultText.text = "waiting".Localize();
        }
    }
    
    private IEnumerator GameStarting()                  // начало игры
    {
        if (gameType != GameType.Single)
        {
            _enemyState = enemyBoltEntity?.GetState<Photon.Bolt.IEFPlayerState>();
        }

        resultText.text = "Defeat".Localize() + numRoundsToWin + "to_win".Localize();
            
        yield return _startWait;                        
    }

    private IEnumerator RoundStarting()                 // начало раунда
    {
        //0. Имя врага
        enemyNameText.text = _enemyState?.Username ?? "enemyBot";        
        //1. Увеличить номер раунда.
        _roundNumber++;
        //2. Сформировать и вывести информационное сообщение.
        resultText.text="round".Localize() + _roundNumber.ToString();
        //3. Установить стартовые параметры игроку и врагу: твикеры (с учетом инвентаря), здоровье, начальные позиции и пр. Заблокировать кнопки управления игроку.
        // в самом начале игры инвентарь не сработает
        // ВОЗМОЖНО, тут лучше вызвать событие, и принять его не только HeroManager-ом, но и HP и пр.
        player.enabled = true;
        enemy.enabled = true;
        //4. Установить задержку на тупизну
        stupitidyChangeDelay = numRoundsToWin - HeroManager.player_countRoundsWon - 1;

        yield return _startWait;                       
    }

    private IEnumerator RoundPlaying()
    {        
        resultText.text = string.Empty;                       // 1. Очистить информационное сообщение
        
        
        while (!OneHeroLeft())                                  // 2. Играем раунд (пропускаем такты), пока кто-то не умрет
        {
            //1. Разблокировать кнопки управления игроку       
            if (!doServerExchange && !doClientExchange) player.m_PlayersControlsCanvas.enabled = true;

            //2. При одиночной игре определить решение врага: удар или смена оружия
            if (gameType == GameType.Single && enemy.decision == Decision.No)
            {
                MakeSinglePlayerEnemyDecision(HeroManager.player_countRoundsWon); 
            }
            //3. Ожидать действие игрока и врага: удара или смены оружия
            if (
                player.decision==Decision.No || enemy.decision==Decision.No
                || gameType == GameType.Server && !doServerExchange
                || gameType == GameType.Client && !doClientExchange)
            {
                yield return null;   // Решения еще нет - заканчиваем этот такт (и почему-то не переходим сразу к началу корутины, а проходим её тело до конца...)
                //Debug.Log("Why am I displaying?"); // А вот так работает yield return null - такт проходится до конца
            }
            //4. иначе рассчитать урон. (Плюс посмотреть, не умер ли кто. Если умер - идем на конец раунда)
            else
            {
                // a. предварительные коэффициенты, результат схода и урон для одиночной игры (для сетевой это сделает сервер в ServerNetworkCallbacks по событию)
                if (gameType == GameType.Single) ExchangeResultsAndDamages();

                // b. Удар 1. Обновление коэфф. и добавление эффектов серий ударов (серия блоков ставится по событию HeroManager'а-->HeroUI-->Series).
                if ((enemy.exchangeResult[0] == ExchangeResult.GetHit) || (enemy.exchangeResult[0] == ExchangeResult.BlockVs2Handed))
                    player.AddStrongSeries(1);
                if ((player.exchangeResult[0] == ExchangeResult.GetHit) || (player.exchangeResult[0] == ExchangeResult.BlockVs2Handed))
                    enemy.AddStrongSeries(1);
                
                if ((enemy.exchangeResult[0] == ExchangeResult.GetHit) || (enemy.exchangeResult[0] == ExchangeResult.BlockVs2Handed))
                    player.AddStrikesSeries();
                if ((player.exchangeResult[0] == ExchangeResult.GetHit) || (player.exchangeResult[0] == ExchangeResult.BlockVs2Handed))
                    enemy.AddStrikesSeries();

                // I. Удар1 состоялся  - запустить событие. 
                ExchangeEvent1?.Invoke();       

                // c. Удар 2. Обновление коэфф. и добавление эффектов серий ударов
                if (enemy.exchangeResult[1] == ExchangeResult.GetHit)
                    player.AddStrongSeries(2);
                if (player.exchangeResult[1] == ExchangeResult.GetHit)
                    enemy.AddStrongSeries(2);
                
                if (enemy.exchangeResult[1] == ExchangeResult.GetHit)
                    player.AddStrikesSeries();
                if (player.exchangeResult[1] == ExchangeResult.GetHit)
                    enemy.AddStrikesSeries();
                
                // d. Коэффициенты серий ударов. Ресет.
                if ((enemy.exchangeResult[0] != ExchangeResult.GetHit) && (enemy.exchangeResult[1] != ExchangeResult.GetHit) && (enemy.exchangeResult[0] != ExchangeResult.BlockVs2Handed))                    
                    player.ResetStrikesSeries();
                if ((player.exchangeResult[0] != ExchangeResult.GetHit)&&(player.exchangeResult[1] != ExchangeResult.GetHit)&&(player.exchangeResult[0] != ExchangeResult.BlockVs2Handed))                    
                    enemy.ResetStrikesSeries();
                
                // e. Смена оружия врага
                if (enemy.decision != Decision.Attack)
                {
                    switch (enemy.decision)
                    {
                        case Decision.ChangeSwordShield:
                            enemy.SetSwordShield();
                            break;
                        case Decision.ChangeSwordSword:
                            enemy.SetSwordSword();
                            break;
                        case Decision.ChangeTwoHandedSword:
                            enemy.SetTwoHandedSword();
                            break;
                    }
                }
                
                // II. Удар2 состоялся  - запустить событие 
                ExchangeEvent2?.Invoke();
                
                //5. Блокировка кнопок управления игрока и задержка на анимацию (атаки или смены)
                if (player.decision == Decision.Attack)
                {
                    if (enemy.decision==Decision.Attack) yield return _attackWait;                       
                    else yield return _changeWait;                                                         
                }
                else //  точно какая-то смена
                {
                    yield return _changeWait;                                                              
                }

                //6. Уменьшить или обнулить задержку на тупизну
                if ((enemy.decision == Decision.Attack) && (player.decision == Decision.Attack)) stupitidyChangeDelay -= 1;
                else if (enemy.decision != Decision.Attack) stupitidyChangeDelay = numRoundsToWin - HeroManager.player_countRoundsWon - 1;

                // III. Сход закончен  - запустить событие 
                ExchangeEndedEvent?.Invoke();
                
                //7. снять меркеры сделанного хода на клиенте и сервере
                if (gameType == GameType.Server)
                {
                    doServerExchange = false;
                }
                if (gameType == GameType.Client)
                {
                    doClientExchange = false;
                }
                
                yield return null;  // заканчиваем этот такт (и не переходим к концу корутины)
            }
        }
    }
    
    private IEnumerator RoundEnding()                       // конец раунда
    {
        //1. Всех на переинициализацию.
        player.enabled = false;
        enemy.enabled = false;
        yield return _deathWait;                           // используем m_DeathWait ( 2.5 сек), чтоб не плодить сущности
        
        //2. Вывести информационное сообщение.
        resultText.text = "round".Localize() + _roundNumber.ToString() + " " + "ended".Localize() + _roundWinner.ToString().Localize() + "win".Localize();
        
        //3. ждём еще одну m_DeathWait, ибо будем выдавать инвентарь
        if (_roundWinner == Heroes.Player || gameType != GameType.Single)
            yield return _deathWait;
        
        //4. Выдать игроку пункт инвентаря за победу в раунде
        if (_roundWinner == Heroes.Player)
        {
            string a = player.GiveOutPrize()?.Name;
            if (a != null) resultText.text = "you_got".Localize() + a.Localize();
            
            // Добавим инвентарь в state
            if (gameType!=GameType.Single && a!=null)
            {
                myBoltEntity.GetState<Photon.Bolt.IEFPlayerState>().InventoryItem = a;
            }
        }
        yield return _endWait;

        player.m_dead = false;
        enemy.m_dead = false;
        
        doClientExchange = false;
        doServerExchange = false;
    }
    
    private void MakeSinglePlayerEnemyDecision (int nicety)                   // Определиться с действием бота. nicety - уровень интеллекта врага
    {
        /* nicety = 0: 
         * 1. на свои серии реагирует, 
         * 2. на чужие - нет,
         * 3. относительно оружия - тупит 3 удара, затем меняет
         * 
         * nicety = 1: 
         * 1. на свои серии реагирует, 
         * 2. на чужие - нет,
         * 3. относительно оружия - тупит 2 удара, затем меняет
         * 
         * nicety = 2: 
         * 1. на свои серии реагирует, 
         * 2. на чужие тоже,
         * 3. относительно оружия - тупит 1 удар, затем меняет
         * 
         * nicety = 3: 
         * 1. на свои серии реагирует, 
         * 2. на чужие тоже,
         * 3. относительно оружия - сразу меняет
        */

        // 1. Если у врага есть серия - продолжаем её (при любом nicety):
        if (enemy.HasSeriesOfStrikes)
        {
            enemy.decision = Decision.Attack;
            enemy.defencePart = enemy.m_Tweakers.ParryChance;
            return;
        }
        if (enemy.HasSeriesOfBlocks)
        {
            enemy.decision = Decision.Attack;
            enemy.defencePart = enemy.m_Tweakers.MaxDefencePart + enemy.m_Tweakers.ParryChance;
            return;
        }
        // если нет
        // 2. Если у игрока есть серия - нейтрализуем её (при nicety > 1):
        if (nicety > 1)
        {
            if (player.HasSeriesOfStrikes)
            {
                enemy.decision = (enemy.weaponSet != WeaponSet.SwordShield) ? Decision.ChangeSwordShield : Decision.Attack;
                enemy.defencePart = enemy.m_Tweakers.MaxDefencePart + enemy.m_Tweakers.ParryChance;
                return;
            }
            if (player.HasSeriesOfBlocks)
            {
                enemy.decision = (enemy.weaponSet != WeaponSet.TwoHandedSword) ? Decision.ChangeTwoHandedSword : Decision.Attack;
                enemy.defencePart = enemy.m_Tweakers.ParryChance;
                return;
            }
        }
        // если нет
        // 3. Варианты относительно типов оружия (с задержкой на тупизну):
        // 1 - полный рандом; 2 - оптимум - то, что сейчас; 3 - идеальное - надо рассчитывать еще необходимость смены в зависимости от оставшегося здоровья
        // еще можно использовать комбинацию вариантов. Например, оптимум с добавлением небольшого шанса на рандом
        if (stupitidyChangeDelay > 0) enemy.decision = Decision.Attack;
        else if ((player.weaponSet == WeaponSet.SwordShield) && (enemy.weaponSet == WeaponSet.SwordSword))
            enemy.decision = Decision.ChangeTwoHandedSword;
        else if ((player.weaponSet == WeaponSet.SwordSword) && (enemy.weaponSet == WeaponSet.TwoHandedSword))
            enemy.decision = Decision.ChangeSwordShield;
        else if ((player.weaponSet == WeaponSet.TwoHandedSword) && (enemy.weaponSet == WeaponSet.SwordShield))
            enemy.decision = Decision.ChangeSwordSword;
        else enemy.decision = Decision.Attack;

        // 4. Тактику при этом пока будем выбирать по рандому:
        var tactic = UnityEngine.Random.value;
        enemy.defencePart = tactic < 0.33f ? enemy.m_Tweakers.MaxDefencePart + enemy.m_Tweakers.ParryChance : enemy.m_Tweakers.ParryChance;
    }
    
    public void MakeMultiplayerEnemyDecision(Decision decision, float defencePart, out int[] clientExchangeResult, out int[] clientDamage, out int[] serverExchangeResult, out int[] serverDamage)     // выполняется на сервере
    {
        //m_Enemy.decision = (Decision) decision;    //лишнее, уже сделано в ServerNetworkCallbacks по событию EFReadyForExchangeEvent
        //m_Enemy.defencePart = defencePart;

        ExchangeResultsAndDamages();

        clientExchangeResult = new int[2];
        clientExchangeResult[0] = (int) enemy.exchangeResult[0];
        clientExchangeResult[1] = (int) enemy.exchangeResult[1];
        clientDamage = new int[2];
        clientDamage[0] = enemy.gotDamage[0];
        clientDamage[1] = enemy.gotDamage[1];
        
        serverExchangeResult = new int[2];                        
        serverExchangeResult[0] = (int) player.exchangeResult[0];
        serverExchangeResult[1] = (int) player.exchangeResult[1];
        serverDamage = new int[2];                                
        serverDamage[0] = player.gotDamage[0];                   
        serverDamage[1] = player.gotDamage[1];                   
    }

    private void ExchangeResultsAndDamages()
    {
        if (gameType == GameType.Client) return;
        // 1. Сперва рассчитаем предварительные коэффициенты на основе текущего набора оружия и решения
        player.CalculatePreCoeffs();
        enemy.CalculatePreCoeffs();
        player.preCoeffs[0].blockVs2Handed = (player.weaponSet == WeaponSet.SwordShield)
                                               && (player.preCoeffs[0].block)
                                               && (enemy.decision == Decision.Attack)
                                               && (enemy.weaponSet == WeaponSet.TwoHandedSword);
        enemy.preCoeffs[0].blockVs2Handed = (enemy.weaponSet == WeaponSet.SwordShield)
                                              && (enemy.preCoeffs[0].block) 
                                              && (player.decision == Decision.Attack) 
                                              && (player.weaponSet == WeaponSet.TwoHandedSword);
        
        // 2.На основе предварительных коэффицентов определяем результат схода и урон
        // Удар 1 
        enemy.exchangeResult[0] = (player.decision == Decision.Attack)
            ? enemy.CalculateExchangeResult(1)
            : ExchangeResult.No;
        if (enemy.exchangeResult[0] == ExchangeResult.BlockVs2Handed)
        {
            player.preCoeffs[0].damage *= player.m_Tweakers.Part2HandedThroughShield;
        }

        player.exchangeResult[0] = (enemy.decision == Decision.Attack)
            ? player.CalculateExchangeResult(1)
            : ExchangeResult.No;
        if (player.exchangeResult[0] == ExchangeResult.BlockVs2Handed)
        {
            enemy.preCoeffs[0].damage *= enemy.m_Tweakers.Part2HandedThroughShield;
        }

        enemy.preCoeffs[0].damage =
            Mathf.Round(enemy.preCoeffs[0].damage - enemy.preCoeffs[0].damage * enemy.defencePart); // уберём часть урона, потраченную на парирование, и округлим
        player.preCoeffs[0].damage =
            Mathf.Round(player.preCoeffs[0].damage - player.preCoeffs[0].damage * player.defencePart); // уберём часть урона, потраченную на парирование, и округлим

        enemy.gotDamage[0] = (int) player.preCoeffs[0].damage;
        player.gotDamage[0] = (int) enemy.preCoeffs[0].damage;
        
        // Удар 2
        enemy.exchangeResult[1] = ((player.decision == Decision.Attack) && (player.preCoeffs[1].damage != 0f))
            ? enemy.CalculateExchangeResult(2)
            : ExchangeResult.No;
        player.exchangeResult[1] = ((enemy.decision == Decision.Attack) && (enemy.preCoeffs[1].damage != 0f))
            ? player.CalculateExchangeResult(2)
            : ExchangeResult.No;
            
        enemy.preCoeffs[1].damage =
            Mathf.Round(enemy.preCoeffs[1].damage - enemy.preCoeffs[1].damage * enemy.defencePart); // уберём часть урона, потраченную на парирование, и округлим
        player.preCoeffs[1].damage =
            Mathf.Round(player.preCoeffs[1].damage - player.preCoeffs[1].damage * player.defencePart); // уберём часть урона, потраченную на парирование, и округлим

        enemy.gotDamage[1] = (int) Mathf.Round(player.preCoeffs[1].damage);
        player.gotDamage[1] = (int) Mathf.Round(enemy.preCoeffs[1].damage);
    }

    private bool OneHeroLeft()                          // кто-то умер
    {
        if (player.m_dead)
        {
            if (enemy.m_dead)                         // ничья
            {
                _roundWinner = Heroes.Nobody;
                return true;
            }
            HeroManager.enemy_countRoundsWon++;         // врагу +1 раунд
            _roundWinner = Heroes.Enemy;          
            return true;
        }
        if (enemy.m_dead)
        {
            HeroManager.player_countRoundsWon++;        // мне +1 раунд
            _roundWinner = Heroes.Player;
            return true;
        }
        return false;
    }

    private Heroes GameWinner()
    {
        var roundsForEnemy = gameType == GameType.Single ? 1 : numRoundsToWin;
        if (HeroManager.enemy_countRoundsWon >= roundsForEnemy) return Heroes.Enemy;
        if (HeroManager.player_countRoundsWon >= numRoundsToWin) return Heroes.Player;
        return Heroes.Nobody;
    }

    private IEnumerator Salute()
    {
        var explodesWait = new WaitForSeconds(explodesInterval);
        var fireExplodeParticles = Instantiate(fireExplodePrefab).GetComponent<ParticleSystem>();
        var grenadeSound = SoundsContainer.GetAudioClip(SoundTypes.Grenade);
        
        // первый выстрел
        fireExplodeParticles.transform.position = new Vector3(-1f, 2f, 2.35f);
        fireExplodeParticles.Play();
        SoundsManager.Instance.PlaySound(grenadeSound);
        yield return explodesWait; 
        
        // второй выстрел
        fireExplodeParticles.transform.position = new Vector3(-3f, 2.5f, 2.55f);
        fireExplodeParticles.Play(); 
        SoundsManager.Instance.PlaySound(grenadeSound);
        yield return explodesWait;
        
        // третий выстрел
        fireExplodeParticles.transform.position = new Vector3(1f, 2.2f, 2.15f);
        fireExplodeParticles.Play();
        SoundsManager.Instance.PlaySound(grenadeSound);
    }

    private void Update()
    {
        if (ClientDisconnected)
        {
            StopAllCoroutines();
            resultText.text = "Partner Disconnected!";
            player.RestartPressed();
        }
    }
}

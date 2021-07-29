using System.Collections;                                 // для сопрограмм
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq; // для Events
using EF.Localization; 
// #pragma warning disable 0649    // убирает предупреждения компилятора о [SerializeField] private переменных, инициализируемых в редакторе   

public enum WeaponSet : short { SwordShield, SwordSword, TwoHandedSword };                              // варианты сетов оружия у героя
public enum Players : short { Player, Enemy, Nobody };                                                  // варианты победителей раундов и игры
public enum Decision : short {No, Attack, ChangeSwordShield, ChangeSwordSword, ChangeTwoHandedSword}; // варианты действий героя - импульс на 1 такт
public enum ExchangeResult : short { No, Evade, Parry, BlockVs2Handed, Block, GetHit };                 // варианты исхода размена ударами для каждого противника
public enum GameType : short { Single, Server, Client };                                                // тип игры

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

    public static GameManager instance;                    // ссылка на себя, сигнализирующая, создан ли (единственный - "singleton") инстанс этого класса или нет. Для Bolt-а
                                                            // GameObject.FindGameObjectsWithTag() в ServerNetworkCallbacks не срабатывает
    public  PlayerManager m_Player;                        // ссылка на менеджер игрока
    public  EnemyManager m_Enemy;                          // ссылка на менеджер врага

    [SerializeField]
    private int m_NumRoundsToWin = 4;                      // надо выиграть раундов для выигрыша игры
    private int m_roundNumber = 0;                         // текущий номер раунда
    private Players m_roundWinner;                         // победитель раунда
    private Players m_gameWinner;                          // победитель игры

    [SerializeField]
    private Text m_resultText;                             // текст для вывода "Игра окончена" и прочего

    [SerializeField]
    private float m_StartDelay = 3.5f;                     // стартовая задержка в секундах
    [SerializeField]
    private float m_EndDelay = 5f;                         // конечная задержка в секундах
    [SerializeField]
    private float m_DeathDelay = 2.5f;                     // задержка на смерть в секундах
    [SerializeField]
    private float m_AttackDelay = 3f;                      // задержка на анимацию размена ударами
    [SerializeField]
    private float m_ChangeDelay = 7.5f;                    // задержка на анимацию смены оружия

    private WaitForSeconds m_DeathWait;                    // задержки понятного сопрограмме вида
    private WaitForSeconds m_StartWait;                    // переведение в него из секунд состоится в ф-ии Start()                 
    private WaitForSeconds m_EndWait;
    private WaitForSeconds m_AttackWait;
    private WaitForSeconds m_ChangeWait;

    // Стафф конца игры
    [SerializeField]
    private Animator m_gameOverAnimator;                    
    [SerializeField]
    private AudioSource SFXAudio;                           // аудио-сорс общих звуковых эффектов игры: пока только звук конца игры
    // SFX и VFX победы
    [SerializeField]
    private AudioClip m_audioClip_GameOver;                 // клип конца игры  
    [SerializeField]
    private GameObject m_FireExplodePrefab;                 // ссылка на объект-салют (префаб, состоящий из particle system и звука)
    [SerializeField]
    private AudioClip m_FireExplodeaudioClip;               // аудио-клип салюта
    private AudioSource m_FireExplodeAudio;                 // ссылки на компоненты конкретного инстанса префаба салюта
    private ParticleSystem m_FireExplodeParticles;
    [SerializeField]
    private float m_ExplodesInterval = 1f;                  // задержка меж выстрелами в секундах
    private WaitForSeconds m_ExplodesWait;                  

    [SerializeField]
    private int stupitidyChangeDelay;                       // задержка на тупизну перед сменой оружия
    
    // Multiplayer staff
    public static GameType gameType;      
    public static BoltEntity myBoltEntity;           
    public static BoltEntity enemyBoltEntity;
    public static bool ClientConnected = false;

    public bool doServerExchange;
    public bool doClientExchange;

    private IEFPlayerState _enemyState;
    [SerializeField] private Text m_myNameText;  
    [SerializeField] private Text m_enemyNameText;
    
    private void Start()
    {
        if (instance == null) instance = this;
        m_DeathWait = new WaitForSeconds(m_DeathDelay);     // инициализируем задержки: переводим секунды в понятный сопрограмме вид. Затем будем использовать их yield-ом
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);
        m_AttackWait = new WaitForSeconds(m_AttackDelay);
        m_ChangeWait = new WaitForSeconds(m_ChangeDelay);
        
        StartCoroutine(GameLoop());                     // запускаем сам процесс боя как сопрограмму
                                                                // Почему как сопрограмму? Потому что будем прерывать её директивой yield return
    }

    private IEnumerator GameLoop()                              // основная петля поединка
    {
        // выход из этих yeild-ов происходит по усоловию - выдаче соотв. функциями true
        if ((gameType == GameType.Server)||(gameType == GameType.Client)) yield return StartCoroutine(WaitForNetworkPartner());
        if (m_roundNumber == 0) yield return StartCoroutine(GameStarting());    // начало игры - обозначить цель

        yield return StartCoroutine(RoundStarting());   // начало раунда: вывод номера раунда и количества побед у бойцов. Стартовая пауза
        yield return StartCoroutine(RoundPlaying());    // сам процесс боя
        yield return StartCoroutine(RoundEnding());     // конец раунда: вывод победителя раунда, количества побед у бойцов и имени победителя. Конечная пауза

        m_gameWinner = GameWinner();
        
        if (m_gameWinner != Players.Nobody)
        {
            var winner = m_gameWinner.ToString().Localize();
            if (gameType != GameType.Single)
            {
                if (m_gameWinner == Players.Player) winner = PlayerPrefs.GetString("username");
                else winner = _enemyState.Username;
            }
            m_resultText.text = "game_over".Localize() + winner + "win".Localize();
            
            m_gameOverAnimator.SetTrigger("GameOver");                   
            SFXAudio.clip = m_audioClip_GameOver;                        
            SFXAudio.Play();
            
            if (m_gameWinner == Players.Player)
            {
                GameSave.LastLoadedSnapshot.tournamentsWon++;
                yield return StartCoroutine(Salute());
            }
            yield return m_EndWait;
            
            m_Player.restartButtonObject.SetActive(true);
        }
        else
        {        
            StartCoroutine(GameLoop());         // рекурсия: текущая GameLoop() завершается и начинается новая 
        }
    }

    private IEnumerator WaitForNetworkPartner()         // ожидание сетевого партнера
    {
        //Debug.LogWarning(this.name + " GameType = " + gameType);
        while (!ClientConnected)
        {
            yield return null;
            m_resultText.text = "waiting".Localize();
        }
    }
    
    private IEnumerator GameStarting()                  // начало игры
    {
        if (gameType != GameType.Single)
        {
            _enemyState = enemyBoltEntity?.GetState<IEFPlayerState>();
        }

        m_resultText.text = "Defeat".Localize() + m_NumRoundsToWin.ToString() + "to_win".Localize();
            
        yield return m_StartWait;                        
    }

    private IEnumerator RoundStarting()                 // начало раунда
    {
        //0. Имена
        m_myNameText.text = PlayerPrefs.GetString("username");
        m_enemyNameText.text = _enemyState?.Username ?? "enemyBot";        
        //1. Увеличить номер раунда.
        m_roundNumber++;
        //2. Сформировать и вывести информационное сообщение.
        m_resultText.text="round".Localize() + m_roundNumber.ToString();
        //3. Установить стартовые параметры игроку и врагу: твикеры (с учетом инвентаря), здоровье, начальные позиции и пр. Заблокировать кнопки управления игроку.
        // в самом начале игры инвентарь не сработает
        // ВОЗМОЖНО, тут лучше вызвать событие, и принять его не только HeroManager-ом, но и HP и пр.
        m_Player.enabled = true;
        m_Enemy.enabled = true;
        //4. Установить задержку на тупизну
        stupitidyChangeDelay = m_NumRoundsToWin - HeroManager.player_countRoundsWon - 1;

        yield return m_StartWait;                       
    }

    private IEnumerator RoundPlaying()
    {        
        m_resultText.text = string.Empty;                       // 1. Очистить информационное сообщение
        
        
        while (!OneHeroLeft())                                  // 2. Играем раунд (пропускаем такты), пока кто-то не умрет
        {
            //a. Разблокировать кнопки управления игроку       
            if (!doServerExchange && !doClientExchange) m_Player.m_PlayersControlsCanvas.enabled = true;

            //b0. При одиночной игре определить решение врага: удар или смена оружия
            if (gameType == GameType.Single)
            {
                MakeSinglePlayerEnemyDecision(HeroManager.player_countRoundsWon); 
            }
            //b1. Ожидать действие игрока и врага: удара или смены оружия
            if (
                ((m_Player.decision==Decision.No || m_Enemy.decision==Decision.No)
                || (gameType == GameType.Server && !doServerExchange)
                || (gameType == GameType.Client && !doClientExchange))
            )
            {
                yield return null;   // Решения еще нет - заканчиваем этот такт (и почему-то не переходим сразу к началу корутины, а проходим её тело до конца...)
                //Debug.Log("Why am I displaying?");
            }
            // иначе рассчитать урон. (Плюс посмотреть, не умер ли кто. Если умер - идем на конец раунда)
            else
            {
                if (gameType == GameType.Single)
                {
                    // 1,2 и 4: предварительные коэффициенты & результат схода и урон. Уже не здесь же серии.
                    ExchangeResultsAndDamages();
                }
               
                
                
                // 3. Удар 1. Установка коэффициентов серий ударов (серия блоков ставится по событию HeroManager'а-->HeroUI-->Series).
                if ((m_Enemy.exchangeResult[0] == ExchangeResult.GetHit) || (m_Enemy.exchangeResult[0] == ExchangeResult.BlockVs2Handed))
                    m_Player.AddStrongSeries(1);
                if ((m_Player.exchangeResult[0] == ExchangeResult.GetHit) || (m_Player.exchangeResult[0] == ExchangeResult.BlockVs2Handed))
                    m_Enemy.AddStrongSeries(1);
                
                if ((m_Enemy.exchangeResult[0] == ExchangeResult.GetHit) || (m_Enemy.exchangeResult[0] == ExchangeResult.BlockVs2Handed))
                    m_Player.AddStrikesSeries();
                if ((m_Player.exchangeResult[0] == ExchangeResult.GetHit) || (m_Player.exchangeResult[0] == ExchangeResult.BlockVs2Handed))
                    m_Enemy.AddStrikesSeries();

                // I. Удар1 состоялся  - запустить событие. 
                ExchangeEvent1?.Invoke();       // эта запись равносильна такой: if (ExchangeEvent1 != null) ExchangeEvent1 ();

                // 5. Удар 2. Коэффициенты серий; установка. 
                if (m_Enemy.exchangeResult[1] == ExchangeResult.GetHit)
                    m_Player.AddStrongSeries(2);
                if (m_Player.exchangeResult[1] == ExchangeResult.GetHit)
                    m_Enemy.AddStrongSeries(2);
                
                if (m_Enemy.exchangeResult[1] == ExchangeResult.GetHit)
                    m_Player.AddStrikesSeries();
                if (m_Player.exchangeResult[1] == ExchangeResult.GetHit)
                    m_Enemy.AddStrikesSeries();
                
                // 6. коэффициент серий ударов. Ресет.
                if ((m_Enemy.exchangeResult[0] != ExchangeResult.GetHit) && (m_Enemy.exchangeResult[1] != ExchangeResult.GetHit) && (m_Enemy.exchangeResult[0] != ExchangeResult.BlockVs2Handed))                    
                    m_Player.ResetStrikesSeries();
                if ((m_Player.exchangeResult[0] != ExchangeResult.GetHit)&&(m_Player.exchangeResult[1] != ExchangeResult.GetHit)&&(m_Player.exchangeResult[0] != ExchangeResult.BlockVs2Handed))                    
                    m_Enemy.ResetStrikesSeries();
                
                // 7. Смена оружия врага
                if (m_Enemy.decision != Decision.Attack)
                {
                    switch (m_Enemy.decision)
                    {
                        case Decision.ChangeSwordShield:
                            m_Enemy.SetSwordShield();
                            break;
                        case Decision.ChangeSwordSword:
                            m_Enemy.SetSwordSword();
                            break;
                        case Decision.ChangeTwoHandedSword:
                            m_Enemy.SetTwoHandedSword();
                            break;
                    }
                }
                
                // II. Удар2 состоялся  - запустить событие 
                ExchangeEvent2?.Invoke();

                
                
                //c. Блокировка кнопок управления игрока и задержка на анимацию (атаки или смены)
                if (m_Player.decision == Decision.Attack)
                {
                    if (m_Enemy.decision==Decision.Attack) yield return m_AttackWait;                       
                    else yield return m_ChangeWait;                                                         
                }
                else //  точно какая-то смена
                {
                    yield return m_ChangeWait;                                                              
                }

                //d. Уменьшить или обнулить задержку на тупизну
                if ((m_Enemy.decision == Decision.Attack) && (m_Player.decision == Decision.Attack)) stupitidyChangeDelay -= 1;
                else if (m_Enemy.decision != Decision.Attack) stupitidyChangeDelay = m_NumRoundsToWin - HeroManager.player_countRoundsWon - 1;

                // III. Сход закончен  - запустить событие 
                ExchangeEndedEvent?.Invoke();
                
                //e. снять меркеры хода на клиенте и сервере
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
        m_Player.enabled = false;
        m_Enemy.enabled = false;
        yield return m_DeathWait;                           // используем m_DeathWait ( 2.5 сек), чтоб не плодить сущности
        
        //2. Вывести информационное сообщение.
        m_resultText.text = "round".Localize() + m_roundNumber.ToString() + " " + "ended".Localize() + m_roundWinner.ToString().Localize() + "win".Localize();
        
        //3. ждём еще одну m_DeathWait, ибо будем выдавать инвентарь
        if (m_roundWinner == Players.Player || gameType != GameType.Single)
            yield return m_DeathWait;
        
        //4. Выдать игроку пункт инвентаря за победу в раунде
        if (m_roundWinner == Players.Player)
        {
            string a = m_Player.GiveOutPrize()?.Name;
            if (a != null) m_resultText.text = "you_got".Localize() + a.Localize();
            
            // Добавим инвентарь в state
            if (gameType != GameType.Single && a != null)
            {
                myBoltEntity.GetState<IEFPlayerState>().InventoryItem = a;
            }
        }
        yield return m_EndWait;

        m_Player.m_dead = false;
        m_Enemy.m_dead = false;
        
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
        if (m_Enemy.HasSeriesOfStrikes)
        {
            m_Enemy.decision = Decision.Attack;
            m_Enemy.defencePart = m_Enemy.m_Tweakers.ParryChance;
            return;
        }
        if (m_Enemy.HasSeriesOfBlocks)
        {
            m_Enemy.decision = Decision.Attack;
            m_Enemy.defencePart = m_Enemy.m_Tweakers.MaxDefencePart + m_Enemy.m_Tweakers.ParryChance;
            return;
        }
        // если нет
        // 2. Если у игрока есть серия - нейтрализуем её (при nicety > 1):
        if (nicety > 1)
        {
            if (m_Player.HasSeriesOfStrikes)
            {
                m_Enemy.decision = (m_Enemy.weaponSet != WeaponSet.SwordShield) ? Decision.ChangeSwordShield : Decision.Attack;
                /*if (m_Enemy.weaponSet != WeaponSet.SwordShield) m_Enemy.decision = Decision.ChangeSwordShield;
                else m_Enemy.decision = Decision.Attack;*/
                m_Enemy.defencePart = m_Enemy.m_Tweakers.MaxDefencePart + m_Enemy.m_Tweakers.ParryChance;
                return;
            }
            if (m_Player.HasSeriesOfBlocks)
            {
                m_Enemy.decision = (m_Enemy.weaponSet != WeaponSet.TwoHandedSword) ? Decision.ChangeTwoHandedSword : Decision.Attack;
                /*if (m_Enemy.weaponSet != WeaponSet.TwoHandedSword) m_Enemy.decision = Decision.ChangeTwoHandedSword;
                else m_Enemy.decision = Decision.Attack;*/
                m_Enemy.defencePart = m_Enemy.m_Tweakers.ParryChance;
                return;
            }
        }
        // если нет
        // 3. Варианты относительно типов оружия (с задержкой на тупизну):
        // 1 - полный рандом; 2 - оптимум - то, что сейчас; 3 - идеальное - надо рассчитывать еще необходимость смены в зависимости от оставшегося здоровья
        // еще можно использовать комбинацию вариантов. Например, оптимум с добавлением небольшого шанса на рандом
        if (stupitidyChangeDelay > 0) m_Enemy.decision = Decision.Attack;
        else if ((m_Player.weaponSet == WeaponSet.SwordShield) && (m_Enemy.weaponSet == WeaponSet.SwordSword))
            m_Enemy.decision = Decision.ChangeTwoHandedSword;
        else if ((m_Player.weaponSet == WeaponSet.SwordSword) && (m_Enemy.weaponSet == WeaponSet.TwoHandedSword))
            m_Enemy.decision = Decision.ChangeSwordShield;
        else if ((m_Player.weaponSet == WeaponSet.TwoHandedSword) && (m_Enemy.weaponSet == WeaponSet.SwordShield))
            m_Enemy.decision = Decision.ChangeSwordSword;
        else m_Enemy.decision = Decision.Attack;

        // 4. Тактику при этом пока будем выбирать по рандому:
        var tactic = UnityEngine.Random.value;                 
        if (tactic < 0.33f)
        {
            m_Enemy.defencePart = m_Enemy.m_Tweakers.MaxDefencePart + m_Enemy.m_Tweakers.ParryChance;
            return;
        }
        m_Enemy.defencePart = m_Enemy.m_Tweakers.ParryChance;
    }
    
    public void MakeMultiplayerEnemyDecision(Decision decision, float defencePart, out int[] clientExchangeResult, out int[] clientDamage, out int[] serverExchangeResult, out int[] serverDamage)
    // выполняется на сервере
    {
        m_Enemy.decision = (Decision) decision;    //лишнее, уже сделано в ServerNetworkCallbacks по событию EFReadyForExchangeEvent
        m_Enemy.defencePart = defencePart;

        ExchangeResultsAndDamages();

        clientExchangeResult = new int[2];
        clientExchangeResult[0] = (int) m_Enemy.exchangeResult[0];
        clientExchangeResult[1] = (int) m_Enemy.exchangeResult[1];
        clientDamage = new int[2];
        clientDamage[0] = m_Enemy.gotDamage[0];
        clientDamage[1] = m_Enemy.gotDamage[1];
        
        serverExchangeResult = new int[2];                        
        serverExchangeResult[0] = (int) m_Player.exchangeResult[0];
        serverExchangeResult[1] = (int) m_Player.exchangeResult[1];
        serverDamage = new int[2];                                
        serverDamage[0] = m_Player.gotDamage[0];                   
        serverDamage[1] = m_Player.gotDamage[1];                   
    }

    private void ExchangeResultsAndDamages()
    {
        // 1. Сперва рассчитаем предварительные коэффициенты на основе текущего набора оружия и решения
        if (gameType != GameType.Client)
        {
            m_Player.CalculatePreCoeffs();
            m_Enemy.CalculatePreCoeffs();
            m_Player.preCoeffs[0].blockVs2Handed = (m_Player.weaponSet == WeaponSet.SwordShield)
                                                       && (m_Player.preCoeffs[0].block)
                                                       && (m_Enemy.decision == Decision.Attack)
                                                       && (m_Enemy.weaponSet == WeaponSet.TwoHandedSword);
            m_Enemy.preCoeffs[0].blockVs2Handed = (m_Enemy.weaponSet == WeaponSet.SwordShield)
                                                      && (m_Enemy.preCoeffs[0].block) 
                                                      && (m_Player.decision == Decision.Attack) 
                                                      && (m_Player.weaponSet == WeaponSet.TwoHandedSword);
        }
        //2. Удар 1. На основе предварительных коэффицентов определяем результат схода и урон.
        if (gameType != GameType.Client)
        {
            m_Enemy.exchangeResult[0] = (m_Player.decision == Decision.Attack)
                ? m_Enemy.CalculateExchangeResult(1)
                : ExchangeResult.No;
            if (m_Enemy.exchangeResult[0] == ExchangeResult.BlockVs2Handed)
            {
                m_Player.preCoeffs[0].damage *= m_Player.m_Tweakers.Part2HandedThroughShield;
            }

            m_Player.exchangeResult[0] = (m_Enemy.decision == Decision.Attack)
                ? m_Player.CalculateExchangeResult(1)
                : ExchangeResult.No;
            if (m_Player.exchangeResult[0] == ExchangeResult.BlockVs2Handed)
            {
                m_Enemy.preCoeffs[0].damage *= m_Enemy.m_Tweakers.Part2HandedThroughShield;
            }

            m_Enemy.preCoeffs[0].damage =
                Mathf.Round(m_Enemy.preCoeffs[0].damage - m_Enemy.preCoeffs[0].damage * m_Enemy.defencePart); // уберём часть урона, потраченную на парирование, и округлим
            m_Player.preCoeffs[0].damage =
                Mathf.Round(m_Player.preCoeffs[0].damage - m_Player.preCoeffs[0].damage * m_Player.defencePart); // уберём часть урона, потраченную на парирование, и округлим

            m_Enemy.gotDamage[0] = (int) m_Player.preCoeffs[0].damage;
            m_Player.gotDamage[0] = (int) m_Enemy.preCoeffs[0].damage;
        }
        // 4. Удар 2. Результат схода, урон. 
        if (gameType != GameType.Client)
        {
            m_Enemy.exchangeResult[1] = ((m_Player.decision == Decision.Attack) && (m_Player.preCoeffs[1].damage != 0f))
                ? m_Enemy.CalculateExchangeResult(2)
                : ExchangeResult.No;
            m_Player.exchangeResult[1] = ((m_Enemy.decision == Decision.Attack) && (m_Enemy.preCoeffs[1].damage != 0f))
                ? m_Player.CalculateExchangeResult(2)
                : ExchangeResult.No;
            
            m_Enemy.preCoeffs[1].damage =
                Mathf.Round(m_Enemy.preCoeffs[1].damage - m_Enemy.preCoeffs[1].damage * m_Enemy.defencePart); // уберём часть урона, потраченную на парирование, и округлим
            m_Player.preCoeffs[1].damage =
                Mathf.Round(m_Player.preCoeffs[1].damage - m_Player.preCoeffs[1].damage * m_Player.defencePart); // уберём часть урона, потраченную на парирование, и округлим

            m_Enemy.gotDamage[1] = (int) Mathf.Round(m_Player.preCoeffs[1].damage);
            m_Player.gotDamage[1] = (int) Mathf.Round(m_Enemy.preCoeffs[1].damage);
        }
    }

    private bool OneHeroLeft()                          // кто-то умер
    {
        if (m_Player.m_dead)
        {
            if (m_Enemy.m_dead)                         // ничья
            {
                m_roundWinner = Players.Nobody;
                return true;
            }
            HeroManager.enemy_countRoundsWon++;         // врагу +1 раунд
            m_roundWinner = Players.Enemy;          
            return true;
        }
        if (m_Enemy.m_dead)
        {
            HeroManager.player_countRoundsWon++;        // мне +1 раунд
            m_roundWinner = Players.Player;
            return true;
        }
        return false;
    }

    private Players GameWinner()
    {
        var roundsForEnemy = gameType == GameType.Single ? 1 : m_NumRoundsToWin;
        if (HeroManager.enemy_countRoundsWon >= roundsForEnemy) return Players.Enemy;
        if (HeroManager.player_countRoundsWon >= m_NumRoundsToWin) return Players.Player;
        return Players.Nobody;
    }

    private IEnumerator Salute()
    {
        m_ExplodesWait = new WaitForSeconds(m_ExplodesInterval);
        // первый выстрел
        m_FireExplodeParticles = Instantiate(m_FireExplodePrefab).GetComponent<ParticleSystem>();    // порождаем инстанс префаба взрыва и берем компонент этого инстанса
        m_FireExplodeAudio = m_FireExplodeParticles.GetComponent<AudioSource>();                     // берём другой компонент (можно ссылаться на объект по его компоненту)   
        m_FireExplodeAudio.clip = m_FireExplodeaudioClip;
        m_FireExplodeParticles.transform.position = new Vector3(-1f, 2f, 2.35f);
        m_FireExplodeParticles.Play();                                    
        m_FireExplodeAudio.Play();                                        
        yield return m_ExplodesWait;                                      
        // второй выстрел
        m_FireExplodeParticles.transform.position = new Vector3(-3f, 2.5f, 2.55f);
        m_FireExplodeParticles.Play();                                    
        m_FireExplodeAudio.Play();                                       
        yield return m_ExplodesWait;                                      
        // третий выстрел
        m_FireExplodeParticles.transform.position = new Vector3(1f, 2.2f, 2.15f);
        m_FireExplodeParticles.Play();                                    
        m_FireExplodeAudio.Play();                                        
    }
    
    private void OnApplicationQuit()
    {
        GameSave.Save();
    }
}

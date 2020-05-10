using System.Collections;                                 // для сопрограмм
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;                                             // Events здесь
#pragma warning disable 0649    // убирает предупреждения компилятора о [SerializeField] private переменных, инициализируемых в редакторе   

public enum WeaponSet : short { SwordShield, SwordSword, TwoHandedSword };                              // варианты сетов оружия у героя
public enum Players : short { Player, Enemy, Nobody };                                                  // варианты победителей раундов и игры
public enum Decision : short { Attack, ChangeSwordShield, ChangeSwordSword, ChangeTwoHandedSword, No }; // варианты действий героя - импульс на 1 такт
public enum ExchangeResult : short { Evade, Parry, BlockVs2Handed, Block, GetHit, No };                 // варианты исхода размена ударами для каждого противника

public class GameManager : MonoBehaviour {

    public static event Action ExchangeEvent1;             // удар1 состоялся
    public static event Action ExchangeEvent2;             // удар2 состоялся
    public static event Action ExchangeEndedEvent;         // сход закончен

    [SerializeField]
    private PlayerManager m_Player;                        // ссылка на менеджер игрока
    [SerializeField]
    private EnemyManager m_Enemy;                          // ссылка на менеджер врага

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
    private Animator m_gameOverAnimator;                    // компонент-аниматор канваса, появляющегося в конце игры
    [SerializeField]
    private AudioSource SFXAudio;                           // аудио-сорс общих звуковых эффектов игры: пока только звук конца игры
    // SFX и VFX победы
    [SerializeField]
    private AudioClip m_audioClip_GameOver;                 // клип конца игры  
    [SerializeField]
    private GameObject m_FireExplodePrefab;                 // ссылка на объект-салют (префаб, состоящий из particle system и звука)
    [SerializeField]
    private AudioClip m_FireExplodeaudioClip1;              // ссылки на клипы салюта
    [SerializeField]
    private AudioClip m_FireExplodeaudioClip2;
    [SerializeField]
    private AudioClip m_FireExplodeaudioClip3;
    private AudioSource m_FireExplodeAudio;                 // ссылки на компоненты конкретного инстанса префаба салюта
    private ParticleSystem m_FireExplodeParticles;
    [SerializeField]
    private float m_ExplodesInterval = 1f;                  // задержка меж выстрелами в секундах
    private WaitForSeconds m_ExplodesWait;                  

    [SerializeField]
    private int stupitidyChangeDelay;                       // задержка на тупизну перед сменой оружия

    private void Start()
    {      
        m_DeathWait = new WaitForSeconds(m_DeathDelay);     // инициализируем задержки: переводим секунды в понятный сопрограмме вид. Затем будем использовать их yield-ом
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);
        m_AttackWait = new WaitForSeconds(m_AttackDelay);
        m_ChangeWait = new WaitForSeconds(m_ChangeDelay);

        StartCoroutine(GameLoop());                         // запускаем сам процесс боя как сопрограмму
                                                            // Почему как сопрограмму? Потому что будем прерывать её директивой yield return
    }

    private IEnumerator GameLoop()                      // основная петля поединка
    {
        // выход из этих yeild-ов происходит по усоловию - выдаче соотв. функциями true
        if (m_roundNumber == 0) yield return StartCoroutine(GameStarting());    // начало игры - обозначить цель
        yield return StartCoroutine(RoundStarting());   // начало раунда: вывод номера раунда и количества побед у бойцов. Стартовая пауза
        yield return StartCoroutine(RoundPlaying());    // сам процесс боя
        yield return StartCoroutine(RoundEnding());     // конец раунда: вывод победителя раунда, количества побед у бойцов и имени победителя. Конечная пауза

        m_gameWinner = GameWinner();
        if (m_gameWinner!=Players.Nobody)                                // победитель игры определен
        {
            m_resultText.text = "GAME OVER! "+ m_gameWinner + " WIN";
            m_gameOverAnimator.SetTrigger("GameOver");                   // запуск анимации конца игры
            SFXAudio.clip = m_audioClip_GameOver;                        // звук конца игры
            SFXAudio.Play();
            if (m_gameWinner == Players.Player) yield return StartCoroutine(Salute());
            yield return m_EndWait;                                      // подождать 3.5 сек
            m_Player.restartButtonObject.SetActive(true);
        }
        else
        {        
            StartCoroutine(GameLoop());                 // рекурсия: текущая GameLoop() завершается и начинается новая 
        }
    }

    private IEnumerator GameStarting()                  // начало игры
    {      
        m_resultText.text = "Defeat " + m_NumRoundsToWin + " knights to win the Tournament!";

        yield return m_StartWait;                        
    }

    private IEnumerator RoundStarting()                 // начало раунда
    {
        //1. Увеличить номер раунда.
        m_roundNumber++;
        //2. Сформировать и вывести информационное сообщение.
        m_resultText.text="ROUND " + m_roundNumber;
        //3. Установить стартовые параметры игроку и врагу: твикеры (с учетом инвентаря), здоровье, начальные позиции и пр. Заблокировать кнопки управления игроку.
        // в самом начале игры инвентарь не сработает
        m_Player.enabled = true;
        m_Enemy.enabled = true;
        //4. Установить задержку на тупизну
        stupitidyChangeDelay = m_NumRoundsToWin - HeroManager.player_countRoundsWon - 1;

        yield return m_StartWait;                       
    }

    private IEnumerator RoundPlaying()
    {
        //1.Очистить информационное сообщение
        m_resultText.text = string.Empty;
        //2.Играем раунд, пока кто-то не умрёт
        while (!OneHeroLeft())                                  // играем раунд (пропускаем такты), пока кто-то не умрет
        {
            //a. Разблокировать кнопки управления игроку        // А как-нибудь через событие??
            m_Player.m_PlayersControlsCanvas.enabled = true;

            //b. Ожидать действие игрока: удара или смены оружия
            if (m_Player.decision == Decision.No)
            {
                MakeEnemyDesition(HeroManager.player_countRoundsWon);
                yield return null;   // Решения еще нет - заканчиваем этот такт (и почему-то не переходим сразу к началу корутины, а проходим её тело до конца...)
                //Debug.Log("Why am I displaying?");
            }
            //c. Рассчитать урон. (Плюс посмотреть, не умер ли кто. Если умер - идем на конец раунда)
            else
            {
                // c0. Тактика для обоих
                m_Player.defencePart = m_Player.tacticSlider.value * m_Player.m_Tweakers.MaxDefencePart + m_Player.m_Tweakers.ParryChance;
                m_Player.parry1 = (UnityEngine.Random.value <= m_Player.defencePart);
                m_Player.parry2 = (UnityEngine.Random.value <= m_Player.defencePart);
                m_Enemy.parry1 = (UnityEngine.Random.value <= m_Enemy.defencePart);
                m_Enemy.parry2 = (UnityEngine.Random.value <= m_Enemy.defencePart);
                // c1. Сперва рассчитаем предварительные коэффициенты  для игрока на основе текущего набора оружия
                switch (m_Player.weaponSet)
                {
                    case WeaponSet.SwordShield:
                        m_Enemy.damage1 = UnityEngine.Random.Range(m_Player.m_Tweakers.DamageBaseMin, m_Player.m_Tweakers.DamageBaseMax + 1);
                        m_Player.series.AddSeriesDamage(ref m_Enemy.damage1);
                        m_Enemy.damage2 = 0f;
                        m_Player.block1 = (UnityEngine.Random.Range(0f, 1f) <= m_Player.m_Tweakers.BlockChance);
                        m_Player.block2 = (UnityEngine.Random.Range(0f, 1f) <= m_Player.m_Tweakers.BlockChance);
                        m_Player.blockVs2Handed = m_Player.block1 && (m_Enemy.decision == Decision.Attack) && (m_Enemy.weaponSet==WeaponSet.TwoHandedSword);
                        break;
                    case WeaponSet.SwordSword:
                        m_Enemy.damage1 = UnityEngine.Random.Range(m_Player.m_Tweakers.DamageBaseMin, m_Player.m_Tweakers.DamageBaseMax + 1);
                        m_Player.series.AddSeriesDamage(ref m_Enemy.damage1);
                        m_Enemy.damage2 = UnityEngine.Random.Range(m_Player.m_Tweakers.DamageBaseMin * m_Player.m_Tweakers.CoefSecondSword, m_Player.m_Tweakers.DamageBaseMax * m_Player.m_Tweakers.CoefSecondSword);
                        m_Player.series.AddSeriesDamage(ref m_Enemy.damage2);
                        m_Player.block1 = false;
                        m_Player.block2 = false;
                        m_Player.blockVs2Handed = false;
                        break;
                    case WeaponSet.TwoHandedSword:
                        m_Enemy.damage1 = UnityEngine.Random.Range(m_Player.m_Tweakers.DamageBaseMin * m_Player.m_Tweakers.Coef2HandedSword, m_Player.m_Tweakers.DamageBaseMax * m_Player.m_Tweakers.Coef2HandedSword);
                        m_Player.series.AddSeriesDamage(ref m_Enemy.damage1);
                        m_Enemy.damage2 = 0f;
                        m_Player.block1 = false;
                        m_Player.block2 = false;
                        m_Player.blockVs2Handed = false;
                        break;
                }
                // c2. А также предварительные коэффициенты  для игрока на основе его решения
                switch (m_Player.decision)
                {
                    case Decision.Attack:
                        m_Player.evade1 = false;
                        m_Player.evade2 = false;
                        break;
                    default: // точно какая-то смена
                        m_Player.evade1 = (UnityEngine.Random.Range(0f, 1f) <= m_Player.m_Tweakers.EvadeOnChangeChance);
                        m_Player.evade2 = (UnityEngine.Random.Range(0f, 1f) <= m_Player.m_Tweakers.EvadeOnChangeChance);
                        m_Player.block1 = false;
                        m_Player.block2 = false;
                        m_Player.blockVs2Handed = false;
                        m_Player.parry1 = false;
                        m_Player.parry2 = false;
                        break;
                }

                // и врага - знаю, что коряво, и нужен цикл
                // c3. Предварительные коэффициенты на основе текущего набора оружия
                switch (m_Enemy.weaponSet)
                {
                    case WeaponSet.SwordShield:
                        m_Player.damage1 = UnityEngine.Random.Range(m_Enemy.m_Tweakers.DamageBaseMin, m_Enemy.m_Tweakers.DamageBaseMax + 1);
                        m_Enemy.series.AddSeriesDamage(ref m_Player.damage1);
                        m_Player.damage2 = 0f;
                        m_Enemy.block1 = (UnityEngine.Random.Range(0f, 1f) <= m_Enemy.m_Tweakers.BlockChance);
                        m_Enemy.block2 = (UnityEngine.Random.Range(0f, 1f) <= m_Enemy.m_Tweakers.BlockChance);
                        m_Enemy.blockVs2Handed = m_Enemy.block1 && (m_Player.decision == Decision.Attack) && (m_Player.weaponSet == WeaponSet.TwoHandedSword);
                        break;
                    case WeaponSet.SwordSword:
                        m_Player.damage1 = UnityEngine.Random.Range(m_Enemy.m_Tweakers.DamageBaseMin, m_Enemy.m_Tweakers.DamageBaseMax + 1);
                        m_Enemy.series.AddSeriesDamage(ref m_Player.damage1);
                        m_Player.damage2 = UnityEngine.Random.Range(m_Enemy.m_Tweakers.DamageBaseMin * m_Enemy.m_Tweakers.CoefSecondSword, m_Enemy.m_Tweakers.DamageBaseMax * m_Enemy.m_Tweakers.CoefSecondSword);
                        m_Enemy.series.AddSeriesDamage(ref m_Player.damage2);
                        m_Enemy.block1 = false;
                        m_Enemy.block2 = false;
                        m_Enemy.blockVs2Handed = false;
                        break;
                    case WeaponSet.TwoHandedSword:
                        m_Player.damage1 = UnityEngine.Random.Range(m_Enemy.m_Tweakers.DamageBaseMin * m_Enemy.m_Tweakers.Coef2HandedSword, m_Enemy.m_Tweakers.DamageBaseMax * m_Enemy.m_Tweakers.Coef2HandedSword);
                        m_Enemy.series.AddSeriesDamage(ref m_Player.damage1);
                        m_Player.damage2 = 0f;
                        m_Enemy.block1 = false;
                        m_Enemy.block2 = false;
                        m_Enemy.blockVs2Handed = false;
                        break;
                }
                // c4. Предварительные коэффициенты на основе решения
                switch (m_Enemy.decision)
                {
                    case Decision.Attack:
                        m_Enemy.evade1 = false;
                        m_Enemy.evade2 = false;
                        break;
                    default: // точно какая-то смена
                        m_Enemy.evade1 = (UnityEngine.Random.Range(0f, 1f) <= m_Enemy.m_Tweakers.EvadeOnChangeChance);
                        m_Enemy.evade2 = (UnityEngine.Random.Range(0f, 1f) <= m_Enemy.m_Tweakers.EvadeOnChangeChance);
                        m_Enemy.block1 = false;
                        m_Enemy.block2 = false;
                        m_Enemy.blockVs2Handed = false;
                        m_Enemy.parry1 = false;
                        m_Enemy.parry2 = false;
                        break;
                }

                //d. Теперь определим, какой нанести урон, на основе предварительных коэффицентов и решения. Удар 1.
                if (m_Player.decision == Decision.Attack)
                {
                    if (m_Enemy.parry1)                                                 // А. парирование
                    {
                        m_Enemy.exchangeResult1 = ExchangeResult.Parry;
                        m_Enemy.series.seriesOfBlocksNum++;                             // Может, вот это должно уйти в SERIES по событию парирования... не, не получится
                    }                                                                   
                    else if (m_Enemy.blockVs2Handed)                                    // Б. пробитие щита двуручником
                    {
                        m_Enemy.damage1 *= m_Player.m_Tweakers.Part2HandedThroughShield;
                        m_Enemy.damage1 = Mathf.Round(m_Enemy.damage1 - m_Enemy.damage1 * m_Player.defencePart);    // уберём часть урона, потраченную на парирование, и округлим
                        m_Enemy.exchangeResult1 = ExchangeResult.BlockVs2Handed;
                    }       
                    else if (m_Enemy.block1)                                            // В. блок
                    {
                        m_Enemy.exchangeResult1 = ExchangeResult.Block;
                        m_Enemy.series.seriesOfBlocksNum++;                                
                    }
                    else if (m_Enemy.evade1)                                            // Г. уворот на смене
                    {
                        m_Enemy.exchangeResult1 = ExchangeResult.Evade;
                    }
                    else
                    {                                                                   // Д. принять полный первый удар
                        m_Enemy.damage1 = Mathf.Round(m_Enemy.damage1 - m_Enemy.damage1 * m_Player.defencePart);    // уберём часть урона, потраченную на парирование, и округлим
                        m_Enemy.exchangeResult1 = ExchangeResult.GetHit;
                    }                   
                }
                if (m_Enemy.decision == Decision.Attack)
                {
                    if (m_Player.parry1)                                                 // А. парирование
                    {
                        m_Player.exchangeResult1 = ExchangeResult.Parry;
                        m_Player.series.seriesOfBlocksNum++;                                       
                    }
                    else if (m_Player.blockVs2Handed)                                    // Б. пробитие щита двуручником
                    {
                        m_Player.damage1 *= m_Enemy.m_Tweakers.Part2HandedThroughShield;
                        m_Player.damage1 = Mathf.Round(m_Player.damage1 - m_Player.damage1 * m_Enemy.defencePart);    // уберём часть урона, потраченную на парирование, и округлим
                        m_Player.exchangeResult1 = ExchangeResult.BlockVs2Handed;
                    }
                    else if (m_Player.block1)                                           // В. блок
                    {
                        m_Player.exchangeResult1 = ExchangeResult.Block;
                        m_Player.series.seriesOfBlocksNum++;                                       
                    }
                    else if (m_Player.evade1)                                           // Г. уворот на смене
                    {
                        m_Player.exchangeResult1 = ExchangeResult.Evade;
                    }
                    else
                    {                                                                   // Д. принять полный первый удар
                        m_Player.damage1 = Mathf.Round(m_Player.damage1 - m_Player.damage1 * m_Enemy.defencePart);    // уберём часть урона, потраченную на парирование, и округлим
                        m_Player.exchangeResult1 = ExchangeResult.GetHit;
                    }                   
                }

                // e. Определяем переменные для рассчета коэффициентов серий (серия блоков внутри предыдушего пункта). Удар 1.
                // е1. коэфф. сильных ударов 
                // игрок
                if (((m_Enemy.exchangeResult1 == ExchangeResult.GetHit) || (m_Enemy.exchangeResult1 == ExchangeResult.BlockVs2Handed))
                    && (m_Enemy.damage1 >= m_Player.series.strongStrikeMin))
                {
                    m_Player.series.strongStrikesNum++;
                    m_Player.series.CheckAndSetStrongStrikesSeries();    // проверить, достигнута ли серия
                }
                // враг
                if (((m_Player.exchangeResult1 == ExchangeResult.GetHit) || (m_Player.exchangeResult1 == ExchangeResult.BlockVs2Handed))
                    && (m_Player.damage1 >= m_Enemy.series.strongStrikeMin))
                {
                    m_Enemy.series.strongStrikesNum++;
                    m_Enemy.series.CheckAndSetStrongStrikesSeries();
                }
                // е2. коэффициент серии ударов
                // игрок
                if ((m_Enemy.exchangeResult1 == ExchangeResult.GetHit) || (m_Enemy.exchangeResult1 == ExchangeResult.BlockVs2Handed))
                {
                    m_Player.series.seriesOfStrikesNum++;
                    m_Player.series.CheckAndSetSeriesOfStrikes();
                }
                // враг
                if ((m_Player.exchangeResult1 == ExchangeResult.GetHit) || (m_Player.exchangeResult1 == ExchangeResult.BlockVs2Handed))
                {
                    m_Enemy.series.seriesOfStrikesNum++;
                    m_Enemy.series.CheckAndSetSeriesOfStrikes();
                }

                // I. Удар1 состоялся  - запустить событие 
                ExchangeEvent1?.Invoke();

                //f. Теперь определим, какой нанести урон, на основе предварительных коэффицентов и решения. Удар 2.
                if ((m_Player.decision == Decision.Attack)&&(m_Enemy.damage2 != 0f))     
                {
                    if (m_Enemy.parry2)
                    {
                        m_Enemy.exchangeResult2 = ExchangeResult.Parry;
                        m_Enemy.series.seriesOfBlocksNum++;
                    }
                    else if (m_Enemy.block2)
                    {
                        m_Enemy.exchangeResult2 = ExchangeResult.Block;
                        m_Enemy.series.seriesOfBlocksNum++;
                    }
                    else if (m_Enemy.evade2)
                    {
                        m_Enemy.exchangeResult2 = ExchangeResult.Evade;
                    }
                    else
                    {
                        m_Enemy.damage2 = Mathf.Round(m_Enemy.damage2 - m_Enemy.damage2 * m_Player.defencePart);    // уберём часть урона, потраченную на парирование, и округлим
                        m_Enemy.exchangeResult2 = ExchangeResult.GetHit;
                    }
                }
                if ((m_Enemy.decision == Decision.Attack)&&(m_Player.damage2 != 0f))     
                {
                    if (m_Player.parry2)
                    {
                        m_Player.exchangeResult2 = ExchangeResult.Parry;
                        m_Player.series.seriesOfBlocksNum++;
                    }
                    else if (m_Player.block2)
                    {
                        m_Player.exchangeResult2 = ExchangeResult.Block;
                        m_Player.series.seriesOfBlocksNum++;
                    }
                    else if (m_Player.evade2)
                    {
                        m_Player.exchangeResult2 = ExchangeResult.Evade;
                    }
                    else
                    {
                        m_Player.damage2 = Mathf.Round(m_Player.damage2 - m_Player.damage2 * m_Enemy.defencePart);    // уберём часть урона, потраченную на парирование, и округлим
                        m_Player.exchangeResult2 = ExchangeResult.GetHit;
                    }
                }

                // h. Определяем переменные для рассчета коэффициентов серий (серия блоков внутри предыдушего пункта)
                // h1. коэфф. сильных ударов 
                // игрок
                if ((m_Enemy.exchangeResult2 == ExchangeResult.GetHit) && (m_Enemy.damage2 >= m_Player.series.strongStrikeMin))
                {
                    m_Player.series.strongStrikesNum++;
                    m_Player.series.CheckAndSetStrongStrikesSeries();    
                }
                // враг
                if ((m_Player.exchangeResult2 == ExchangeResult.GetHit) && (m_Player.damage2 >= m_Enemy.series.strongStrikeMin))
                {
                    m_Enemy.series.strongStrikesNum++;
                    m_Enemy.series.CheckAndSetStrongStrikesSeries();    
                }
                // h2. коэффициент серии ударов
                // игрок
                if ((m_Enemy.exchangeResult1 != ExchangeResult.GetHit)&&(m_Enemy.exchangeResult2 != ExchangeResult.GetHit)&&(m_Enemy.exchangeResult1 != ExchangeResult.BlockVs2Handed))
                {
                    m_Player.series.ResetSeriesOfStrikes();
                    m_Player.series.CheckAndSetSeriesOfStrikes();
                }
                if (m_Enemy.exchangeResult2 == ExchangeResult.GetHit)
                {
                    m_Player.series.seriesOfStrikesNum++;
                    m_Player.series.CheckAndSetSeriesOfStrikes();
                }
                // враг
                if ((m_Player.exchangeResult1 != ExchangeResult.GetHit)&&(m_Player.exchangeResult2 != ExchangeResult.GetHit)&&(m_Player.exchangeResult1 != ExchangeResult.BlockVs2Handed))
                {
                    m_Enemy.series.ResetSeriesOfStrikes();
                    m_Enemy.series.CheckAndSetSeriesOfStrikes();
                }
                if (m_Player.exchangeResult2 == ExchangeResult.GetHit)
                {
                    m_Enemy.series.seriesOfStrikesNum++;
                    m_Enemy.series.CheckAndSetSeriesOfStrikes();
                }

                //i. Смена оружия врага
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

                //j. Блокировка кнопок управления игрока и задержка на анимацию (атаки или смены)
                if (m_Player.decision == Decision.Attack)
                {
                    if (m_Enemy.decision==Decision.Attack) yield return m_AttackWait;                       
                    else yield return m_ChangeWait;                                                         
                }
                else //  точно какая-то смена
                {
                    yield return m_ChangeWait;                                                              
                }

                //k. Уменьшить или обнулить задержку на тупизну
                if ((m_Enemy.decision == Decision.Attack) && (m_Player.decision == Decision.Attack)) stupitidyChangeDelay -= 1;
                else if (m_Enemy.decision != Decision.Attack) stupitidyChangeDelay = m_NumRoundsToWin - HeroManager.player_countRoundsWon - 1;

                // III. Сход закончен  - запустить событие 
                ExchangeEndedEvent?.Invoke();

                yield return null;  // заканчиваем этот такт (и не переходим к концу корутины)
            }
        }
    }

    private IEnumerator RoundEnding()                 // конец раунда
    {
        //1. Всех на переинициализацию.
        m_Player.enabled = false;
        m_Enemy.enabled = false;
        yield return m_DeathWait;                           // используем m_DeathWait ( 2.5 сек), чтоб не плодить сущности
        //2. Вывести информационное сообщение.
        m_resultText.text = "in ROUND " + m_roundNumber + " " + m_roundWinner + " wins";
        //3. Выдать игроку пункт инвентаря за победу в раунде
        if (m_roundWinner == Players.Player)
        {
            yield return m_DeathWait;                       // ждём еще одну m_DeathWait
            int a;
            do a = m_Player.inventory.AddItem(AllItems.Instance.items[UnityEngine.Random.Range(0, AllItems.Instance.items.Length)]);
            while (a == -2);                                //добавить уникальный инвентарь
            if (a != -1)                                    // и чтоб не был полный инвенторий (т.е. мы выиграли 4 раунд, т.е. игру)
            {
                m_resultText.text = "You've got a " + m_Player.inventory.items[a].name;
                m_Player.inventory.ShowItemDescription(a);  // отобразить описание выданного инвентаря
            }
        }
        yield return m_EndWait;                      

        m_Player.m_dead = false;
        m_Enemy.m_dead = false;
    }

    private void MakeEnemyDesition (int nicety)                   // Определиться с действием бота. nicety - уровень интеллекта врага
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
        if (m_Enemy.series.seriesOfStrikesNum > m_Enemy.series.seriesStrikeBeginning)
        {
            m_Enemy.decision = Decision.Attack;
            m_Enemy.defencePart = m_Enemy.m_Tweakers.ParryChance;
            return;
        }
        if (m_Enemy.series.seriesOfBlocksNum > m_Enemy.series.seriesBlockBeginning)
        {
            m_Enemy.decision = Decision.Attack;
            m_Enemy.defencePart = m_Enemy.m_Tweakers.MaxDefencePart + m_Enemy.m_Tweakers.ParryChance;
            return;
        }
        // если нет
        // 2. Если у игрока есть серия - нейтрализуем её (при nicety > 1):
        if (nicety > 1)
        {
            if (m_Player.series.seriesOfStrikesNum > m_Player.series.seriesStrikeBeginning)
            {
                if (m_Enemy.weaponSet != WeaponSet.SwordShield) m_Enemy.decision = Decision.ChangeSwordShield;
                else m_Enemy.decision = Decision.Attack;
                m_Enemy.defencePart = m_Enemy.m_Tweakers.MaxDefencePart + m_Enemy.m_Tweakers.ParryChance;
                return;
            }
            if (m_Player.series.seriesOfBlocksNum > m_Player.series.seriesBlockBeginning)
            {
                if (m_Enemy.weaponSet != WeaponSet.TwoHandedSword) m_Enemy.decision = Decision.ChangeTwoHandedSword;
                else m_Enemy.decision = Decision.Attack;
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
        float tactic = UnityEngine.Random.value;                 // рандомное вещественное [0..1]
        if (tactic < 0.33f)
        {
            m_Enemy.defencePart = m_Enemy.m_Tweakers.MaxDefencePart + m_Enemy.m_Tweakers.ParryChance;
            return;
        }
        m_Enemy.defencePart = m_Enemy.m_Tweakers.ParryChance;
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
        if (HeroManager.enemy_countRoundsWon >= 1/*m_NumRoundsToWin*/) return Players.Enemy;
        if (HeroManager.player_countRoundsWon >= m_NumRoundsToWin) return Players.Player;
        return Players.Nobody;
    }

    private IEnumerator Salute()
    {
        m_ExplodesWait = new WaitForSeconds(m_ExplodesInterval);
        // первый выстрел
        m_FireExplodeParticles = Instantiate(m_FireExplodePrefab).GetComponent<ParticleSystem>();    // порождаем инстанс префаба взрыва и берем компонент этого инстанса
        m_FireExplodeAudio = m_FireExplodeParticles.GetComponent<AudioSource>();                     // берём другой компонент (можно ссылаться на объект по его компоненту)   
        m_FireExplodeAudio.clip = m_FireExplodeaudioClip1;
        m_FireExplodeParticles.transform.position = new Vector3(-1f, 2f, 2.35f);
        m_FireExplodeParticles.Play();                                    // воспроизводим систему частиц
        m_FireExplodeAudio.Play();                                        // воспроизводим аудио
        yield return m_ExplodesWait;                                      // ждём
        // второй выстрел
        m_FireExplodeAudio.clip = m_FireExplodeaudioClip2;
        m_FireExplodeParticles.transform.position = new Vector3(-3f, 2.5f, 2.55f);
        m_FireExplodeParticles.Play();                                    
        m_FireExplodeAudio.Play();                                       
        yield return m_ExplodesWait;                                      
        // третий выстрел
        m_FireExplodeAudio.clip = m_FireExplodeaudioClip3;
        m_FireExplodeParticles.transform.position = new Vector3(1f, 2.2f, 2.15f);
        m_FireExplodeParticles.Play();                                    
        m_FireExplodeAudio.Play();                                        
    }
}

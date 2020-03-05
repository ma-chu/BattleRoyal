using System.Collections;                                 // для сопрограмм
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum WeaponSet : short { SwordShield, SwordSword, TwoHandedSword };                              // варианты сетов оружия у героя
public enum Players : short { Player, Enemy, Nobody};                                                   // варианты победителей раундов и игры
public enum Decision : short { Attack, ChangeSwordShield, ChangeSwordSword, ChangeTwoHandedSword, No};  // варианты действий героя - импульс на 1 такт

public class GameManager : MonoBehaviour {

    public Text m_resultText;                             // текст для вывода "Игра окончена" и прочего
    public Text m_GetHit1PlayerText;                      // тексты для вывода полученного урона
    public Text m_GetHit2PlayerText;
    public Text m_GetHit1EnemyText;
    public Text m_GetHit2EnemyText;

    public GameObject m_PlayersControlsObject;            // объект, содержащий в себе кнопки управления игрока

    public PlayerManager m_Player;                        // ссылка на менеджер игрока
    public EnemyManager m_Enemy;                          // ссылка на менеджер врага

    public int m_NumRoundsToWin = 4;                      // надо выиграть раундов для выигрыша игры
    private int m_roundNumber = 0;                        // текущий номер раунда
    private Players m_roundWinner;                        // победитель раунда
    private Players m_gameWinner;                         // победитель игры

    public float m_StartDelay = 3.5f;                     // стартовая задержка в секундах
    public float m_EndDelay = 5f;                         // конечная задержка в секундах
    public float m_DeathDelay = 2.5f;                     // задержка на смерть в секундах
    public float m_AttackDelay = 3f;                      // задержка на анимацию размена ударами
    public float m_ChangeDelay = 7.5f;                    // задержка на анимацию смены оружия
    private WaitForSeconds m_DeathWait;                   // задержки понятного сопрограмме типа.
    private WaitForSeconds m_StartWait;                   // переведение в него из секунд состоится в ф-ии Start()                 
    private WaitForSeconds m_EndWait;
    private WaitForSeconds m_AttackWait;
    private WaitForSeconds m_ChangeWait;

    public Animator m_gameOverAnimator;                     // компонент-аниматор канваса, появляющегося в конце игры

    // Настройки балланса боёвки - tweakers - глобальные
    public int damageBaseMin = 5;                           // минимальный базовый урон
    public int damageBaseMax = 15;                          // максимальный базовый урон
    public float koef2HandedSword = 1.54f;                  // увеличение урона при двуручнике
    public float koefSecondSword = 0.7f;                    // уменьшение при ударе вторым мечом
    public float blockChance = 0.5f;                        // шанс блока шитом
    public float part2HandedThroughShield = 0.5f;           // доля урона двурой, что проходит сквозь щит
    public float evadeOnChangeChance = 0.33f;               // шанс уворота на смене
    public float maxDefencePart = 0.33f;                    // процент урона, что меняется на шанс парирования при защите

    // Параметры коэффициентов серий:
    // кол-во сильных ударов - +0.5 к урону к strenghtStrikeBeginning и каждому последующему удару на strenghtStrikeMin (+0.5 к четвертому, +1 к пятому ...)
    // серия ударов - +0.5 к урону к seriesStrikeBeginning и каждому последующему удару подряд (при двух мечах должен пройти хоть один) (+0.5 к четвертому, +1 к пятому ...)
    // серия блоков - +1 к здоровью за seriesBlockBeginning блок подряд и далее (+2 к четвертому, +3 к пятому, ... При двух мечах должны быть заблокированы оба удара)
    public float strongStrikeMin = 14;                      // минимальный урон для определения сильных ударов
    public static int strongStrikeSeriesBeginning = 2;      // после какого удара начинаются бонусы за сильные удары    
    public static int seriesStrikeBeginning = 3;            // после какого удара начинаются бонусы за серию ударов    
    public static int seriesBlockBeginning = 3;             // после какого блока начинаются бонусы за серию блоков 

    public Material Material1;                              // Материал для оружия усложнения 1 (на второй раунд)
    public Material Material2;                              // Материал для оружия усложнения 2 (на третий раунд)
    public Material Material3;                              // Материал для оружия усложнения 3 (на 4-ый раунд)
    public Mesh ShieldMesh1;                                // Форма щита 1
    public Mesh SwordMesh1;                                 // Форма меча 1
    public Mesh ShieldMesh2;                                // Форма щита 2
    public Mesh TwoHandedSwordMesh2;                        // Форма двуручного меча 2
    public Mesh ShieldMesh3;                                // Форма щита 3
    public Mesh TwoHandedSwordMesh3;                        // Форма двуручного меча 3

    public Item enemiesItem1;                               // что выдавать врагу на последний раунд

    //public AudioSource BackgroundMusicAudio;                // аудио-сорс фоновой музыки
    public AudioSource SFXAudio;                            // аудио-сорс общих звуковых эффектов игры: начало серии, победа
    // SFX и VFX победы
    public AudioClip m_audioClip_GameOver;                  // клип конца игры  
    public GameObject m_FireExplodePrefab;                  // ссылка на объект-салют (префаб, состоящий из particle system и звука)
    public AudioClip m_FireExplodeaudioClip1;               // ссылки на клипы салюта
    public AudioClip m_FireExplodeaudioClip2;
    public AudioClip m_FireExplodeaudioClip3;
    private AudioSource m_FireExplodeAudio;                 // ссылки на компоненты конкретного инстанса префаба салюта
    private ParticleSystem m_FireExplodeParticles;
    public float m_ExplodesInterval = 1f;                   // задержка меж выстрелами в секундах
    private WaitForSeconds m_ExplodesWait;                  // задержка понятного сопрограмме типа

    [SerializeField]
    private int stupitidyChangeDelay;                       // задержка на тупизну перед сменой оружия

    void Start()
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
            m_resultText.text = "GAME OVER! "+ m_gameWinner + " WON";
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
        m_PlayersControlsObject.SetActive(false);       // Заблокировать кнопки управления игроку.
        m_Player.inventory.CloseItemDescription();      // Cкрыть описание инвентаря (если он был выигран в предыдущем раунде) 
        m_GetHit1EnemyText.text = string.Empty;
        m_GetHit2EnemyText.text = string.Empty;
        m_GetHit1PlayerText.text = string.Empty;
        m_GetHit2PlayerText.text = string.Empty;

        m_resultText.text = "Defeat " + m_NumRoundsToWin + " knights to win the Tournament!";
        yield return m_StartWait;                       // ждём 3 сек.  
    }

    private IEnumerator RoundStarting()                 // начало раунда
    {
        //0.Заблокировать кнопки управления игроку.
        m_PlayersControlsObject.SetActive(false);
        m_Player.inventory.CloseItemDescription();      // скрыть описание инвентаря (если он был выигран в предыдущем раунде) 
        m_Player.decision = Decision.No;
        m_Enemy.decision = Decision.No;
        m_GetHit1EnemyText.text = string.Empty;
        m_GetHit2EnemyText.text = string.Empty;
        m_GetHit1PlayerText.text = string.Empty;
        m_GetHit2PlayerText.text = string.Empty;
        //1. Установить начальные позиции и здоровье игроку и врагу.
        m_Player.enabled = true;
        m_Enemy.enabled = true;
        //2. Увеличить номер раунда.
        m_roundNumber++;
        //3. Сформировать и вывести информационное сообщение.
        m_resultText.text="ROUND " + m_roundNumber;
        //4. инициализируем локальные твикеры игрока и врага
        m_Player.m_damageBaseMin = damageBaseMin;
        m_Player.m_damageBaseMax = damageBaseMax;
        m_Player.m_koef2HandedSword = koef2HandedSword;
        m_Player.m_koefSecondSword = koefSecondSword;
        m_Player.m_blockChance = blockChance;
        m_Player.m_part2HandedThroughShield = part2HandedThroughShield;
        m_Player.m_evadeOnChangeChance = evadeOnChangeChance;
        m_Player.m_maxDefencePart = maxDefencePart;
        m_Enemy.m_damageBaseMin = damageBaseMin;
        m_Enemy.m_damageBaseMax = damageBaseMax;
        m_Enemy.m_koef2HandedSword = koef2HandedSword;
        m_Enemy.m_koefSecondSword = koefSecondSword;
        m_Enemy.m_blockChance = blockChance;
        m_Enemy.m_part2HandedThroughShield = part2HandedThroughShield;
        m_Enemy.m_evadeOnChangeChance = evadeOnChangeChance;
        m_Enemy.m_maxDefencePart = maxDefencePart;      
        //5. Обнулить серийные коэффициенты
        m_Player.strongStrikesNum = 0;
        m_Player.seriesOfStrikesNum = 0;
        m_Player.seriesOfBlocksNum = 0;
        m_Player.hasStrongStrikesSeries = false;
        m_Player.hasSeriesOfStrikes = false;
        m_Player.hasSeriesOfBlockes = false;
        m_Enemy.strongStrikesNum = 0;
        m_Enemy.seriesOfStrikesNum = 0;
        m_Enemy.seriesOfBlocksNum = 0;
        m_Enemy.hasStrongStrikesSeries = false;
        m_Enemy.hasSeriesOfStrikes = false;
        m_Enemy.hasSeriesOfBlockes = false;
        //5а. Обнулить подсказки серий
        m_Player.SetStrongStrikesStarUI();
        m_Player.SetSeriesOfBlocksStarUI();
        m_Player.SetSeriesOfStrikesStarUI();
        m_Enemy.SetStrongStrikesStarUI();
        m_Enemy.SetSeriesOfBlocksStarUI();
        m_Enemy.SetSeriesOfStrikesStarUI();
        //6. Усложнить игру базовым уроном в зависимости от кол-ва выигранных раундов
        m_Enemy.m_damageBaseMin = m_Player.m_countRoundsWon > 0 ? damageBaseMin + 1 : damageBaseMin;                // усложняем с раунда 2
        m_Enemy.m_damageBaseMax = m_Player.m_countRoundsWon > 1 ? damageBaseMax + 1 : damageBaseMax;                // усложняем с раунда 3
        m_Enemy.m_damageBaseMin = m_Player.m_countRoundsWon > 1 ? damageBaseMin + 2 : m_Enemy.m_damageBaseMin;      // усложняем с раунда 3
        m_Enemy.m_damageBaseMax = m_Player.m_countRoundsWon > 2 ? damageBaseMax + 2 : m_Enemy.m_damageBaseMax;      // усложняем с раунда 4
        //6а. Доп изменения: цвет врага, инвентарь врагу
        m_Enemy.inventory.RemoveItem(enemiesItem1);             // убираем инвентарь врага с прошлого раунда (если был, то максимум один)
        
        switch (m_Player.m_countRoundsWon)                      // Меняем цвет врага в зависимости от № раунда - пока только щит...
        {
            case 1:
                m_Enemy.GetComponentInChildren<MeshRenderer>().material.color = Color.red;            // цвет основного материала (щит в основном)
                m_Enemy.m_HeroSword_2.GetComponent<MeshRenderer>().material = Material1;              // цвета материалов мечей
                m_Enemy.m_HeroSword.GetComponent<MeshRenderer>().material = Material1;
                m_Enemy.m_Hero2HandedSword.GetComponent<MeshRenderer>().material = Material1;
                m_Enemy.m_HeroShield.GetComponent<MeshFilter>().sharedMesh = ShieldMesh1;             // вид щита меняем
 // Св-во sharedMesh компонента MeshFilter ссылается на существующий инстанс меша, в отличие от GetComponent<MeshFilter>().mesh, который создает инстанс-дубликат
                m_Enemy.m_HeroSword.GetComponent<MeshFilter>().sharedMesh = SwordMesh1;               // вид меча меняем
                m_Enemy.m_HeroSword_2.GetComponent<MeshFilter>().sharedMesh = SwordMesh1;             // вид меча №2 тоже меняем
                break;
            case 2:
                m_Enemy.GetComponentInChildren<MeshRenderer>().material.color = Color.cyan;
                m_Enemy.m_HeroSword_2.GetComponent<MeshRenderer>().material = Material2;
                m_Enemy.m_HeroSword.GetComponent<MeshRenderer>().material = Material2;
                m_Enemy.m_Hero2HandedSword.GetComponent<MeshRenderer>().material = Material2;
                // если щит еще не перевёрнут
                if (m_Enemy.m_HeroShield.GetComponent<MeshFilter>().sharedMesh == ShieldMesh1) m_Enemy.m_HeroShield.transform.Rotate(Vector3.forward, 180);  // перевернуть щит один раз на модели 2 и 3
                m_Enemy.m_HeroShield.GetComponent<MeshFilter>().sharedMesh = ShieldMesh2;              // вид щита меняем
                m_Enemy.m_Hero2HandedSword.GetComponent<MeshFilter>().sharedMesh = TwoHandedSwordMesh2;// вид двуручного меча меняем
                break;
            case 3:
                m_Enemy.GetComponentInChildren<MeshRenderer>().material.color = Color.yellow;
                m_Enemy.m_HeroSword_2.GetComponent<MeshRenderer>().material = Material3;
                m_Enemy.m_HeroSword.GetComponent<MeshRenderer>().material = Material3;
                m_Enemy.m_Hero2HandedSword.GetComponent<MeshRenderer>().material = Material3;
                m_Enemy.m_HeroShield.GetComponent<MeshFilter>().sharedMesh = ShieldMesh3;              // вид щита опять меняем
                m_Enemy.m_Hero2HandedSword.GetComponent<MeshFilter>().sharedMesh = TwoHandedSwordMesh3;// вид двуручного меча еще раз меняем
                m_Enemy.inventory.AddItem(enemiesItem1);                                               // кольцо даем врагу только на 4 раунде
                break;
        }
        //7. Обновить локальные твикеры с учетом инвентаря
        m_Player.CalculateLocalTweakers();
        m_Enemy.CalculateLocalTweakers();
        //8. Установить задержку на тупизну
        stupitidyChangeDelay = m_NumRoundsToWin - m_Player.m_countRoundsWon - 1;

        yield return m_StartWait;                               // ждём 3 сек.
    }

    private IEnumerator RoundPlaying()
    {
        //1.Очистить информационное сообщение
        m_resultText.text = string.Empty;
        //2.Играем раунд, пока кто-то не умрёт
        while (!OneHeroLeft())                                  // играем раунд (пропускаем такты), пока кто-то не умрет
        {
            //a. Разблокировать кнопки управления игроку, очистить надписи
            m_PlayersControlsObject.SetActive(true);
            m_GetHit1EnemyText.text = string.Empty;
            m_GetHit2EnemyText.text = string.Empty;
            m_GetHit1PlayerText.text = string.Empty;
            m_GetHit2PlayerText.text = string.Empty;

            //b. Ожидать действие игрока: удара или смены оружия
            if (m_Player.decision == Decision.No)
            {
                MakeEnemyDesition(m_Player.m_countRoundsWon);
                yield return null;   // Решения еще нет - заканчиваем этот такт (и почему-то не переходим сразу к началу корутины, а проходим её тело до конца...)
                //Debug.Log("Why am I displaying?");
            }
            //c. Рассчитать урон. (Плюс посмотреть, не умер ли кто. Если умер - идем на конец раунда)
            else
            {
                // c0. Тактика для обоих
                m_Player.defencePart = m_Player.tacticSlider.value * m_Player.m_maxDefencePart;
                m_Player.parry1 = (Random.value <= m_Player.defencePart);
                m_Player.parry2 = (Random.value <= m_Player.defencePart);
                m_Enemy.parry1 = (Random.value <= m_Enemy.defencePart);
                m_Enemy.parry2 = (Random.value <= m_Enemy.defencePart);
                // c1. Сперва рассчитаем предварительные коэффициенты  для игрока на основе текущего набора оружия
                switch (m_Player.weaponSet)
                {
                    case WeaponSet.SwordShield:
                        m_Player.damage1 = Random.Range(m_Player.m_damageBaseMin, m_Player.m_damageBaseMax + 1)
                            + (m_Player.seriesOfStrikesNum > seriesStrikeBeginning ? (m_Player.seriesOfStrikesNum - seriesStrikeBeginning) * 0.5f : 0) + (m_Player.strongStrikesNum > strongStrikeSeriesBeginning ? (m_Player.strongStrikesNum - strongStrikeSeriesBeginning) * 0.5f : 0);    // плюс сер.коэфф.
                        m_Player.damage2 = 0f;
                        m_Player.block1 = (Random.Range(0f, 1f) <= m_Player.m_blockChance);
                        m_Player.block2 = (Random.Range(0f, 1f) <= m_Player.m_blockChance);
                        m_Player.blockVs2Handed = m_Player.block1 && (m_Enemy.decision == Decision.Attack) && (m_Enemy.weaponSet==WeaponSet.TwoHandedSword);
                        break;
                    case WeaponSet.SwordSword:
                        m_Player.damage1 = Random.Range(m_Player.m_damageBaseMin, m_Player.m_damageBaseMax + 1)
                            + (m_Player.seriesOfStrikesNum > seriesStrikeBeginning ? (m_Player.seriesOfStrikesNum - seriesStrikeBeginning) * 0.5f : 0) + (m_Player.strongStrikesNum > strongStrikeSeriesBeginning ? (m_Player.strongStrikesNum - strongStrikeSeriesBeginning) * 0.5f : 0);    // плюс сер.коэфф.
                        m_Player.damage2 = Random.Range(m_Player.m_damageBaseMin * m_Player.m_koefSecondSword, m_Player.m_damageBaseMax * m_Player.m_koefSecondSword)
                            + (m_Player.seriesOfStrikesNum > seriesStrikeBeginning ? (m_Player.seriesOfStrikesNum - seriesStrikeBeginning) * 0.5f : 0) + (m_Player.strongStrikesNum > strongStrikeSeriesBeginning ? (m_Player.strongStrikesNum - strongStrikeSeriesBeginning) * 0.5f : 0);    // плюс сер.коэфф.
                        m_Player.block1 = false;
                        m_Player.block2 = false;
                        m_Player.blockVs2Handed = false;
                        break;
                    case WeaponSet.TwoHandedSword:
                        m_Player.damage1 = Random.Range(m_Player.m_damageBaseMin * m_Player.m_koef2HandedSword, m_Player.m_damageBaseMax * m_Player.m_koef2HandedSword)
                            + (m_Player.seriesOfStrikesNum > seriesStrikeBeginning ? (m_Player.seriesOfStrikesNum - seriesStrikeBeginning) * 0.5f : 0) + (m_Player.strongStrikesNum > strongStrikeSeriesBeginning ? (m_Player.strongStrikesNum - strongStrikeSeriesBeginning) * 0.5f : 0);    // плюс сер.коэфф.
                        m_Player.damage2 = 0f;
                        m_Player.block1 = false;
                        m_Player.block2 = false;
                        m_Player.blockVs2Handed = false;
                        break;
                }
                // c2. А также предварительные коэффициенты  для игрока на основе его решения
                switch (m_Player.decision)
                {
                    case Decision.Attack:
                        m_Player.dodge1 = false;
                        m_Player.dodge2 = false;
                        break;
                    default: // точно какая-то смена
                        m_Player.dodge1 = (Random.Range(0f, 1f) <= m_Player.m_evadeOnChangeChance);
                        m_Player.dodge2 = (Random.Range(0f, 1f) <= m_Player.m_evadeOnChangeChance);
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
                        m_Enemy.damage1 = Random.Range(m_Enemy.m_damageBaseMin, m_Enemy.m_damageBaseMax + 1)
                            + (m_Enemy.seriesOfStrikesNum > seriesStrikeBeginning ? (m_Enemy.seriesOfStrikesNum - seriesStrikeBeginning) * 0.5f : 0) + (m_Enemy.strongStrikesNum > strongStrikeSeriesBeginning ? (m_Enemy.strongStrikesNum - strongStrikeSeriesBeginning) * 0.5f : 0);    // плюс сер.коэфф.
                        m_Enemy.damage2 = 0f;
                        m_Enemy.block1 = (Random.Range(0f, 1f) <= m_Enemy.m_blockChance);
                        m_Enemy.block2 = (Random.Range(0f, 1f) <= m_Enemy.m_blockChance);
                        m_Enemy.blockVs2Handed = m_Enemy.block1 && (m_Player.decision == Decision.Attack) && (m_Player.weaponSet == WeaponSet.TwoHandedSword);
                        break;
                    case WeaponSet.SwordSword:
                        m_Enemy.damage1 = Random.Range(m_Enemy.m_damageBaseMin, m_Enemy.m_damageBaseMax + 1)
                            + (m_Enemy.seriesOfStrikesNum > seriesStrikeBeginning ? (m_Enemy.seriesOfStrikesNum - seriesStrikeBeginning) * 0.5f : 0) + (m_Enemy.strongStrikesNum > strongStrikeSeriesBeginning ? (m_Enemy.strongStrikesNum - strongStrikeSeriesBeginning) * 0.5f : 0);    // плюс сер.коэфф.
                        m_Enemy.damage2 = Random.Range(m_Enemy.m_damageBaseMin * m_Enemy.m_koefSecondSword, m_Enemy.m_damageBaseMax * m_Enemy.m_koefSecondSword)
                            + (m_Enemy.seriesOfStrikesNum > seriesStrikeBeginning ? (m_Enemy.seriesOfStrikesNum - seriesStrikeBeginning) * 0.5f : 0) + (m_Enemy.strongStrikesNum > strongStrikeSeriesBeginning ? (m_Enemy.strongStrikesNum - strongStrikeSeriesBeginning) * 0.5f : 0);    // плюс сер.коэфф.
                        m_Enemy.block1 = false;
                        m_Enemy.block2 = false;
                        m_Enemy.blockVs2Handed = false;
                        break;
                    case WeaponSet.TwoHandedSword:
                        m_Enemy.damage1 = Random.Range(m_Enemy.m_damageBaseMin * m_Enemy.m_koef2HandedSword, m_Enemy.m_damageBaseMax * m_Enemy.m_koef2HandedSword)
                            + (m_Enemy.seriesOfStrikesNum > seriesStrikeBeginning ? (m_Enemy.seriesOfStrikesNum - seriesStrikeBeginning) * 0.5f : 0) + (m_Enemy.strongStrikesNum > strongStrikeSeriesBeginning ? (m_Enemy.strongStrikesNum - strongStrikeSeriesBeginning) * 0.5f : 0);    // плюс сер.коэфф.
                        m_Enemy.damage2 = 0f;
                        m_Enemy.block1 = false;
                        m_Enemy.block2 = false;
                        m_Enemy.blockVs2Handed = false;
                        break;
                }
                // c4. Предварительные коэффициенты на основе решения
                switch (m_Enemy.decision)
                {
                    case Decision.Attack:
                        m_Enemy.dodge1 = false;
                        m_Enemy.dodge2 = false;
                        break;
                    default: // точно какая-то смена
                        m_Enemy.dodge1 = (Random.Range(0f, 1f) <= m_Enemy.m_evadeOnChangeChance);
                        m_Enemy.dodge2 = (Random.Range(0f, 1f) <= m_Enemy.m_evadeOnChangeChance);
                        m_Enemy.block1 = false;
                        m_Enemy.block2 = false;
                        m_Enemy.blockVs2Handed = false;
                        m_Enemy.parry1 = false;
                        m_Enemy.parry2 = false;
                        break;
                }

                //d. Теперь определим, какой нанести урон, на основе предварительных коэффицентов и решения
                // и продублируем его в пояснительный всплывающий текст
                if (m_Player.decision == Decision.Attack)
                {
                    if (m_Enemy.parry1)                                                 // А. парирование
                    {
                        m_GetHit1EnemyText.text = "parried";
                        m_Enemy.m_FirstHeroAudio.clip = m_Enemy.m_audioClipParry;
                        m_Enemy.m_FirstHeroAudio.Play();
                        m_Enemy.seriesOfBlocksNum++;                             // количество блоков в серии
                        if (m_Enemy.seriesOfBlocksNum == seriesBlockBeginning) m_Enemy.SetSeriesOfBlocks();  // сыграть звук достижения серии и выставить меркер
                        if (m_Enemy.seriesOfBlocksNum > seriesBlockBeginning)   // начислить здоровья за серию блоков
                        {
                            m_Enemy.RegenHealth(m_Enemy.seriesOfBlocksNum - seriesBlockBeginning);
                            m_GetHit1EnemyText.text = m_GetHit1EnemyText.text + " +" + (m_Enemy.seriesOfBlocksNum - seriesBlockBeginning).ToString();
                        }
                    }
                    else if (m_Enemy.blockVs2Handed)                                    // Б. пробитие щита двуручником
                    {
                        m_Player.damage1 = ((Random.Range(m_Player.m_damageBaseMin * m_Player.m_koef2HandedSword, m_Player.m_damageBaseMax * m_Player.m_koef2HandedSword)
                            + (m_Player.seriesOfStrikesNum > seriesStrikeBeginning ? (m_Player.seriesOfStrikesNum - seriesStrikeBeginning) * 0.5f : 0) + (m_Player.strongStrikesNum > strongStrikeSeriesBeginning ? (m_Player.strongStrikesNum - strongStrikeSeriesBeginning) * 0.5f : 0)) * part2HandedThroughShield
                            );                             // плюс сер.коэфф.
                        m_Player.damage1 = Mathf.Round(m_Player.damage1 - m_Player.damage1 * m_Player.defencePart);    // уберём часть урона, потраченную на парирование, и округлим
                        m_Enemy.m_getHit1 = true;
                        m_GetHit1EnemyText.text = "shield: -" + m_Player.damage1.ToString();
                        m_Enemy.m_FirstHeroAudio.clip = m_Enemy.m_audioClipThrough;
                        m_Enemy.m_FirstHeroAudio.Play();
                        m_Enemy.ResetSeriesOfBlocks();
                    }       
                    else if (m_Enemy.block1)                                            // В. блок
                    {
                        m_GetHit1EnemyText.text = "blocked";
                        m_Enemy.m_FirstHeroAudio.clip = m_Enemy.m_audioClipBlock;
                        m_Enemy.m_FirstHeroAudio.Play();
                        m_Enemy.seriesOfBlocksNum++;                            // количество блоков в серии
                        if (m_Enemy.seriesOfBlocksNum == seriesBlockBeginning) m_Enemy.SetSeriesOfBlocks();  // сыграть звук достижения серии и выставить меркер
                        if (m_Enemy.seriesOfBlocksNum > seriesBlockBeginning)   // начислить здоровья за серию блоков
                        {
                            m_Enemy.RegenHealth(m_Enemy.seriesOfBlocksNum - seriesBlockBeginning);
                            m_GetHit1EnemyText.text = m_GetHit1EnemyText.text + " +" + (m_Enemy.seriesOfBlocksNum - seriesBlockBeginning).ToString();
                        }
                    }
                    else if (m_Enemy.dodge1)                                            // Г. уворот на смене
                    {
                        m_GetHit1EnemyText.text = "evaded";
                        m_Enemy.m_FirstHeroAudio.clip = m_Enemy.m_audioClipEvade;
                        m_Enemy.m_FirstHeroAudio.Play();
                    }
                    else
                    {                                                                   // Д. принять полный первый удар
                        m_Enemy.m_getHit1 = true;
                        m_Player.damage1 = Mathf.Round(m_Player.damage1 - m_Player.damage1 * m_Player.defencePart);    // уберём часть урона, потраченную на парирование, и округлим
                        m_GetHit1EnemyText.text = "-" + m_Player.damage1.ToString();
                        m_Enemy.ResetSeriesOfBlocks();                              
                    }
                    if (m_Player.damage2 != 0f)                                         // пп. А-Д для удара вторым мечом
                    {
                        if (m_Enemy.parry2)
                        {
                            m_GetHit2EnemyText.text = "parried";
                            m_Enemy.m_SecondHeroAudio.clip = m_Enemy.m_audioClipParry;
                            m_Enemy.m_SecondHeroAudio.PlayDelayed(0.3f);
                            m_Enemy.seriesOfBlocksNum++;                            // количество блоков в серии
                            if (m_Enemy.seriesOfBlocksNum == seriesBlockBeginning) m_Enemy.SetSeriesOfBlocks();  // сыграть звук достижения серии и выставить меркер
                            if (m_Enemy.seriesOfBlocksNum > seriesBlockBeginning)   // начислить здоровья за серию блоков
                            {
                                m_Enemy.RegenHealth(m_Enemy.seriesOfBlocksNum - seriesBlockBeginning);
                                m_GetHit2EnemyText.text = m_GetHit2EnemyText.text + " +" + (m_Enemy.seriesOfBlocksNum - seriesBlockBeginning).ToString();
                            }
                        }
                        else if (m_Enemy.block2)
                        {
                            m_GetHit2EnemyText.text = "blocked";
                            m_Enemy.m_SecondHeroAudio.clip = m_Enemy.m_audioClipBlock;
                            m_Enemy.m_SecondHeroAudio.PlayDelayed(0.3f);
                            m_Enemy.seriesOfBlocksNum++;                            // количество блоков в серии
                            if (m_Enemy.seriesOfBlocksNum == seriesBlockBeginning) m_Enemy.SetSeriesOfBlocks();  // сыграть звук достижения серии и выставить меркер
                            if (m_Enemy.seriesOfBlocksNum > seriesBlockBeginning)   // начислить здоровья за серию блоков
                            {
                                m_Enemy.RegenHealth(m_Enemy.seriesOfBlocksNum - seriesBlockBeginning);
                                m_GetHit2EnemyText.text = m_GetHit2EnemyText.text + " +" + (m_Enemy.seriesOfBlocksNum - seriesBlockBeginning).ToString();
                            }
                        }
                        else if (m_Enemy.dodge2)
                        {
                            m_GetHit2EnemyText.text = "evaded";
                            m_Enemy.m_SecondHeroAudio.clip = m_Enemy.m_audioClipEvade;
                            m_Enemy.m_SecondHeroAudio.PlayDelayed(0.3f);
                        }
                        else
                        {
                            m_Enemy.m_getHit2 = true;
                            m_Player.damage2 = Mathf.Round(m_Player.damage2 - m_Player.damage2 * m_Player.defencePart);    // уберём часть урона, потраченную на парирование, и округлим
                            m_GetHit2EnemyText.text = "-" + m_Player.damage2.ToString();
                            m_Enemy.ResetSeriesOfBlocks();
                        }
                    }
                }
                if (m_Enemy.decision == Decision.Attack)
                {
                    if (m_Player.parry1)                                                 // А. парирование
                    {
                        m_GetHit1PlayerText.text = "parried";
                        m_Player.m_FirstHeroAudio.clip = m_Enemy.m_audioClipParry;
                        m_Player.m_FirstHeroAudio.Play();
                        m_Player.seriesOfBlocksNum++;                           // количество блоков в серии
                        if (m_Player.seriesOfBlocksNum == seriesBlockBeginning) m_Player.SetSeriesOfBlocks();  // сыграть звук достижения серии и выставить меркер
                        if (m_Player.seriesOfBlocksNum > seriesBlockBeginning)  // начислить здоровья за серию блоков
                        {
                            m_Player.RegenHealth(m_Player.seriesOfBlocksNum - seriesBlockBeginning);
                            m_GetHit1PlayerText.text = m_GetHit1PlayerText.text + " +" + (m_Player.seriesOfBlocksNum - seriesBlockBeginning).ToString();
                        }
                    }
                    else if (m_Player.blockVs2Handed)                                    // Б. пробитие щита двуручником
                    {
                        m_Enemy.damage1 = ((Random.Range(m_Enemy.m_damageBaseMin * m_Enemy.m_koef2HandedSword, m_Enemy.m_damageBaseMax * m_Enemy.m_koef2HandedSword)
                          + (m_Enemy.seriesOfStrikesNum > seriesStrikeBeginning ? (m_Enemy.seriesOfStrikesNum - seriesStrikeBeginning) * 0.5f : 0) + (m_Enemy.strongStrikesNum > strongStrikeSeriesBeginning ? (m_Enemy.strongStrikesNum - strongStrikeSeriesBeginning) * 0.5f : 0)) * part2HandedThroughShield
                          );                               // плюс сер.коэфф.
                        m_Enemy.damage1 = Mathf.Round(m_Enemy.damage1 - m_Enemy.damage1 * m_Enemy.defencePart);    // уберём часть урона, потраченную на парирование, и округлим
                        m_Player.m_getHit1 = true;
                        m_GetHit1PlayerText.text = "shield: -" + m_Enemy.damage1.ToString();
                        m_Player.m_FirstHeroAudio.clip = m_Enemy.m_audioClipThrough;
                        m_Player.m_FirstHeroAudio.Play();
                        m_Player.ResetSeriesOfBlocks();
                    }
                    else if (m_Player.block1)                                           // В. блок
                    {
                        m_GetHit1PlayerText.text = "blocked";
                        m_Player.m_FirstHeroAudio.clip = m_Enemy.m_audioClipBlock;
                        m_Player.m_FirstHeroAudio.Play();
                        m_Player.seriesOfBlocksNum++;                           // количество блоков в серии
                        if (m_Player.seriesOfBlocksNum == seriesBlockBeginning) m_Player.SetSeriesOfBlocks();  // сыграть звук достижения серии и выставить меркер
                        if (m_Player.seriesOfBlocksNum > seriesBlockBeginning)  // начислить здоровья за серию блоков
                        {
                            m_Player.RegenHealth(m_Player.seriesOfBlocksNum - seriesBlockBeginning);
                            m_GetHit1PlayerText.text = m_GetHit1PlayerText.text + " +" + (m_Player.seriesOfBlocksNum - seriesBlockBeginning).ToString();
                        }
                    }
                    else if (m_Player.dodge1)
                    {
                        m_GetHit1PlayerText.text = "evaded";                            // Г. уворот на смене
                        m_Player.m_FirstHeroAudio.clip = m_Player.m_audioClipEvade;
                        m_Player.m_FirstHeroAudio.Play();
                    }
                    else
                    {                                                                   // Д. принять полный первый удар
                        m_Player.m_getHit1 = true;
                        m_Enemy.damage1 = Mathf.Round(m_Enemy.damage1 - m_Enemy.damage1 * m_Enemy.defencePart);    // уберём часть урона, потраченную на парирование, и округлим
                        m_GetHit1PlayerText.text = "-" + m_Enemy.damage1.ToString();
                        m_Player.ResetSeriesOfBlocks();
                    }
                    if (m_Enemy.damage2 != 0f)                                          // пп. А-Д для удара вторым мечом
                    {
                        if (m_Player.parry2)
                        {
                            m_GetHit2PlayerText.text = "parried";
                            m_Player.m_SecondHeroAudio.clip = m_Enemy.m_audioClipParry;
                            m_Player.m_SecondHeroAudio.PlayDelayed(0.3f);
                            m_Player.seriesOfBlocksNum++;                           // количество блоков в серии
                            if (m_Player.seriesOfBlocksNum == seriesBlockBeginning) m_Player.SetSeriesOfBlocks();  // сыграть звук достижения серии и выставить меркер
                            if (m_Player.seriesOfBlocksNum > seriesBlockBeginning)  // начислить здоровья за серию блоков
                            {
                                m_Player.RegenHealth(m_Player.seriesOfBlocksNum - seriesBlockBeginning);
                                m_GetHit2PlayerText.text = m_GetHit2PlayerText.text + " +" + (m_Player.seriesOfBlocksNum - seriesBlockBeginning).ToString();
                            }
                        }
                        else if (m_Player.block2)
                        {
                            m_GetHit2PlayerText.text = "blocked";
                            m_Player.m_SecondHeroAudio.clip = m_Enemy.m_audioClipBlock;
                            m_Player.m_SecondHeroAudio.PlayDelayed(0.3f);
                            m_Player.seriesOfBlocksNum++;                           // количество блоков в серии
                            if (m_Player.seriesOfBlocksNum == seriesBlockBeginning) m_Player.SetSeriesOfBlocks();  // сыграть звук достижения серии и выставить меркер
                            if (m_Player.seriesOfBlocksNum > seriesBlockBeginning)  // начислить здоровья за серию блоков
                            {
                                m_Player.RegenHealth(m_Player.seriesOfBlocksNum - seriesBlockBeginning);
                                m_GetHit2PlayerText.text = m_GetHit2PlayerText.text + " +" + (m_Player.seriesOfBlocksNum - seriesBlockBeginning).ToString();
                            }
                        }
                        else if (m_Player.dodge2)
                        {
                            m_GetHit2PlayerText.text = "evaded";
                            m_Player.m_SecondHeroAudio.clip = m_Player.m_audioClipEvade;
                            m_Player.m_SecondHeroAudio.PlayDelayed(0.3f);
                        }
                        else
                        {
                            m_Player.m_getHit2 = true;
                            m_Enemy.damage2 = Mathf.Round(m_Enemy.damage2 - m_Enemy.damage2 * m_Enemy.defencePart);    // уберём часть урона, потраченную на парирование, и округлим
                            m_GetHit2PlayerText.text = "-" + m_Enemy.damage2.ToString();
                            m_Player.ResetSeriesOfBlocks();
                        }
                    }
                }

                // e. Определяем переменные для рассчета коэффициентов серий (серия блоков в предыдушем пункте)
                // е1. коэфф. сильных ударов игрок
                if (m_Enemy.m_getHit1 && (m_Player.damage1 >= strongStrikeMin)) m_Player.strongStrikesNum++;
                if (!m_Player.hasStrongStrikesSeries && (m_Player.strongStrikesNum == strongStrikeSeriesBeginning))
                    m_Player.SetStrongStrikesSeries();                      // серия достигнута: сыграть звук достижения серии и выставить меркер
                    // коэфф. сильных ударов враг
                if (m_Player.m_getHit1 && (m_Enemy.damage1 >= strongStrikeMin)) m_Enemy.strongStrikesNum++;
                if (!m_Enemy.hasStrongStrikesSeries && (m_Enemy.strongStrikesNum == strongStrikeSeriesBeginning))
                    m_Enemy.SetStrongStrikesSeries();                      // серия достигнута: сыграть звук достижения серии и выставить меркер

                // е2. коэффициент серии ударов игрок
                if (!m_Enemy.m_getHit1 && !m_Enemy.m_getHit2) m_Player.ResetSeriesOfStrikes();
                if (m_Enemy.m_getHit1) m_Player.seriesOfStrikesNum++;
                if (!m_Player.hasSeriesOfStrikes && (m_Player.seriesOfStrikesNum == seriesStrikeBeginning))
                    m_Player.SetSeriesOfStrikes();                         // серия достигнута: сыграть звук достижения серии и выставить меркер
                if (m_Enemy.m_getHit2) m_Player.seriesOfStrikesNum++;
                if (!m_Player.hasSeriesOfStrikes && (m_Player.seriesOfStrikesNum == seriesStrikeBeginning))
                    m_Player.SetSeriesOfStrikes();                         // серия достигнута: сыграть звук достижения серии и выставить меркер
                    // коэффициент серии ударов враг
                if (!m_Player.m_getHit1 && !m_Player.m_getHit2) m_Enemy.ResetSeriesOfStrikes();
                if (m_Player.m_getHit1) m_Enemy.seriesOfStrikesNum++;
                if (!m_Enemy.hasSeriesOfStrikes && (m_Enemy.seriesOfStrikesNum == seriesStrikeBeginning))
                    m_Enemy.SetSeriesOfStrikes();                         // серия достигнута: сыграть звук достижения серии и выставить меркер
                if (m_Player.m_getHit2) m_Enemy.seriesOfStrikesNum++;
                if (!m_Enemy.hasSeriesOfStrikes && (m_Enemy.seriesOfStrikesNum == seriesStrikeBeginning))
                    m_Enemy.SetSeriesOfStrikes();                         // серия достигнута: сыграть звук достижения серии и выставить меркер

                // Анимируем серийные подсказки
                m_Player.SetStrongStrikesStarUI();
                m_Player.SetSeriesOfBlocksStarUI();
                m_Player.SetSeriesOfStrikesStarUI();
                m_Enemy.SetStrongStrikesStarUI();
                m_Enemy.SetSeriesOfBlocksStarUI();
                m_Enemy.SetSeriesOfStrikesStarUI();

                //f1. Анимация врага
                if (m_Enemy.decision == Decision.Attack) m_Enemy.m_attack = true;
                else
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
                    m_Enemy.m_change = true;
                }
                //f2. Блокировка кнопок управления игрока, анимация и задержка на неё 
                if (m_Player.decision == Decision.Attack)
                {
                    m_PlayersControlsObject.SetActive(false);
                    m_Player.m_attack = true;
                    if (m_Enemy.decision==Decision.Attack) yield return m_AttackWait;                       // ждём 2.5 сек
                    else yield return m_ChangeWait;                                                         // ждём 7.5 сек, точно какая-то смена
                }
                else //  точно какая-то смена
                {
                    m_PlayersControlsObject.SetActive(false);
                    m_Player.m_change = true;
                    yield return m_ChangeWait;                       // ждём 7.5 сек
                }

                // g. Здесь уменьшить или обнулить задержку на тупизну
                if ((m_Enemy.decision == Decision.Attack)&& (m_Player.decision == Decision.Attack)) stupitidyChangeDelay -= 1;
                else if (m_Enemy.decision != Decision.Attack) stupitidyChangeDelay = m_NumRoundsToWin - m_Player.m_countRoundsWon - 1;

                m_Player.decision = Decision.No;
                m_Enemy.decision = Decision.No;

                yield return null;  // заканчиваем этот такт (и не переходим к концу корутины)
            }
        }
    }

    private IEnumerator RoundEnding()                 // конец раунда
    {
        //1. Заблокировать кнопки управления игроку.
        m_PlayersControlsObject.SetActive(false);
        //2. Всех на переинициализацию.
        m_Player.enabled = false;
        m_Enemy.enabled = false;
        yield return m_DeathWait;                      // ждём 2.5 сек
        //3. Вывести информационное сообщение.
        m_resultText.text = "in ROUND " + m_roundNumber + " " + m_roundWinner + " wins";
        //4. Выдать игроку пункт инвентаря за победу в раунде
        if (m_roundWinner == Players.Player)
        {
            yield return m_DeathWait;                  // ждём еще 2.5 сек
            int a;
            do a = m_Player.inventory.AddItem(AllItems.Instance.items[Random.Range(0, AllItems.Instance.items.Length)]); //добавить уникальный инвентарь
            while (a == -2);
            if (a != -1)                               // чтоб не был полный инвенторий (т.е. мы выиграли 4 раунд, т.е. игру)
            {
                m_resultText.text = "You've got a " + m_Player.inventory.items[a].name;
                m_Player.inventory.ShowItemDescription(a);              // отобразить описание выданного инвентаря
            }
        }
        yield return m_EndWait;                        // ждём 3 сек
    }

    private void MakeEnemyDesition (int nicety)                   // Определиться с действием бота. nicety - уровень интеллекта врага
    {
        /* nicety = 0: 
         * 1. на свои серии реагирует, 
         * 2. на чужие - нет,
         * 3. относительно оружия - тупит 3 удара, затем меняет рандомно
         * 
         * nicety = 1: 
         * 1. на свои серии реагирует, 
         * 2. на чужие - нет,
         * 3. относительно оружия - тупит 2 удара, затем меняет правильно
         * 
         * nicety = 2: 
         * 1. на свои серии реагирует, 
         * 2. на чужие тоже,
         * 3. относительно оружия - тупит 1 удар, затем меняет правильно
         * 
         * nicety = 3: 
         * 1. на свои серии реагирует, 
         * 2. на чужие тоже,
         * 3. относительно оружия - сразу меняет правильно
        */

        // 1. Если у врага есть серия - продолжаем её (при любом nicety):
        if (m_Enemy.seriesOfStrikesNum > seriesStrikeBeginning)
        {
            m_Enemy.decision = Decision.Attack;
            m_Enemy.defencePart = 0.0f;
            return;
        }
        if (m_Enemy.seriesOfBlocksNum > seriesBlockBeginning)
        {
            m_Enemy.decision = Decision.Attack;
            m_Enemy.defencePart = m_Enemy.m_maxDefencePart;
            return;
        }

        // если нет
        // 2. Если у игрока есть серия - нейтрализуем её (при nicety > 1):
        if (nicety > 1)
        {
            if (m_Player.seriesOfStrikesNum > seriesStrikeBeginning)
            {
                if (m_Enemy.weaponSet != WeaponSet.SwordShield) m_Enemy.decision = Decision.ChangeSwordShield;
                else m_Enemy.decision = Decision.Attack;
                m_Enemy.defencePart = m_Enemy.m_maxDefencePart;
                return;
            }
            if (m_Player.seriesOfBlocksNum > seriesBlockBeginning)
            {
                if (m_Enemy.weaponSet != WeaponSet.TwoHandedSword) m_Enemy.decision = Decision.ChangeTwoHandedSword;
                else m_Enemy.decision = Decision.Attack;
                m_Enemy.defencePart = 0.0f;
                return;
            }
        }

        // если нет
        // 3. Варианты относительно типов оружия (с задержкой на тупизну):
        // 1 - полный рандом; 2 - оптимум - то, что сейчас; 3 - идеальное - надо рассчитывать еще необходимость смены в зависимости от оставшегося здоровья
        if (stupitidyChangeDelay > 0) m_Enemy.decision = Decision.Attack;
        else if ((m_Player.weaponSet == WeaponSet.SwordShield) && (m_Enemy.weaponSet == WeaponSet.SwordSword))
            m_Enemy.decision = Decision.ChangeTwoHandedSword;
        else if ((m_Player.weaponSet == WeaponSet.SwordSword) && (m_Enemy.weaponSet == WeaponSet.TwoHandedSword))
            m_Enemy.decision = Decision.ChangeSwordShield;
        else if ((m_Player.weaponSet == WeaponSet.TwoHandedSword) && (m_Enemy.weaponSet == WeaponSet.SwordShield))
            m_Enemy.decision = Decision.ChangeSwordSword;
        else m_Enemy.decision = Decision.Attack;
        // еще можно использовать комбинацию вариантов. Например, оптимум с добавлением небольшого шанса на рандом

        // 4. Тактику при этом пока будем выбирать по рандому:
        float a = Random.value;                 // рандомное вещественное [0..1]
        if (a < 0.33f) { m_Enemy.defencePart = m_Enemy.m_maxDefencePart; return; }
        /*if (a < 0.2f) { m_Enemy.defencePart = 0; return; }
        if (a < 0.4f) { m_Enemy.defencePart = 0.125f; return; }
        if (a < 0.6f) { m_Enemy.defencePart = 0.25f; return; }
        if (a < 0.8f) { m_Enemy.defencePart = 0.375f; return; }*/
        m_Enemy.defencePart = 0.0f;
    }

    private bool OneHeroLeft()                   // кто-то умер
    {
        if (m_Player.m_dead)
        {
            if (m_Enemy.m_dead)                  // ничья
            {
                m_roundWinner = Players.Nobody;
                return true;
            }
            m_Enemy.m_countRoundsWon++;          // врагу +1 раунд
            m_roundWinner = Players.Enemy;          
            return true;
        }
        if (m_Enemy.m_dead)
        {
            m_Player.m_countRoundsWon++;         // мне +1 раунд
            m_roundWinner = Players.Player;
            return true;
        }
        return false;
    }

    private Players GameWinner()
    {
        if (m_Enemy.m_countRoundsWon >= 1/*m_NumRoundsToWin*/) return Players.Enemy;
        if (m_Player.m_countRoundsWon >= m_NumRoundsToWin) return Players.Player;
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
        m_FireExplodeParticles = Instantiate(m_FireExplodePrefab).GetComponent<ParticleSystem>();    // порождаем инстанс префаба взрыва и берем компонент этого инстанса
        m_FireExplodeAudio = m_FireExplodeParticles.GetComponent<AudioSource>();                     // берём другой компонент (можно ссылаться на объект по его компоненту)   
        m_FireExplodeAudio.clip = m_FireExplodeaudioClip2;
        m_FireExplodeParticles.transform.position = new Vector3(-3f, 2.5f, 2.55f);
        m_FireExplodeParticles.Play();                                    // воспроизводим систему частиц
        m_FireExplodeAudio.Play();                                        // воспроизводим аудио
        yield return m_ExplodesWait;                                      // ждём

        // третий выстрел
        m_FireExplodeParticles = Instantiate(m_FireExplodePrefab).GetComponent<ParticleSystem>();    // порождаем инстанс префаба взрыва и берем компонент этого инстанса
        m_FireExplodeAudio = m_FireExplodeParticles.GetComponent<AudioSource>();                     // берём другой компонент (можно ссылаться на объект по его компоненту)   
        m_FireExplodeAudio.clip = m_FireExplodeaudioClip3;
        m_FireExplodeParticles.transform.position = new Vector3(1f, 2.2f, 2.15f);
        m_FireExplodeParticles.Play();                                    // воспроизводим систему частиц
        m_FireExplodeAudio.Play();                                        // воспроизводим аудио
    }

    private void Update()               // Напоминаю - всё это выполняется каждый такт! Сюда валим всё, что должно выполняться независимо от фазы игры - нажатия клавиш и пр.
    {
        // Игрок

        if (m_Player.m_attack) m_Player.Attack();
        
        if (m_Player.m_getHit1 == true)                         // Анимация урона игроку (и его нанесение)
        {
            m_Player.TakeDamage(m_Enemy.damage1);
            m_Player.m_getHit1 = false;
        }
        if (m_Player.m_getHit2 == true)
        {
            m_Player.TakeDamage(m_Enemy.damage2);
            m_Player.m_getHit2 = false;
        }

        if ((m_Player.m_change)&&(!m_Player.m_dead)) m_Player.ChangeWeapon();

        if (m_Player.m_rotateToCenter) m_Player.RotateToCenter();

        if (m_Player.m_toPosition) m_Player.ToPosition();

        // Враг

        if (m_Enemy.m_attack) m_Enemy.Attack();

        if (m_Enemy.m_getHit1 == true)                          // Анимация урона врага (и его нанесение)
        {
            m_Enemy.TakeDamage(m_Player.damage1);
            m_Enemy.m_getHit1 = false;
        }
        if (m_Enemy.m_getHit2 == true)
        {
            m_Enemy.TakeDamage(m_Player.damage2);
            m_Enemy.m_getHit2 = false;
        }

        if ((m_Enemy.m_change)&&(!m_Enemy.m_dead)) m_Enemy.ChangeWeapon();

        if (m_Enemy.m_rotateToCenter) m_Enemy.RotateToCenter();

        if (m_Enemy.m_toPosition) m_Enemy.ToPosition();
    }

}

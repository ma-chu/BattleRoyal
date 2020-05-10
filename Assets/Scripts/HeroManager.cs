using UnityEngine;
using UnityEngine.UI;


public class HeroManager : MonoBehaviour
{
    [HideInInspector]
    public int m_countRoundsWon = 0;                    // Сколько раундов выиграл

    // СОСТОЯНИЯ
    public bool m_dead;                                 // герой мёртв                                                 
    public bool m_attack;
    public bool m_getHit1;                              // принять удар1
    public bool m_getHit2;                              // принять удар2
    public bool m_change;
    public bool m_toPosition;                           // Выход на ристалище - в начале боя и после смены оружия
    public bool m_rotateToCenter;                       // Разворот после смены оружия

    // ЗНАЧЕНИЯ ДЛЯ РАССЧЕТА УРОНА
    public float damage1;                               // Урон1 на этот ход
    public float damage2;                               // Урон2 на этот ход
    public bool dodge1;                                 // Уворот от первого удара на смене на этот ход    
    public bool dodge2;                                 // Уворот от второго удара на смене на этот ход    
    public bool block1;                                 // Блок первого удара на этот ход    
    public bool block2;                                 // Блок второго удара на этот ход
    public bool blockVs2Handed;                         // Блок удара двуручником - половина урона
    public float defencePart;                           // Тактика боя - ориентированность на защиту: от 0 до 33% урона меняется на возможность парирования (шаги 0%, 33%)
    public bool parry1;                                 // Парирование первого удара на этот ход    
    public bool parry2;                                 // Парирование второго удара на этот ход

    // Настройки балланса боёвки - tweakers - частные
    public int m_damageBaseMin;                         // минимальный базовый урон
    public int m_damageBaseMax;                         // максимальный базовый урон
    public float m_koef2HandedSword;                    // увеличение урона при двуручнике
    public float m_koefSecondSword;                     // уменьшение при ударе вторым мечом
    public float m_blockChance;                         // шанс блока шитом
    public float m_part2HandedThroughShield;            // доля урона двурой, что проходит сквозь щит
    public float m_evadeOnChangeChance;                 // шанс уворота на смене
    public float m_maxDefencePart;                      // процент урона, что меняется на шанс парирования при защите


    // ПЕРЕМЕННЫЕ ДЛЯ РАССЧЁТИА КОЭФФИЦИЕНТОВ СЕРИЙ:
    // Бонус за кол-во сильных ударов: +0.5 к урону за каждый сильный удар (strenghtStrikeMin) после strenghtStrikeBeginning (+0.5 к четвертому, +1 к пятому ...)
    public int strongStrikesNum;                        // Количество сильных ударов общее
    [HideInInspector]
    public bool hasStrongStrikesSeries = false;         // уже набрано
    // Бонус за серию ударов: +0.5 к урону за каждый удар подряд после seriesStrikeBeginning /при двух мечах должен пройти хоть один/ (+0.5 к четвертому, +1 к пятому ...)
    public int seriesOfStrikesNum;                      // Количество ударов в серии
    [HideInInspector]
    public bool hasSeriesOfStrikes = false;             // серия ударов набрана
    // Бонус за серию блоков: +1 к здоровью за каждый блок подряд после seriesBlockBeginning /при двух мечах должны быть заблокированы оба удара/ (+1 к четвертому, +2 к пятому ...)
    public int seriesOfBlocksNum;                       // Количество блоков в серии
    [HideInInspector]
    public bool hasSeriesOfBlockes = false;             // серия блоков набрана

    public float zeroZposition;                         // координата Z позиции на ристалище 
    public float zeroYrotation;                         // вращение позиции на ристалище
    public float stockXposition;                        // координата Z позиции склада оружия

    public WeaponSet weaponSet = WeaponSet.SwordShield; // Какой набор оружия использовать
    public Decision decision;                           // Решение на этот ход

    public float m_StartingHealth = 100f;               // начальное количество здоровья 
    public Slider m_Slider;                             // ссылка на слайдер здоровья                
    public Image m_FillImage;                           // будем контролировать перемещение именно fillImage  (относительно фона)            
    public Color m_FullHealthColor = Color.green;       // цвет для 100% здоровья
    public Color m_ZeroHealthColor = Color.red;         // цвет для 0% здоровья
    private float m_CurrentHealth;                      // текущее количество здоровья   

    public AudioSource SFXAudio;                        // аудио-сорс общих звуковых эффектов игры: начало серии, победа
    // Подсказка (звезда) сильных ударов
    public Slider m_StrengthStrikesStarSlider;                         
    public Image m_StrengthStrikesStarFillImage;
    // Подсказка (звезда) серии блоков
    public Slider m_SeriesOfBlocksStarSlider;
    public Image m_SeriesOfBlocksStarFillImage;
    // Подсказка (звезда) серии ударов
    public Slider m_SeriesOfStrikesStarSlider;
    public Image m_SeriesOfStrikesStarFillImage;

    public GameObject m_GetWoundPrefab;                 // ссылка на объект получения раны (префаб, состоящий из particle system и звука)

    public AudioClip m_audioClip;                       // ссылки на компоненты конкретного инстанса префаба раны (и крика)
    private AudioSource m_WoundAudio;                        
    private ParticleSystem m_WoundParticles;

    private Animator m_Anim;                            // ссылка на компонент-аниматор этого героя

    [HideInInspector]
    public GameObject m_Hero2HandedSword;                     // ссылка на объект-двуручный меч этого героя
    [HideInInspector]
    public GameObject m_HeroSword;                            // ссылка на объект-основной меч этого героя
    [HideInInspector]
    public GameObject m_HeroSword_2;                          // ссылка на объект-второй меч этого героя
    [HideInInspector]
    public GameObject m_HeroShield;                           // ссылка на объект-щит этого героя

    public RuntimeAnimatorController m_ACSwordShield;                   // ссылка на контроллер анимации Щит-Меч
    public AnimatorOverrideController m_AC2HandedSword;                 // ссылка на контроллер анимации 2-ручный Меч
    public AnimatorOverrideController m_ACSwordSword;                   // ссылка на контроллер анимации Меч-Меч

    public Inventory inventory;                                             // ссылка на объект-инвенторий этого героя (инстанс класса Inventory)
    [HideInInspector]
    public GameObject[] itemSlots = new GameObject[Inventory.numItemSlots]; // ссылки на солты пунктов инвентория этого героя (графические объекты)
    
    // ССЫЛКИ НА ЗВУКИ
    public AudioClip m_audioClipParry;         // Парирование               
    public AudioClip m_audioClipThrough;       // Пробитие блока двурой
    public AudioClip m_audioClipBlock;         // Блок
    public AudioClip m_audioClipEvade;         // Уворот на смене
    public AudioClip m_audioClipDeath;         // Смерть
    public AudioClip m_audioClipBonus;         // Бонус набран
    public AudioClip m_audioClipStep;          // Шаг
    public AudioClip m_audioClipRun;           // Бег

    public AudioSource m_FirstHeroAudio;        // аудио-источник для первого удара
    public AudioSource m_SecondHeroAudio;       // аудио-источник для второго удара
    public AudioSource m_MovementHeroAudio;     // аудио-источник для перемещений

    public virtual void Awake()                        // вызывается (1 раз) перед start, даже если скрипт неактивен
    {
        // Вся эта борода с порождениями делается 1 раз для эффективности, но объекты не сразу на сцене для возможности расширения
        m_WoundParticles = Instantiate(m_GetWoundPrefab).GetComponent<ParticleSystem>();    // порождаем инстанс префаба раны и берем компонент этого инстанса
        m_WoundAudio = m_WoundParticles.GetComponent<AudioSource>();                        // берём другой компонент (можно ссылаться на объект по его компоненту)
        // отправляем объект  в пассив... Наверное, так надо делать, если у нас есть компоненты "Play on Awake", чтобы они не отобразились сразу. Сейчас не надобно
        //m_WoundParticles.gameObject.SetActive(false);   
        m_WoundAudio.clip = m_audioClip;

        // получаем ссылку на компонент-аниматор этого героя (на случай, если игру расширю и героя придется порождать в процессе игры)
        m_Anim = GetComponentInChildren<Animator>();

        // Установим максимальные значения слайдеров-подсказок серий
        m_StrengthStrikesStarSlider.maxValue = GameManager.strongStrikeSeriesBeginning;
        m_SeriesOfBlocksStarSlider.maxValue = GameManager.seriesBlockBeginning;
        m_SeriesOfStrikesStarSlider.maxValue = GameManager.seriesStrikeBeginning;
    }


    public virtual void OnEnable()                          // что мы делаем, когда герой снова жив (back on again, следующий раунд)
    {
        m_CurrentHealth = m_StartingHealth;                 // здоровье на максимум
        m_dead = false;
        SetHealthUI();                                      // обновляем UI, чтоб колесо здоровья отобразилось правильно (о.с. с юзером)

        transform.position = new Vector3(stockXposition, 0, zeroZposition);     // установить начальное положение

        // убираем лишние объекты-оружия, кроме начальных щит-меч
        m_Hero2HandedSword.SetActive(false);
        m_HeroSword_2.SetActive(false);
        m_HeroSword.SetActive(true);
        m_HeroShield.SetActive(true);

        weaponSet = WeaponSet.SwordShield;                  // набор оружия по умолчанию - щит-меч
        m_Anim.runtimeAnimatorController = m_ACSwordShield; // установим АС по умолчанию - щит-меч
        m_Anim.Rebind();                                    // перезапустить АС, чтоб перешел в исходное состояние

        m_toPosition = true;                                // триггер выхода на ристалище
        m_MovementHeroAudio.clip = m_audioClipStep;         // звук запускаем 1 раз в конце ф-ии
        m_MovementHeroAudio.PlayDelayed(0.7f);
    }


    // I. АТАКА
    public void Attack()
    {
        m_attack = false;                                    // сбрасываем глобальный триггер состояния
        m_Anim.SetBool("Attack", true);                      // взводим анимационный триггер
    }


    // III. ПРИНЯТЬ УДАР
    public void TakeDamage(float amount)        // public, следовательно будет вызываться другим объектом - менеджером игры
    {
        m_CurrentHealth -= amount;                                  // уменьшаем здоровье
        m_Anim.SetBool("GetHit", true);                             // анимация

        // визуальный эффект крови:
        m_WoundParticles.transform.position = transform.position;   // перемещаем рану на героя
        //m_WoundParticles.gameObject.SetActive(true);              // активируем
        m_WoundParticles.Play();                                    // воспроизводим систему частиц
        m_WoundAudio.Play();                                        // воспроизводим аудио крика боли

        SetHealthUI();                                              // двигаем слайдер (и корректируем его цвет)

        if (m_CurrentHealth <= 0f && !m_dead)                       // а если умер
        {
            OnDeath();
        }
    }
    private void SetHealthUI()                  // двигаем слайдер (и корректируем его цвет)
    {
        m_Slider.value = m_CurrentHealth;                           // приводим значение слайдера в соответствие текущему значению здоровья
        m_FillImage.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / m_StartingHealth);   // плавно меняем цвет верхней (зеленой при 100% жизни) картинки слайдера на красную при 0%
    }
    public void RegenHealth(float amount)        // public, следовательно будет вызываться другим объектом - менеджером игры
    {
        m_CurrentHealth += amount;                                  // увеличиваем здоровье
        SetHealthUI();                                              // двигаем слайдер (и корректируем его цвет)
    }

    // IV. УМЕРЕТЬ
    private void OnDeath()
    {
        // Play the effects for the death of the hero and deactivate it.
        m_dead = true;
        m_change = false;                                       // чтобы не было анимации смены, коли уж умер - возможно, лишнее
        m_Anim.SetBool("Die", true);
        m_FirstHeroAudio.clip = m_audioClipDeath;
        m_FirstHeroAudio.PlayDelayed(1.8f);
        //    m_ExplosionParticles.transform.position = transform.position;   // перемещаем рану на героя
        //    m_ExplosionParticles.gameObject.SetActive(true);   // активируем
        //   m_ExplosionParticles.Play();                        // воспроизводим систему частиц
        //   m_ExplosionAudio.Play();                            // воспроизводим аудио
        //   gameObject.SetActive(false);                        // переводим героя в пассив  (вместе с системой частиц)
    }


    // II. СМЕНА ОРУЖИЯ
    public float line_speed = 1.2f;                               // линейная скорость бега героя
    public float angle_speed = 180f;                              // угловая скорость поворота героя
    public void ChangeWeapon()
    {
        if (SmoothRotation(90f))                                                    // вращение на 90 градусов
        {
            if (!m_Anim.GetBool("Change")) // анимация - запустить 1 раз. "Change" - булева переменная, сама не сбростися. Остальные параметры АС - триггеры, сами ресетятся через такт   
            {
                m_Anim.SetBool("Change", true);
                m_MovementHeroAudio.clip = m_audioClipRun;   // делаем 1 раз в начале ф-ии
                m_MovementHeroAudio.Play();
            }                   
            if (SmoothMotion(stockXposition))                                       // Перемещение к краю ристалища
            {
                m_change = false;                                                   // сбрасываем глобальный триггер 
                m_Anim.SetBool("Change", false);                                    // и анимационный

                switch (weaponSet)                   // Отображаем нужный набор оружия и включаем нужный анимационный контроллер 
                {
                    case WeaponSet.SwordShield:
                        m_Hero2HandedSword.SetActive(false);
                        m_HeroSword_2.SetActive(false);
                        m_HeroSword.SetActive(true);
                        m_HeroShield.SetActive(true);
                        m_Anim.runtimeAnimatorController = m_ACSwordShield;
                        break;
                    case WeaponSet.SwordSword:
                        m_Hero2HandedSword.SetActive(false);
                        m_HeroSword_2.SetActive(true);
                        m_HeroSword.SetActive(true);
                        m_HeroShield.SetActive(false);
                        m_Anim.runtimeAnimatorController = m_ACSwordSword;
                        break;
                    case WeaponSet.TwoHandedSword:
                        m_Hero2HandedSword.SetActive(true);
                        m_HeroSword_2.SetActive(false);
                        m_HeroSword.SetActive(false);
                        m_HeroShield.SetActive(false);
                        m_Anim.runtimeAnimatorController = m_AC2HandedSword;
                        break;
                }

                m_rotateToCenter = true;                                     // Взвести триггер - вращение на центр

            }
        }
    }


    public virtual void SetSwordSword()                     // по нажатию кнопки, например, меч-меч 
    {
        weaponSet = WeaponSet.SwordSword;
    }

    public virtual void SetSwordShield()                  
    {
        weaponSet = WeaponSet.SwordShield;
    }

    public virtual void SetTwoHandedSword()                 
    {
        weaponSet = WeaponSet.TwoHandedSword;
    }


    // Функция плавного поворота до newY [0-360) градусов вокруг оси Y. Возвращает true, если поворот достигнут.
    // Если надо прокрутиться через 0, использовать 2 раза: до 359 и далее...
    
        /* 
        Вариант со штатным демпфером - наверное, он хорош, когда мы не знаем угла (и скорости) поворота заранее, а сейчас слишком сложен 
        private float RotationVelocity;                                             //переменная, нужная Mathf.SmoothDampAngle. Задать глобально
        float yAngel = Mathf.SmoothDampAngle(0f, 90f, ref RotationVelocity, 0.3f);  // Рассчитать угол, на который надо повернуться за такт, чтоб на 90 градусов повернуться за 0.3 сек
        Quaternion rotationToStore = Quaternion.Euler(0f, yAngel, 0f);              // Выдать вращение, равное этому углу, относительно оси Y в кватернионе
        transform.rotation *= rotationToStore;                                      // Применить его к текущему вращению
        */

    public bool SmoothRotation(float newY)
    {
        float newYPE = newY + zeroYrotation;                                              // заданное вращение, противоположное для игрока и врага
        float currentY = transform.rotation.eulerAngles.y;                                // текущее вращение [0-360 градусов)
        if (currentY == newYPE) return true;                                              // поворот выполнен
        Quaternion newRotation = Quaternion.Euler(0f, angle_speed * Time.deltaTime, 0f);  
        if (currentY + newRotation.eulerAngles.y >= 360f)                                 // на этом шаге перешагнем через 0... Все остальное вращение произойдет мигом
        {
            currentY = newYPE;
        }
        transform.rotation *= newRotation;                                                 // крутимся по часовой
        //transform.rotation *= Quaternion.Inverse(newRotation);                           // так было бы против часовой...
        if (currentY >= newYPE)                                                            // докрутились ли?
            {
                transform.rotation = Quaternion.Euler(0f, newYPE + 0.1f, 0f);              // подравнять вращение
                return true;
            }
            else return false;
    }


    //Функция плавного перемещения вдоль оси X. Возвращает true, если нужная позиция достигнута

            /*
            Вариант со штатным демпфером. Наверное, он хорош, когда мы не знаем вектора (и скорости) заранее, а сейчас слишком сложен 
            private Vector3 Velocity = Vector3.zero;                             // эту переменную надо задать глобально
            Vector3 Destination = new Vector3(2.5f, 0, -1.5f);                   // вектор края ристалища (туда бежим на смену оружия)
            transform.position = Vector3.SmoothDamp(transform.position, Destination, ref Velocity, 0.6f);
            */

    public bool SmoothMotion(float X)
    {
        if (transform.position.x == X) return true;                                                 // перемещение достигнуто
        if (X > transform.position.x)                                                               // Если координату X необходимо увеличивать
        {
            Vector3 Destination = new Vector3(X+0.1f, 0, zeroZposition) - transform.position;               // то увеличиваем
            transform.position += Destination * line_speed * Time.deltaTime;
            if (transform.position.x >= X)                                                          // и сравниваем, не стало ли X больше задания
            {
                transform.position = new Vector3(X, 0, zeroZposition);                                      // подравнять X
                return true;
            }
            else return false;
        }
        else                                                                                        // иначе координату X необходимо уменьшать
        {
            Vector3 Destination = new Vector3(X-0.1f, 0, zeroZposition) - transform.position;               // уменьшаем
            transform.position += Destination * line_speed * Time.deltaTime;
            if (transform.position.x <= X)                                                          // и сравниваем, не стало ли X меньше задания
            {
                transform.position = new Vector3(X, 0, zeroZposition);                                      // подравнять X
                return true;
            }
            else return false;
        }
    }


    // Разворот после смены оружия
    public void RotateToCenter()
    {
        // a= transform.rotation.eulerAngles.y;          // для отладки  
        if (SmoothRotation(270f))                                
        { 
            m_rotateToCenter = false;            // сбрось триггер - вращение на центр
            m_toPosition = true;                 // взвести триггер - на исходную позицию
            m_MovementHeroAudio.clip = m_audioClipStep;   // делаем 1 раз в конце ф-ии
            m_MovementHeroAudio.Play();
        }
    }


    // Выход на центр ристалища
    public void ToPosition()
    {
        if (SmoothMotion(0f))
        {
            if (SmoothRotation(359.9f)) m_toPosition = false;                    // сбрасываем глобальный триггер, когда стоим лицом к врагу                                                      
        }
    }


    // Рассчет частных tweaker-ов, исходя из модификаторов предметов инвентаря
    public void CalculateLocalTweakers()
    {       
        for (int i = 0; i < inventory.items.Length; i++)
        {
            if (inventory.items[i] != null)
            {
                m_damageBaseMin += inventory.items[i].damageModifier;
                m_damageBaseMax += inventory.items[i].damageModifier;
                m_koef2HandedSword = inventory.items[i].koef2HandSwordModMulAdd ? m_koef2HandedSword * (1 + inventory.items[i].koef2HandSwordModifier / 100f) : m_koef2HandedSword + inventory.items[i].koef2HandSwordModifier;
                m_koefSecondSword = inventory.items[i].koefSecondSwordModMulAdd ? m_koefSecondSword * (1 + inventory.items[i].koefSecondSwordModifier / 100f) : m_koefSecondSword + inventory.items[i].koefSecondSwordModifier;
                m_blockChance = inventory.items[i].blockChanceModMulAdd ? m_blockChance * (1 + inventory.items[i].blockChanceModifier / 100f) : m_blockChance + inventory.items[i].blockChanceModifier;
                m_part2HandedThroughShield = inventory.items[i].part2HandedThroughShieldModMulAdd ? m_part2HandedThroughShield * (1 + inventory.items[i].part2HandedThroughShieldModifier / 100f) : m_part2HandedThroughShield + inventory.items[i].part2HandedThroughShieldModifier;
                m_evadeOnChangeChance = inventory.items[i].evadeOnChangeChanceModMulAdd ? m_evadeOnChangeChance * (1 + inventory.items[i].evadeOnChangeChanceModifier / 100f) : m_evadeOnChangeChance + inventory.items[i].evadeOnChangeChanceModifier;
                m_maxDefencePart = inventory.items[i].parringChanceModMulAdd ? m_maxDefencePart * (1 + inventory.items[i].parringChanceModifier / 100f) : m_maxDefencePart + inventory.items[i].parringChanceModifier;
                m_CurrentHealth = inventory.items[i].startHealthModMulAdd ? m_CurrentHealth * (1 + inventory.items[i].startHealthModifier / 100f) : m_CurrentHealth + inventory.items[i].startHealthModifier;
            }
        }
    }

    // подвинуть слайдеры подсказок о сериях
    public void SetStrongStrikesStarUI()
    {
        m_StrengthStrikesStarSlider.value = strongStrikesNum;
        m_StrengthStrikesStarFillImage.color = Color.Lerp(Color.magenta, Color.red, strongStrikesNum / GameManager.strongStrikeSeriesBeginning);
    }
    public void SetSeriesOfBlocksStarUI()
    {
        m_SeriesOfBlocksStarSlider.value = seriesOfBlocksNum;
        m_SeriesOfBlocksStarFillImage.color = Color.Lerp(Color.yellow, Color.green, seriesOfBlocksNum / GameManager.seriesBlockBeginning);
    }
    public void SetSeriesOfStrikesStarUI()
    {
        m_SeriesOfStrikesStarSlider.value = seriesOfStrikesNum;
        m_SeriesOfStrikesStarFillImage.color = Color.Lerp(Color.blue, Color.cyan, seriesOfStrikesNum / GameManager.seriesStrikeBeginning);
    }

    // Set/Reset Series
    public void SetStrongStrikesSeries()        // серия достигнута: сыграть звук достижения серии и выставить меркер
    {
        //SFXAudio.clip = m_audioClipBonus;     // клип пока что один
        SFXAudio.Play();
        hasStrongStrikesSeries = true;
    }
    public void ResetStrongStrikesSeries() 
    {
        strongStrikesNum = 0;
        hasStrongStrikesSeries = false;
    }
    public void SetSeriesOfStrikes()
    {
        //SFXAudio.clip = m_audioClipBonus;
        SFXAudio.Play();
        hasSeriesOfStrikes = true;
    }
    public void ResetSeriesOfStrikes()
    {
        seriesOfStrikesNum = 0;
        hasSeriesOfStrikes = false;
    }
    public void SetSeriesOfBlocks()
    {
        //SFXAudio.clip = m_audioClipBonus;
        SFXAudio.Play();
        hasSeriesOfBlockes = true;
    }
    public void ResetSeriesOfBlocks()
    {
        seriesOfBlocksNum = 0;
        hasSeriesOfBlockes = false;
    }
}

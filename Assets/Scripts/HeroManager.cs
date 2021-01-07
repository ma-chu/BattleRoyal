using UnityEngine;
using System;                                               // for Events


public class HeroManager : MonoBehaviour
{
    /* ССЫЛКИ НА ДРУГИЕ MonoBehaviour-классы, относящиеся к этому герою.
     * Хотел сделать их вложенными классами, но косолапая реализация паттерна комповщика в Unity С#
     * (отсутствие ссылки на внешний класс и инициализации полей вложенного класса редактором) мешает */
    private HP _HP;                                             // Здоровье
    private Series series;                                      // Серии ударов и блоков
    protected HeroAnimation m_HeroAnimation;                    // Анимация

    [SerializeField]
    protected Inventory inventory;                              // Инвенторий
    [HideInInspector]
    protected GameObject[] itemSlots = new GameObject[Inventory.numItemSlots]; // ссылки на солты пунктов инвентория этого героя (графические объекты)
    
    public Tweakers m_Tweakers;                                 // Настройки балланса боёвки

    public PreCoeffs[] preCoeffs = new PreCoeffs[2];            // Предв. значения для рассчета урона

    public float defencePart;                                   // Тактика боя - ориентированность на защиту: от 0 до 33% урона меняется на возможность парирования (шаги на сегодня: 0%, 33%)
    public float gotDamage;                                     // Возможный получаемый урон на текущий удар

    public bool m_dead;                                         // герой мёртв 


    public bool HasStrongStrikesSeries
    {
        get { return series.HasStrongStrikesSeries; }
    }
    public bool HasSeriesOfStrikes
    {
        get { return series.HasSeriesOfStrikes; }
    }
    public bool HasSeriesOfBlocks
    {
        get { return series.HasSeriesOfBlocks; }
    }


    // СОБЫТИЯ - выставляются в основном по событию GameManager.ExchangeEvent с учетом значений
    public event Action DeathEvent;                     
    private void InvokeDeathEvent()                         // Вызываем из HP           
    {
        m_dead = true;
        DeathEvent?.Invoke();
    }
    public event Action AttackEvent;
    public event Action ChangeEvent;
    public event Action ToPositionEvent;
    public void InvokeToPositionEvent()                     // костылик, чтобы вызвать событие из другого класса - HeroAnimation
    {
        ToPositionEvent?.Invoke();
    }
    public event Action<int> GetHitEvent;
    public event Action<int> ParryEvent;
    public event Action BlockVs2HandedEvent;
    public event Action<int> BlockEvent;
    public event Action<int> EvadeEvent;                                              

    public static int player_countRoundsWon = 0;                // Сколько раундов выиграл игрок
    public static int enemy_countRoundsWon = 0;                 // Сколько раундов выиграл враг

    public WeaponSet weaponSet = WeaponSet.SwordShield;         // Какой набор оружия использовать
    public Decision decision;                                   // Решение на этот ход

    // ссылки на объекты-оружие 
    public GameObject heroSword;
    public GameObject heroShield;
    public GameObject hero2HandedSword;
    public GameObject heroSword_2;
    // их компоненты
    protected MeshFilter shieldMeshFilter;
    protected MeshFilter twoHandedSwordMeshFilter;
    protected MeshRenderer swordMeshRenderer;
    protected MeshRenderer sword2MeshRenderer;
    protected MeshRenderer shieldMeshRenderer;
    protected MeshRenderer twoHandedSwordMeshRenderer;

    protected virtual void Awake()                             
    {
        _HP = GetComponent("HP") as HP;
        series = GetComponent("Series") as Series;
        m_HeroAnimation = GetComponent("HeroAnimation") as HeroAnimation;

        m_Tweakers = new Tweakers();

        player_countRoundsWon = 0;
        enemy_countRoundsWon = 0;

        // получаем ссылки на компоненты оружия
        shieldMeshFilter = heroShield.GetComponent<MeshFilter>();
        twoHandedSwordMeshFilter = hero2HandedSword.GetComponent<MeshFilter>();
        swordMeshRenderer = heroSword.GetComponent<MeshRenderer>();
        sword2MeshRenderer = heroSword_2.GetComponent<MeshRenderer>();
        shieldMeshRenderer = heroShield.GetComponent<MeshRenderer>();
        twoHandedSwordMeshRenderer = hero2HandedSword.GetComponent<MeshRenderer>();
    }

    protected virtual void OnEnable()                          // что мы делаем, когда герой снова жив (back on again, следующий раунд)
    {
        GameManager.ExchangeEvent1 += OnExchange1;
        GameManager.ExchangeEvent2 += OnExchange2;
        GameManager.ExchangeEndedEvent += OnExchangeEnded;

        if (!m_HeroAnimation.enabled) m_HeroAnimation.enabled = true;   // переинициализируем движетеля героя

        m_Tweakers = new Tweakers();                                    // переинициализируем твикеры героя на дефолтные
        m_Tweakers.AddInventoryTweakers(inventory);                     

        _HP.SetStartHealth(m_Tweakers.StartingHealth);                  // здоровье на максимум

        // Обнулим серии героя
        series.ResetStrongStrikesSeries();
        series.ResetSeriesOfBlocks();
        series.ResetSeriesOfStrikes();
        // Обнулим подсказки серий
        series.UpdateStrongStrikesSeries();
        series.UpdateSeriesOfBlocks();
        series.UpdateSeriesOfStrikes();
        // убираем лишние объекты-оружия, кроме начальных щит-меч
        hero2HandedSword.SetActive(false);
        heroSword_2.SetActive(false);
        heroSword.SetActive(true);
        heroShield.SetActive(true);

        weaponSet = WeaponSet.SwordShield;                              // набор оружия по умолчанию - щит-меч

        decision = Decision.No;
    }

    private void OnDisable()
    {
        GameManager.ExchangeEvent1 -= OnExchange1;
        GameManager.ExchangeEvent2 -= OnExchange2;
        GameManager.ExchangeEndedEvent -= OnExchangeEnded;
        m_HeroAnimation.enabled = false;
    }

    public virtual void SetSwordSword()                                 // по нажатию кнопки, например, меч-меч 
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

    // Функции-запускатели событий этого класса, что подписываются на GameManager.ExchangeEvent1-2
    private void OnExchange1()
    {
        if ((preCoeffs[0].exchangeResult == ExchangeResult.GetHit) || (preCoeffs[0].exchangeResult == ExchangeResult.BlockVs2Handed))
        {
            GetHitEvent?.Invoke(1);
            if (_HP.TakeDamage(/*damage1*/gotDamage)) InvokeDeathEvent();
        }

        if (decision == Decision.Attack)
        {
            if (preCoeffs[0].exchangeResult == ExchangeResult.Parry) ParryEvent?.Invoke(1);
            if (preCoeffs[0].exchangeResult == ExchangeResult.Block) BlockEvent?.Invoke(1);
            if (preCoeffs[0].exchangeResult == ExchangeResult.BlockVs2Handed) BlockVs2HandedEvent?.Invoke();
        }

        if (preCoeffs[0].exchangeResult == ExchangeResult.Evade) EvadeEvent?.Invoke(1);
    }

    protected virtual void OnExchange2()
    {
        if (decision == Decision.Attack) AttackEvent?.Invoke();

        if ((preCoeffs[1].exchangeResult == ExchangeResult.GetHit) && !m_dead)    // если не помер после первого удара
        {
            GetHitEvent?.Invoke(2);                                               // то принимаем второй
            if (_HP.TakeDamage(/*damage2*/gotDamage)) InvokeDeathEvent();  
        }

        if (((decision == Decision.ChangeSwordShield) || (decision == Decision.ChangeSwordSword) || (decision == Decision.ChangeTwoHandedSword))
            && !m_dead) ChangeEvent?.Invoke();
        else
        {        
        if (preCoeffs[1].exchangeResult == ExchangeResult.Parry) ParryEvent?.Invoke(2);
        if (preCoeffs[1].exchangeResult == ExchangeResult.Block) BlockEvent?.Invoke(2);
        }

        if (preCoeffs[1].exchangeResult == ExchangeResult.Evade) EvadeEvent?.Invoke(2);
    }

    private void OnExchangeEnded()
    {
        decision = Decision.No;
    }

    public virtual void CalculatePreCoeffs()
    {
        preCoeffs[0].exchangeResult = ExchangeResult.No;
        preCoeffs[1].exchangeResult = ExchangeResult.No;

        preCoeffs[0].parry = (UnityEngine.Random.value <= defencePart);
        preCoeffs[1].parry = (UnityEngine.Random.value <= defencePart);

        // Предварительные коэффициенты на основе текущего набора оружия
        switch (weaponSet)
        {
            case WeaponSet.SwordShield:
                preCoeffs[0].damage = UnityEngine.Random.Range(m_Tweakers.DamageBaseMin, m_Tweakers.DamageBaseMax + 1);
                preCoeffs[0].damage += series.AddSeriesDamage();
                preCoeffs[1].damage = 0f;
                preCoeffs[0].block = (UnityEngine.Random.Range(0f, 1f) <= m_Tweakers.BlockChance);
                preCoeffs[1].block = (UnityEngine.Random.Range(0f, 1f) <= m_Tweakers.BlockChance);
                break;
            case WeaponSet.SwordSword:
                preCoeffs[0].damage = UnityEngine.Random.Range(m_Tweakers.DamageBaseMin, m_Tweakers.DamageBaseMax + 1);
                preCoeffs[0].damage += series.AddSeriesDamage();
                preCoeffs[1].damage = UnityEngine.Random.Range(m_Tweakers.DamageBaseMin * m_Tweakers.CoefSecondSword, m_Tweakers.DamageBaseMax * m_Tweakers.CoefSecondSword);
                preCoeffs[1].damage += series.AddSeriesDamage();
                preCoeffs[0].block = false;
                preCoeffs[1].block = false;
                preCoeffs[0].blockVs2Handed = false;
                break;
            case WeaponSet.TwoHandedSword:
                preCoeffs[0].damage = UnityEngine.Random.Range(m_Tweakers.DamageBaseMin * m_Tweakers.Coef2HandedSword, m_Tweakers.DamageBaseMax * m_Tweakers.Coef2HandedSword);
                preCoeffs[0].damage += series.AddSeriesDamage();
                preCoeffs[1].damage = 0f;
                preCoeffs[0].block = false;
                preCoeffs[1].block = false;
                preCoeffs[0].blockVs2Handed = false;
                break;
        }
        // А также предварительные коэффициенты на основе решения
        switch (decision)
        {
            case Decision.Attack:
                preCoeffs[0].evade = false;
                preCoeffs[1].evade = false;
                break;
            default:                                                    // точно какая-то смена
                preCoeffs[0].evade = (UnityEngine.Random.Range(0f, 1f) <= m_Tweakers.EvadeOnChangeChance);
                preCoeffs[1].evade = (UnityEngine.Random.Range(0f, 1f) <= m_Tweakers.EvadeOnChangeChance);
                preCoeffs[0].block = false;
                preCoeffs[1].block = false;
                preCoeffs[0].blockVs2Handed = false;
                preCoeffs[0].parry = false;
                preCoeffs[1].parry = false;
                break;
        }
    }

    public ExchangeResult CalculateExchangeResult(int strikeNumber)
    {
        if (preCoeffs[strikeNumber - 1].parry)                                        // А. парирование
        {
            return ExchangeResult.Parry;
        }
        else if (preCoeffs[strikeNumber - 1].blockVs2Handed)                          // Б. пробитие щита двуручником
        {
            return ExchangeResult.BlockVs2Handed;
        }
        else if (preCoeffs[strikeNumber - 1].block)                                   // В. блок
        {
            return ExchangeResult.Block;
        }
        else if (preCoeffs[strikeNumber - 1].evade)                                   // Г. уворот на смене
        {
            return ExchangeResult.Evade;
        }
        else
        {
            return ExchangeResult.GetHit;                                            // Д. принять полный первый удар
        }
    }

    public void AddStrongSeries(int strikeNumber)
    {
        series.AddStrongSeries(strikeNumber);
    }

    public void AddStrikesSeries()
    {
        series.AddSeriesOfStrikes();
    }
    public void ResetStrikesSeries()
    {
        series.ResetSeriesOfStrikes();
    }

    public string GiveOutPrize()
    {
        int a;
        do a = inventory.AddItem(AllItems.Instance.items[UnityEngine.Random.Range(0, AllItems.Instance.items.Length)]);
        while (a == -2);                                //добавить уникальный инвентарь
        if (a != -1)                                    // и чтоб не был полный инвенторий (т.е. мы выиграли 4 раунд, т.е. игру)
        {
            inventory.ShowItemDescription(a);           // отобразить описание выданного инвентаря
            return inventory.items[a].name;
        }
        else return null;
    }
}

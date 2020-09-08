using UnityEngine;
using System;                   // Events здесь

public class HeroManager : MonoBehaviour
{
    /* ССЫЛКИ НА ДРУГИЕ MonoBehaviour-классы, относящиеся к этому герою.
     * Хотел сделать их вложенными классами, но косолапая реализация паттерна комповщика в Unity С#
     * (отсутствие ссылки на внешний класс и инициализации полей вложенного класса редактором) мешает */
    public HP _HP;                                          // Здоровье
    public Series series;                                   // Серии ударов и блоков
    public Inventory inventory;                             // Инвенторий
    [SerializeField]
    protected HeroAnimation m_HeroAnimation;                // Анимация
    [HideInInspector]
    protected GameObject[] itemSlots = new GameObject[Inventory.numItemSlots]; // ссылки на солты пунктов инвентория этого героя (графические объекты)
    // Класс, порождаемый здесь 
    public Tweakers m_Tweakers;                             // Настройки балланса боёвки

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

    // ЗНАЧЕНИЯ для рассчета урона, выставляются GameManager-ом
    public float damage1;                                       // Возможный получаемый урон1 на этот ход
    public float damage2;                                       // Возможный получаемый урон2 на этот ход
    public bool evade1;                                         // Уворот от первого удара на смене на этот ход    
    public bool evade2;                                         // Уворот от второго удара на смене на этот ход    
    public bool block1;                                         // Блок первого удара на этот ход    
    public bool block2;                                         // Блок второго удара на этот ход
    public bool blockVs2Handed;                                 // Блок удара двуручником - половина урона
    public bool parry1;                                         // Парирование первого удара на этот ход    
    public bool parry2;                                         // Парирование второго удара на этот ход
    public float defencePart;                                   // Тактика боя - ориентированность на защиту: от 0 до 33% урона меняется на возможность парирования (шаги на сегодня: 0%, 33%)
    
    public bool m_dead;                                         // герой мёртв                                               

    public static int player_countRoundsWon = 0;                // Сколько раундов выиграл игрок
    public static int enemy_countRoundsWon = 0;                 // Сколько раундов выиграл враг

    public WeaponSet weaponSet = WeaponSet.SwordShield;         // Какой набор оружия использовать
    public Decision decision;                                   // Решение на этот ход
    public ExchangeResult exchangeResult1 = ExchangeResult.No;  // Результат схода для героя c первым ударом врага
    public ExchangeResult exchangeResult2 = ExchangeResult.No;  // Результат схода для героя cо вторым ударом врага

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
        series.CheckAndSetStrongStrikesSeries();
        series.CheckAndSetSeriesOfBlocks(this);
        series.CheckAndSetSeriesOfStrikes();
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
        if ((exchangeResult1 == ExchangeResult.GetHit) || (exchangeResult1 == ExchangeResult.BlockVs2Handed))
        {
            GetHitEvent?.Invoke(1);
            if (_HP.TakeDamage(damage1)) InvokeDeathEvent();
        }

        if (decision == Decision.Attack)
        {
            if (exchangeResult1 == ExchangeResult.Parry) ParryEvent?.Invoke(1);
            if (exchangeResult1 == ExchangeResult.Block) BlockEvent?.Invoke(1);
            if (exchangeResult1 == ExchangeResult.BlockVs2Handed) BlockVs2HandedEvent?.Invoke();
        }

        if (exchangeResult1 == ExchangeResult.Evade) EvadeEvent?.Invoke(1);
    }

    protected virtual void OnExchange2()
    {
        if (decision == Decision.Attack) AttackEvent?.Invoke();

        if ((exchangeResult2 == ExchangeResult.GetHit) && !m_dead)    // если не помер после первого удара
        {
            GetHitEvent?.Invoke(2);                                   // то принимаем второй
            if (_HP.TakeDamage(damage2)) InvokeDeathEvent();  
        }

        if (((decision == Decision.ChangeSwordShield) || (decision == Decision.ChangeSwordSword) || (decision == Decision.ChangeTwoHandedSword))
            && !m_dead) ChangeEvent?.Invoke();
        else
        {        
        if (exchangeResult2 == ExchangeResult.Parry) ParryEvent?.Invoke(2);
        if (exchangeResult2 == ExchangeResult.Block) BlockEvent?.Invoke(2);
        }

        if (exchangeResult2 == ExchangeResult.Evade) EvadeEvent?.Invoke(2);
    }

    private void OnExchangeEnded()
    {
        decision = Decision.No;
        exchangeResult1 = ExchangeResult.No;
        exchangeResult2 = ExchangeResult.No;
    }
}

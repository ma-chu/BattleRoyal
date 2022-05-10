using UnityEngine;
using System;
using System.Linq; 
// Смена сетов оружия, инвенторий и изменение цвета/формы оружия
public class HeroManager : MonoBehaviour
{
    //private const int StrikesQuantity = 2;    // не доделано
    /* ССЫЛКИ НА ДРУГИЕ MonoBehaviour-классы, относящиеся к этому герою.
     * Хотел сделать их вложенными классами, но косолапая реализация паттерна комповщика в Unity (или все же недопонял?) С#
     * (отсутствие ссылки на внешний класс и инициализации полей вложенного класса редактором) мешает */
    //private HP _HP;                                                 // Здоровье
    //private Series series;                                          // Серии ударов и блоков
    //protected HeroAnimation m_HeroAnimation;                        // Анимация

    [SerializeField] protected Inventory inventory;                 // Инвенторий
    protected GameObject[] itemSlots = new GameObject[Inventory.numItemSlots]; // ссылки на солты пунктов инвентория этого героя (графические объекты)
    
    //public Tweakers m_Tweakers;                                     // Настройки балланса боёвки
    //public PreCoeffs[] preCoeffs = new PreCoeffs[StrikesQuantity];  // Предв. значения для рассчета урона
    //public ExchangeResult[] exchangeResult = new ExchangeResult[StrikesQuantity]; // Результаты ударов
    //public float defencePart;                                       // Тактика боя - ориентированность на защиту: от 0 до 33% урона меняется на возможность парирования (шаги на сегодня: 0%, 33%)
    //public int[] gotDamage;                                         // Возможный получаемый урон на текущий удар

    public bool dead;                                             // герой мёртв 

    [HideInInspector] public Heroes heroType;

    /*public bool HasStrongStrikesSeries => series.HasStrongStrikesSeries;
    public bool HasSeriesOfStrikes => series.HasSeriesOfStrikes;
    public bool HasSeriesOfBlocks => series.HasSeriesOfBlocks;*/

    // СОБЫТИЯ - выставляются в основном по событию GameManager.ExchangeEvent с учетом значений
    public event Action DeathEvent;                     
    private void InvokeDeathEvent()                                
    {
        dead = true;
        DeathEvent?.Invoke();
    }
    public event Action AttackEvent;
    public event Action ChangeEvent;
    public event Action ToPositionEvent;
    public void InvokeToPositionEvent()                     // костылик, чтобы вызвать событие из другого класса - HeroAnimation
    {
        ToPositionEvent?.Invoke();
    }
    public event Action<int, int> GetHitEvent;
    public event Action<int> ParryEvent;
    public event Action<int> BlockVs2HandedEvent;
    public event Action<int> BlockEvent;
    public event Action<int> EvadeEvent;                                              
    public event Action ExchangeEndedEvent;


    //public static int player_countRoundsWon = 0;                // Сколько раундов выиграл игрок
    //public static int enemy_countRoundsWon = 0;                 // Сколько раундов выиграл враг

    public WeaponSet weaponSet = WeaponSet.SwordShield;           // Какой набор оружия использовать
    //public Decision decision;                                   // Решение на этот ход

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
        //_HP = GetComponent<HP>() /*as HP*/;
        //series = GetComponent<Series>() /*as Series*/;
        //m_HeroAnimation = GetComponent<HeroAnimation>() /*as HeroAnimation*/;

        //m_Tweakers = new Tweakers();

        //player_countRoundsWon = 0;
        //enemy_countRoundsWon = 0;

        // получаем ссылки на компоненты оружия
        shieldMeshFilter = heroShield.GetComponent<MeshFilter>();
        twoHandedSwordMeshFilter = hero2HandedSword.GetComponent<MeshFilter>();
        swordMeshRenderer = heroSword.GetComponent<MeshRenderer>();
        sword2MeshRenderer = heroSword_2.GetComponent<MeshRenderer>();
        shieldMeshRenderer = heroShield.GetComponent<MeshRenderer>();
        twoHandedSwordMeshRenderer = hero2HandedSword.GetComponent<MeshRenderer>();
        
        //gotDamage = new int[StrikesQuantity];        // Так и не понял, почему нельзя писать при определении массива public int[] gotDamage = new int[StrikesQuantity]; 
    }

    protected virtual void OnEnable()                          // что мы делаем, когда герой снова жив (back on again, следующий раунд)
    {
        /*GameManager.ExchangeEvent1 += OnExchange1;
        GameManager.ExchangeEvent2 += OnExchange2;
        GameManager.ExchangeEndedEvent += OnExchangeEnded;*/

        //if (!m_HeroAnimation.enabled) m_HeroAnimation.enabled = true;   // переинициализируем движетеля героя
/*
        m_Tweakers = new Tweakers();                                    // переинициализируем твикеры героя на дефолтные
        m_Tweakers.AddInventoryTweakers(inventory.items);                     

        _HP.SetStartHealth(m_Tweakers.StartingHealth);                  // здоровье на максимум
*/
        // Обнулим подсказки серий героя (это все про SeriesView - отображение серий)
/*      series.UpdateStrongStrikesSeries();
        series.UpdateSeriesOfBlocks();
        series.UpdateSeriesOfStrikes();
*/
        // убираем лишние объекты-оружия, кроме начальных щит-меч
        hero2HandedSword.SetActive(false);
        heroSword_2.SetActive(false);
        heroSword.SetActive(true);
        heroShield.SetActive(true);

        weaponSet = WeaponSet.SwordShield;                              // (пока не избавился) Для анимации: набор оружия по умолчанию - щит-меч

        //decision = Decision.No;
    }

    private void OnDisable()
    {
        /*GameManager.ExchangeEvent1 -= OnExchange1;
        GameManager.ExchangeEvent2 -= OnExchange2;
        GameManager.ExchangeEndedEvent -= OnExchangeEnded;*/
        //m_HeroAnimation.enabled = false;
    }
    
    /*
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
    */
    public void Exchange(ExchangeResult[] exchangeResults, int[] gotDamages, Decision decision, int hp)
    {
        // Функции-запускатели событий этого класса, что подписываются на GameManager.ExchangeEvent1-2
        //public void OnExchange1()
        //{
        if ((exchangeResults[0] == ExchangeResult.GetHit) || (exchangeResults[0] == ExchangeResult.BlockVs2Handed))
        {
            GetHitEvent?.Invoke(1, gotDamages[0]);
            // HPView отобразить gotDamages[0]
        }

        if (decision == Decision.Attack)
        {
            if (exchangeResults[0] == ExchangeResult.Parry) ParryEvent?.Invoke(1);
            if (exchangeResults[0] == ExchangeResult.Block) BlockEvent?.Invoke(1);
            if (exchangeResults[0] == ExchangeResult.BlockVs2Handed) BlockVs2HandedEvent?.Invoke(gotDamages[0]);
        }

        if (exchangeResults[0] == ExchangeResult.Evade) EvadeEvent?.Invoke(1);
        //}

        //public /*virtual*/ void OnExchange2()
        //{
        if (decision == Decision.Attack) AttackEvent?.Invoke();

        if ((exchangeResults[1] == ExchangeResult.GetHit) && !dead)   
        {
            GetHitEvent?.Invoke(2, gotDamages[1]);
            // HPView отобразить gotDamages[1]
        }
        if (hp <= 0) InvokeDeathEvent(); 

        if (((decision == Decision.ChangeSwordShield) || (decision == Decision.ChangeSwordSword) || (decision == Decision.ChangeTwoHandedSword))
            && !dead) ChangeEvent?.Invoke();
        else
        {        
            if (exchangeResults[1] == ExchangeResult.Parry) ParryEvent?.Invoke(2);
            if (exchangeResults[1] == ExchangeResult.Block) BlockEvent?.Invoke(2);
        }

        if (exchangeResults[1] == ExchangeResult.Evade) EvadeEvent?.Invoke(2);
        //}
    }

    public void ExchangeEnded() => ExchangeEndedEvent?.Invoke();


        /*public void  OnExchangeEnded()
        {
            decision = Decision.No;
        }*/

    

    /*public void AddStrongSeries(int strikeNumber)
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
    }*/

    /*public Item GiveOutPrize()        // уйдет в сервер
    {
        int item;
        do item = inventory.AddItem(AllItems.Instance.items[UnityEngine.Random.Range(0, AllItems.Instance.items.Length)]);
        while (item == -2);                                // добавить уникальный инвентарь
        if (item != -1)                                    // и чтоб не был полный инвенторий (т.е. мы выиграли 4 раунд, т.е. игру)
        {
            inventory.ShowItemDescription(item);           // отобразить описание выданного инвентаря
            return inventory.items[item];
        }
        else return null;
    }*/
    public Item AddPrize(string name, bool showDesc = true) // все проверки выполнены на сервере
    {
        if (name.Equals(String.Empty)) return null;
        int item = inventory.AddItem(AllItems.Instance.items.First(i => i.Name == name));
        if (showDesc) inventory.ShowItemDescription(item);           
        return inventory.items[item];
    }
    
    public void SetInventory(Item item)    // очень похоже, что дублирует AddPrize
    {
        inventory.AddItem(item);
    }
}

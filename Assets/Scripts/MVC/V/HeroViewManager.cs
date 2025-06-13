using UnityEngine;
using System;
using System.Linq;

/// <summary>
/// Управление визуалом героя: 
/// Смена сетов оружия, инвенторий и изменение цвета/формы оружия и пр
/// </summary>

public class HeroViewManager : MonoBehaviour
{
    [SerializeField] private HeroUI heroUI;
    [SerializeField] private HPView hpView;
    [SerializeField] private HeroAnimation heroAnimations; // Here!!
    [SerializeField] private SeriesView seriesView;
    
    [SerializeField] protected Inventory inventory;
    
    [SerializeField] private GameObject heroSword;
    [SerializeField] protected GameObject heroShield;
    [SerializeField] private GameObject hero2HandedSword;
    [SerializeField] private GameObject heroSword_2;
    
    private readonly GameObject[] _itemSlots = new GameObject[Inventory.numItemSlots]; 

    protected MeshFilter _shieldMeshFilter;
    protected MeshFilter _twoHandedSwordMeshFilter;
    protected MeshRenderer _swordMeshRenderer;
    protected MeshRenderer _sword2MeshRenderer;
    protected MeshRenderer _shieldMeshRenderer;
    protected MeshRenderer _twoHandedSwordMeshRenderer;

    public event Action DeathEvent;                     
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
    
    public bool Dead { get; set; } 
    public WeaponSet WeaponSet { get; set; } = WeaponSet.SwordShield;
    public Heroes HeroType { get; protected set; }

    protected virtual void Awake()                             
    {
        _shieldMeshFilter = heroShield.GetComponent<MeshFilter>();
        _twoHandedSwordMeshFilter = hero2HandedSword.GetComponent<MeshFilter>();
        _swordMeshRenderer = heroSword.GetComponent<MeshRenderer>();
        _sword2MeshRenderer = heroSword_2.GetComponent<MeshRenderer>();
        _shieldMeshRenderer = heroShield.GetComponent<MeshRenderer>();
        _twoHandedSwordMeshRenderer = hero2HandedSword.GetComponent<MeshRenderer>();
    }

    protected virtual void OnEnable()
    {
        var eventTriggers = inventory.GetComponentsInChildren<UnityEngine.EventSystems.EventTrigger>();
        for (int i = 0; i < Inventory.numItemSlots; i++)
            _itemSlots[i] = eventTriggers[i].gameObject;
        
        WeaponSet = WeaponSet.SwordShield;                              // (пока не избавился) Для анимации: набор оружия по умолчанию - щит-меч
        SetSwordShield();
        
        heroUI.Initialize(this);
        heroAnimations.Initialize(this);
        
        ToPositionEvent?.Invoke();
    }

    private void OnDisable()
    {
        heroAnimations.enabled = false;       
    }

    public void Exchange(ExchangeResult[] exchangeResults, int[] gotDamages, Decision decision, int hp)
    {
        if (exchangeResults[0] == ExchangeResult.GetHit || exchangeResults[0] == ExchangeResult.BlockVs2Handed)
        {
            GetHitEvent?.Invoke(1, gotDamages[0]);
        }

        if (decision == Decision.Attack)
        {
            if (exchangeResults[0] == ExchangeResult.Parry)
                ParryEvent?.Invoke(1);
            
            if (exchangeResults[0] == ExchangeResult.Block) 
                BlockEvent?.Invoke(1);
            
            if (exchangeResults[0] == ExchangeResult.BlockVs2Handed) 
                BlockVs2HandedEvent?.Invoke(gotDamages[0]);
        }

        if (exchangeResults[0] == ExchangeResult.Evade) 
            EvadeEvent?.Invoke(1);
        
        if (decision == Decision.Attack) 
            AttackEvent?.Invoke();

        if (exchangeResults[1] == ExchangeResult.GetHit && !Dead)   
        {
            GetHitEvent?.Invoke(2, gotDamages[1]);
        }
        
        if (hp <= 0)
        {
            Dead = true;
            DeathEvent?.Invoke();
        }

        if (decision is Decision.ChangeSwordShield or Decision.ChangeSwordSword or 
                Decision.ChangeTwoHandedSword && !Dead)
        {
            ChangeEvent?.Invoke();
        }
        else
        {        
            if (exchangeResults[1] == ExchangeResult.Parry) ParryEvent?.Invoke(2);
            if (exchangeResults[1] == ExchangeResult.Block) BlockEvent?.Invoke(2);
        }

        if (exchangeResults[1] == ExchangeResult.Evade) 
            EvadeEvent?.Invoke(2);
    }

    public void ExchangeEnded() => ExchangeEndedEvent?.Invoke();

    public void SetSwordShield()
    {
        hero2HandedSword.SetActive(false);
        heroSword_2.SetActive(false);
        heroSword.SetActive(true);
        heroShield.SetActive(true);
    }
    
    public void SetSwordSword()
    {
        hero2HandedSword.SetActive(false);
        heroSword_2.SetActive(true);
        heroSword.SetActive(true);
        heroShield.SetActive(false);
    }
    
    public void Set2HandedSword()
    {
        hero2HandedSword.SetActive(true);
        heroSword_2.SetActive(false);
        heroSword.SetActive(false);
        heroShield.SetActive(false);
    }

    public Item AddPrize(string prizeName, bool showDesc = true) // все проверки выполнены на сервере
    {
        if (prizeName.Equals(string.Empty)) 
            return null;
        
        var item = inventory.AddItem(AllItems.Instance.items.First(i => i.Name == prizeName));
        if (showDesc) 
            inventory.ShowItemDescription(item); 
        
        return inventory.items[item];
    }
    
    public void SetName(string value) => heroUI.Name = value;
    
    public string GetName() => heroUI.Name;

    public void SetSeries(int[] nums, bool[] sets)
    { 
        seriesView.UpdateStrongSeries(nums[0], sets[0]);
        seriesView.UpdateSeriesOfBlocks(nums[1], sets[1]);
        seriesView.UpdateSeriesOfStrikes(nums[2], sets[2]);

        heroUI.SetRegenValues(nums[1]);
    }
    
    public void SetStartPosition()
    {
        if (!heroAnimations.enabled) 
            heroAnimations.enabled = true; 
        
        heroAnimations.SetStartPosition();
    }

    public void SetStartHealth(float startHealth)
    {
        hpView.SetStartHealth(startHealth);
    }
    
    public void SetHealth(float health)
    {
        hpView.SetHealth(health);
    }
}

using UnityEngine;
using System;
using System.Linq; 
// Смена сетов оружия, инвенторий и изменение цвета/формы оружия
public class HeroViewManager : MonoBehaviour
{
    [SerializeField] protected Inventory inventory;                            // Инвенторий
    protected GameObject[] itemSlots = new GameObject[Inventory.numItemSlots]; // ссылки на солты пунктов инвентория этого героя (графические объекты)

    public bool dead;                                                          // герой мёртв 
    public WeaponSet weaponSet = WeaponSet.SwordShield;           // Какой набор оружия использовать
    [HideInInspector] public Heroes heroType;

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
        // определимся со ссылками на слоты инвентория
        var eventTriggers = inventory.GetComponentsInChildren<UnityEngine.EventSystems.EventTrigger>();
        for (int i = 0; i < Inventory.numItemSlots; i++)
            itemSlots[i] = eventTriggers[i].gameObject;
        
        // убираем лишние объекты-оружия, кроме начальных щит-меч
        hero2HandedSword.SetActive(false);
        heroSword_2.SetActive(false);
        heroSword.SetActive(true);
        heroShield.SetActive(true);

        weaponSet = WeaponSet.SwordShield;                              // (пока не избавился) Для анимации: набор оружия по умолчанию - щит-меч
    }
    
    public void Exchange(ExchangeResult[] exchangeResults, int[] gotDamages, Decision decision, int hp)
    {
        // Функции-запускатель событий этого класса
        if ((exchangeResults[0] == ExchangeResult.GetHit) || (exchangeResults[0] == ExchangeResult.BlockVs2Handed))
        {
            GetHitEvent?.Invoke(1, gotDamages[0]);
        }

        if (decision == Decision.Attack)
        {
            if (exchangeResults[0] == ExchangeResult.Parry) ParryEvent?.Invoke(1);
            if (exchangeResults[0] == ExchangeResult.Block) BlockEvent?.Invoke(1);
            if (exchangeResults[0] == ExchangeResult.BlockVs2Handed) BlockVs2HandedEvent?.Invoke(gotDamages[0]);
        }

        if (exchangeResults[0] == ExchangeResult.Evade) EvadeEvent?.Invoke(1);

        
        
        if (decision == Decision.Attack) AttackEvent?.Invoke();

        if ((exchangeResults[1] == ExchangeResult.GetHit) && !dead)   
        {
            GetHitEvent?.Invoke(2, gotDamages[1]);
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
    }

    public void ExchangeEnded() => ExchangeEndedEvent?.Invoke();

    public Item AddPrize(string prizeName, bool showDesc = true) // все проверки выполнены на сервере
    {
        if (prizeName.Equals(string.Empty)) return null;
        var item = inventory.AddItem(AllItems.Instance.items.First(i => i.Name == prizeName));
        if (showDesc) inventory.ShowItemDescription(item);           
        return inventory.items[item];
    }
    
    public void SetInventory(Item item) =>  inventory.AddItem(item);    // очень похоже, что дублирует AddPrize. Разобраться с ней при фотоне
}

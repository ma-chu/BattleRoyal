using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerManager : HeroManager
{
    const float zeroZposition = -1.5f;      // позиция героя на ристалище 
    const float zeroYrotation = 0f;         // вращение героя на ристалище   
    const float startRotation = 270f;       // начальное вращение героя  
    const float stockXposition = 2.2f;      // начальная позиция героя (позиция склада)

    // UI-элементы
    [SerializeField] private GameObject weaponSetButtonsObject;
    [SerializeField] private Button swordSwordButton;
    [SerializeField] private Button swordShieldButton;
    [SerializeField] private Button twoHandedSwordButton;

    public GameObject restartButtonObject;
    [SerializeField] private Slider tacticSlider;            // ссылка на слайдер тактики 
    public Canvas m_PlayersControlsCanvas;  // компонент Canvas холста, содержащего в себе кнопки управления игрока
    // отключаем именно компонент холста Canvas, чтобы не помечать сам объект-подканвас (элемент родительского канваса) как dirty с перестройкой род. канваса

    protected override void Awake()
    {
        // определимся со ссылками на слоты инвентория
        var eventTriggers = inventory.GetComponentsInChildren<UnityEngine.EventSystems.EventTrigger>();
        for (int i = 0; i < Inventory.numItemSlots; i++)
        {
            //itemSlots = GameObject.FindGameObjectsWithTag("itemSlot_player");
            itemSlots[i] = eventTriggers[i].gameObject;
        }

        base.Awake();                                               

        inventory.CloseItemDescription();                           // Cкрыть описание инвентаря 

        heroType = Heroes.Player;
    }

    protected override void OnEnable()                                 // (back on again, следующий раунд)
    {
        weaponSetButtonsObject.SetActive(false);                    
        m_PlayersControlsCanvas.enabled = false;                    

        inventory.CloseItemDescription();                           // скрыть описание инвентаря (если он был выигран в предыдущем раунде)

        // Установить начальное положение героя, задать исходное на ристалище 
        m_HeroAnimation.SetStartPositions(zeroZposition, zeroYrotation, stockXposition, startRotation);

        base.OnEnable();                                            
    }

    private void Start()
    {
        m_PlayersControlsCanvas.enabled = false;                  
    }

    protected override void OnExchange2()
    {
        m_PlayersControlsCanvas.enabled = false;
        base.OnExchange2();
    }

    public void AttackPressed()                    
    {
        decision = Decision.Attack;
        weaponSetButtonsObject.SetActive(false);         // убираем кнопки сетов оружия, если вдруг до "атака" игрок нажимал "смену оружия"
    }

    public void ChangeWeaponPressed()                      
    {
        weaponSetButtonsObject.SetActive(true);        

        if (weaponSet == WeaponSet.SwordShield) swordShieldButton.enabled = false;    // не даём выбрать тот же сет
        if (weaponSet == WeaponSet.SwordSword) swordSwordButton.enabled = false;
        if (weaponSet == WeaponSet.TwoHandedSword) twoHandedSwordButton.enabled = false;
    }

    public void RestartPressed()
    {
        GameSave.Save();
        if (GameManager.gameType != GameType.Single) Photon.Bolt.BoltLauncher.Shutdown();
        SceneManager.LoadScene(0);                          // на перезагрузку сцены
    }

    public override void SetSwordSword()                     
    {
        base.SetSwordSword();
        decision = Decision.ChangeSwordSword;
        weaponSetButtonsObject.SetActive(false);            // убираем кнопки сетов
        swordShieldButton.enabled = true;                   // но делаем все их доступными для следующей смены
        swordSwordButton.enabled = true;
        twoHandedSwordButton.enabled = true;
    }

    public override void SetSwordShield()
    {
        base.SetSwordShield();
        decision = Decision.ChangeSwordShield;
        weaponSetButtonsObject.SetActive(false);            
        swordShieldButton.enabled = true;                  
        swordSwordButton.enabled = true;
        twoHandedSwordButton.enabled = true;
    }

    public override void SetTwoHandedSword()
    {
        base.SetTwoHandedSword();
        decision = Decision.ChangeTwoHandedSword;
        weaponSetButtonsObject.SetActive(false);            
        swordShieldButton.enabled = true;                   
        swordSwordButton.enabled = true;
        twoHandedSwordButton.enabled = true;
    }

    public void SetDefencePart()                // вызывается по изменению значения слайдера
    {
        defencePart = tacticSlider.value * m_Tweakers.MaxDefencePart + m_Tweakers.ParryChance;
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerManager : HeroManager
{
    // UI-элементы
    public GameObject restartButtonObject;
    public GameObject weaponSetButtonsObject;
    public Button swordSwordButton;
    public Button swordShieldButton;
    public Button twoHandedSwordButton;
    public Slider tacticSlider;                             // ссылка на слайдер тактики                

    public override void Awake()
    {
        // получаем ссылки на объекты-оружия
        m_Hero2HandedSword = GameObject.FindGameObjectWithTag("twohandedsword");
        m_HeroSword = GameObject.FindGameObjectWithTag("sword");
        m_HeroSword_2 = GameObject.FindGameObjectWithTag("sword_2");
        m_HeroShield = GameObject.FindGameObjectWithTag("shield");

        // определимся со ссылками на слоты инвентория
        for (int i = 0; i < Inventory.numItemSlots; i++)
        {
            itemSlots = GameObject.FindGameObjectsWithTag("itemSlot_player");
        }

        base.Awake();                                               // запустить базовую Awake из класса-родителя
    }

    public override void OnEnable()                                 // что мы делаем, когда герой снова жив (back on again, следующий раунд)
    {
        weaponSetButtonsObject.SetActive(false);                    // этот UI пока в пассив (кнопки сетов оружия)

        zeroZposition = -1.5f;
        zeroYrotation = 0f;
        stockXposition = 2.2f;

        transform.rotation = Quaternion.Euler(0f, 270f, 0f);        // установить начальное вращение
        base.OnEnable();                                            // запустить бозовую OnEnable из класса-родителя
    }

    public void AttackPressed()                       // по нажатию кнопки атаки
    {
        decision = Decision.Attack;
        weaponSetButtonsObject.SetActive(false);         // убираем кнопки сетов оружия, если вдруг до "атака" игрок нажимал "смену оружия"
    }

    public void ChangeWeaponPressed()                       // по нажатию кнопки смены оружия
    {
        weaponSetButtonsObject.SetActive(true);         // вываливаем кнопки сетов оружия

        if (weaponSet == WeaponSet.SwordShield) swordShieldButton.enabled = false;    // не даём выбрать тот же сет
        if (weaponSet == WeaponSet.SwordSword) swordSwordButton.enabled = false;
        if (weaponSet == WeaponSet.TwoHandedSword) twoHandedSwordButton.enabled = false;
    }

    public void RestartPressed()
    {
        SceneManager.LoadScene(0);                          // на перезагрузку сцены
    }

    public override void SetSwordSword()                     // по нажатию кнопки, например, меч-меч. Не override, так как в классе-родителе не virtual (обязателен к переопределению)
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
        weaponSetButtonsObject.SetActive(false);            // убираем кнопки сетов
        swordShieldButton.enabled = true;                   // но делаем все их доступными для следующей смены
        swordSwordButton.enabled = true;
        twoHandedSwordButton.enabled = true;
    }

    public override void SetTwoHandedSword()
    {
        base.SetTwoHandedSword();
        decision = Decision.ChangeTwoHandedSword;
        weaponSetButtonsObject.SetActive(false);    // убираем кнопки сетов
        swordShieldButton.enabled = true;           // но делаем все их доступными для следующей смены
        swordSwordButton.enabled = true;
        twoHandedSwordButton.enabled = true;
    }
}

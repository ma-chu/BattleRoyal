using System;
using System.Collections;
using EF.Sounds;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Общий текст, кнопки ввода, салют в конце
/// </summary>

public class CommonView : MonoBehaviour
{
    [SerializeField] private Canvas playersControlsCanvas; // отключаем именно компонент холста Canvas, чтобы не помечать сам объект-подканвас (элемент родительского канваса) как dirty с перестройкой род. канваса
    
    [Header("Control Buttons")]
    [SerializeField] private GameObject weaponSetButtonsObject;
    [SerializeField] private Button changeWeaponButton;
    [SerializeField] private Button swordSwordButton;
    [SerializeField] private Button swordShieldButton;
    [SerializeField] private Button twoHandedSwordButton;
    [SerializeField] private Button attackButton;

    [SerializeField] private Slider tacticSlider;
    [SerializeField] private Text resultText;                              // текст для вывода "Игра окончена" и прочего
    [SerializeField] private Button restartButton;
    [SerializeField] private GameObject restartButtonGameObject;
    
    [Header("EndGameStuff")]
    [SerializeField] private Animator gameOverAnimator;
    [SerializeField] private GameObject fireExplodePrefab;
    [SerializeField] private float explodesInterval = 1f;
    
    private Decision _decision;
    private float _defencePart;
    
    public Action<TurnInInfo> TurnDataReady;
    
    public Canvas PlayersControlsCanvas => playersControlsCanvas;
    public GameObject WeaponSetButtonsObject => weaponSetButtonsObject;
    public Button SwordSwordButton => swordSwordButton;
    public Button SwordShieldButton => swordShieldButton;
    public Button TwoHandedSwordButton => twoHandedSwordButton;
    public Button RestartButton => restartButton;                                 
    public GameObject RestartButtonGameObject => restartButtonGameObject;
    public Animator GameOverAnimator => gameOverAnimator;

    public string ResultText { set => resultText.text = value; }
    
    private void OnEnable()
    {
        changeWeaponButton.onClick.AddListener(ChangeWeaponPressedHandler);
        attackButton.onClick.AddListener(AttackPressedHandler);
        swordSwordButton.onClick.AddListener(SetSwordSword);
        swordShieldButton.onClick.AddListener(SetSwordShield);
        twoHandedSwordButton.onClick.AddListener(SetTwoHandedSword);
        tacticSlider.onValueChanged.AddListener(SetDefencePart);
    }
    
    private void OnDisable()
    {
        changeWeaponButton.onClick.RemoveListener(ChangeWeaponPressedHandler);
        attackButton.onClick.RemoveListener(AttackPressedHandler);
        swordSwordButton.onClick.RemoveListener(SetSwordSword);
        swordShieldButton.onClick.RemoveListener(SetSwordShield);
        twoHandedSwordButton.onClick.RemoveListener(SetTwoHandedSword);
        tacticSlider.onValueChanged.RemoveListener(SetDefencePart);
    }
    
    private void ChangeWeaponPressedHandler() => weaponSetButtonsObject.SetActive(true);
    
    private void AttackPressedHandler()                      
    {
        _decision = Decision.Attack;
        SendDataToViewModel();
    }

    private void SetSwordSword()
    {
        _decision = Decision.ChangeSwordSword;
        SendDataToViewModel();
    }

    private void SetSwordShield()
    {
        _decision = Decision.ChangeSwordShield;
        SendDataToViewModel();
    }

    private void SetTwoHandedSword()
    {
        _decision = Decision.ChangeTwoHandedSword;
        SendDataToViewModel();
    }
    
    private void SetDefencePart(float value) => _defencePart = value;        
    
    private void SendDataToViewModel()
    {
        var t = new TurnInInfo()
        {
            PlayerDecision = _decision,
            PlayerDefencePart = _defencePart
        };
        
        TurnDataReady?.Invoke(t);
    }
    
    public IEnumerator Salute()
    {
        var explodesWait = new WaitForSeconds(explodesInterval);
        var fireExplodeParticles = Instantiate(fireExplodePrefab).GetComponent<ParticleSystem>();
        var grenadeSound = SoundsContainer.GetAudioClip(SoundTypes.Grenade);
        
        SaluteShot(new Vector3(-1f, 2f, 2.35f));
        yield return explodesWait; 
        SaluteShot(new Vector3(-3f, 2.5f, 2.55f));
        yield return explodesWait;
        SaluteShot(new Vector3(1f, 2.2f, 2.15f));
        
        void SaluteShot(Vector3 position)
        {
            fireExplodeParticles.transform.position = position;
            fireExplodeParticles.Play();
            SoundsManager.Instance.PlaySound(grenadeSound);
        }
    }
}

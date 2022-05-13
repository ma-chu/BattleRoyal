using System;
using System.Collections;
using System.Collections.Generic;
using EF.Sounds;
using UnityEngine;
using UnityEngine.UI;

public class CommonView : MonoBehaviour
{
    public Action<TurnInInfo> TurnDataReady;

    public void SubscribeOnTurnInDataReady(Action<TurnInInfo> onTurnDataReady)    // возможно, не Action<TurnInInfo>, а EventHandler <TurnInInfo> c переописанием события
    {
        TurnDataReady += onTurnDataReady;
    }
    
    [SerializeField] private Text resultText;                                     // текст для вывода "Игра окончена" и прочего
    public string ResultText { get => resultText.text; set => resultText.text = value; }

    [SerializeField] private Button restartButton;
    public Button RestartButton => restartButton;                                 
    [SerializeField] private GameObject restartButtonGameObject;
    public GameObject RestartButtonGameObject => restartButtonGameObject;
    
    // Кнопки управления
    [SerializeField] private GameObject weaponSetButtonsObject;
    public GameObject WeaponSetButtonsObject => weaponSetButtonsObject;
    [SerializeField] private Button swordSwordButton;
    public Button SwordSwordButton => swordSwordButton;
    [SerializeField] private Button swordShieldButton;
    public Button SwordShieldButton => swordShieldButton;
    [SerializeField] private Button twoHandedSwordButton;
    public Button TwoHandedSwordButton => twoHandedSwordButton;
    [SerializeField] private Slider tacticSlider;                                  // слайдер тактики 
    public Slider TacticSlider => tacticSlider;
    [SerializeField] private Canvas playersControlsCanvas;                 // компонент Canvas холста, содержащего в себе кнопки управления игрока
    // отключаем именно компонент холста Canvas, чтобы не помечать сам объект-подканвас (элемент родительского канваса) как dirty с перестройкой род. канваса
    public Canvas PlayersControlsCanvas => playersControlsCanvas;

    // Стафф конца игры
    [SerializeField] private Animator gameOverAnimator;
    public Animator GameOverAnimator => gameOverAnimator;
    [SerializeField] private GameObject fireExplodePrefab;                 // ссылка на объект-салют (префаб, состоящий из particle system (уже без звука)
    [SerializeField] private float explodesInterval = 1f;                  // задержка меж выстрелами в секундах

    private Decision _decision;
    private float _defencePart;
    
    public void SetDefencePart() => _defencePart = tacticSlider.value;        

    public void ChangeWeaponPressed() => weaponSetButtonsObject.SetActive(true);
    
    public void AttackPressed()                      
    {
        _decision = Decision.Attack;
        SendDataToPresenter();
    }

    public void SetSwordSword()
    {
        _decision = Decision.ChangeSwordSword;
        SendDataToPresenter();
    }

    public void SetSwordShield()
    {
        _decision = Decision.ChangeSwordShield;
        SendDataToPresenter();
    }

    public void SetTwoHandedSword()
    {
        _decision = Decision.ChangeTwoHandedSword;
        SendDataToPresenter();
    }
    
    private void SendDataToPresenter()
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
        
        // первый выстрел
        fireExplodeParticles.transform.position = new Vector3(-1f, 2f, 2.35f);
        fireExplodeParticles.Play();
        SoundsManager.Instance.PlaySound(grenadeSound);
        yield return explodesWait; 
        
        // второй выстрел
        fireExplodeParticles.transform.position = new Vector3(-3f, 2.5f, 2.55f);
        fireExplodeParticles.Play(); 
        SoundsManager.Instance.PlaySound(grenadeSound);
        yield return explodesWait;
        
        // третий выстрел
        fireExplodeParticles.transform.position = new Vector3(1f, 2.2f, 2.15f);
        fireExplodeParticles.Play();
        SoundsManager.Instance.PlaySound(grenadeSound);
    }
}

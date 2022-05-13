using EF.Sounds;
using UnityEngine;
//  ОЗВУЧКА ГЕРОЯ
public class HeroAudio : MonoBehaviour      
{
    [SerializeField] private AudioSource m_FirstStrikeHeroAudio; // аудио-источник для первого удара
    [SerializeField] private AudioSource m_MovementHeroAudio;    // аудио-источник для перемещений
   
    [SerializeField] private GameObject m_GetWoundPrefab;        // ссылка на объект получения раны (префаб, состоящий из particle system и звука)
    private AudioSource _woundAudio;
    private ParticleSystem _woundParticles;
    
    private HeroManager _heroManager;

    private void Awake() 
    {
        _heroManager = GetComponent<HeroManager>() as HeroManager;

        _woundParticles = Instantiate(m_GetWoundPrefab).GetComponent<ParticleSystem>();    // порождаем инстанс префаба раны и берем компонент этого инстанса
        _woundAudio = _woundParticles.GetComponent<AudioSource>();                        // берём другой компонент (можно ссылаться на объект по его компоненту)
        // отправляем объект  в пассив... Наверное, так надо делать, если у нас есть компоненты "Play on Awake", чтобы они не отобразились сразу. Сейчас не надобно
        //m_WoundParticles.gameObject.SetActive(false);   
        //цвет материала крови красный
        _woundParticles.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
        _woundParticles.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.red);
    }

    private void OnEnable()                 
    {
        if (_heroManager != null)
        {
            _heroManager.GetHitEvent += OnHit;
            _heroManager.DeathEvent += OnDeath;
            _heroManager.ChangeEvent += OnChange;
            _heroManager.ToPositionEvent += OnToPosition;
            _heroManager.ParryEvent += OnParry;
            _heroManager.BlockVs2HandedEvent += OnBlockVs2Handed;
            _heroManager.BlockEvent += OnBlock;
            _heroManager.EvadeEvent += OnEvade;
        }
    }
    private void OnDisable()    
    {
        if (_heroManager != null)
        {
            _heroManager.GetHitEvent -= OnHit;
            _heroManager.DeathEvent -= OnDeath;
            _heroManager.ChangeEvent -= OnChange;
            _heroManager.ToPositionEvent -= OnToPosition;
            _heroManager.ParryEvent -= OnParry;
            _heroManager.BlockVs2HandedEvent -= OnBlockVs2Handed;
            _heroManager.BlockEvent -= OnBlock;
            _heroManager.EvadeEvent -= OnEvade;
        }
    }

    private void OnHit(int strikeNumber, int gotDamage = 0)
    {
        float delay = 0f;
        if (strikeNumber == 2) delay = 0.3f;
        
        _woundParticles.transform.position = _heroManager.transform.position;   // перемещаем рану на героя
        //m_WoundParticles.gameObject.SetActive(true);                          // активируем
        _woundParticles.Play();                                                // воспроизводим систему частиц
        
        _woundAudio.clip = SoundsContainer.GetAudioClip(SoundTypes.Hurt, _heroManager.heroType);
        _woundAudio.PlayDelayed(delay);                                       // воспроизводим аудио крика боли

    }
    private void OnDeath()
    {
        m_FirstStrikeHeroAudio.clip = SoundsContainer.GetAudioClip(SoundTypes.Death, _heroManager.heroType);
        m_FirstStrikeHeroAudio.PlayDelayed(1.2f);
    }
    private void OnChange()         
    {
        m_MovementHeroAudio.clip = SoundsContainer.GetAudioClip(SoundTypes.Run);  
        m_MovementHeroAudio.Play();
    }
    private void OnToPosition()
    {
        m_MovementHeroAudio.clip = SoundsContainer.GetAudioClip(SoundTypes.Step);
        m_MovementHeroAudio.PlayDelayed(0.7f);
    }
    private void OnParry(int strikeNumber)
    {
        float delay = 0f;
        if (strikeNumber == 2) delay = 0.3f;
        m_FirstStrikeHeroAudio.clip = SoundsContainer.GetAudioClip(SoundTypes.Parry);
        m_FirstStrikeHeroAudio.PlayDelayed(delay);
    }
    private void OnBlockVs2Handed(int gotDamage = 0)
    {
        m_FirstStrikeHeroAudio.clip = SoundsContainer.GetAudioClip(SoundTypes.TwoVsShield);
        m_FirstStrikeHeroAudio.Play();
    }
    private void OnBlock(int strikeNumber)
    {
        float delay = 0f;
        if (strikeNumber == 2) delay = 0.3f;
        m_FirstStrikeHeroAudio.clip = SoundsContainer.GetAudioClip(SoundTypes.Block);
        m_FirstStrikeHeroAudio.PlayDelayed(delay);
    }
    private void OnEvade(int strikeNumber)
    {
        float delay = 0f;
        if (strikeNumber == 2) delay = 0.3f;
        m_FirstStrikeHeroAudio.clip = SoundsContainer.GetAudioClip(SoundTypes.Evade);
        m_FirstStrikeHeroAudio.PlayDelayed(delay);
    }
}

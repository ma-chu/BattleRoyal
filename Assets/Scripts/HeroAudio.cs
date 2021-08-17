using EF.Tools;
using EF.Sounds;
using UnityEngine;
//  ОЗВУЧКА ГЕРОЯ
public class HeroAudio : MonoBehaviour      
{
    // ССЫЛКИ НА ЗВУКИ
    /*
    [SerializeField]
    private AudioClip m_audioClipParry;         // Парирование               
    [SerializeField]
    private AudioClip m_audioClipThrough;       // Пробитие блока цвайхандером
    [SerializeField]
    private AudioClip m_audioClipBlock;         // Блок
    [SerializeField]
    private AudioClip m_audioClipEvade;         // Уворот на смене
    [SerializeField]
    private AudioClip m_audioClipDeath;         // Смерть
    [SerializeField]
    private AudioClip m_audioClipBonus;         // Бонус набран
    [SerializeField]
    private AudioClip m_audioClipStep;          // Шаг
    [SerializeField]
    private AudioClip m_audioClipRun;           // Бег
*/
    [SerializeField] private AudioSource m_FirstStrikeHeroAudio; // аудио-источник для первого удара
    [SerializeField] private AudioSource m_MovementHeroAudio;    // аудио-источник для перемещений
   
    [SerializeField] private GameObject m_GetWoundPrefab;        // ссылка на объект получения раны (префаб, состоящий из particle system и звука)
    private AudioSource m_WoundAudio;
    private ParticleSystem m_WoundParticles;
    
    private HeroManager heroManager;

    private void Awake() 
    {
        //heroManager = GetComponent<PlayerManager>();
        //if (heroManager.IsNull()) heroManager = GetComponent<EnemyManager>();
        heroManager = GetComponent("HeroManager") as HeroManager;

        m_WoundParticles = Instantiate(m_GetWoundPrefab).GetComponent<ParticleSystem>();    // порождаем инстанс префаба раны и берем компонент этого инстанса
        m_WoundAudio = m_WoundParticles.GetComponent<AudioSource>();                        // берём другой компонент (можно ссылаться на объект по его компоненту)
        // отправляем объект  в пассив... Наверное, так надо делать, если у нас есть компоненты "Play on Awake", чтобы они не отобразились сразу. Сейчас не надобно
        //m_WoundParticles.gameObject.SetActive(false);   
        //цвет материала крови красный
        m_WoundParticles.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
        m_WoundParticles.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.red);
    }

    private void OnEnable()                 
    {
        if (heroManager != null)
        {
            heroManager.GetHitEvent += OnHit;
            heroManager.DeathEvent += OnDeath;
            heroManager.ChangeEvent += OnChange;
            heroManager.ToPositionEvent += OnToPosition;
            heroManager.ParryEvent += OnParry;
            heroManager.BlockVs2HandedEvent += OnBlockVs2Handed;
            heroManager.BlockEvent += OnBlock;
            heroManager.EvadeEvent += OnEvade;
        }
    }
    private void OnDisable()    
    {
        if (heroManager != null)
        {
            heroManager.GetHitEvent -= OnHit;
            heroManager.DeathEvent -= OnDeath;
            heroManager.ChangeEvent -= OnChange;
            heroManager.ToPositionEvent -= OnToPosition;
            heroManager.ParryEvent -= OnParry;
            heroManager.BlockVs2HandedEvent -= OnBlockVs2Handed;
            heroManager.BlockEvent -= OnBlock;
            heroManager.EvadeEvent -= OnEvade;
        }
    }

    private void OnHit(int strikeNumber)
    {
        float delay = 0f;
        if (strikeNumber == 2) delay = 0.3f;
        
        m_WoundParticles.transform.position = heroManager.transform.position;   // перемещаем рану на героя
        //m_WoundParticles.gameObject.SetActive(true);                          // активируем
        m_WoundParticles.Play();                                                // воспроизводим систему частиц
        
        m_WoundAudio.clip = /*m_audioClip*/SoundsContainer.GetAudioClip(SoundTypes.Hurt, heroManager.heroType);
        m_WoundAudio.PlayDelayed(delay);                                       // воспроизводим аудио крика боли

    }
    private void OnDeath()
    {
        m_FirstStrikeHeroAudio.clip = /*m_audioClipDeath*/SoundsContainer.GetAudioClip(SoundTypes.Death, heroManager.heroType);
        m_FirstStrikeHeroAudio.PlayDelayed(1.2f);
    }
    private void OnChange()         
    {
        m_MovementHeroAudio.clip = /*m_audioClipRun*/SoundsContainer.GetAudioClip(SoundTypes.Run);  
        m_MovementHeroAudio.Play();
    }
    private void OnToPosition()
    {
        m_MovementHeroAudio.clip = /*m_audioClipStep*/SoundsContainer.GetAudioClip(SoundTypes.Step);
        m_MovementHeroAudio.PlayDelayed(0.7f);
    }
    private void OnParry(int strikeNumber)
    {
        float delay = 0f;
        if (strikeNumber == 2) delay = 0.3f;
        m_FirstStrikeHeroAudio.clip = /*m_audioClipParry*/SoundsContainer.GetAudioClip(SoundTypes.Parry);
        m_FirstStrikeHeroAudio.PlayDelayed(delay);
    }
    private void OnBlockVs2Handed()
    {
        m_FirstStrikeHeroAudio.clip = /*m_audioClipThrough*/SoundsContainer.GetAudioClip(SoundTypes.TwoVsShield);
        m_FirstStrikeHeroAudio.Play();
    }
    private void OnBlock(int strikeNumber)
    {
        float delay = 0f;
        if (strikeNumber == 2) delay = 0.3f;
        m_FirstStrikeHeroAudio.clip = /*m_audioClipBlock*/SoundsContainer.GetAudioClip(SoundTypes.Block);
        m_FirstStrikeHeroAudio.PlayDelayed(delay);
    }
    private void OnEvade(int strikeNumber)
    {
        float delay = 0f;
        if (strikeNumber == 2) delay = 0.3f;
        m_FirstStrikeHeroAudio.clip = /*m_audioClipEvade*/SoundsContainer.GetAudioClip(SoundTypes.Evade);
        m_FirstStrikeHeroAudio.PlayDelayed(delay);
    }
}

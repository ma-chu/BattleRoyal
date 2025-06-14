using EF.Sounds;
using UnityEngine;

/// <summary>
/// Озвучка героя (и Pacticles)
/// </summary>
 
public class HeroAudio : MonoBehaviour
{
    [SerializeField] private AudioSource battleHeroAudio;
    [SerializeField] private AudioSource movementHeroAudio;
    [SerializeField] private GameObject getWoundPrefab;

    private const float HitDelay = 0.3f;
    private const float DeathDelay = 1.2f;
    private const float ToPositionDelay = 0.7f;

    private AudioSource _woundAudio;
    private ParticleSystem _woundParticles;
    private HeroViewManager _heroViewManager;
    private bool _isInitialized;

    public void Initialize(HeroViewManager heroViewManager)
    {
        _heroViewManager = heroViewManager;
        SubscribeEvents();
        _isInitialized = true;
    }
    
    private void Awake() 
    {
        _woundParticles = Instantiate(getWoundPrefab).GetComponent<ParticleSystem>();
        _woundAudio = _woundParticles.GetComponent<AudioSource>();

        var woundRendererMaterial = _woundParticles.GetComponent<Renderer>().material;
        woundRendererMaterial.EnableKeyword("_EMISSION");
        woundRendererMaterial.SetColor("_EmissionColor", Color.red);
    }
    
    private void OnDisable() => UnsubscribeEvents();

    private void SubscribeEvents()
    {
        _heroViewManager.GetHitEvent += OnHit;
        _heroViewManager.DeathEvent += OnDeath;
        _heroViewManager.ChangeEvent += OnChange;
        _heroViewManager.ToPositionEvent += OnToPosition;
        _heroViewManager.ParryEvent += OnParry;
        _heroViewManager.BlockVs2HandedEvent += OnBlockVs2Handed;
        _heroViewManager.BlockEvent += OnBlock;
        _heroViewManager.EvadeEvent += OnEvade;
    }
    
    private void UnsubscribeEvents()
    {
        if (!_isInitialized)
            return;

        _heroViewManager.GetHitEvent -= OnHit;
        _heroViewManager.DeathEvent -= OnDeath;
        _heroViewManager.ChangeEvent -= OnChange;
        _heroViewManager.ToPositionEvent -= OnToPosition;
        _heroViewManager.ParryEvent -= OnParry;
        _heroViewManager.BlockVs2HandedEvent -= OnBlockVs2Handed;
        _heroViewManager.BlockEvent -= OnBlock;
        _heroViewManager.EvadeEvent -= OnEvade;
    }

    private void OnHit(int strikeNumber, int gotDamage = 0)
    {
        _woundParticles.transform.position = _heroViewManager.transform.position;
        _woundParticles.Play();
        
        var delay = strikeNumber == 2 ? HitDelay : 0f;
        _woundAudio.clip = SoundsContainer.GetAudioClip(SoundTypes.Hurt, _heroViewManager.HeroType);
        _woundAudio.PlayDelayed(delay);

    }
    
    private void OnDeath()
    {
        battleHeroAudio.clip = SoundsContainer.GetAudioClip(SoundTypes.Death, _heroViewManager.HeroType);
        battleHeroAudio.PlayDelayed(DeathDelay);
    }
    
    private void OnChange()         
    {
        movementHeroAudio.clip = SoundsContainer.GetAudioClip(SoundTypes.Run);  
        movementHeroAudio.Play();
    }
    
    private void OnToPosition()
    {
        movementHeroAudio.clip = SoundsContainer.GetAudioClip(SoundTypes.Step);
        movementHeroAudio.PlayDelayed(ToPositionDelay);
    }
    
    private void OnParry(int strikeNumber)
    {
        var delay = strikeNumber == 2 ? HitDelay : 0f;
        battleHeroAudio.clip = SoundsContainer.GetAudioClip(SoundTypes.Parry);
        battleHeroAudio.PlayDelayed(delay);
    }
    
    private void OnBlockVs2Handed(int gotDamage = 0)
    {
        battleHeroAudio.clip = SoundsContainer.GetAudioClip(SoundTypes.TwoVsShield);
        battleHeroAudio.Play();
    }
    
    private void OnBlock(int strikeNumber)
    {
        var delay = strikeNumber == 2 ? HitDelay : 0f;
        battleHeroAudio.clip = SoundsContainer.GetAudioClip(SoundTypes.Block);
        battleHeroAudio.PlayDelayed(delay);
    }
    
    private void OnEvade(int strikeNumber)
    {
        var delay = strikeNumber == 2 ? HitDelay : 0f;
        battleHeroAudio.clip = SoundsContainer.GetAudioClip(SoundTypes.Evade);
        battleHeroAudio.PlayDelayed(delay);
    }
}
using EF.Tools;
using UnityEngine;
using EF.UI;
using UnityEngine.Audio;

// только для общих звуков
namespace EF.Sounds
{
    public class SoundsManager : MonoBehaviour
    {
        private static SoundsManager _instance;
        public static SoundsManager Instance => _instance;
        
        [SerializeField] private AudioSource musicAudioSource;
        [SerializeField] private AudioSource globalSFXAudioSource;
        
        private SaveSnapshot _snapshot;
        
        public AudioMixer masterMixer;   

        private void Awake()
        {
            _instance = this;
        }

        private void OnEnable()
        {
            EFButton.ClickSound += OnAction;
        }

        private void Start()
        {
            _snapshot = GameSave.LastLoadedSnapshot ?? GameSave.Load();
            UpdateAudioSettings();
            PlayMusic(SoundsContainer.GetAudioClip(SoundTypes.BackgroundMusic));
        }

        private void OnDisable()
        {
            EFButton.ClickSound -= OnAction;
        }

        public void PlaySound(AudioClip clip, float delay = 0f)
        {
            if (globalSFXAudioSource.mute ||
                globalSFXAudioSource.IsNull() ||
                globalSFXAudioSource.enabled == false)
                return;
            
            globalSFXAudioSource.PlayOneShot(clip);
        }
        
        public void PlayMusic(AudioClip clip, bool loop = true, float fadeLengths = 0f)
        {
            if (musicAudioSource.mute ||
                musicAudioSource.IsNull() ||
                musicAudioSource.enabled == false)
                return;
            
            musicAudioSource.clip = clip;
            musicAudioSource.loop = loop;
            musicAudioSource.time = 0f;
            musicAudioSource.Play();
        }

        public void StopMusic()
        {
            musicAudioSource.Stop();
        }

        private void UpdateAudioSettings()
        {
            musicAudioSource.mute = _snapshot.SFXLvl <= -80f;
            globalSFXAudioSource.mute = _snapshot.musicLvl <= -80f;
            
            masterMixer.SetFloat("sfxVolume", _snapshot.SFXLvl);
            masterMixer.SetFloat("moveVolume", _snapshot.SFXLvl - 12f);
            masterMixer.SetFloat ("musicVolume", _snapshot.musicLvl);
        }

        private void OnAction(SoundTypes type)  
        {
            if (type == SoundTypes.None) return;

            PlaySound(SoundsContainer.GetAudioClip(type));
        }
    }
}
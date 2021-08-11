using System;
using EF.Tools;
using UnityEngine;
using EF.UI;
// В сцене Main пока не прикручивал
namespace EF.Sounds
{
    public class SoundsManager : MonoBehaviour
    {
        private static SoundsManager _instance;
        public static SoundsManager Instance => _instance;

        [SerializeField] private AudioSource SFXAudioSourceSound/*, _audioSourceMusic*/;
        
        private void Awake()
        {
            _instance = this;
        }

        private void OnEnable()
        {
            UpdateSettings();

            //PlayerSettings.SoundSettingsChanged += UpdateSettings;
            EFButton.ClickSound += OnAction;
        }
        private void OnDisable()
        {
            //PlayerSettings.SoundSettingsChanged -= UpdateSettings;
            EFButton.ClickSound -= OnAction;
        }

        public void PlaySound(AudioClip clip)
        {
            if (/*PlayerSettings.Instance.IsMuteSound ||*/
                SFXAudioSourceSound.IsNull() ||
                SFXAudioSourceSound.enabled == false)
                return;
            
            SFXAudioSourceSound.PlayOneShot(clip);
            //Debug.Log("CLICK!! + clipName = " + clip.name);
        }
        
        /*public void PlayMusic(AudioClip clip, bool loop = true, float fadeLengths = 0f)
        {
            _audioSourceMusic.clip = clip;
            _audioSourceMusic.time = 0f;
            _audioSourceMusic.Play();
            
            if(clip == null) return;
            _audioSourceMusic.loop = loop;
            _audioSourceMusic.clip = clip;
            _audioSourceMusic.Play();

            if (fadeLengths > 0)
            {
                var duration = clip.length - fadeLengths;
                var a = 0f;
                DOTween.To(() => a, value => a = value, 1f, duration).onComplete =
                    () =>
                    {
                        FadeMusic(fadeLengths);
                    };
            }
        }
        
        public void FadeMusic(float delay, Action onComplete = null)
        {
            DOTween.To(() => _audioSourceMusic.volume, value => _audioSourceMusic.volume = value, 0f, delay).onComplete =
                () =>
                {
                    onComplete?.Invoke();
                };
        }

        public void StopMusic()
        {
            _audioSourceMusic.Stop();
        }*/

        private void UpdateSettings()
        {
            //_audioSourceMusic.mute = PlayerSettings.Instance.IsMuteMusic;
            //_audioSourceSound.mute = PlayerSettings.Instance.IsMuteSound;
        }

        private void OnAction(SoundTypes type)
        {
            if (type == SoundTypes.None) return;
            PlaySound(SoundsContainer.GetAudioClip(type));
        }
    }
}
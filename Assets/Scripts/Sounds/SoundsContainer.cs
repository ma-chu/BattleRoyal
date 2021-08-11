using System;
using System.Collections.Generic;
using UnityEngine;

namespace EF.Sounds
{
    public enum SoundTypes 
    {
        ButtonClick,
        None
    }
    
    [Serializable]
    public class SoundTypePair
    {
        public SoundTypes Key;
        public AudioClip Value;
        
        public SoundTypePair(SoundTypes key, AudioClip value)
        {
            Key = key;
            Value = value;
        }
    }
    
    [CreateAssetMenu(fileName = "SoundsContainer", menuName = "_EF/Sounds container", order = 1)]
    public class SoundsContainer : ScriptableObject
    {
        [SerializeField] private List<SoundTypePair> _sounds;

        private static SoundsContainer _instance;
        
        public static SoundsContainer Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = Resources.Load<SoundsContainer>("SoundsContainer");
                return _instance;
            }
        }

        public static AudioClip GetAudioClip(SoundTypes soundType)
        {
            return Instance._sounds.Find(m => m.Key == soundType)?.Value;
        }
    }
}

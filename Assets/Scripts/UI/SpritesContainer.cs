using System.Collections.Generic;
using UnityEngine;
using System;
using EF.Tools;


namespace EF.UI
{
    public enum SpriteTypes 
    {
        ButtonImage,
        None
    }
    
    [Serializable]
    public class SpriteTypesPair
    {
        public SpriteTypes Key;
        public Sprite Value;
        
        public SpriteTypesPair(SpriteTypes key, Sprite value)
        {
            Key = key;
            Value = value;
        }
    }
    
    [CreateAssetMenu(fileName = "SpritesContainer", menuName = "_EF/Sprites container", order = 1)]
    public class SpritesContainer : ScriptableObject
    {
        [SerializeField] private List<SpriteTypesPair> _sprites;

        private static SpritesContainer _instance;
        
        public static SpritesContainer Instance
        {
            get
            {
                if (!_instance.IsNull()) return _instance;
                _instance = Resources.Load<SpritesContainer>("SpritesContainer");
                return _instance;
            }
        }

        public static Sprite GetSprite(SpriteTypes spriteType)
        {
            return Instance._sprites.Find(m => m.Key == spriteType)?.Value;
        }
    }
}

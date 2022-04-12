using EF.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EF.Localization
{
    public class AutoLocalization : MonoBehaviour
    {
        //[SerializeField, HideInInspector] private Text/*MeshProUGUI*/ _text;
        
        private TextMeshProUGUI _TMPtext;
        private Text _text;
        protected virtual string Text
        {
            get =>
                _TMPtext != null  ? _TMPtext.text :
                _text != null ? _text.text : null;

            set
            {
                if (_text != null) _text.text = value;
                if (_TMPtext != null) _TMPtext.text = value;
            }
        }
        
        [SerializeField] private string _stringToken;

        public void UpdateLocalization()    // вызывается менеджером для всех button из массива _buttons и всех autoLocalizations из массива _autoLocalizations
        {
            if (Text.IsNull()) return;
            Text = _stringToken.Localize();
        } 
        
        private void Awake()
        { 
            _text = GetComponentInChildren<Text>();
            _TMPtext = GetComponentInChildren<TextMeshProUGUI>();
        }
    }
}
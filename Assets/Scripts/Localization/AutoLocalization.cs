using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EF.Localization
{
 //   [ExecuteInEditMode]
    public class AutoLocalization : /*BaseBehaviour*/MonoBehaviour
    {
        [SerializeField, HideInInspector] private Text/*MeshProUGUI*/ _text;
        [SerializeField] private string _stringToken;

        public void UpdateLocalization()    // в исходнике вызывается для всех BaseButton из массива _allButtons и всех autoLocalizations из массива _autoLocalizations
        {
            //Debug.Log("+" + this.name);
            /*if (_text == null)*/
            {
                _text = GetComponentInChildren<Text/*MeshProUGUI*/>();
                if (_text == null) return;
            }

            _text.text = _stringToken.Localize();
            //Debug.Log(_text.text);
        }
        

#if UNITY_EDITOR
        private void Update()
        {
            if (Application.isPlaying || _text != null) return;
            _text = GetComponentInChildren<Text/*MeshProUGUI*/>();
        }
#endif
    }
}
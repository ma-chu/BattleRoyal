using EF.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EF.Localization
{
    public class AutoLocalization : MonoBehaviour
    {
        [SerializeField, HideInInspector] private Text/*MeshProUGUI*/ _text;
        [SerializeField] private string _stringToken;

        public void UpdateLocalization()    // вызывается для всех button из массива _buttons и всех autoLocalizations из массива _autoLocalizations
        {
            {
                _text = GetComponentInChildren<Text/*MeshProUGUI*/>();
                if (_text.IsNull()) return;
            }

            _text.text = _stringToken.Localize();
            //Debug.Log(_text.text);
        }       
    }
}
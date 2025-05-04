using System;
using DG.Tweening;
using EF.Localization;
using EF.Tools;
using EF.Sounds;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace EF.UI
{
    public class EFButton : EFBaseUI
    {
        public static event Action<SoundTypes> ClickSound;

        [SerializeField] private Button _button;
        [SerializeField] private Image _image;
        [SerializeField] private TextMeshProUGUI _TMPtext;
        [SerializeField] private Text _text;
        [SerializeField] private string _localizationToken;
        [SerializeField] private SoundTypes _soundType;

        protected virtual string Text
        {
            get =>
                _TMPtext != null ? _TMPtext.text :
                _text != null ? _text.text : null;

            set
            {
                if (_text != null) _text.text = value;
                if (_TMPtext != null) _TMPtext.text = value;
            }
        }

        public bool Interactable
        {
            get => _button.interactable;
            set => _button.interactable = value;
        }

        private void Awake()
        {
            VerifyLocalizationToken();

            if (_image == null) _image = GetComponent<Image>();
            if (_button == null) _button = GetComponent<Button>();
            if (_text == null) _text = GetComponentInChildren<Text>();
            if (_TMPtext == null) _TMPtext = GetComponentInChildren<TextMeshProUGUI>();
        }

        protected void OnEnable()
        {
            if (!_button.IsNull())
            {
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(OnClick);
            }
        }

        protected virtual void OnClick()
        {
            if (!Interactable)
                return;

            ClickSound?.Invoke(_soundType);
            DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
            AnimateScale(1.3f, 0.5f);
            AnimateRotation(1.5f, .25f);
            AnimatePosition(RectTrans.anchoredPosition + new Vector2(15, 0), 0.5f,
                () => { ; });
        }

        private void VerifyLocalizationToken()
        {
            if (!_localizationToken.IsNullOrEmpty() || Text.IsNull())
                return;

            _localizationToken = Text.ToLower();
            foreach (var smb in new[] { " ", ",", ":" })
            {
                if (!_localizationToken.Contains(smb)) continue;
                _localizationToken = "";
                break;
            }
        }

        public void UpdateLocalization()
        {
            if (_localizationToken.IsNullOrEmpty()) return;
            Text = _localizationToken.Localize();
        }
    }
}

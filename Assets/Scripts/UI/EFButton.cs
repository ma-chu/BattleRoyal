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
        [SerializeField] private Button _button;
        [SerializeField] private Image _image;
        [SerializeField] private TextMeshProUGUI _TMPtext;
        [SerializeField] private Text _text;
        [SerializeField] private string _localizationToken;
        [SerializeField] private SoundTypes _soundType;

        private const float TwinScaleDuration = 0.5f;
        private const float TwinPositionDuration = 0.5f;
        private const float TwinRotationDuration = 0.25f;
        private const float TwinScaleFactor = 1.3f;
        private const float TwinRotationFactor = 1.5f;
        
        private Vector2 _twinPositionOffset;
        
        public static event Action<SoundTypes> ClickSound;

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

            if (!_image)
                _image = GetComponent<Image>();
            
            if (!_button)
                _button = GetComponent<Button>();
            
            if (!_text)
                _text = GetComponentInChildren<Text>();
            
            if (!_TMPtext)
                _TMPtext = GetComponentInChildren<TextMeshProUGUI>();

            _twinPositionOffset = new Vector2(15f, 0f);
            DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
        }

        protected void OnEnable()
        {
            if (_button.IsNull()) 
                return;
            
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnClick);
        }

        protected virtual void OnClick()
        {
            if (!Interactable)
                return;

            ClickSound?.Invoke(_soundType);
            AnimateScale(TwinScaleFactor, TwinScaleDuration);
            AnimateRotation(TwinRotationFactor, TwinRotationDuration);
            AnimatePosition(RectTrans.anchoredPosition + _twinPositionOffset, TwinPositionDuration);
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

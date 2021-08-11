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
    public Action Listener { get; set; }                    // конкретно для этой кнопки (пока что дополнительно к листенеру unity)
    public static event Action<SoundTypes> ClickSound;      // звук для всех кнопок
    //public static event Action<EFButton> TutorialAction;  // еще что-нибудь для всех кнопок, но с передачей своего инстанса, например, можно добавить туториал
    
    // Все ссылки в одном месте - ?и весь профит?
    // если сразу ссылки не задашь, это сделает awake()
    [SerializeField] private Button _button;
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _TMPtext;
    [SerializeField] private Text _text;
    [SerializeField] private string _localizationToken;
    [SerializeField] private SoundTypes _soundType;
    [SerializeField] private SpriteTypes _spriteType;
  
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

    protected Sprite Sprite
    {
        get => _image.sprite;
        set => _image.sprite = value;
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

        Sprite = SpritesContainer.GetSprite(_spriteType);
    }
        
    public void SetListener(Action listener)
    {
        Listener = listener;
    }

    protected virtual void OnClick()
    {
        if (!Interactable) return;
        
        ClickSound?.Invoke(_soundType);
        Listener?.Invoke();
        
        DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
        AnimateScale(1.5f, 0.5f);
        AnimateRotation(5f, .25f);
        AnimatePosition(RectTrans.anchoredPosition + new Vector2(20,0), 0.5f,  ()=>Debug.Log("Position Animated!"));
    }

    private void VerifyLocalizationToken()
    {
        if (!_localizationToken.IsNullOrEmpty() || Text.IsNull()) return;
			
        _localizationToken = Text.ToLower();

        foreach (var smb in new[] {" ", ":", ","})
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
    
    /*
        public void SetTextColor(Color color)
        {
            _TMPtext.color = color;
        }
        
        public void SetTextFont(TMP_FontAsset font)
        {
            _TMPtext.font = font;
        }
        
        public void SetImage(Image image)
        {
            _image = image;
        }
        
        public void SetImageColor(Color color)
        {
            _image.color = color;
        }
        
        public void SetSprite(Sprite image)
        {
            _image.sprite = image;
        }
*/
}
}

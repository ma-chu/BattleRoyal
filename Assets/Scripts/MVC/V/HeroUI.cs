using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EF.Localization;

/// <summary>
/// Пока здесь только тексты для вывода урона
/// </summary>

public class HeroUI : MonoBehaviour
{
    [SerializeField] private Text nameText;
    [SerializeField] private Text getHit1Text;
    [SerializeField] private Text getHit2Text;
    
    private HeroViewManager _heroViewManager;
    private readonly List<bool> _isRegen = new List<bool>();
    private readonly List<float> _regenValues = new List<float>();
    private bool _isInitialized;

    public string Name
    {
        get => nameText.text;
        set => nameText.text = value;
    }

    public void Initialize(HeroViewManager heroViewManager)
    {
        _heroViewManager = heroViewManager;
        SubscribeEvents();
        ResetHitTexts();
        _isInitialized = true;
    }

    private void OnDisable() => UnsubscribeEvents();
    
    private void SubscribeEvents()
    {
        _heroViewManager.ExchangeEndedEvent += OnExchangeEnded;
        _heroViewManager.GetHitEvent += OnHit;
        _heroViewManager.ParryEvent += OnParry;
        _heroViewManager.BlockVs2HandedEvent += OnBlockVs2Handed;
        _heroViewManager.BlockEvent += OnBlock;
        _heroViewManager.EvadeEvent += OnEvade;
    }
    
    private void UnsubscribeEvents()
    {
        if (!_isInitialized)
            return;
        
        _heroViewManager.ExchangeEndedEvent -= OnExchangeEnded;
        _heroViewManager.GetHitEvent -= OnHit;
        _heroViewManager.ParryEvent -= OnParry;
        _heroViewManager.BlockVs2HandedEvent -= OnBlockVs2Handed;
        _heroViewManager.BlockEvent -= OnBlock;
        _heroViewManager.EvadeEvent -= OnEvade;
    }
    
    public void SetRegenValues(int blocksNum)
    {
        _isRegen.Clear();
        _regenValues.Clear();

        var really = blocksNum - Series.SeriesBlockBeginning;
        _isRegen.Add(really > 0);
        _regenValues.Add(really * Series.SeriesBlockStepValue);

        really--;
        _isRegen.Add(really > 0);
        if (_isRegen[1]) _regenValues.Insert(0,really * Series.SeriesBlockStepValue);
    }

    private void OnExchangeEnded() => ResetHitTexts();
    
    private void ResetHitTexts()
    { 
        getHit1Text.text = string.Empty;
        getHit2Text.text = string.Empty;
    } 

    private void OnHit(int strikeNumber, int gotDamage)
    {
        switch (strikeNumber)
        {
            case 1:
                getHit1Text.text = "-" + gotDamage;  // Если не использовать метод примитива .ToString(), а просто передать concat-у heroManager.damage1, будет производиться его упаковка, что менее эффективно
                break;
            case 2:
                getHit2Text.text = "-" + gotDamage;
                break;
        }
    }

    private void OnParry(int strikeNumber)
    {
        switch (strikeNumber)                
        {
            case 1:
                getHit1Text.text = "parried".Localize();
                if (_isRegen[0]) 
                    getHit1Text.text = getHit1Text.text + " +" + _regenValues[0];
                break;
            case 2:
                getHit2Text.text = "parried".Localize();
                if (_isRegen[1])
                {
                    getHit2Text.text = getHit2Text.text + " +" + _regenValues[0];
                    getHit1Text.text = "parried".Localize() + " +" + _regenValues[1];
                }
                break;
        }
    }

    private void OnBlockVs2Handed(int gotDamage) => getHit1Text.text = "shield".Localize() + gotDamage;
    
    private void OnBlock(int strikeNumber)
    {
        switch (strikeNumber)
        {
            case 1:
                getHit1Text.text = "blocked".Localize();
                if (_isRegen[0]) getHit1Text.text = getHit1Text.text + " +" + _regenValues[0];
                break;
            case 2:
                getHit2Text.text = "blocked".Localize();
                if (_isRegen[1])
                {
                    getHit2Text.text = getHit2Text.text + " +" + _regenValues[0];
                    getHit1Text.text = "blocked".Localize() + " +" + _regenValues[1];
                }
                break;
        }
    }

    private void OnEvade(int strikeNumber)
    {
        switch (strikeNumber)
        {
            case 1:
                getHit1Text.text = "evaded".Localize();
                break;
            case 2:
                getHit2Text.text = "evaded".Localize();
                break;
        }
    }
}

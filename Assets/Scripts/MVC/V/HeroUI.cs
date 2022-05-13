using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EF.Localization; 

// Пока здесь только тексты для вывода урона
public class HeroUI : MonoBehaviour
{
    [SerializeField] private new Text name;     
    public string Name { get => name.text; set => name.text = value; }
    
    [SerializeField] private Text getHit1Text;
    [SerializeField] private Text getHit2Text;

    private List<bool> isRegen = new List<bool>();
    private List<float> regenValues = new List<float>();
    public void SetRegenValues(int blocksNum)
    {
        isRegen.Clear();
        regenValues.Clear();

        var really = blocksNum - Series.SeriesBlockBeginning;
        isRegen.Add(really > 0);
        regenValues.Add(really * Series.SeriesBlockStepValue);

        really--;
        isRegen.Add(really > 0);
        if (isRegen[1]) regenValues.Insert(0,really * Series.SeriesBlockStepValue);
    }
    
    private HeroManager _heroManager;

    private void Awake()
    {
        _heroManager = GetComponent<HeroManager>() /*as HeroManager*/;
    }

    private void OnEnable()
    {
        if (_heroManager != null)
        {
            _heroManager.ExchangeEndedEvent += OnExchangeEnded;
            _heroManager.GetHitEvent += OnHit;
            _heroManager.ParryEvent += OnParry;
            _heroManager.BlockVs2HandedEvent += OnBlockVs2Handed;
            _heroManager.BlockEvent += OnBlock;
            _heroManager.EvadeEvent += OnEvade;
        }
        getHit1Text.text = string.Empty;
        getHit2Text.text = string.Empty;
    }
    private void OnDisable()
    {
        if (_heroManager != null)
        {
            _heroManager.ExchangeEndedEvent -= OnExchangeEnded;
            _heroManager.GetHitEvent -= OnHit;
            _heroManager.ParryEvent -= OnParry;
            _heroManager.BlockVs2HandedEvent -= OnBlockVs2Handed;
            _heroManager.BlockEvent -= OnBlock;
            _heroManager.EvadeEvent -= OnEvade;
        }
    }

    private void OnExchangeEnded()
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
                if (isRegen[0]) getHit1Text.text = getHit1Text.text + " +" + regenValues[0];
                break;
            case 2:
                getHit2Text.text = "parried".Localize();
                if (isRegen[1])
                {
                    getHit2Text.text = getHit2Text.text + " +" + regenValues[0];
                    getHit1Text.text = "parried".Localize() + " +" + regenValues[1];
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
                if (isRegen[0]) getHit1Text.text = getHit1Text.text + " +" + regenValues[0];
                break;
            case 2:
                getHit2Text.text = "blocked".Localize();
                if (isRegen[1])
                {
                    getHit2Text.text = getHit2Text.text + " +" + regenValues[0];
                    getHit1Text.text = "blocked".Localize() + " +" + regenValues[1];
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

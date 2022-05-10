using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EF.Localization; 

// Пока здесь только тексты для вывода урона
public class HeroUI : MonoBehaviour
{
    [SerializeField] private new Text name;     
    public string Name { get => name.text; set => name.text = value; }


    // тексты для вывода полученного урона
    public Text m_GetHit1Text;
    public Text m_GetHit2Text;

    private HeroManager heroManager;
    //private Series series;

    private void Awake()
    {
        heroManager = GetComponent<HeroManager>() /*as HeroManager*/;
        //series = GetComponent<Series>() /*as Series*/;
    }

    private void OnEnable()
    {
        if (heroManager != null)
        {
            heroManager.ExchangeEndedEvent += OnExchangeEnded;
            heroManager.GetHitEvent += OnHit;
            heroManager.ParryEvent += OnParry;
            heroManager.BlockVs2HandedEvent += OnBlockVs2Handed;
            heroManager.BlockEvent += OnBlock;
            heroManager.EvadeEvent += OnEvade;
        }
        m_GetHit1Text.text = string.Empty;
        m_GetHit2Text.text = string.Empty;
    }
    private void OnDisable()
    {
        if (heroManager != null)
        {
            heroManager.ExchangeEndedEvent -= OnExchangeEnded;
            heroManager.GetHitEvent -= OnHit;
            heroManager.ParryEvent -= OnParry;
            heroManager.BlockVs2HandedEvent -= OnBlockVs2Handed;
            heroManager.BlockEvent -= OnBlock;
            heroManager.EvadeEvent -= OnEvade;
        }
    }

    private void OnExchangeEnded()
    {
        m_GetHit1Text.text = string.Empty;
        m_GetHit2Text.text = string.Empty;
    }

    private void OnHit(int strikeNumber, int gotDamage)
    {
        switch (strikeNumber)
        {
            case 1:
                m_GetHit1Text.text = "-" + gotDamage;  // Если не использовать метод примитива .ToString(), а просто передать concat-у heroManager.damage1, будет производиться его упаковка, что менее эффективно
                break;
            case 2:
                m_GetHit2Text.text = "-" + gotDamage;
                break;
        }
        //series.ResetSeriesOfBlocks();
    }

    private void OnParry(int strikeNumber)
    {
        switch (strikeNumber)                
        {
            case 1:
                m_GetHit1Text.text = "parried".Localize();
                //series.AddSeriesOfBlocks(m_GetHit1Text);           // проверить, достигнута ли серия, и начислить здоровья за серию блоков
                break;
            case 2:
                m_GetHit2Text.text = "parried".Localize();
                //series.AddSeriesOfBlocks(m_GetHit2Text);
                break;
        }
    }

    private void OnBlockVs2Handed(int gotDamage)
    {
        m_GetHit1Text.text = "shield".Localize() + gotDamage;
        //series.ResetSeriesOfBlocks();
    }


    private void OnBlock(int strikeNumber)
    {
        switch (strikeNumber)
        {
            case 1:
                m_GetHit1Text.text = "blocked".Localize();
                //series.AddSeriesOfBlocks(m_GetHit1Text);           // проверить, достигнута ли серия, и начислить здоровья за серию блоков
                break;
            case 2:
                m_GetHit2Text.text = "blocked".Localize();
                //series.AddSeriesOfBlocks(m_GetHit2Text);
                break;
        }
    }

    private void OnEvade(int strikeNumber)
    {
        switch (strikeNumber)
        {
            case 1:
                m_GetHit1Text.text = "evaded".Localize();
                break;
            case 2:
                m_GetHit2Text.text = "evaded".Localize();
                break;
        }
    }
}

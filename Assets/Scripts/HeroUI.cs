using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// Пока только тексты для вывода урона
public class HeroUI : MonoBehaviour
{
    // тексты для вывода полученного урона
    public Text m_GetHit1Text;
    public Text m_GetHit2Text;

    [SerializeField]                            // может, как-то получить через GetComponent?
    private HeroManager heroManager;

    private void OnEnable()
    {
        GameManager.ExchangeEndedEvent += OnExchangeEnded;
        if (heroManager != null)
        {
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
        GameManager.ExchangeEndedEvent -= OnExchangeEnded;
        if (heroManager != null)
        {
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

    private void OnHit(int strikeNumber)
    {
        switch (strikeNumber)
        {
            case 1:
                m_GetHit1Text.text = "-" + heroManager.damage1.ToString();  // Если не использовать метод примитива .ToString(), а просто передать concat-у heroManager.damage1, будет производиться его упаковка, что менее эффективно
                break;
            case 2:
                m_GetHit2Text.text = "-" + heroManager.damage2.ToString();
                break;
        }
        heroManager.series.ResetSeriesOfBlocks();
        heroManager.series.CheckAndSetSeriesOfBlocks();
    }

    private void OnParry(int strikeNumber)
    {
        switch (strikeNumber)                
        {
            case 1:
                m_GetHit1Text.text = "parried";
                heroManager.series.CheckAndSetSeriesOfBlocks(heroManager, m_GetHit1Text);           // проверить, достигнута ли серия, и начислить здоровья за серию блоков
                break;
            case 2:
                m_GetHit2Text.text = "parried";
                heroManager.series.CheckAndSetSeriesOfBlocks(heroManager, m_GetHit2Text);
                break;
        }
    }

    private void OnBlockVs2Handed()
    {
        m_GetHit1Text.text = "shield: -" + heroManager.damage1.ToString();
        heroManager.series.ResetSeriesOfBlocks();
        heroManager.series.CheckAndSetSeriesOfBlocks();
    }


    private void OnBlock(int strikeNumber)
    {
        switch (strikeNumber)
        {
            case 1:
                m_GetHit1Text.text = "blocked";
                heroManager.series.CheckAndSetSeriesOfBlocks(heroManager, m_GetHit1Text);           // проверить, достигнута ли серия, и начислить здоровья за серию блоков
                break;
            case 2:
                m_GetHit2Text.text = "blocked";
                heroManager.series.CheckAndSetSeriesOfBlocks(heroManager, m_GetHit2Text);
                break;
        }
    }

    private void OnEvade(int strikeNumber)
    {
        switch (strikeNumber)
        {
            case 1:
                m_GetHit1Text.text = "evaded";
                break;
            case 2:
                m_GetHit2Text.text = "evaded";
                break;
        }
    }
}

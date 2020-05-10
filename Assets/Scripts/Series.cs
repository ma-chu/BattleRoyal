using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#pragma warning disable 0649    // убирает предупреждения компилятора о [SerializeField] private переменных, инициализируемых в редакторе   
//  СЕРИИ ГЕРОЯ
public class Series : MonoBehaviour
{

    // ПАРАМЕТРЫ для рассчета бонусов за серии:
    public float strongStrikeMin = 14;               // минимальный урон для определения сильных ударов
    public int strongStrikeSeriesBeginning = 2;      // после какого удара начинаются бонусы за сильные удары    
    public int seriesStrikeBeginning = 3;            // после какого удара начинаются бонусы за серию ударов    
    public int seriesBlockBeginning = 3;             // после какого блока начинаются бонусы за серию блоков
    [SerializeField]
    private float strongStrikeSeriesStepValue = 0.5f; // ценность каждого сильного удара после strongStrikeSeriesBeginning
    [SerializeField]
    private float seriesStrikeStepValue = 0.5f;       // ценность каждого последующего удара в серии после seriesStrikeBeginning
    [SerializeField]
    private float seriesBlockStepValue = 1f;          // ценность каждого последующего блока в серии после seriesBlockBeginning

    // Бонус за кол-во сильных ударов: +0.5 к урону за каждый сильный удар (на strenghtStrikeMin) после strenghtStrikeBeginning (+0.5 к третьему, +1 к четвертому ...)
    public int strongStrikesNum = 0;                    // Количество сильных ударов общее
    private bool hasStrongStrikesSeries = false;         // уже набрано
    // Бонус за серию ударов: +0.5 к урону за каждый удар подряд после seriesStrikeBeginning /при двух мечах должен пройти хоть один/ (+0.5 к четвертому, +1 к пятому ...)
    public int seriesOfStrikesNum = 0;                  // Количество ударов в серии
    private bool hasSeriesOfStrikes = false;             // серия ударов набрана
    // Бонус за серию блоков: +1 к здоровью за каждый блок подряд после seriesBlockBeginning /при двух мечах должны быть заблокированы оба удара/ (+1 к четвертому, +2 к пятому ...)
    public int seriesOfBlocksNum = 0;                   // Количество блоков в серии
    private bool hasSeriesOfBlocks = false;             // серия блоков набрана

    // Слайдеры-звезды для отображения серий
    [SerializeField]
    private Slider m_StrengthStrikesStarSlider;
    [SerializeField]
    private Image m_StrengthStrikesStarFillImage;
    [SerializeField]
    private Slider m_SeriesOfBlocksStarSlider;
    [SerializeField]
    private Image m_SeriesOfBlocksStarFillImage;
    [SerializeField]
    private Slider m_SeriesOfStrikesStarSlider;
    [SerializeField]
    private Image m_SeriesOfStrikesStarFillImage;
    [SerializeField]
    // аудио-сорс для звука достижения серии
    private AudioSource SFXAudio;

    [SerializeField]                            // может, как-то получить через GetComponent?
    private HeroManager heroManager;

    private void Awake()
    {
        // Установим максимальные значения слайдеров-подсказок серий
        m_StrengthStrikesStarSlider.maxValue = strongStrikeSeriesBeginning;
        m_SeriesOfBlocksStarSlider.maxValue = seriesBlockBeginning;
        m_SeriesOfStrikesStarSlider.maxValue = seriesStrikeBeginning;
    }

    // 1. Set/Reset Series
    public void CheckAndSetStrongStrikesSeries()        
    {
        // подвинуть слайдер подсказки
        m_StrengthStrikesStarSlider.value = strongStrikesNum;
        m_StrengthStrikesStarFillImage.color = Color.Lerp(Color.magenta, Color.red, strongStrikesNum / strongStrikeSeriesBeginning);
        // проверить, достигнута ли серия: сыграть звук достижения серии и выставить меркер
        if (!hasStrongStrikesSeries && (strongStrikesNum == strongStrikeSeriesBeginning))
        {
            SFXAudio.Play();
            hasStrongStrikesSeries = true;
        }
    }
    public void ResetStrongStrikesSeries()
    {
        strongStrikesNum = 0;
        hasStrongStrikesSeries = false;
    }

    public void CheckAndSetSeriesOfStrikes()
    {
        m_SeriesOfStrikesStarSlider.value = seriesOfStrikesNum;
        m_SeriesOfStrikesStarFillImage.color = Color.Lerp(Color.blue, Color.cyan, seriesOfStrikesNum / seriesStrikeBeginning);

        if (!hasSeriesOfStrikes && (seriesOfStrikesNum == seriesStrikeBeginning))
        {
            SFXAudio.Play();
            hasSeriesOfStrikes = true;
        }
    }
    public void ResetSeriesOfStrikes()
    {
        seriesOfStrikesNum = 0;
        hasSeriesOfStrikes = false;
    }

    public void CheckAndSetSeriesOfBlocks(HeroManager whom = null, Text where = null)     // who - кому добавлять здоровье, where - в какой строке писать (не обязательные параметры)
    {
        m_SeriesOfBlocksStarSlider.value = seriesOfBlocksNum;
        m_SeriesOfBlocksStarFillImage.color = Color.Lerp(Color.yellow, Color.green, seriesOfBlocksNum / seriesBlockBeginning);

        int diff = seriesOfBlocksNum - seriesBlockBeginning;

        if (diff == 0)
        {
            SFXAudio.Play();
            hasSeriesOfBlocks = true;
        }
        else if ((diff > 0) && (whom != null) && (where != null))
        {
            whom._HP.RegenHealth(diff * seriesBlockStepValue); 
            where.text = where.text + " +" + diff.ToString();
        }
    }
    public void ResetSeriesOfBlocks()
    {
        seriesOfBlocksNum = 0;
        hasSeriesOfBlocks = false;
    }

    // 2. Добавить к урону бонусы за серии ударов
    public void AddSeriesDamage(ref float damage)       // параметр передаем по ссылке и изменяем
    {
        // за серию ударов
        damage += seriesOfStrikesNum > seriesStrikeBeginning ? ((seriesOfStrikesNum - seriesStrikeBeginning) * seriesStrikeStepValue) : 0;
        // за кол-во сильных ударов
        damage += strongStrikesNum > strongStrikeSeriesBeginning ? ((strongStrikesNum - strongStrikeSeriesBeginning) * strongStrikeSeriesStepValue) : 0; 
    }
}

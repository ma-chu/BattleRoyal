using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//  СЕРИИ ГЕРОЯ
public class Series : MonoBehaviour
{
    [SerializeField]
    private HP _HP;                                   // Ссылка на компонент-здоровье

    // ПАРАМЕТРЫ для рассчета бонусов за серии:
    [SerializeField]
    private float strongStrikeMin = 14;               // минимальный урон для определения сильных ударов
    [SerializeField]
    private int strongStrikeSeriesBeginning = 2;      // после какого удара начинаются бонусы за сильные удары    
    [SerializeField]
    private int seriesStrikeBeginning = 3;            // после какого удара начинаются бонусы за серию ударов    
    [SerializeField]
    private int seriesBlockBeginning = 3;             // после какого блока начинаются бонусы за серию блоков
    public float StrongStrikeMin
    {
        get { return strongStrikeMin; }
    }
    public int StrongStrikeSeriesBeginning
    {
        get { return strongStrikeSeriesBeginning; }
    }
    public int SeriesStrikeBeginning
    {
        get { return seriesStrikeBeginning; }
    }
    public int SeriesBlockBeginning
    {
        get { return seriesBlockBeginning; }
    }

    [SerializeField]
    private float strongStrikeSeriesStepValue = 0.5f; // ценность каждого сильного удара после strongStrikeSeriesBeginning
    [SerializeField]
    private float seriesStrikeStepValue = 0.5f;       // ценность каждого последующего удара в серии после seriesStrikeBeginning
    [SerializeField]
    private float seriesBlockStepValue = 1f;          // ценность каждого последующего блока в серии после seriesBlockBeginning

    // Бонус за кол-во сильных ударов: +0.5 к урону за каждый сильный удар (на strenghtStrikeMin) после strenghtStrikeBeginning (+0.5 к третьему, +1 к четвертому ...)
    private int strongStrikesNum = 0;                    // Количество сильных ударов общее
    [SerializeField]
    private bool hasStrongStrikesSeries = false;        // уже набрано
    // Бонус за серию ударов: +0.5 к урону за каждый удар подряд после seriesStrikeBeginning /при двух мечах должен пройти хоть один/ (+0.5 к четвертому, +1 к пятому ...)
    private int seriesOfStrikesNum = 0;                  // Количество ударов в серии
    [SerializeField]
    private bool hasSeriesOfStrikes = false;            // серия ударов набрана
    // Бонус за серию блоков: +1 к здоровью за каждый блок подряд после seriesBlockBeginning /при двух мечах должны быть заблокированы оба удара/ (+1 к четвертому, +2 к пятому ...)
    private int seriesOfBlocksNum = 0;                   // Количество блоков в серии
    [SerializeField]
    private bool hasSeriesOfBlocks = false;             // серия блоков набрана
    public bool HasStrongStrikesSeries
    {
        get { return hasStrongStrikesSeries; }
    }
    public bool HasSeriesOfStrikes
    {
        get { return hasSeriesOfStrikes; }
    }
    public bool HasSeriesOfBlocks
    {
        get { return hasSeriesOfBlocks; }
    }

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
    private AudioSource SFXAudio;               // аудио-сорс для звука достижения серии

    [SerializeField]                            // может, как-то получить через GetComponent?
    private HeroManager heroManager;

    private void Awake()
    {
        _HP = GetComponent("HP") as HP;

        // Установим максимальные значения слайдеров-подсказок серий
        m_StrengthStrikesStarSlider.maxValue = StrongStrikeSeriesBeginning;
        m_SeriesOfBlocksStarSlider.maxValue = SeriesBlockBeginning;
        m_SeriesOfStrikesStarSlider.maxValue = SeriesStrikeBeginning;
    }


    public void AddStrongSeries(int strikeNumber)
    {
        switch (strikeNumber)
        {
            case 1:
                if (heroManager.preCoeffs[0].damage >= StrongStrikeMin) { strongStrikesNum++; }
                break;
            case 2:
                if (heroManager.preCoeffs[1].damage >= StrongStrikeMin) { strongStrikesNum++; }
                break;
        }

        // проверить, достигнута ли серия: сыграть звук достижения серии и выставить меркер
        if (!hasStrongStrikesSeries && (strongStrikesNum == StrongStrikeSeriesBeginning))
        {
            //Debug.Log("STRONG YEE");
            SFXAudio.PlayDelayed(0.2f);
            hasStrongStrikesSeries = true;
        }

        UpdateStrongStrikesSeries();    // обновить подсказку
    }
    public void ResetStrongStrikesSeries()
    {
        strongStrikesNum = 0;
        hasStrongStrikesSeries = false;

        UpdateStrongStrikesSeries();    // обновить подсказку
    }
    public void UpdateStrongStrikesSeries()        
    {
        // подвинуть слайдер подсказки
        m_StrengthStrikesStarSlider.value = strongStrikesNum;
        m_StrengthStrikesStarFillImage.color = Color.Lerp(Color.magenta, Color.red, strongStrikesNum / StrongStrikeSeriesBeginning);
    }


    public void AddSeriesOfStrikes()
    {
        seriesOfStrikesNum++;

        if (!hasSeriesOfStrikes && (seriesOfStrikesNum == SeriesStrikeBeginning))
        {
            SFXAudio.PlayDelayed(0.2f);
            hasSeriesOfStrikes = true;
        }

        UpdateSeriesOfStrikes();    // обновить подсказку
    }
    public void ResetSeriesOfStrikes()
    {
        seriesOfStrikesNum = 0;
        hasSeriesOfStrikes = false;

        UpdateSeriesOfStrikes();    // обновить подсказку
    }
    public void UpdateSeriesOfStrikes()
    {
        m_SeriesOfStrikesStarSlider.value = seriesOfStrikesNum;
        m_SeriesOfStrikesStarFillImage.color = Color.Lerp(Color.blue, Color.cyan, seriesOfStrikesNum / SeriesStrikeBeginning);
    }

    public void AddSeriesOfBlocks(Text where = null)     // where - в какой строке писать (не обязательный параметр)
    {
        seriesOfBlocksNum++;
        UpdateSeriesOfBlocks();

        int diff = seriesOfBlocksNum - SeriesBlockBeginning;

        if (diff == 0)
        {
            SFXAudio.PlayDelayed(0.2f);
            hasSeriesOfBlocks = true;
        }
        else if ((diff > 0) && (where != null))
        {
            _HP.RegenHealth(diff * seriesBlockStepValue); 
            where.text = where.text + " +" + diff.ToString();
        }
    }
    public void ResetSeriesOfBlocks()
    {
        seriesOfBlocksNum = 0;
        hasSeriesOfBlocks = false;

        UpdateSeriesOfBlocks();
    }
    public void UpdateSeriesOfBlocks()
    {
        m_SeriesOfBlocksStarSlider.value = seriesOfBlocksNum;
        m_SeriesOfBlocksStarFillImage.color = Color.Lerp(Color.yellow, Color.green, seriesOfBlocksNum / SeriesBlockBeginning);
    }

    // 2. Добавить к урону бонусы за серии ударов
    public float AddSeriesDamage()  
    {
        float damage;
        damage = HasSeriesOfStrikes ? ((seriesOfStrikesNum - SeriesStrikeBeginning) * seriesStrikeStepValue) : 0;                   // за серию ударов
        damage += HasStrongStrikesSeries ? ((strongStrikesNum - StrongStrikeSeriesBeginning) * strongStrikeSeriesStepValue) : 0;    // за кол-во сильных ударов
        return damage;
    }
}

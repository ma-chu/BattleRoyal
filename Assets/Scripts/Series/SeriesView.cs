using EF.Sounds;
using UnityEngine;
using UnityEngine.UI;
//  СЕРИИ ГЕРОЯ: Отображение
public class SeriesView : MonoBehaviour
{
    // Слайдеры-звезды для отображения серий
    [SerializeField] private Slider m_StrengthStrikesStarSlider;
    [SerializeField] private Image m_StrengthStrikesStarFillImage;
    [SerializeField] private Slider m_SeriesOfBlocksStarSlider;
    [SerializeField] private Image m_SeriesOfBlocksStarFillImage;
    [SerializeField] private Slider m_SeriesOfStrikesStarSlider;
    [SerializeField] private Image m_SeriesOfStrikesStarFillImage;
    
    [SerializeField] private Heroes heroType;

    private AudioClip _bonusSound;
    private float _delay;

    private void Awake()
    {
        // Установим максимальные значения слайдеров-подсказок серий
        m_StrengthStrikesStarSlider.maxValue = Series.StrongStrikeSeriesBeginning;
        m_SeriesOfBlocksStarSlider.maxValue = Series.SeriesBlockBeginning;
        m_SeriesOfStrikesStarSlider.maxValue = Series.SeriesStrikeBeginning;
    }

    private void Start()
    {
        _bonusSound = SoundsContainer.GetAudioClip(SoundTypes.Bonus, heroType);
        _delay = heroType == Heroes.Player ? 0.2f : 0.5f;
    }


    public void UpdateStrongSeries(int strongStrikesNum, bool set)
    {
        if (set) SoundsManager.Instance.PlaySound(_bonusSound, _delay);
        m_StrengthStrikesStarSlider.value = strongStrikesNum;
        m_StrengthStrikesStarFillImage.color = Color.Lerp(Color.magenta, Color.red, strongStrikesNum / Series.StrongStrikeSeriesBeginning);
    }
    
    public void UpdateSeriesOfStrikes(int seriesOfStrikesNum, bool set)
    {
        if (set) SoundsManager.Instance.PlaySound(_bonusSound, _delay);
        m_SeriesOfStrikesStarSlider.value = seriesOfStrikesNum;
        m_SeriesOfStrikesStarFillImage.color = Color.Lerp(Color.blue, Color.cyan, seriesOfStrikesNum / Series.SeriesStrikeBeginning);
    }
    
    public void UpdateSeriesOfBlocks(int seriesOfBlocksNum, bool set, Text where = null)    // where - где выводить реген жизней
    {
        if (set) SoundsManager.Instance.PlaySound(_bonusSound, _delay);
        
        var diff = seriesOfBlocksNum - Series.SeriesBlockBeginning;
        if (diff>0 && where!=null)
        {
            //_HP.RegenHealth(diff * seriesBlockStepValue); 
            where.text = where.text + " +" + diff.ToString();
        }
        
        m_SeriesOfBlocksStarSlider.value = seriesOfBlocksNum;
        m_SeriesOfBlocksStarFillImage.color = Color.Lerp(Color.yellow, Color.green, seriesOfBlocksNum / Series.SeriesBlockBeginning);
    }
}

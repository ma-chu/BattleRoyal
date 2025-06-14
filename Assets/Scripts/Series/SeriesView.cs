using EF.Sounds;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Серии героя: отображение
/// </summary>
 
public class SeriesView : MonoBehaviour
{
    [SerializeField] private Slider strengthStrikesStarSlider;
    [SerializeField] private Image strengthStrikesStarFillImage;
    [SerializeField] private Slider seriesOfBlocksStarSlider;
    [SerializeField] private Image seriesOfBlocksStarFillImage;
    [SerializeField] private Slider seriesOfStrikesStarSlider;
    [SerializeField] private Image seriesOfStrikesStarFillImage;
    
    [SerializeField] private Heroes heroType;

    private const float PlayerSoundDelay = 0.2f;
    private const float EnemySoundDelay = 0.5f;

    private AudioClip _bonusSound;
    private float _delay;

    private void Awake()
    {
        strengthStrikesStarSlider.maxValue = Series.StrongStrikeSeriesBeginning;
        seriesOfBlocksStarSlider.maxValue = Series.SeriesBlockBeginning;
        seriesOfStrikesStarSlider.maxValue = Series.SeriesStrikeBeginning;
    }

    private void Start()
    {
        _bonusSound = SoundsContainer.GetAudioClip(SoundTypes.Bonus, heroType);
        _delay = heroType == Heroes.Player ? PlayerSoundDelay : EnemySoundDelay;
    }


    public void UpdateStrongSeries(int strongStrikesNum, bool set)
    {
        strengthStrikesStarSlider.value = strongStrikesNum;
        strengthStrikesStarFillImage.color = Color.Lerp(Color.magenta, Color.red, strongStrikesNum / Series.StrongStrikeSeriesBeginning);
        if (set) 
            SoundsManager.Instance.PlaySound(_bonusSound, _delay);
    }
    
    public void UpdateSeriesOfStrikes(int seriesOfStrikesNum, bool set)
    {
        seriesOfStrikesStarSlider.value = seriesOfStrikesNum;
        seriesOfStrikesStarFillImage.color = Color.Lerp(Color.blue, Color.cyan, seriesOfStrikesNum / Series.SeriesStrikeBeginning);
        if (set)
            SoundsManager.Instance.PlaySound(_bonusSound, _delay);
    }
    
    public void UpdateSeriesOfBlocks(int seriesOfBlocksNum, bool set, Text where = null)    // where - где выводить реген жизней
    {
        var diff = seriesOfBlocksNum - Series.SeriesBlockBeginning;
        if (diff > 0 && where != null)
            where.text = where.text + " +" + diff;
        
        seriesOfBlocksStarSlider.value = seriesOfBlocksNum;
        seriesOfBlocksStarFillImage.color = Color.Lerp(Color.yellow, Color.green, seriesOfBlocksNum / Series.SeriesBlockBeginning);
        
        if (set) 
            SoundsManager.Instance.PlaySound(_bonusSound, _delay);
    }
}

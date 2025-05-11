/// <summary>
/// Настройки серий
/// </summary>
public class Series
{
    private PlayerObject _player;
    
    // TODO: could de made different for each player
    private static readonly float StrongStrikeMin = 14;                // минимальный урон для определения сильных ударов
    public static readonly int StrongStrikeSeriesBeginning = 2;        // после какого удара начинаются бонусы за сильные удары    
    public static readonly int SeriesStrikeBeginning = 3;              // после какого удара начинаются бонусы за серию ударов    
    public static readonly int SeriesBlockBeginning = 3;               // после какого блока начинаются бонусы за серию блоков

    private static readonly float StrongStrikeSeriesStepValue = 1.5f;  // ценность каждого сильного удара после StrongStrikeSeriesBeginning
    public static readonly float SeriesBlockStepValue = 1f;            // ценность каждого последующего блока в серии после SeriesBlockBeginning
    private static readonly float SeriesStrikeStepValue = 0.5f;        // ценность каждого последующего удара в серии после SeriesStrikeBeginning

    public int StrongStrikesNum { get; private set; }
    public int SeriesOfBlocksNum { get; private set; }
    public int SeriesOfStrikesNum { get; private set; }
    
    public Series(PlayerObject player)
    {
        _player = player;
    }

    public void TryToAddStrongSeries(int strikeNumber)
    {
        if (_player.preCoeffs[strikeNumber - 1].damage >= StrongStrikeMin)
            StrongStrikesNum++;
    }

    public void AddSeriesOfStrikes()
    {
        SeriesOfStrikesNum++;
    }
    
    public void AddSeriesOfBlocks()    
    {
        SeriesOfBlocksNum++;

        var diff = SeriesOfBlocksNum - SeriesBlockBeginning;
        if (diff > 0)
            _player.Hp.RegenHealth(diff * SeriesBlockStepValue);
    }
    
    public float AddSeriesDamage()                        
    {
        var sos = SeriesOfStrikesNum - SeriesStrikeBeginning;
        var damage = sos > 0 ? sos * SeriesStrikeStepValue : 0f;   
        
        sos = StrongStrikesNum - StrongStrikeSeriesBeginning;
        damage += sos > 0 ? sos * StrongStrikeSeriesStepValue : 0f;                     
        return damage;
    }

    public void ResetAll()
    {
        ResetStrongStrikesSeries();
        ResetSeriesOfBlocks();
        ResetSeriesOfStrikes();
    }
    
    public void ResetStrongStrikesSeries() => StrongStrikesNum = 0;
    public void ResetSeriesOfBlocks() => SeriesOfBlocksNum = 0;
    public void ResetSeriesOfStrikes() => SeriesOfStrikesNum = 0;
}

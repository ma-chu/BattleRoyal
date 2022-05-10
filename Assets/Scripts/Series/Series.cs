//  СЕРИИ ГЕРОЯ: Логика
public class Series
{
    private PlayerObject _player;
    
    // ПАРАМЕТРЫ для рассчета бонусов за серии:
    // пока статические, затем можно переделать на разные для каждого игрока, но тогда надо передавать их с сервера в начале матча 
    public static readonly float StrongStrikeMin = 14;                 // минимальный урон для определения сильных ударов
    public static readonly int StrongStrikeSeriesBeginning = 2;        // после какого удара начинаются бонусы за сильные удары    
    public static readonly int SeriesStrikeBeginning = 3;              // после какого удара начинаются бонусы за серию ударов    
    public static readonly int SeriesBlockBeginning = 3;               // после какого блока начинаются бонусы за серию блоков

    public static readonly float StrongStrikeSeriesStepValue = 1.5f;   // ценность каждого сильного удара после StrongStrikeSeriesBeginning
    // Бонус за кол-во сильных ударов: +1.5 к урону за каждый сильный удар (на strenghtStrikeMin) после strenghtStrikeBeginning (+1.5 к третьему, +3 к четвертому ...)

    public static readonly float SeriesBlockStepValue = 1f;            // ценность каждого последующего блока в серии после SeriesBlockBeginning
    // Бонус за серию блоков: +1 к здоровью за каждый блок подряд после seriesBlockBeginning /при двух мечах должны быть заблокированы оба удара/ (+1 к четвертому, +2 к пятому ...)

    public static readonly float SeriesStrikeStepValue = 0.5f;         // ценность каждого последующего удара в серии после SeriesStrikeBeginning
    // Бонус за серию ударов: +0.5 к урону за каждый удар подряд после seriesStrikeBeginning /при двух мечах должен пройти хоть один/ (+0.5 к четвертому, +1 к пятому ...)

    public int StrongStrikesNum { get; private set; }
    public int SeriesOfBlocksNum { get; private set; }
    public int SeriesOfStrikesNum { get; private set; }

    
    public Series(PlayerObject player)
    {
        _player = player;
    }
    
    public void AddStrongSeries(int strikeNumber)
    {
        switch (strikeNumber)
        {
            case 1:
                if (_player.preCoeffs[0].damage >= StrongStrikeMin)
                    StrongStrikesNum++;
                break;
            case 2:
                if (_player.preCoeffs[1].damage >= StrongStrikeMin)
                    StrongStrikesNum++;
                break;
        }
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

    public void ResetAll()
    {
        ResetStrongStrikesSeries();
        ResetSeriesOfBlocks();
        ResetSeriesOfStrikes();
    }
    public void ResetStrongStrikesSeries() => StrongStrikesNum = 0;
    public void ResetSeriesOfBlocks() => SeriesOfBlocksNum = 0;
    public void ResetSeriesOfStrikes() => SeriesOfStrikesNum = 0;
    
    public float AddSeriesDamage()                        // Добавить к урону бонусы за серии ударов
    {
        var sos = SeriesOfStrikesNum - SeriesStrikeBeginning;
        float damage = sos > 0 ? (sos * SeriesStrikeStepValue) : 0;                       // за серию ударов
        sos = StrongStrikesNum - StrongStrikeSeriesBeginning;
        damage += sos > 0 ? (sos * StrongStrikeSeriesStepValue) : 0;                      // за кол-во сильных ударов
        return damage;
    }
}

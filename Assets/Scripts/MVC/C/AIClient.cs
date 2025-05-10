public class AIClient : Client
{
    private int _stupidityChangeDelay;                                   // Задержка на тупизну бота перед сменой оружия
    
    private TurnInInfo _turnInInfo = new TurnInInfo()
    {
        PlayerDecision = Decision.No
    };

    protected override void CheckForSeries()
    {
        _isPlayerStrongStrikesSeries = _currentResults.PlayerSeries[0] >= Series.StrongStrikeSeriesBeginning;
        _isEnemyStrongStrikesSeries = _currentResults.EnemySeries[0] >= Series.StrongStrikeSeriesBeginning;
        _isPlayerSeriesOfBlocks = _currentResults.PlayerSeries[1] >= Series.SeriesBlockBeginning;
        _isEnemySeriesOfBlocks = _currentResults.EnemySeries[1] >= Series.SeriesBlockBeginning;
        _isPlayerSeriesOfStrikes = _currentResults.PlayerSeries[2] >= Series.SeriesStrikeBeginning;
        _isEnemySeriesOfStrikes = _currentResults.EnemySeries[2] >= Series.SeriesStrikeBeginning;
    }
    
    protected override void OnStartRound(object _, StartRoundInfo startRoundInfo)
    {
        if (!startRoundInfo.PlayerName.Equals(PlayerName)) 
            return;
        
        base.OnStartRound(_, startRoundInfo);
        _stupidityChangeDelay = NumRoundsToWin - _roundsLost - 1;
        MakeTurn(_roundsLost);
    }

    protected override void OnResultsReady(object _, TurnOutInfo results)
    {
        if (!results.PlayerName.Equals(PlayerName)) 
            return;
        
        if (_turnInInfo.PlayerDecision == Decision.Attack && results.EnemyDecision == Decision.Attack ) 
            _stupidityChangeDelay--;
        else if (_turnInInfo.PlayerDecision != Decision.Attack) 
            _stupidityChangeDelay = NumRoundsToWin - _roundsLost - 1;

        base.OnResultsReady(_, results);
    }

    /// <summary>
    /// nicety = 0:
    /// 1. на свои серии реагирует,
    /// 2. на чужие - нет,
    /// 3. относительно оружия - тупит 3 удара, затем меняет
    ///
    /// nicety = 1:
    /// 1. на свои серии реагирует,
    /// 2. на чужие - нет,
    /// 3. относительно оружия - тупит 2 удара, затем меняет
    ///
    /// nicety = 2:
    /// 1. на свои серии реагирует,
    /// 2. на чужие тоже,
    /// 3. относительно оружия - тупит 1 удар, затем меняет
    ///
    /// nicety = 3:
    /// 1. на свои серии реагирует,
    /// 2. на чужие тоже,
    /// 3. относительно оружия - сразу меняет
    ///
    /// /// </summary>
    /// <param name="nicety"></param>
    protected override void MakeTurn (int nicety)       
    {
        if (TryToContinueSeries())
            return;
        
        if (TryToCounterPlayerSeries(nicety))
            return;
        
        MainDecision();
    }

    private bool TryToContinueSeries()
    {
        var result = false;
        
        if (_isPlayerSeriesOfStrikes)
        {
            _turnInInfo.PlayerDecision = Decision.Attack;
            _turnInInfo.PlayerDefencePart = 0f;
            result = true;
        }
        else if (_isPlayerSeriesOfBlocks)
        {
            _turnInInfo.PlayerDecision = Decision.Attack;
            _turnInInfo.PlayerDefencePart = 1f;
            result = true;
        }

        if (result)
            SendDataToServer(_turnInInfo);
        
        return result;
    }

    private bool TryToCounterPlayerSeries(int nicety)
    {
        if (nicety <= 1) 
            return false;
        
        var result = false;
        
        if (_isEnemySeriesOfStrikes)
        {
            _turnInInfo.PlayerDecision = (PlayerWeaponSet != WeaponSet.SwordShield) ? Decision.ChangeSwordShield : Decision.Attack;
            _turnInInfo.PlayerDefencePart = 1f;
            SetWeaponSet(_turnInInfo.PlayerDecision);
            result = true;
        }
        else if (_isEnemySeriesOfBlocks)
        {
            _turnInInfo.PlayerDecision = (PlayerWeaponSet != WeaponSet.TwoHandedSword) ? Decision.ChangeTwoHandedSword : Decision.Attack;
            _turnInInfo.PlayerDefencePart = 1f;
            SetWeaponSet(_turnInInfo.PlayerDecision);
            result = true;
        }
            
        if (result)
            SendDataToServer(_turnInInfo);

        return result;
    }

    /// <summary>
    /// Варианты относительно типов оружия (с задержкой на тупизну):
    /// 1 - полный рандом; 2 - оптимум - то, что сейчас; 3 - идеальное - надо рассчитывать еще необходимость смены в зависимости от оставшегося здоровья
    /// еще можно использовать комбинацию вариантов. Например, оптимум с добавлением небольшого шанса на рандом
    /// </summary>
    private void MainDecision()
    {
        if (_stupidityChangeDelay > 0)
            _turnInInfo.PlayerDecision = Decision.Attack;
        else if (EnemyWeaponSet == WeaponSet.SwordShield && PlayerWeaponSet == WeaponSet.SwordSword)
            _turnInInfo.PlayerDecision = Decision.ChangeTwoHandedSword;
        else if (EnemyWeaponSet == WeaponSet.SwordSword && PlayerWeaponSet == WeaponSet.TwoHandedSword)
            _turnInInfo.PlayerDecision = Decision.ChangeSwordShield;
        else if (EnemyWeaponSet == WeaponSet.TwoHandedSword && PlayerWeaponSet == WeaponSet.SwordShield)
            _turnInInfo.PlayerDecision = Decision.ChangeSwordSword;
        else _turnInInfo.PlayerDecision = Decision.Attack;

        var tactic = UnityEngine.Random.value;
        _turnInInfo.PlayerDefencePart = tactic < 0.33f ? 1f : 0f;
        
        SetWeaponSet(_turnInInfo.PlayerDecision);
        SendDataToServer(_turnInInfo);
    }

    private void SetWeaponSet(Decision decision)
    {
        if (_turnInInfo.PlayerDecision != Decision.Attack)
        {
            PlayerWeaponSet = decision switch
            {
                Decision.ChangeSwordShield => WeaponSet.SwordShield,
                Decision.ChangeSwordSword => WeaponSet.SwordSword,
                Decision.ChangeTwoHandedSword => WeaponSet.TwoHandedSword
            };
        }
    }
}

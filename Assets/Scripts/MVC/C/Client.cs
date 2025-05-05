public abstract class Client
{
    public const int NumRoundsToWin = 4;                // правильнее получать от сервера в начале матча

    private IServer _server;
    protected TurnOutInfo _currentResults;
    protected int _roundNumber;
    protected int _roundsLost;
    
    protected bool _isPlayerStrongStrikesSeries;                       
    protected bool _isPlayerSeriesOfStrikes;                           
    protected bool _isPlayerSeriesOfBlocks;                            
    protected bool _isEnemyStrongStrikesSeries;                   
    protected bool _isEnemySeriesOfStrikes;                        
    protected bool _isEnemySeriesOfBlocks;
    
    public Decision Decision { get; set; }
    public WeaponSet PlayerWeaponSet { get; set; } = WeaponSet.SwordShield;
    public WeaponSet EnemyWeaponSet { get; private set; } = WeaponSet.SwordShield;

    public int RoundsWon { get; private set; }
    public string PlayerName { get; private set; }
    
    public virtual void Init(string name)
    {
        PlayerName = name;
        Join(GameManager.server);
    }

    private void Join(IServer server)                    
    {
        _server = server;
        _server.Join(PlayerName, OnJoined);
    }
    
    protected virtual void OnJoined(object _, string clientName)
    {
        if (!clientName.Equals(PlayerName))
            return;
        
        _server.SubscribeOnStartMatch(OnStartMatch);
        _server.SubscribeOnStartRound(OnStartRound);
        _server.SubscribeOnResultsReady(OnResultsReady);
        _server.SubscribeOnEndRound(OnEndRound);
        _server.SubscribeOnEndMatch(OnEndMatch);
    }

    protected virtual void OnStartMatch(object _, StartMatchInfo startMatchInfo)
    {
        if (!startMatchInfo.PlayerName.Equals(PlayerName)) 
            return;

        RoundsWon = _roundsLost = 0;
    }
    
    protected virtual void OnStartRound(object _, StartRoundInfo startRoundInfo)
    {
        _roundNumber = startRoundInfo.RoundNumber;
        PlayerWeaponSet = EnemyWeaponSet = WeaponSet.SwordShield;
    }
    
    protected virtual void OnResultsReady(object _, TurnOutInfo results)
    {
        if (!results.PlayerName.Equals(PlayerName)) 
            return;
        
        _currentResults = results;
        
        EnemyWeaponSet = results.EnemyDecision switch
        {
            Decision.ChangeSwordShield => WeaponSet.SwordShield,
            Decision.ChangeSwordSword => WeaponSet.SwordSword,
            Decision.ChangeTwoHandedSword => WeaponSet.TwoHandedSword,
            _ => EnemyWeaponSet
        };

        CheckForSeries();
        
        MakeTurn(_roundsLost);
    }

    protected abstract void CheckForSeries();

    // <summary>
    // AI: Определяется с действием бота (nicety - уровень интеллекта врага) /+ вызывает SendDataToServer()/
    // Player:  через ViewModel отображает анимации, звуки и пр., ожидает TurnInInfo с кнопок
    // </summary>
    /// <param name="nicety"></param>
    protected abstract void MakeTurn(int nicety);

    public void SendDataToServer(TurnInInfo t) => _server.TakeDecision(PlayerName, t);   // по нажатию кнопки решения у player'а или по выполнении ф-ии MakeTurn AI
    
    protected virtual void OnEndRound(object _, EndRoundInfo endRoundInfo)
    {
        if (!endRoundInfo.PlayerName.Equals(PlayerName)) 
            return;

        if (endRoundInfo.RoundWinner == PlayerName)
            RoundsWon++;
        else if (!endRoundInfo.RoundWinner.Equals(string.Empty))
            _roundsLost++;
        
        _isPlayerStrongStrikesSeries = _isPlayerSeriesOfBlocks = _isPlayerSeriesOfStrikes = false;
        _isEnemyStrongStrikesSeries = _isEnemySeriesOfBlocks = _isEnemySeriesOfStrikes = false;
    }
    
    protected virtual void OnEndMatch(object o, EndMatchInfo endMatchInfo) { }
}
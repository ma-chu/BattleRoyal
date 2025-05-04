public class Client
{
    private IServer _server;
    protected TurnOutInfo currentResults;

    public const int NumRoundsToWin = 4;                // правильнее получать от сервера в начале матча
    protected int RoundNumber;
    public int roundsWon;
    protected int roundsLost;
    
    public string PlayerName { get; private set; }

    public Decision decision;
    public WeaponSet PlayerWeaponSet = WeaponSet.SwordShield;      
    public WeaponSet EnemyWeaponSet = WeaponSet.SwordShield;      

    // серия набрана
    protected bool PlayerStrongStrikesSeries;                       
    protected bool PlayerSeriesOfStrikes;                           
    protected bool PlayerSeriesOfBlocks;                            
    protected bool EnemyStrongStrikesSeries;                   
    protected bool EnemySeriesOfStrikes;                        
    protected bool EnemySeriesOfBlocks;                        


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
    
    protected virtual void OnJoined(object o, string e)
    {
        if (!e.Equals(PlayerName))
            return;
        
        _server.SubscribeOnStartMatch(OnStartMatch);
        _server.SubscribeOnStartRound(OnStartRound);
        _server.SubscribeOnResultsReady(OnResultsReady);
        _server.SubscribeOnEndRound(OnEndRound);
        _server.SubscribeOnEndMatch(OnEndMatch);
    }

    protected virtual void OnStartMatch(object o, StartMatchInfo startMatchInfo)
    {
        if (!startMatchInfo.PlayerName.Equals(PlayerName)) 
            return;

        roundsWon = roundsLost = 0;
    }
    protected virtual void OnResultsReady(object o, TurnOutInfo results)
    {
        if (!results.PlayerName.Equals(PlayerName)) return;
        
        currentResults = results;

        // обработать результаты хода
        // 1. При смене оружия врагом поменять его weaponSet (а свой поменяем при вводе с кнопок)
        switch (results.EnemyDecision)
        {
            case Decision.ChangeSwordShield:
                EnemyWeaponSet = WeaponSet.SwordShield;
                break;
            case Decision.ChangeSwordSword:
                EnemyWeaponSet = WeaponSet.SwordSword;
                break;
            case Decision.ChangeTwoHandedSword:
                EnemyWeaponSet = WeaponSet.TwoHandedSword;
                break;
        }

        // 2. Определить, есть ли серии у меня и противника
        CheckForSeries();
        
        // 3. Сам процесс хода
        MakeTurn(roundsLost);
        // AI: Определяется с действием бота (nicety - уровень интеллекта врага) /+ вызывает SendDataToServer()/
        // Player:  через ViewModel отображает анимации, звуки и пр., ожидает TurnInInfo с кнопок
    }

    protected virtual void CheckForSeries() { }
    
    protected virtual void MakeTurn(int nicety) { }

    public void SendDataToServer(TurnInInfo t) => _server.TakeDecision(PlayerName, t);   // по нажатию кнопки решения у player'а или по выполнении ф-ии MakeTurn AI

    protected virtual void OnStartRound(object o, StartRoundInfo startRoundInfo)
    {
        RoundNumber = startRoundInfo.RoundNumber;
        PlayerWeaponSet = EnemyWeaponSet = WeaponSet.SwordShield;
    }
    
    protected virtual void OnEndRound(object o, EndRoundInfo endRoundInfo)
    {
        if (!endRoundInfo.PlayerName.Equals(PlayerName)) return;

        if (endRoundInfo.RoundWinner == PlayerName)
            roundsWon++;
        else if (!endRoundInfo.RoundWinner.Equals(string.Empty)) roundsLost++;
        
        // обнулить серии
        PlayerStrongStrikesSeries = PlayerSeriesOfBlocks = PlayerSeriesOfStrikes = false;
        EnemyStrongStrikesSeries = EnemySeriesOfBlocks = EnemySeriesOfStrikes = false;
    }
    
    protected virtual void OnEndMatch(object o, EndMatchInfo endMatchInfo) { }
}
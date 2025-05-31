using EF.Localization;

public class PlayerClient : Client
{
    private readonly bool[] _playerSeriesSet = new bool[3];
    private readonly bool[] _enemySeriesSet = new bool[3];

    protected override void OnJoined(object _, string clientName)
    {
        base.OnJoined(_, clientName);
        _mainSceneManager.ChangeResultText("waiting".Localize());
    }
    
    protected override void OnStartMatch(object _, StartMatchInfo startMatchInfo)
    {
        if (!startMatchInfo.PlayerName.Equals(PlayerName)) 
            return;
        
        base.OnStartMatch(_, startMatchInfo);
        _mainSceneManager.SetEnemyName(startMatchInfo.EnemyName);
        _mainSceneManager.SetPlayerName(startMatchInfo.PlayerName);
        _mainSceneManager.StartCoroutine(_mainSceneManager.GameStarting());
    }

    protected override void OnStartRound(object _, StartRoundInfo startRoundInfo)
    {
        if (!startRoundInfo.PlayerName.Equals(PlayerName)) 
            return;
        
        base.OnStartRound(_, startRoundInfo);
        _mainSceneManager.StartCoroutine(_mainSceneManager.RoundStarting(_roundNumber, startRoundInfo.PlayerStartHealth, startRoundInfo.EnemyStartHealth));
    }
    
    protected override void MakeTurn(int nicety)
    {
        _mainSceneManager.StartCoroutine(_mainSceneManager.RoundPlaying(_currentResults));
    }
    
    protected override void CheckForSeries()
    {
        HandleStrongSeries();
        HandleSeriesOfStrikes();
        HandleSeriesOfBlocks();  
        
        _mainSceneManager.SetPlayerSeries(_currentResults.PlayerSeries, _playerSeriesSet);
        _mainSceneManager.SetEnemySeries(_currentResults.EnemySeries, _enemySeriesSet);
    }

    private void HandleStrongSeries()
    {
        if (!_isPlayerStrongStrikesSeries && _currentResults.PlayerSeries[0] == Series.StrongStrikeSeriesBeginning)
        {
            _isPlayerStrongStrikesSeries = true;
            _playerSeriesSet[0] = true;
        }
        else
        {
            _playerSeriesSet[0] = false;
        }

        if (!_isEnemyStrongStrikesSeries && _currentResults.EnemySeries[0] == Series.StrongStrikeSeriesBeginning)
        {
            _isEnemyStrongStrikesSeries = true;
            _enemySeriesSet[0] = true;
        }
        else
        {
            _enemySeriesSet[0] = false;
        }
    }

    private void HandleSeriesOfStrikes()
    {
        if (!_isPlayerSeriesOfStrikes && _currentResults.PlayerSeries[2] == Series.SeriesStrikeBeginning)
        {
            _isPlayerSeriesOfStrikes = true;
            _playerSeriesSet[2] = true;
        }
        else
        {
            _playerSeriesSet[2] = false;
            if (_currentResults.PlayerSeries[2]==0) _isPlayerSeriesOfStrikes = false;
        }
        
        if (!_isEnemySeriesOfStrikes && _currentResults.EnemySeries[2] == Series.SeriesStrikeBeginning)
        {
            _isEnemySeriesOfStrikes = true;
            _enemySeriesSet[2] = true;
        }
        else
        {
            _enemySeriesSet[2] = false;
            if (_currentResults.EnemySeries[2]==0) _isEnemySeriesOfStrikes = false;
        }
    }

    private void HandleSeriesOfBlocks()
    {
        
        if (!_isPlayerSeriesOfBlocks && _currentResults.PlayerSeries[1] == Series.SeriesBlockBeginning)
        {
            _isPlayerSeriesOfBlocks = true;
            _playerSeriesSet[1] = true;
        }
        else
        {
            _playerSeriesSet[1] = false;
            if (_currentResults.PlayerSeries[1]==0) _isPlayerSeriesOfBlocks = false;
        }
        
        if (!_isEnemySeriesOfBlocks && _currentResults.EnemySeries[1] == Series.SeriesBlockBeginning)
        {
            _isEnemySeriesOfBlocks = true;
            _enemySeriesSet[1] = true;
        }
        else
        {
            _enemySeriesSet[1] = false;
            if (_currentResults.EnemySeries[1]==0) _isEnemySeriesOfBlocks = false;
        }
    }
    
    protected override void OnEndRound(object _, EndRoundInfo endRoundInfo)
    {
        if (!endRoundInfo.PlayerName.Equals(PlayerName)) 
            return;
        
        base.OnEndRound(_, endRoundInfo);
        _mainSceneManager.StartCoroutine(_mainSceneManager.RoundEnding(_roundNumber, endRoundInfo.RoundWinner, endRoundInfo.Prize));
    }
    
    protected override void OnEndMatch(object o, EndMatchInfo endMatchInfo)
    {
        if (!endMatchInfo.PlayerName.Equals(PlayerName)) 
            return;
        
        _mainSceneManager.StartCoroutine(_mainSceneManager.GameOver(endMatchInfo.MatchWinner));
    }
}

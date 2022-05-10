using UnityEngine;
using System.Collections;
using EF.Localization;

public class PlayerClient : Client
{
    private ViewModel _viewModel;        // Мост. _viewModel будет запускать анимации, менять тексты и петь звуки

    bool[] playerSeriesSet = new bool[3];
    bool[] enemySeriesSet = new bool[3];
    
    public override void Init(string name)
    {
        _viewModel = new ViewModel();
        _viewModel.Init(this);
        
        base.Init(name);
    }

    protected override void OnJoined(object o, string e)
    {
        base.OnJoined(o, e);
        _viewModel.ChangeResultText("waiting".Localize());
    }

    protected override void CheckForSeries()
    {
        if (!PlayerStrongStrikesSeries && (_currentResults.PlayerSeries[0] == Series.StrongStrikeSeriesBeginning))
        {
            PlayerStrongStrikesSeries = true;
            playerSeriesSet[0] = true;
        }

        if (!EnemyStrongStrikesSeries && (_currentResults.EnemySeries[0] == Series.StrongStrikeSeriesBeginning))
        {
            EnemyStrongStrikesSeries = true;
            enemySeriesSet[0] = true;
        }

        if (!PlayerSeriesOfStrikes && (_currentResults.PlayerSeries[2] == Series.SeriesStrikeBeginning))
        {
            PlayerSeriesOfStrikes = true;
            playerSeriesSet[2] = true;
        }

        if (!EnemySeriesOfStrikes && _currentResults.EnemySeries[2] == Series.SeriesStrikeBeginning)
        {
            EnemySeriesOfStrikes = true;
            enemySeriesSet[2] = true;
        }

        if (!PlayerSeriesOfBlocks && (_currentResults.PlayerSeries[1] == Series.SeriesBlockBeginning))
        {
            PlayerSeriesOfBlocks = true;
            playerSeriesSet[1] = true;
        }

        if (!EnemySeriesOfBlocks && (_currentResults.EnemySeries[1] == Series.SeriesBlockBeginning))
        {
            EnemySeriesOfBlocks = true;
            enemySeriesSet[1] = true;
        }
        
        _viewModel.SetPlayerSeries(_currentResults.PlayerSeries, playerSeriesSet);
        _viewModel.SetEnemySeries(_currentResults.EnemySeries, enemySeriesSet);
    }

    protected override void OnStartMatch(object o, StartMatchInfo startMatchInfo)
    {
        base.OnStartMatch(o, startMatchInfo);
        _viewModel.SetEnemyName(startMatchInfo.EnemyName);
        _viewModel.SetPlayerName(startMatchInfo.PlayerName);
        MainGameManager.Instance.StartCoroutine(_viewModel.GameStarting());     // MainGameManager вызывает все корутины
        // Каким-то образом надо дождаться конца корутины, прежде, чем реагировать на событие OnStartRound
    }

    protected override void OnStartRound(object o, StartRoundInfo startRoundInfo)
    {
        base.OnStartRound(o, startRoundInfo);
        MainGameManager.Instance.StartCoroutine(_viewModel.RoundStarting(RoundNumber, startRoundInfo.PlayerStartHealth, startRoundInfo.EnemyStartHealth));
    }
    
    protected override void OnEndRound(object o, EndRoundInfo endRoundInfo)
    {
        base.OnEndRound(o, endRoundInfo);
        MainGameManager.Instance.StartCoroutine(_viewModel.RoundEnding(RoundNumber, endRoundInfo.roundWinner, endRoundInfo.prize));
    }
    
    protected override void MakeTurn(int nicety)
    {
        MainGameManager.Instance.StartCoroutine(_viewModel.RoundPlaying(_currentResults));
    }
    
    protected override void OnEndMatch(object o, EndMatchInfo endMatchInfo)
    {
        if (!endMatchInfo.PlayerName.Equals(PlayerName)) return;
        MainGameManager.Instance.StartCoroutine(_viewModel.GameOver(endMatchInfo.matchWinner));
    }
}

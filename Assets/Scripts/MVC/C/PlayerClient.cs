using UnityEngine;
using System.Collections;
using EF.Localization;

public class PlayerClient : Client
{
    private ViewModel _viewModel /*1 IViewModel*/;        // Мост. _viewModel будет запускать анимации, менять тексты и петь звуки

    bool[] playerSeriesSet = new bool[3];
    bool[] enemySeriesSet = new bool[3];
    
    public override void Init(string name /*2 , vm ViewModel*/)
    {
        _viewModel = new ViewModel() /*3 vm*/;
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
        if (!PlayerStrongStrikesSeries && (currentResults.PlayerSeries[0] == Series.StrongStrikeSeriesBeginning))
        {
            PlayerStrongStrikesSeries = true;
            playerSeriesSet[0] = true;
        }
        else playerSeriesSet[0] = false;

        if (!EnemyStrongStrikesSeries && (currentResults.EnemySeries[0] == Series.StrongStrikeSeriesBeginning))
        {
            EnemyStrongStrikesSeries = true;
            enemySeriesSet[0] = true;
        }
        else enemySeriesSet[0] = false;

        
        if (!PlayerSeriesOfStrikes && (currentResults.PlayerSeries[2] == Series.SeriesStrikeBeginning))
        {
            PlayerSeriesOfStrikes = true;
            playerSeriesSet[2] = true;
        }
        else
        {
            playerSeriesSet[2] = false;
            if (currentResults.PlayerSeries[2]==0) PlayerSeriesOfStrikes = false;
        }
        
        if (!EnemySeriesOfStrikes && currentResults.EnemySeries[2] == Series.SeriesStrikeBeginning)
        {
            EnemySeriesOfStrikes = true;
            enemySeriesSet[2] = true;
        }
        else
        {
            enemySeriesSet[2] = false;
            if (currentResults.EnemySeries[2]==0) EnemySeriesOfStrikes = false;
        }
        
        
        if (!PlayerSeriesOfBlocks && (currentResults.PlayerSeries[1] == Series.SeriesBlockBeginning))
        {
            PlayerSeriesOfBlocks = true;
            playerSeriesSet[1] = true;
        }
        else
        {
            playerSeriesSet[1] = false;
            if (currentResults.PlayerSeries[1]==0) PlayerSeriesOfBlocks = false;
        }
        
        if (!EnemySeriesOfBlocks && (currentResults.EnemySeries[1] == Series.SeriesBlockBeginning))
        {
            EnemySeriesOfBlocks = true;
            enemySeriesSet[1] = true;
        }
        else
        {
            enemySeriesSet[1] = false;
            if (currentResults.EnemySeries[1]==0) EnemySeriesOfBlocks = false;
        }

        
        _viewModel.SetPlayerSeries(currentResults.PlayerSeries, playerSeriesSet);
        _viewModel.SetEnemySeries(currentResults.EnemySeries, enemySeriesSet);
    }

    protected override void OnStartMatch(object o, StartMatchInfo startMatchInfo)
    {
        if (!startMatchInfo.PlayerName.Equals(PlayerName)) return;
        base.OnStartMatch(o, startMatchInfo);
        _viewModel.SetEnemyName(startMatchInfo.EnemyName);
        _viewModel.SetPlayerName(startMatchInfo.PlayerName);
        MainGameManager.Instance.StartCoroutine(_viewModel.GameStarting());     // MainGameManager будет вызывать все корутины
    }

    protected override void OnStartRound(object o, StartRoundInfo startRoundInfo)
    {
        if (!startRoundInfo.PlayerName.Equals(PlayerName)) return;
        base.OnStartRound(o, startRoundInfo);
        MainGameManager.Instance.StartCoroutine(_viewModel.RoundStarting(RoundNumber, startRoundInfo.PlayerStartHealth, startRoundInfo.EnemyStartHealth));
    }
    
    protected override void OnEndRound(object o, EndRoundInfo endRoundInfo)
    {
        if (!endRoundInfo.PlayerName.Equals(PlayerName)) return;
        base.OnEndRound(o, endRoundInfo);
        MainGameManager.Instance.StartCoroutine(_viewModel.RoundEnding(RoundNumber, endRoundInfo.roundWinner, endRoundInfo.prize));
    }
    
    protected override void MakeTurn(int nicety)
    {
        MainGameManager.Instance.StartCoroutine(_viewModel.RoundPlaying(currentResults));
    }
    
    protected override void OnEndMatch(object o, EndMatchInfo endMatchInfo)
    {
        if (!endMatchInfo.PlayerName.Equals(PlayerName)) return;
        MainGameManager.Instance.StartCoroutine(_viewModel.GameOver(endMatchInfo.matchWinner));
    }
}

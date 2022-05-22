using System;
using Photon.Bolt;
using UnityEngine;


[BoltGlobalBehaviour(BoltNetworkModes.Client)]
public class ClientPhotonAdapter : GlobalEventListener, IServer
{
    private static ClientPhotonAdapter _instance;
    public static ClientPhotonAdapter Instance => _instance;
    private void Awake()
    {
        _instance ??= this;
        Debug.Log(_instance.GetType()+" is me");
    }

    private string _name;
    
    
    // Каждый метод IServer реализуется двумя подобными методами (+событие) здесь и двумя в ServerPhotonAdapter (и двумя событиями фотона)
    public void Join(string name, EventHandler<string> onJoined)
    {
        _name = name;

        var evnt = EFJoin.Create();   
        evnt.clientName = name;
        evnt.Send(); 
 
        JoinedAction += onJoined;
    }
    public event EventHandler<string> JoinedAction;
    public override void OnEvent(EFOnJoined evnt)
    {
        JoinedAction?.Invoke(this, evnt.clientName); 
        Debug.LogWarning(GetType() + ": OnJoined run!");
    }
    
    
    public void SubscribeOnStartMatch(EventHandler<StartMatchInfo> onStartMatch) => StartMatchAction += onStartMatch;
    public event EventHandler<StartMatchInfo> StartMatchAction;
    public override void OnEvent(EFStartMatch evnt)
    {
        var startMatchInfo = new StartMatchInfo()
        {
            PlayerName = _name,
            EnemyName = evnt.EnemyName,
            PlayerInventoryItems = new []{evnt.PlayerInventoryItem1, evnt.PlayerInventoryItem2, evnt.PlayerInventoryItem3},
            EnemyInventoryItems = new []{evnt.EnemyInventoryItem1, evnt.EnemyInventoryItem2, evnt.EnemyInventoryItem3},
        };
        StartMatchAction?.Invoke(this, startMatchInfo);
        Debug.LogWarning(GetType() + ": Match started!");
    }

    
    public void SubscribeOnStartRound(EventHandler<StartRoundInfo> onStartRound) => StartRoundAction += onStartRound;
    public event EventHandler<StartRoundInfo> StartRoundAction;
    public override void OnEvent(EFStartRound evnt)
    {
        var startRoundInfo = new StartRoundInfo()
        {
            PlayerName = _name,
            roundNumber = evnt.roundNumber,
            PlayerStartHealth = evnt.PlayerStartHealth,
            EnemyStartHealth = evnt.EnemyStartHealth
        };
        StartRoundAction?.Invoke(this, startRoundInfo);
    }


    public void SubscribeOnResultsReady(EventHandler<TurnOutInfo> onResultsReady) => ResultsReadyAction += onResultsReady;
    public event EventHandler<TurnOutInfo> ResultsReadyAction;
    public override void OnEvent(EFResultsReady evnt)
    {
        var results = new TurnOutInfo()
        {
            PlayerName = _name,
            EnemyDecision = (Decision)evnt.EnemyDecision,
            PlayerExchangeResults = new []{(ExchangeResult)evnt.PlayerExchangeResult1, (ExchangeResult)evnt.PlayerExchangeResult2},
            EnemyExchangeResults = new []{(ExchangeResult)evnt.EnemyExchangeResult1, (ExchangeResult)evnt.EnemyExchangeResult2},
            PlayerDamages = new []{evnt.PlayerDamage1, evnt.PlayerDamage2},
            EnemyDamages = new []{evnt.EnemyDamage1, evnt.EnemyDamage2},
            PlayerHP = evnt.PlayerHP,
            EnemyHP = evnt.EnemyHP,
            PlayerSeries = new []{evnt.PlayerSeries1, evnt.PlayerSeries2, evnt.PlayerSeries3},
            EnemySeries = new []{evnt.EnemySeries1, evnt.EnemySeries2, evnt.EnemySeries3}
        };
        ResultsReadyAction?.Invoke(this, results);
    }


    public void SubscribeOnEndRound(EventHandler<EndRoundInfo> onEndRound) => EndRoundAction += onEndRound;
    public event EventHandler<EndRoundInfo> EndRoundAction;
    public override void OnEvent(EFEndRound evnt)
    {
        var endRoundInfo = new EndRoundInfo()
        {
            PlayerName = _name,
            roundWinner = evnt.roundWinner,
            prize = evnt.prize
        };
        EndRoundAction?.Invoke(this, endRoundInfo);
    }
    
    
    public void SubscribeOnEndMatch(EventHandler<EndMatchInfo> onEndMatch) => EndMatchAction += onEndMatch;
    public event EventHandler<EndMatchInfo> EndMatchAction;
    public override void OnEvent(EFEndMatch evnt)
    {
        var endMatchInfo = new EndMatchInfo()
        {
            PlayerName = _name,
            matchWinner = evnt.matchWinner
        };
        EndMatchAction?.Invoke(this, endMatchInfo);
    }
    
    
    public void TakeDecision(string name, TurnInInfo turnInInfo)
    {
        var evnt = EFSendDataToServer.Create();
        evnt.PlayerDecision = (int) turnInInfo.PlayerDecision;
        evnt.PlayerDefencePart = turnInInfo.PlayerDefencePart;
        evnt.Send();
    }
    
    
    public override void Disconnected(BoltConnection connection)
    {
        // Ф-ия Client.Disconnect()
    }
}

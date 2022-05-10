using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Bolt;
using UnityEngine;

public class ClientPhotonAdapter : GlobalEventListener, IServer
{

    public void Join(string name, EventHandler<string> onTournamentJoined)
    {
        
    }
    public void SubscribeOnStartMatch(EventHandler<StartMatchInfo> onStartMatch)
    {
        
    }
    public void TakeDecision(string name,TurnInInfo turnInInfo)
    {
        
    }
    public void SubscribeOnResultsReady(EventHandler<TurnOutInfo> onResultsReady)
    {
        
    }
    public void SubscribeOnEndMatch(EventHandler<EndMatchInfo> onEndMatch)
    {
        
    }
    public void SubscribeOnStartRound(EventHandler<StartRoundInfo> onStartRound)
    {
        
    }
    public void SubscribeOnEndRound(EventHandler<EndRoundInfo> onEndRound)
    {
        
    }  
    
}

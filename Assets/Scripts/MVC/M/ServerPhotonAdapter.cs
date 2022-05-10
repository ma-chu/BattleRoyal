using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Bolt;
using UnityEngine;

// Реализует перевод интерфейса IServer в события photon'а в и обратно.
// Очевидно, что знает оба интерфейса
public class ServerPhotonAdapter : IServer
{
    private static ServerPhotonAdapter _instance;
    public static ServerPhotonAdapter Instance => _instance;
    
    private IServer _server;

    public ServerPhotonAdapter()
    {
        _instance = this;
        BoltLauncher.StartServer(); 
        _server = Server.Instance;
    }

    public void SubscribeOnStartMatch(EventHandler<StartMatchInfo> onStartMatch)
    {
        // Используем события болта
    }
    
    public void MakeMultiplayerEnemyDecision(Decision decision, float defencePart, out int[] clientExchangeResult, out int[] clientDamage, out int[] serverExchangeResult, out int[] serverDamage)     // выполняется на сервере
    {
        //m_Enemy.decision = (Decision) decision;    //лишнее, уже сделано в ServerNetworkCallbacks по событию EFReadyForExchangeEvent
        //m_Enemy.defencePart = defencePart;
        

        clientExchangeResult = new int[2];
//  ВЕРНУТЬСЯ при фотоне      clientExchangeResult[0] = (int) enemy.exchangeResult[0];
// В       clientExchangeResult[1] = (int) enemy.exchangeResult[1];
        clientDamage = new int[2];
// В       clientDamage[0] = enemy.gotDamage[0];
// В       clientDamage[1] = enemy.gotDamage[1];
        
        serverExchangeResult = new int[2];                        
// В       serverExchangeResult[0] = (int) player.exchangeResult[0];
// В      serverExchangeResult[1] = (int) player.exchangeResult[1];
        serverDamage = new int[2];                                
// В       serverDamage[0] = player.gotDamage[0];                   
// В       serverDamage[1] = player.gotDamage[1]; 
                  
    }

    public void Join(string name, EventHandler<string> onTournamentJoined)
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

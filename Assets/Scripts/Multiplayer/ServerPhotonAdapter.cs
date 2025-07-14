using Photon.Bolt;
using UnityEngine;

/// <summary>
/// Реализует перевод интерфейса IServer в события photon'а в и обратно
/// Не имплементирует интерфейс IServer, а вызывает его ф-ии 
/// </summary>


// Можно прикрепить к постоянному объекту на сцене
// А можно указать такой атрибут, как ниже. Bolt породит инстанс скрипта самостоятельно без прикрепления к объекту сцены
// (только на сервере и только для сцены Main)
[BoltGlobalBehaviour(BoltNetworkModes.Server, "Main")]
public class ServerPhotonAdapter : GlobalEventListener
{
    private IServer _server;
    
    private void Awake()
    {
        _server = Server.Instance;
        
        _server.SubscribeOnStartMatch(OnStartMatch);   
        _server.SubscribeOnStartRound(OnStartRound);
        _server.SubscribeOnResultsReady(OnResultsReady);
        _server.SubscribeOnEndRound(OnEndRound);
        _server.SubscribeOnEndMatch(OnEndMatch);
    }
    
    public override void Connected(BoltConnection connection)       
    {
        Debug.LogWarning(GetType() + ": Client Connected to session!"); 
        PhotonPlayerObjectRegisty.CreateClientPlayer(connection);
    }
    
    public override void SceneLoadRemoteDone(BoltConnection connection, IProtocolToken token)
    {
        Debug.Log(GetType() + ": Client Scene Loaded!");
    }

    
    // Каждый метод IServer реализуется двумя подобными методами здесь и двумя в ClientPhotonAdapter (и двумя событиями фотона)
    public override void OnEvent(EFJoin evnt)
    {
        var playerObject = PhotonPlayerObjectRegisty.GetPlayer(evnt.RaisedBy);
        playerObject.SetName(evnt.clientName);
        _server.Join(evnt.clientName, OnJoined, false);
        Debug.Log(GetType() + ": Client joined to tournament");
    }
    private void OnJoined(object o, string playerName)
    {
        var playerObject = PhotonPlayerObjectRegisty.GetPlayer(playerName);
        var evnt = EFOnJoined.Create(playerObject.connection);   
        evnt.clientName = playerName;
        evnt.Send(); 
    }


    private void OnStartMatch(object _, StartMatchInfo startMatchInfo)
    {
        var playerObject = PhotonPlayerObjectRegisty.GetPlayer(startMatchInfo.PlayerName);
        if (playerObject == null)           // этот клиент не сетевой
            return;
        
        var evnt = EFStartMatch.Create(playerObject.connection);
        evnt.EnemyName = startMatchInfo.EnemyName;
        if (startMatchInfo.PlayerInventoryItems != null)
        {
            evnt.PlayerInventoryItem1 = startMatchInfo.PlayerInventoryItems[0] ?? string.Empty;
            evnt.PlayerInventoryItem2 = startMatchInfo.PlayerInventoryItems[1] ?? string.Empty;
            evnt.PlayerInventoryItem3 = startMatchInfo.PlayerInventoryItems[2] ?? string.Empty;
        }

        if (startMatchInfo.EnemyInventoryItems != null)
        {
            evnt.EnemyInventoryItem1 = startMatchInfo.EnemyInventoryItems[0] ?? string.Empty;
            evnt.EnemyInventoryItem2 = startMatchInfo.EnemyInventoryItems[1] ?? string.Empty;
            evnt.EnemyInventoryItem3 = startMatchInfo.EnemyInventoryItems[2] ?? string.Empty;
        }
        
        evnt.Send();
        Debug.Log(GetType() + ": Match started");
    }

    
    private void OnStartRound(object _, StartRoundInfo startRoundInfo)
    {
        var playerObject = PhotonPlayerObjectRegisty.GetPlayer(startRoundInfo.PlayerName);
        if (playerObject == null)
            return;
        
        var evnt = EFStartRound.Create(playerObject.connection);
        evnt.roundNumber = startRoundInfo.RoundNumber;
        evnt.PlayerStartHealth = startRoundInfo.PlayerStartHealth;
        evnt.EnemyStartHealth = startRoundInfo.EnemyStartHealth;
        evnt.Send(); 
    }

    
    private void OnResultsReady(object _, TurnOutInfo results)
    {
        var playerObject = PhotonPlayerObjectRegisty.GetPlayer(results.PlayerName);
        if (playerObject == null)
            return;
        
        var evnt = EFResultsReady.Create(playerObject.connection);
        evnt.EnemyDecision = (int) results.EnemyDecision;
        evnt.PlayerExchangeResult1 = (int) results.PlayerExchangeResults[0];
        evnt.PlayerExchangeResult2 = (int) results.PlayerExchangeResults[1];
        evnt.EnemyExchangeResult1 = (int) results.EnemyExchangeResults[0];
        evnt.EnemyExchangeResult2 = (int) results.EnemyExchangeResults[1];
        evnt.PlayerDamage1 = results.PlayerDamages[0];
        evnt.PlayerDamage2 = results.PlayerDamages[1];
        evnt.EnemyDamage1 = results.EnemyDamages[0];
        evnt.EnemyDamage2 = results.EnemyDamages[1];
        evnt.PlayerHP = results.PlayerHP;
        evnt.EnemyHP = results.EnemyHP;
        evnt.PlayerSeries1 = results.PlayerSeries[0];
        evnt.PlayerSeries2 = results.PlayerSeries[1];
        evnt.PlayerSeries3 = results.PlayerSeries[2];
        evnt.EnemySeries1 = results.EnemySeries[0];
        evnt.EnemySeries2 = results.EnemySeries[1];
        evnt.EnemySeries3 = results.EnemySeries[2];
        evnt.Send(); 
    }


    private void OnEndRound(object _, EndRoundInfo endRoundInfo)
    {
        var playerObject = PhotonPlayerObjectRegisty.GetPlayer(endRoundInfo.PlayerName);
        if (playerObject == null)
            return;
       
        var evnt = EFEndRound.Create(playerObject.connection);
        evnt.roundWinner = endRoundInfo.RoundWinner;
        evnt.prize = endRoundInfo.Prize;
        evnt.Send(); 
    }


    private void OnEndMatch(object _, EndMatchInfo endMatchInfo)
    {
        var playerObject = PhotonPlayerObjectRegisty.GetPlayer(endMatchInfo.PlayerName);
        if (playerObject == null) 
            return;
        
        var evnt = EFEndMatch.Create(playerObject.connection);
        evnt.matchWinner = endMatchInfo.MatchWinner;
        evnt.Send(); 
    }

    public override void OnEvent(EFSendDataToServer evnt)
    {
        var playerObject = PhotonPlayerObjectRegisty.GetPlayer(evnt.RaisedBy);
        var turnInInfo = new TurnInInfo
        {
            PlayerDecision = (Decision)evnt.PlayerDecision,
            PlayerDefencePart = evnt.PlayerDefencePart
        };
        _server.TakeDecision(playerObject.name, turnInInfo);
    }
    
    public override void Disconnected(BoltConnection connection)
    {
        // Ф-ия Server.Disconnect()
    }
}

using Photon.Bolt;
using UnityEngine;
using System.Linq;
using Photon.Bolt.Matchmaking;
using UdpKit;

// Типа GameManager'а, прикрепляем к постоянному объекту на сцене
// [BoltGlobalBehaviour(BoltNetworkModes.Host, "Level2")] - если указать такой атрибут, то Bolt породит инстанс скрипта самостоятельно без прикрепления к объекту сцены
// (только на сервере и только для сцены Level2)

[BoltGlobalBehaviour(BoltNetworkModes.Server, "Main")]
public class ServerNetworkCallbacks : GlobalEventListener
{
    /* Регистрировать токены либо Bolt->Protocol Tokens Registry->Refresh, либо так: 
     public override void BoltStartBegin()
      {
          BoltNetwork.RegisterTokenClass<PlayerClientToken>();
          BoltNetwork.RegisterTokenClass<PlayerServerToken>();
      }
     //На тек. момент  токены не используются */

    private GameManager _gameManager;
    private PlayerManager _playerManager;
    private EnemyManager _enemyManager;
    
    private void Awake()
    {
        PhotonPlayerObjectRegisty.CreateServerPlayer();
    }
    
    public override void Connected(BoltConnection connection)
    {
        Debug.LogWarning(this.name + ": Client Connected!"); 
        PhotonPlayerObjectRegisty.CreateClientPlayer(connection);
    }

    // ф-ия-событие, когда удаленная сцена (клиента) болта стартанула
    public override void SceneLoadRemoteDone(BoltConnection connection, IProtocolToken token)
    {
        Debug.Log(this.name + ": Client Scene Loaded!");
        
        var playerObject= PhotonPlayerObjectRegisty.GetPlayer(connection);
        playerObject.Spawn();                                // А если сущность клиента передать в токене (зачем?)
        
        connection.SetCanReceiveEntities(true);
        var evnt = EFStartBattleServerEvent.Create(connection);          // Создаем событие только для этого соединения
        evnt.EnemyEntity = PhotonPlayerObjectRegisty.ServerPhotonPlayer.character;   // В качестве противника передаем клиенту себя
        // evnt.YourEntity = playerObject.character; // так бы можно передать клиенту его entity уже с сервера, но PlayerEntityController.SimulateOwner потребует ссылок на GameManager сразу
        evnt.Send();
        
        /*
        // Так было бы с токеном
        connection.SetCanReceiveEntities(true);
        var serverToken = new PlayerServerToken();
        serverToken.enemyEntity = PlayerObjectRegisty.ServerPlayer.character;
        var evnt = EFStartBattleEVE.Create(connection);          
        evnt.ServerToken = serverToken;
        evnt.Send();  
        */
    }

    public override void OnEvent(EFStartBattleClientReplyEvent evnt)
    {
// ВЕРНУТЬСЯ при фотоне         GameManager.enemyBoltEntity = evnt?.clientEntity;
        var enemyState = evnt?.clientEntity.GetState<IEFPlayerState>();

        Debug.LogWarning(this.name + " Client entity recieved. Name = " + enemyState.Username); 
        
// ВЕРНУТЬСЯ при фотоне         GameManager.ClientConnected = true; 
    }
    

    // ф-ия-событие, когда локальныая сцена (здесь сервера)  (или сервера/клиента, если бы не было флага BoltNetworkModes.Server) болта стартанула
    public override void SceneLoadLocalDone(string scene, IProtocolToken token)
    {
        var playerObject = PhotonPlayerObjectRegisty.ServerPhotonPlayer;
        playerObject.Spawn();
        var poEntity = playerObject.character;                                            
// ВЕРНУТЬСЯ при фотоне         GameManager.myBoltEntity = poEntity;
        var myState = poEntity.GetState<IEFPlayerState>();
        myState.Username = PlayerPrefs.GetString("username");            
        
        Debug.LogWarning(this.name + "  My(Server's) BoltEntity created. My name is " + myState.Username);    

        poEntity.GetComponent<PlayerEntityController>().SetLinks();
        
        _gameManager = GameManager.Instance;
// ВЕРНУТЬСЯ при фотоне         _playerManager = GameManager.Instance.player;
// ВЕРНУТЬСЯ при фотоне         _enemyManager = GameManager.Instance.enemy;
    }


    public override void OnEvent(EFReadyForExchangeEvent evnt)
    {
        if (!evnt.FromSelf)
        {
/* ВЕРНУТЬСЯ при фотоне 
            _enemyManager.decision = (Decision) evnt.Decision;
            _enemyManager.defencePart = evnt.DefencePart;
*/
        }
/* ВЕРНУТЬСЯ при фотоне 
        if ((_enemyManager.decision != Decision.No) && (_playerManager.decision != Decision.No))
        {
            _gameManager.MakeMultiplayerEnemyDecision(_enemyManager.decision, _enemyManager.defencePart,
                out int[] clientExchangeResult, out int[] clientDamage, out int[] serverExchangeResult,
                out int[] serverDamage);

            var evnt1 = EFExchangeResultsReady.Create();

            evnt1.ClientExchangeResult1 = clientExchangeResult[0];
            evnt1.ClientExchangeResult2 = clientExchangeResult[1];
            evnt1.ClientDamage1 = clientDamage[0];
            evnt1.ClientDamage2 = clientDamage[1];

            evnt1.ServerExchangeResult1 = serverExchangeResult[0];
            evnt1.ServerExchangeResult2 = serverExchangeResult[1];
            evnt1.ServerDamage1 = serverDamage[0];
            evnt1.ServerDamage2 = serverDamage[1];
            evnt1.ServerDecision = (int) _playerManager.decision;
            
            evnt1.Send();
            //Debug.LogWarning(this.name + "  EFExchangeResultReady send. ev.ServerDecision = " + evnt1.ServerDecision);
            
            _gameManager.doServerExchange = true;
        }
*/
    }

    public override void OnEvent(EFInventoryItemAdded evnt)
    {
        if (!evnt.FromSelf)
        {
            string str = evnt.ItemName; 
            Debug.LogWarning(this.name + " Имя предмета инвентаря = " + str);
            Item item = AllItems.Instance.items.SingleOrDefault(s => s.Name == str);
            if (item != null) _enemyManager.SetInventory(item);
        }
    }
    
    public override void Disconnected(BoltConnection connection)
    {
        BoltLauncher.Shutdown();
        //GameManager.ClientConnected = false;
        //GameManager.ClientDisconnected = true;
    }
    public override void BoltShutdownBegin(AddCallback registerDoneCallback, 
        UdpConnectionDisconnectReason disconnectReason = UdpConnectionDisconnectReason.Disconnected)
    {
        registerDoneCallback(() =>
        {
// ВЕРНУТЬСЯ при фотоне             GameManager.ClientConnected = false;
// ВЕРНУТЬСЯ при фотоне             GameManager.ClientDisconnected = true;
        });
    }
}


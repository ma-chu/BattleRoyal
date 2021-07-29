using System;
using Bolt;
using Bolt.Matchmaking;
using UdpKit;
using UnityEngine;
using System.Linq;

// Типа GameManager'а, прикрепляем к постоянному объекту на сцене
// [BoltGlobalBehaviour(BoltNetworkModes.Host, "Level2")] - если указать такой атрибут, то Bolt породит инстанс скрипта самостоятельно без прикрепления к объекту сцены
// (только на сервере и только для сцены Level2)

[BoltGlobalBehaviour(BoltNetworkModes.Server, "Main")]

public class ServerNetworkCallbacks : Bolt.GlobalEventListener
{
    /* Регистрировать токены либо так, либо Bolt / Protocol Tokens Registry / Refresh
       Не использую токены
     public override void BoltStartBegin()
      {
          BoltNetwork.RegisterTokenClass<PlayerClientToken>();
          BoltNetwork.RegisterTokenClass<PlayerServerToken>();
      }
   */

 private GameManager _gameManager;
 private HeroManager _playerManager;
 private HeroManager _enemyManager;

 private void Awake()
    {
        PlayerObjectRegisty.CreateServerPlayer();
    }

   public override void Connected(BoltConnection connection)
    {
        PlayerObjectRegisty.CreateClientPlayer(connection);
    }
    

    public override void Disconnected(BoltConnection connection)
    {
        GameManager.ClientConnected = false;
    }
    
    // ф-ия-событие, когда удаленная сцена (клиента) болта стартанула
    public override void SceneLoadRemoteDone(BoltConnection connection, IProtocolToken token)
    {
        
        var playerObject= PlayerObjectRegisty.GetPlayer(connection);
        playerObject.Spawn();        // если сущность клиента успешна передана в токене, сервер не порождает сущность сам, а берет из токена
        
        // В качестве противника передаем клиенту себя (в событии пока не получается, см. PlayerEntityController / Attached)
        connection.SetCanReceiveEntities(true);
        var evnt = EFStartBattleServerEvent.Create(connection);          // Создаем событие только для этого соединения
        evnt.EnemyEntity = PlayerObjectRegisty.ServerPlayer.character;
        evnt.Send();
        
        // Так было бы с токеном
        /*
        var serverToken = new PlayerServerToken();
        serverToken.enemyEntity = PlayerObjectRegisty.ServerPlayer.character;
        var evnt = EFStartBattleEvent.Create(connection);          
        evnt.EnemyEntity = serverToken;
        evnt.Send();  
        */   
    }

    public override void OnEvent(EFStartBattleClientReplyEvent evnt)
    {
        GameManager.enemyBoltEntity = evnt?.clientEntity;
        var enemyState = evnt?.clientEntity.GetState<IEFPlayerState>();

        Debug.LogWarning(this.name + " Client entity recieved. Name = " + enemyState.Username); 
        
        GameManager.ClientConnected = true; 
    }

    

    // ф-ия-событие, когда локальныая сцена (здесь сервера)  (или сервера/клиента, если бы не было флага BoltNetworkModes.Server) болта стартанула
    public override void SceneLoadLocalDone(string scene, IProtocolToken token)
    {
        var playerObject = PlayerObjectRegisty.ServerPlayer;
        playerObject.Spawn();
        var poEntity = playerObject.character;                                            
        GameManager.myBoltEntity = poEntity;
        var myState = poEntity.GetState<IEFPlayerState>();
        myState.Username = PlayerPrefs.GetString("username");            
        
        Debug.LogWarning(this.name + "  My(Server's) BoltEntity created. My name is " + myState.Username);    

        poEntity.GetComponent<PlayerEntityController>().SetLinks();
        
        _gameManager = GameManager.instance;
        _playerManager = GameManager.instance.m_Player;
        _enemyManager = GameManager.instance.m_Enemy;
    }


    public override void OnEvent(EFReadyForExchangeEvent evnt)
    {
        if (!evnt.FromSelf)
        {
            _enemyManager.decision = (Decision) evnt.Decision;
            _enemyManager.defencePart = evnt.DefencePart;
        }

        if ((_enemyManager.decision != Decision.No) && (_playerManager.decision != Decision.No))
        {
            _gameManager.MakeMultiplayerEnemyDecision(_enemyManager.decision, _enemyManager.defencePart,
                out int[] clientExchangeResult, out int[] clientDamage, out int[] serverExchangeResult,
                out int[] serverDamage
            );

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
}


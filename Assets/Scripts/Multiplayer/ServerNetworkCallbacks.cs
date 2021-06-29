using System;
using Bolt;
using Bolt.Matchmaking;
using UdpKit;
using UnityEngine;
using System.Linq;

// Типа GameManager'а, прикрепляем к постоянному объекту на сцене
// [BoltGlobalBehaviour(BoltNetworkModes.Host, "Level2")] - если указать такой атрибут, то Bolt породит инстанс скрипта самостоятельно без прикрепления к объекту сцены
// (только на сервере и только для сцены Level2)

[BoltGlobalBehaviour(BoltNetworkModes.Server)]

public class ServerNetworkCallbacks : Bolt.GlobalEventListener
{
   // [SerializeField] private Transform root;
   // private PlayerClientToken clientToken;
   
   
 /*  public override void BoltStartBegin()
   {
       BoltNetwork.RegisterTokenClass<PlayerClientToken>();
       BoltNetwork.RegisterTokenClass<PlayerServerToken>();
   }*/
   
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
        
        var po=PlayerObjectRegisty.GetPlayer(connection);
        po.Spawn();
        //var poState  = po.character.GetState<IEFPlayerState>();                             //25
        var poState  = po.character;                                                //25

        // (токены не передаются, если интернет раздаешь с телефона и на нем же клиент!!!. Поэтому NoNamePlayer)
        var clientToken = connection?.ConnectToken as PlayerClientToken;
        //Debug.Log("SceneLoadRemoteDone: "+ clientToken);                                    //25
        //poState.Username = clientToken?.username??"NoNamePlayer";                            //25
        poState.GetState<IEFPlayerState>().Username = clientToken?.username ?? "NoNamePlayer"; 
        GameManager.enemyBoltState = poState;
        //Debug.LogWarning(this.name + " Clients BoltEntity created. His name is " + GameManager.enemyBoltState.Username);    //25
        Debug.LogWarning(this.name + " Clients BoltEntity created. His name is " + GameManager.enemyBoltState.GetState<IEFPlayerState>().Username);    //25

        // В качестве противника передаем клиенту себя (в событи пока не получается, см. PlayerEntityController / Initialize)
        //connection.SetCanReceiveEntities(true);
        var serverToken = new PlayerServerToken();
        serverToken.enemyEntity = PlayerObjectRegisty.ServerPlayer.character;
        var evnt = EFStartBattleEvent.Create(connection);          // Создаем событие только для этого соединения
        evnt.EnemyData = serverToken;
        evnt.Send();                                               // Отправляем событие
        
        GameManager.ClientConnected = true;
    }
    

    // ф-ия-событие, когда локальныая сцена (сервера здесь или сервера/клиента, если бы не было флага BoltNetworkModes.Server) болта стартанула
    public override void SceneLoadLocalDone(string scene, IProtocolToken token)
    {
        // порождаем из префаба сущность, которая будет содержать сетевые данные о герое
        //BoltEntity be = BoltNetwork.Instantiate(BoltPrefabs.HeroBoltEntity, Vector3.zero, Quaternion.identity);
        //be.transform.SetParent(root);

        var po = PlayerObjectRegisty.ServerPlayer;
        po.Spawn();
        //var poState  = po.character.GetState<IEFPlayerState>();                        //25
        var poState = po.character;                                            //25
        GameManager.myBoltState = poState;
        //GameManager.myBoltState.Username = PlayerPrefs.GetString("username");            //25
        GameManager.myBoltState.GetState<IEFPlayerState>().Username = PlayerPrefs.GetString("username");            //25
        
        //Debug.LogWarning(this.name + "  My BoltEntity created. My name is " + GameManager.myBoltState.Username);            //25
        Debug.LogWarning(this.name + "  My BoltEntity created. My name is " + GameManager.myBoltState.GetState<IEFPlayerState>().Username);        //25

    }
}

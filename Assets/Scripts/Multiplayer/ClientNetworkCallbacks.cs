﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;
using UdpKit;
using System.Linq;

[BoltGlobalBehaviour(BoltNetworkModes.Client, "Main")]
public class ClientNetworkCallbacks : GlobalEventListener
{ 
    private GameManager _gameManager;
    private PlayerManager _playerManager; 
    private EnemyManager _enemyManager;

    public override void SceneLoadLocalDone(string scene, IProtocolToken token)
   {
       _gameManager = GameManager.Instance;
       _playerManager = GameManager.Instance.m_Player;
       _enemyManager = GameManager.Instance.m_Enemy;
   }
   
   
   // Отслеживаем посланное сервером событие
   public override void OnEvent(EFStartBattleServerEvent evnt)
   {
       if ((evnt?.EnemyEntity) != null)
       {
           GameManager.enemyBoltEntity = evnt?.EnemyEntity;
           Debug.LogWarning(this.name + " Server entity recieved. Name = " + GameManager.enemyBoltEntity.GetState<IEFPlayerState>().Username); 
       }

       var evnt1 = EFStartBattleClientReplyEvent.Create();   
       var entity = BoltNetwork.Instantiate(BoltPrefabs.HeroBoltEntity, Vector3.zero, Quaternion.identity); 
       entity.GetState<IEFPlayerState>().Username = PlayerPrefs.GetString("username");
       evnt1.clientEntity = entity;
       evnt1.Send();
       
       GameManager.myBoltEntity = entity;
       
       GameManager.ClientConnected = true;
   }

   public override void OnEvent(EFExchangeResultsReady evnt)
   {
       _playerManager.exchangeResult[0] = (ExchangeResult) evnt.ClientExchangeResult1;
       _playerManager.exchangeResult[1] = (ExchangeResult) evnt.ClientExchangeResult2;
       _playerManager.gotDamage[0] = evnt.ClientDamage1;
       _playerManager.gotDamage[1] = evnt.ClientDamage2;
       
        // для вычисления коэфф-в сильных ударов
       _playerManager.preCoeffs[0].damage = evnt.ServerDamage1;    
       _playerManager.preCoeffs[1].damage = evnt.ServerDamage2; 
       _enemyManager.preCoeffs[0].damage = evnt.ClientDamage1;
       _enemyManager.preCoeffs[1].damage = evnt.ClientDamage2;

       _enemyManager.exchangeResult[0] = (ExchangeResult) evnt.ServerExchangeResult1;
       _enemyManager.exchangeResult[1] = (ExchangeResult) evnt.ServerExchangeResult2;
       _enemyManager.gotDamage[0] = evnt.ServerDamage1;
       _enemyManager.gotDamage[1] = evnt.ServerDamage2;

       _enemyManager.decision = (Decision) evnt.ServerDecision;
       
       _gameManager.doClientExchange = true;
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

   /* public override void ConnectFailed(UdpEndPoint endpoint, IProtocolToken token)
     {
         GameManager.ClientConnected = false;
     }*/
}

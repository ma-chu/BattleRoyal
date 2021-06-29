using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;
using UdpKit;
using System.Linq;

[BoltGlobalBehaviour(BoltNetworkModes.Client)]
public class ClientNetworkCallbacks : GlobalEventListener
{
   /* public override void SessionConnected(UdpSession session, IProtocolToken token)
    {
        
        GameManager.enemyBoltState = PlayerObjectRegisty.ServerPlayer.character.GetState<IEFHeroState>();
        GameManager.myBoltState = PlayerObjectRegisty.GetPlayer(BoltNetwork.Server).character.GetState<IEFHeroState>();
        
        GameManager.myBoltState.Username = PlayerPrefs.GetString("username");
        
        GameManager.ClientConnected = true;
        

        Debug.LogWarning("I joined to " + GameManager.enemyBoltState.Username + " server");
    }*/
   
   // Отслеживаем посланное сервером событие
   public override void OnEvent(EFStartBattleEvent evnt)
   {
       /*Debug.LogWarning(evnt.Username + " joined");
       GameManager.ClientConnected = true;

       //Найти сущность партнера!!!
       //if (BoltNetwork.IsServer)
       var bea = FindObjectsOfType<BoltEntity>();
       foreach (var be in bea)
       {
           var state = be.GetState<IEFHeroState>();
           if (state == GameManager.myBoltState) continue;
           GameManager.enemyBoltState = state;
       }
       Debug.LogWarning(" enemyBoltState = " + GameManager.enemyBoltState + " " + GameManager.enemyBoltState.Username);
       */
       
       var serverToken = evnt.EnemyData as PlayerServerToken;
       if((serverToken?.enemyEntity)!=null) GameManager.enemyBoltState = serverToken?.enemyEntity/*?.GetState<IEFPlayerState>()*/;            //25

       //Debug.LogWarning(this.name + ". enemyBoltState = " + GameManager.enemyBoltState?.Username);                                          //25
       Debug.LogWarning(this.name + ". enemyBoltState = " + GameManager.enemyBoltState.GetState<IEFPlayerState>().Username);         //25

       GameManager.ClientConnected = true;
   }

   public override void ConnectFailed(UdpEndPoint endpoint, IProtocolToken token)
    {
        GameManager.ClientConnected = false;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bolt;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerEntityController : EntityBehaviour<IEFPlayerState>
{
    private PlayerManager _playerManager;
    private GameManager _gameManager;
    //private EnemyManager _enemyManager;

    public override void Attached() // аналог Start()
    {
        // !!!Не ясно почему, но entity.HasControl не встает на сервере после выполнения ф-ии character.TakeControl() в PlayerObject/Spawn!!!
        /*Debug.LogWarning(this.name + " BoltNetwork.IsClient = " + BoltNetwork.IsClient + " BoltNetwork.IsServer = " + BoltNetwork.IsServer +
                         " entity.IsOwner = " + entity.IsOwner + " entity.HasControl = " + entity.HasControl);*/

        // Если сервер, то присваивание enemy (плюс вызов SetLinks) произойдет в ServerNetworkCallbacks/OnConnected (SceneLoadLocalDone)
        // Если клиент, то здесь
        if (BoltNetwork.IsClient)
        {
            if (entity.IsOwner)
            {
                if (GameManager.myBoltEntity == null) GameManager.myBoltEntity = entity;            //страховка. null не должно быть                                                                                 
                SetLinks();
            }
            
            // так, пока не научусь передавать ссылку на сущность соперника через событие (EFStartBattleEvent в SceneLoadRemoteDone) --> научился
            else
            {
                if (GameManager.enemyBoltEntity == null) GameManager.enemyBoltEntity = entity;     //страховка. null не должно быть 
            }
        }
        else    //server
        {
            // так, пока не научусь передавать ссылку на сущность соперника через clientToken (ServerNetworkCallbacks / SceneLoadRemoteDone) --> научился
            if (!entity.IsOwner)
            {
                PlayerObjectRegisty.GetPlayer(entity.Source).character = entity;
                if (GameManager.enemyBoltEntity == null) GameManager.enemyBoltEntity = entity;     //страховка. null не должно быть 
            }
        }
    }

    public override void SimulateOwner()        // одинаково для игрока-клиента и игрока-сервера
    {
        if (!_gameManager.doClientExchange && !_gameManager.doServerExchange)
        {
            state.Decision = (int) _playerManager.decision;
            state.defencePart = _playerManager.defencePart;
        }
    }

    public void SetLinks()                     // установить ссылки на свои GameManager & PlayerManager
    {
        _playerManager = GameManager.instance.m_Player;
        _gameManager = GameManager.instance;

        state.AddCallback("Decision", DecisionCallback);
        state.AddCallback("InventoryItem", InventoryCallback);
    }

    private void DecisionCallback()    
    {
        if (state.Decision != 0)
        {
            var evnt = EFReadyForExchangeEvent.Create();
            evnt.Decision = state.Decision;
            evnt.DefencePart = state.defencePart;
            evnt.Send();

            // Если делать с командой, то как-то так
            //IEFReadyForExchangeCommandInput input = EFReadyForExchangeCommand.Create();
            //input.Decision = (int) /*_decision*/state.Desicion;
            //input.DefencePart = /*_defencePart*/state.defencePart;
            //entity.QueueInput(input); // вот этой командой мы посылаем входы и на сервер, и на клиент ?для предсказания?
        }
    }

    public void InventoryCallback()
    {
        var evnt = EFInventoryItemAdded.Create();
        evnt.ItemName = state.InventoryItem;
        evnt.Send();
    }
    
/*  Если делать с командой, то как-то так
    public override void ExecuteCommand(Command command, bool resetState)
    {
        EFReadyForExchangeCommand cmd = (EFReadyForExchangeCommand)command;

        if (resetState)
        {
            // Тут сервер исправляет посчитанное контроллером - выполняется только на контроллере!

                _playerManager.exchangeResult[0] = (ExchangeResult) cmd.Result.ExchangeResult1;
                _playerManager.exchangeResult[1] = (ExchangeResult) cmd.Result.ExchangeResult2;
                _playerManager.gotDamage[0] = cmd.Result.Damage1;
                _playerManager.gotDamage[1] = cmd.Result.Damage2;

                GameManager.doClientExchange = true;
        }
        else
        {
            // Тут считают по отдельности и сервер, и контроллер
            _gameManager.MakeMultiplayerEnemyDesicion(cmd.Input.Decision, cmd.Input.DefencePart);
            if (!entity.HasControl)   
            {    // не обязательно?
                cmd.Result.ExchangeResult1 = (int) _playerManager.exchangeResult[0];
                cmd.Result.ExchangeResult2 = (int) _playerManager.exchangeResult[1];
                cmd.Result.Damage1 = _playerManager.gotDamage[0];
                cmd.Result.Damage2 = _playerManager.gotDamage[1];
            }
            else
            {
                cmd.Result.ExchangeResult1 = localEnemyBoltState.ExchangeResult[0];
                cmd.Result.ExchangeResult2 = localEnemyBoltState.ExchangeResult[1];
                cmd.Result.Damage1 = localEnemyBoltState.Damage[0];
                cmd.Result.Damage2 = localEnemyBoltState.Damage[1];
            }
        }
    }
*/
    /* public override void Detached() 
    {
        var evnt = EFPLayerLeftEvent.Create();
        evnt.Send(); 
    }*/
}

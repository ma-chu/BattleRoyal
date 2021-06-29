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
    private EnemyManager _enemyManager;
    private GameManager _gameManager;

    // inputs for command
    private Decision _decision;
    private float _defencePart;
    private Decision _enemyDecision;

    private BoltEntity serverPlayerEntity;
    
 //  public override void Initialized(){} //  перед Attached()


    public override void Attached() // аналог Start()
    {
        // Если сервер, то присваивание enemy произойдет в ServerNetworkCallbacks/OnConnected
        // Если клиент, то здесь
        
        // !!!Не ясно почему, но entity.HasControl не встает на сервере после выполнения ф-ии character.TakeControl() в PlayerObject/Spawn!!!
        /*Debug.LogWarning(this.name + " BoltNetwork.IsClient = " + BoltNetwork.IsClient + " BoltNetwork.IsServer = " + BoltNetwork.IsServer +
                         " entity.IsOwner = " + entity.IsOwner + " entity.HasControl = " + entity.HasControl);*/

        if (BoltNetwork.IsClient)
        {
            if (entity.HasControl)//||
            {
                //state.Username = PlayerPrefs.GetString("username");
                GameManager.myBoltState = /*state*/entity;                                                                                 //25
                //Debug.LogWarning(this.name + ". myBoltState = " + GameManager.myBoltState.Username);                                     //25
                Debug.LogWarning(this.name + ". myBoltState = " + GameManager.myBoltState.GetState<IEFPlayerState>().Username);   //25

                //GameManager.ClientConnected = true;
            }
            
            // так, пока не научусь передавать ссылку на сущность соперника через событие (EFStartBattleEvent в SceneLoadRemoteDone) -->
            else
            {
                GameManager.enemyBoltState = /*state*/entity;                                                                                 //25
                //Debug.LogWarning(this.name + ". enemyBoltState = " + GameManager.enemyBoltState.Username);                                  //25
                Debug.LogWarning(this.name + ". enemyBoltState = " + GameManager.enemyBoltState.GetState<IEFPlayerState>().Username);//25
            }
            // <--
        }
        
        if ((GameManager.enemyBoltState!=null)&&(GameManager.myBoltState!=null))
        {
            if(BoltNetwork.IsClient) Debug.LogWarning(this.name + " I, " + GameManager.myBoltState.GetState<IEFPlayerState>().Username + ", joined to " + GameManager.enemyBoltState.GetState<IEFPlayerState>().Username + " server");//25
            if(BoltNetwork.IsServer) Debug.LogWarning( this.name + GameManager.myBoltState.GetState<IEFPlayerState>().Username + " joined to my, " + GameManager.enemyBoltState.GetState<IEFPlayerState>().Username + ", server");//25
        }
    }

    private void Start()
    {
        serverPlayerEntity = PlayerObjectRegisty.ServerPlayer.character;
    }

    private void Update()
    { 
        if ((entity.HasControl)||(entity==serverPlayerEntity))   // одинаково для игрока-клиента и игрока-сервера
        {
            if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Main"))
            {
                if (_playerManager == null)
                {
                    _playerManager = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerManager>();
                    _enemyManager = GameObject.FindGameObjectWithTag("Enemy")?.GetComponent<EnemyManager>();
                    _gameManager = GameObject.FindGameObjectWithTag("GameController")?.GetComponent<GameManager>();

                    if (entity.HasControl)
                    {
                        _enemyDecision = _enemyManager.decision;
                        _decision = _playerManager.decision; // чтоб сразу не срабатывало - попробуй убрать
                        state.AddCallback("_decision", DecisionCallback);
                        state.AddCallback("_enemyDecision", DecisionCallback);
                    }
                }
                
                _enemyDecision = _enemyManager.decision;
                _decision = _playerManager.decision;
                _defencePart = _playerManager.defencePart;
            }
        }
    }

    public void DecisionCallback()    
    {
        if ((_decision != Decision.No) && (_enemyDecision != Decision.No))
        {
            IEFReadyForExchangeCommandInput input = EFReadyForExchangeCommand.Create();
            input.Decision = (int) _decision;
            input.DefencePart = _defencePart;
            entity.QueueInput(input); // вот этой командой мы посылаем входы и на сервер, и на клиент ?для предсказания?
        }
    }

    public override void ExecuteCommand(Command command, bool resetState)
    {
        EFReadyForExchangeCommand cmd = (EFReadyForExchangeCommand)command;

        if (resetState)
        {
            // Тут сервер исправляет посчитанное контроллером
            if (!entity.HasControl)
            {
                _playerManager.exchangeResult[0] = (ExchangeResult) cmd.Result.ExchangeResult1;
                _playerManager.exchangeResult[1] = (ExchangeResult) cmd.Result.ExchangeResult2;
                _playerManager.gotDamage[0] = cmd.Result.Damage1;
                _playerManager.gotDamage[1] = cmd.Result.Damage2;
            }
            else
            {
                _enemyManager.exchangeResult[0] = (ExchangeResult) cmd.Result.ExchangeResult1;
                _enemyManager.exchangeResult[1] = (ExchangeResult) cmd.Result.ExchangeResult2;
                _enemyManager.gotDamage[0] = cmd.Result.Damage1;
                _enemyManager.gotDamage[1] = cmd.Result.Damage2;
            }
        }
        else
        {
            // Тут считаем (и сервер, и контроллер)
            _gameManager.MakeMultiplayerEnemyDesicion(cmd.Input.Decision, cmd.Input.DefencePart);
            if (!entity.HasControl)
            {
                cmd.Result.ExchangeResult1 = (int) _playerManager.exchangeResult[0];
                cmd.Result.ExchangeResult2 = (int) _playerManager.exchangeResult[1];
                cmd.Result.Damage1 = _playerManager.gotDamage[0];
                cmd.Result.Damage2 = _playerManager.gotDamage[1];
            }
            else
            {
                cmd.Result.ExchangeResult1 = (int) _enemyManager.exchangeResult[0];
                cmd.Result.ExchangeResult2 = (int) _enemyManager.exchangeResult[1];
                cmd.Result.Damage1 = _enemyManager.gotDamage[0];
                cmd.Result.Damage2 = _enemyManager.gotDamage[1];
            }
        }
    }

    /* public override void Detached() 
    {
        var evnt = EFPLayerLeftEvent.Create();
        evnt.Send(); 
    }*/

  /*  public override void ControlGained()
    {
        // Если мы внутри этого события, то значит, это уже сущность нашего героя
        if (BoltNetwork.IsClient)
        {
            GameManager.myBoltState = state;
            state.Username = PlayerPrefs.GetString("username");

            GameManager.ClientConnected = true;
            //Debug.LogWarning("I joined to " + GameManager.enemyBoltState.Username + " server");
        }
    }*/

}

//0. владельцем entity должен быть клиент
//1. инвентарь нужно передавать на сервер
//2. Результат схода и урон вычисляет сервер и как-то передает всем: очевидно, еще 1 сущность "результаты"


// на клиенте: по изменению Decision создавать событие (callback) и отправлять на сервер. Расчет урона самому не выполнять, ожидать ответного события!
// на сервере: как только есть это событие и решение серверного игрока, выполняем расчет урона и генерим ответное событие. (плюс выполнить анимацию и пр) 
// на клиенте: отлавливаем ответное событие и выполняем анимацию и пр.  
using Photon.Bolt;

public class PlayerEntityController : EntityBehaviour<IEFPlayerState>
{
    private PlayerManager _playerManager;
    private GameManager _gameManager;
    //private EnemyManager _enemyManager;

    public override void Attached() // аналог Start()
    {
        // !!!Не ясно почему, но entity.HasControl не встает на сервере после выполнения ф-ии character.TakeControl() в PlayerObject.Spawn()!!!
        /*Debug.LogWarning(this.name + " BoltNetwork.IsClient = " + BoltNetwork.IsClient + " BoltNetwork.IsServer = " + BoltNetwork.IsServer +
                         " entity.IsOwner = " + entity.IsOwner + " entity.HasControl = " + entity.HasControl);*/

        // Если сервер, то присваивание своего entity (плюс вызов SetLinks) произойдет в ServerNetworkCallbacks.SceneLoadLocalDone
        // Если клиент, то присваивание своего entity произойдет в ClientNetworkCallbacks.OnEvent(EFStartBattleServerEvent evnt), а SetLinks здесь
        if (BoltNetwork.IsClient)
            if (entity.IsOwner)
            {
                // (GameManager.myBoltEntity == null) GameManager.myBoltEntity = entity;            // страховка. null не должно быть                                                                                 
                SetLinks();
            }
    }

    public override void SimulateOwner()        // Аналог Update. Одинаково для игрока-клиента и игрока-сервера
    {
/* ВЕРНУТЬСЯ при фотоне
        if (!_gameManager.doClientExchange && !_gameManager.doServerExchange)
        {
            state.Decision = (int) _playerManager.decision;
            state.defencePart = _playerManager.defencePart;
        }
*/
    }

    public void SetLinks()                     // установить ссылки на свои GameManager & PlayerManager
    {
//  ВЕРНУТЬСЯ при фотоне        _playerManager = GameManager.Instance.player;
        _gameManager = GameManager.Instance;

        state.AddCallback("Decision", DecisionCallback);
        state.AddCallback("InventoryItem", InventoryCallback);
    }

    private void DecisionCallback()            // Callback вызывается, когда меняется св-во стейта Desicion (типа UniRx)
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

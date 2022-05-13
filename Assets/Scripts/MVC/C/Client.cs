using System;
using System.Collections;
using EF.Localization;
using EF.Sounds;
using UnityEngine;

/* Существует 2 типа клиентов: игрок (реализация клиента локального и удаленного игроков одна и та же) и AI
  При этом 3 типа игры:
    SinglePlayer, то создается 2 клиента (на объекте GameManager?) помимо сервера: локальный игрок и AI;
    MultiplayerServer, то 1 игрок (и сервер);
    MultiplayerClient, то 1 игрок (и паттерн-адаптер IServer->photon в отдельном классе (_server будет указывать на этот адаптер))
  
  1. Если созданный клиент AI, реализовать для из него вывод входной информации серверу (TurnInInfo) из ф-ии MakeDesition
  2. Если созданный клиент игрок, реализовать для него ввод входной информации с кнопок (PlayerUI->ViewModel->Client)
     ViewModel будет содержать поля с данными для всех View: PlayerUI, EnemyUI, PlayerAnimation, EnemyAnimation и CommonView
     Уведомление всех View с помощью событий, описанных и интерфейсе (поля тоже в интерфейс)
     Основная петля крутится тоже во ViewModel

  Итого схема наследования:
    Client --> AIClient: реализация OnTurnInDataReady в виде ф-ии MakeDesition
           --> PlayerClient: + ViewModel, реализация OnTurnInDataReady с кнопок (PlayerUI->ViewModel)
*/

// Интерфейс между сервером и клиентом. Его должен реализовать сервер
public interface IServer
{
    void Join(string name, EventHandler<string> onTournamentJoined);
    void SubscribeOnStartMatch(EventHandler<StartMatchInfo> onStartMatch);
    void TakeDecision(string name,TurnInInfo turnInInfo);
    void SubscribeOnResultsReady(EventHandler<TurnOutInfo> onResultsReady);
    void SubscribeOnEndMatch(EventHandler<EndMatchInfo> onEndMatch);
    void SubscribeOnStartRound(EventHandler<StartRoundInfo> onStartRound);
    void SubscribeOnEndRound(EventHandler<EndRoundInfo> onEndRound);
}
public struct StartMatchInfo
{
    public string PlayerName;
    public string EnemyName;
    public string[] PlayerInventoryItems;
    public string[] EnemyInventoryItems;
}
public struct EndMatchInfo
{
    public string PlayerName;
    public string matchWinner;
}
public struct StartRoundInfo
{
    public string PlayerName;
    public int roundNumber;
    public int PlayerStartHealth;
    public int EnemyStartHealth;
}
public struct EndRoundInfo
{
    public string PlayerName;
    public string roundWinner;
    public string prize;
}
public struct TurnInInfo
{
    public Decision PlayerDecision { get; set; }
    public float PlayerDefencePart { get; set; }
}
public struct TurnOutInfo
{
    public string PlayerName;
    public Decision EnemyDecision;
    public ExchangeResult[] PlayerExchangeResults;
    public ExchangeResult[] EnemyExchangeResults;
    public int[] PlayerDamages;
    public int[] EnemyDamages;
    public int PlayerHP;
    public int EnemyHP;
    public int[] PlayerSeries;
    public int[] EnemySeries;
}

public class Client
{
    private IServer _server;
    protected TurnOutInfo currentResults;

    public const int NumRoundsToWin = 4;                // правильнее получать от сервера в начале матча
    protected int RoundNumber;
    public int roundsWon;
    protected int roundsLost;
    
    public string PlayerName { get; private set; }

    public Decision decision;
    public WeaponSet PlayerWeaponSet = WeaponSet.SwordShield;      
    public WeaponSet EnemyWeaponSet = WeaponSet.SwordShield;      

    // серия набрана
    protected bool PlayerStrongStrikesSeries;                       
    protected bool PlayerSeriesOfStrikes;                           
    protected bool PlayerSeriesOfBlocks;                            
    protected bool EnemyStrongStrikesSeries;                   
    protected bool EnemySeriesOfStrikes;                        
    protected bool EnemySeriesOfBlocks;                        


    public virtual void Init(string name)
    {
        PlayerName = name;
        Join(GameManager.server);
    }

    private void Join(IServer server)                    
    {
        _server = server;
        _server.Join(PlayerName, OnJoined);
    }
    
    protected virtual void OnJoined(object o, string e)
    {
        if (!e.Equals(PlayerName)) return;
        
        _server.SubscribeOnStartMatch(OnStartMatch);
        _server.SubscribeOnResultsReady(OnResultsReady);
        _server.SubscribeOnStartRound(OnStartRound);
        _server.SubscribeOnEndRound(OnEndRound);
        _server.SubscribeOnEndMatch(OnEndMatch);
    }

    protected virtual void OnStartMatch(object o, StartMatchInfo startMatchInfo)
    {
        if (!startMatchInfo.PlayerName.Equals(PlayerName)) return;

        roundsWon = roundsLost = 0;
    }
    protected virtual void OnResultsReady(object o, TurnOutInfo results)
    {
        if (!results.PlayerName.Equals(PlayerName)) return;
        
        currentResults = results;

        // обработать результаты хода
        // 1. При смене оружия врагом поменять его weaponSet (а свой поменяем при вводе с кнопок)
        switch (results.EnemyDecision)
        {
            case Decision.ChangeSwordShield:
                EnemyWeaponSet = WeaponSet.SwordShield;
                break;
            case Decision.ChangeSwordSword:
                EnemyWeaponSet = WeaponSet.SwordSword;
                break;
            case Decision.ChangeTwoHandedSword:
                EnemyWeaponSet = WeaponSet.TwoHandedSword;
                break;
        }

        // 2. Определить, есть ли серии у меня и противника
        CheckForSeries();
        
        // 3. Сам процесс хода
        MakeTurn(roundsLost);
        // AI: Определяется с действие бота (nicety - уровень интеллекта врага) /+ вызвать SendDataToServer()/
        // Player:  через ViewModel отображает анимации, звуки и пр., ожидает TurnInInfo с кнопок
    }

    protected virtual void CheckForSeries() { }
    
    protected virtual void MakeTurn(int nicety) { }

    public void SendDataToServer(TurnInInfo t) => _server.TakeDecision(PlayerName, t);   // по нажатию кнопки решения у player'а или по выполнении ф-ии MakeTurn AI

    protected virtual void OnStartRound(object o, StartRoundInfo startRoundInfo)
    {
        RoundNumber = startRoundInfo.roundNumber;
        PlayerWeaponSet = EnemyWeaponSet = WeaponSet.SwordShield;
    }
    
    protected virtual void OnEndRound(object o, EndRoundInfo endRoundInfo)
    {
        if (!endRoundInfo.PlayerName.Equals(PlayerName)) return;

        if (endRoundInfo.roundWinner == PlayerName)
            roundsWon++;
        else if (!endRoundInfo.roundWinner.Equals(string.Empty)) roundsLost++;
        
        // обнулить серии
        PlayerStrongStrikesSeries = PlayerSeriesOfBlocks = PlayerSeriesOfStrikes = false;
        EnemyStrongStrikesSeries = EnemySeriesOfBlocks = EnemySeriesOfStrikes = false;
    }
    
    protected virtual void OnEndMatch(object o, EndMatchInfo endMatchInfo) { }
}
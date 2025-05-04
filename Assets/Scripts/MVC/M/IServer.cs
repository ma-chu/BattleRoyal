using System;

/// <summary>
/// Существует 2 типа клиентов: игрок (реализация клиента локального и удаленного игроков одна и та же) и AI
/// При этом 3 типа игры:
///   SinglePlayer, то создается 2 клиента (на объекте GameManager?) помимо сервера: локальный игрок и AI;
///   MultiplayerServer, то 1 игрок (и сервер);
///   MultiplayerClient, то 1 игрок (и паттерн-адаптер IServer->photon в отдельном классе (_server будет указывать на этот адаптер))
///
/// 1. Если созданный клиент AI, реализовать для из него вывод входной информации серверу (TurnInInfo) из ф-ии MakeDecision
/// 2. Если созданный клиент игрок, реализовать для него ввод входной информации с кнопок (PlayerUI->ViewModel->Client)
///    ViewModel будет содержать поля с данными для всех View: PlayerUI, EnemyUI, PlayerAnimation, EnemyAnimation и CommonView
///    Уведомление всех View с помощью событий, описанных и интерфейсе (поля тоже в интерфейс)
///    Основная петля крутится тоже во ViewModel
///
/// Cхема наследования:
/// Client --> AIClient: реализация OnTurnInDataReady в виде ф-ии MakeDecision
///        --> PlayerClient: + ViewModel, реализация OnTurnInDataReady с кнопок (PlayerUI->ViewModel)
/// </summary>

public interface IServer
{
    void Join(string name, EventHandler<string> onTournamentJoined);
    void SubscribeOnStartMatch(EventHandler<StartMatchInfo> onStartMatch);
    void TakeDecision(string playerName,TurnInInfo turnInInfo);
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
    public string MatchWinner;
}

public struct StartRoundInfo
{
    public string PlayerName;
    public int RoundNumber;
    public int PlayerStartHealth;
    public int EnemyStartHealth;
}

public struct EndRoundInfo
{
    public string PlayerName;
    public string RoundWinner;
    public string Prize;
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
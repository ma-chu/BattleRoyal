using System.Collections;                                 // для сопрограмм
using UnityEngine;
using UnityEngine.UI;
using System;
using EF.Localization;
using EF.Sounds;
using EF.Tools;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

// #pragma warning disable 0649    // убирает предупреждения компилятора о [SerializeField] private переменных, инициализируемых в редакторе   

public enum WeaponSet : short { SwordShield, SwordSword, TwoHandedSword };                             // варианты сетов оружия у героя
public enum Heroes : short { Player, Enemy, Nobody };                                                  // варианты героев (победителей раундов и игры)

public enum Decision : short {No, Attack, ChangeSwordShield, ChangeSwordSword, ChangeTwoHandedSword};  // варианты действий героя - импульс на 1 такт
public enum ExchangeResult : short { No, Evade, Parry, BlockVs2Handed, Block, GetHit };                // варианты исхода размена ударами для каждого из 2 ударов противника
public enum GameType : short { Single, Server, Client };                                               // тип игры

public class GameManager : MonoBehaviour {
    private static GameManager _instance;                 
    public static GameManager Instance => _instance;

    public static IServer server;

    public static GameType gameType;

    private void Awake()
    {
        _instance ??= this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        //if (!SceneManager.GetSceneByName("Start").isLoaded) SceneManager.LoadScene ("Start", LoadSceneMode.Additive);
        if (!SceneManager.GetSceneByBuildIndex(1).isLoaded) SceneManager.LoadScene (1, LoadSceneMode.Additive);
    }

    public void StartGame(GameType type)
    {
        gameType = type;
        switch (gameType)
        {
            case GameType.Single:
                server = Server.Instance;
                SceneManager.LoadScene (2, LoadSceneMode.Single); // LoadScene, в отличие от LoadSceneAcync, делает ее активной?
                break;
            case GameType.Server:
                server = Server.Instance;
                //gameObject.AddComponent<ServerPhotonAdapter>();
                //new ServerPhotonAdapter();
                break;
            case GameType.Client:
                Server.Instance.Disable();
                //server = gameObject.AddComponent<ClientPhotonAdapter>();
                server = ClientPhotonAdapter.Instance;
                break;
        }
    }
    
// Как клиент распознает, сетевой он или нет?? А никак. Не нужно ему это знать
}

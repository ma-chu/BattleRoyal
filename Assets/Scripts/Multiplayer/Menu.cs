using UnityEngine;
using System;
using  System.Collections.Generic;
using System.Linq; // для List
using Photon.Bolt;
using Photon.Bolt.Matchmaking;
using UdpKit;
using UdpKit.Platform.Photon;
//using UnityEngine.Experimental.UIElements;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR                // скрипт запущен из редактора (или как приложение?)
using UnityEditor;
#endif


public class Menu : GlobalEventListener
{
    [SerializeField] private GameObject MultiPlayerGO;
    [SerializeField] private GameObject ClientGO;
    [SerializeField] private GameObject setUsernamePanel;
    [SerializeField] private Dropdown serverListDropdown;

    private List <UdpSession/*PhotonSession*/> sessionList = new List<UdpSession/*PhotonSession*/>();   
    private string _userName;

    private void Awake()
    {
        if (!SceneManager.GetSceneByBuildIndex(1).isLoaded) SceneManager.LoadScene (1, LoadSceneMode.Additive);
    }

    public void Start()
    {
        //PlayerPrefs.DeleteKey("username");
        // выводим панель, только если не задано имя пользователя
        setUsernamePanel.SetActive(!PlayerPrefs.HasKey("username") || PlayerPrefs.GetString("username") == "" || PlayerPrefs.GetString("username") == null);
    }

    public void ChangeUsername()
    {
        setUsernamePanel.SetActive(true);
        setUsernamePanel.GetComponentInChildren<InputField>().text = PlayerPrefs.GetString("username");
    }

    public void OnSetUsernameValueChanged()
    {
        var input = setUsernamePanel.GetComponentInChildren<InputField>().text;
        PlayerPrefs.SetString("username", input);
        setUsernamePanel.SetActive(false);
    }
    
    // used from buttons
    public void StartSinglePlayer()
    {
        GameManager.gameType = GameType.Single;
        //SceneManager.UnloadSceneAsync (/*SceneManager.GetActiveScene ().buildIndex*/0);
        SceneManager.LoadScene (2, LoadSceneMode.Single); // LoadScene, в отличие от LoadSceneAcync, делает ее активной?
        //SceneManager.SetActiveScene (SceneManager.GetSceneByName("Main"));
    }
    
    public void StartMultiPlayer()
    {
        MultiPlayerGO.SetActive(true);
    }
    
    public void Quit()                                     
    {
    #if UNITY_EDITOR 
        EditorApplication.isPlaying = false;
    #else 
		Application.Quit();
    #endif
    }
    
    public void StartServer()
    {
        GameManager.gameType = GameType.Server;
        BoltLauncher.StartServer();                // AI. Запуск Сервера
    }
    
    public void StartClient()
    {
        ClientGO.SetActive(true);
        BoltLauncher.StartClient();                // BI. Запуск Клиента
    }

    // ф-ия-событие, когда сервер болта стартанул: будет загружать всем клиентам сцену Main
    public override void BoltStartDone()           // AII. Событие на сервере "Сервер Стартанул"
    {
        _userName =  PlayerPrefs.GetString("username") ?? "Joe Doe";
        if (BoltNetwork.IsServer)
        {
            Debug.LogWarning("connections max = " + BoltMatchmaking.CurrentSession.ConnectionsMax);
            BoltMatchmaking.CreateSession(sessionID: _userName, sceneToLoad: "Main");    // AIII. Создать сессию (матч, room?)
        }
    }
    
    // ф-ия вызывается (на клиенте?), когда создается/разрушается сессия (room) и затем каждые несколько секунд
    public override void SessionListUpdated(Map<Guid, UdpSession> sessionList)    // BII. Событие на клиенте "список сессий обновился"
    {
        ClearSessions();

        foreach (var session in sessionList)
        {
            UdpSession/*PhotonSession*/ photonSession = session.Value/*as PhotonSession*/;    
            
            this.sessionList.Add(photonSession);
            
            serverListDropdown.options.Add(new Dropdown.OptionData(photonSession.HostName));
        }
        serverListDropdown.RefreshShownValue();
    }

    public void JoinSession(int photonSession)    // BIII. Присоединиться к матчу
    {
        GameManager.gameType = GameType.Client;
        
        var clientToken = new PlayerClientToken {username = _userName};
        // clientToken.entity почему-то не принимается...
        /*var entity = BoltNetwork.Instantiate(BoltPrefabs.HeroBoltEntity, Vector3.zero, Quaternion.identity);
        clientToken.entity = entity;*/
        
        Debug.Log("JoinSession: me, " + clientToken.username + ", to host " + sessionList[photonSession].HostName);
        BoltMatchmaking.JoinSession(sessionList[photonSession], clientToken);
        /*Если надо аутентификацию, используем методы
         1 BoltNetwork.Connect(UdpEndPoint endpoint, IProtocolToken token);  клиент; с передачей в токене username, password
         2 ConnectRequest(UdpEndPoint endpoint, IProtocolToken token);       сервер; c приемом токена и формированием AuthResultToken 
         3 Connected(BoltConnection connection);                             клиент; с использованием connection.AcceptToken
         */                                                      
    }

    private void ClearSessions()
    {
        serverListDropdown.options.Clear();
    }
}

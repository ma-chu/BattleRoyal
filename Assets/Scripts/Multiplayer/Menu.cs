using UnityEngine;
using System;
using System.Collections.Generic;
using Photon.Bolt;
using Photon.Bolt.Matchmaking;
using UdpKit;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR               
using UnityEditor;
#endif

public class Menu : GlobalEventListener
{
    [SerializeField] private Button singlePlayerGameButton;
    [SerializeField] private Button multiPlayerGameButton;
    [SerializeField] private Button quitButton;
    [Header("MultiPlayer")]
    [SerializeField] private GameObject multiPlayerPanel;
    [SerializeField] private GameObject clientPanel;
    [SerializeField] private Dropdown serverListDropdown;
    [SerializeField] private Button startServerButton;
    [SerializeField] private Button startClientButton;
    [Header("SetUsername")]
    [SerializeField] private Button setUsernameButton;
    [SerializeField] private GameObject setUsernamePanel;
    [SerializeField] private InputField setUsernameInputField;
    [SerializeField] private Button usernameOkButton;

    private readonly List <UdpSession/*PhotonSession*/> _sessionList = new ();   
    private string _userName;

    private void Awake()
    {
        if (!SceneManager.GetSceneByBuildIndex(1).isLoaded) 
            SceneManager.LoadScene (1, LoadSceneMode.Additive);
        
        setUsernameButton.onClick.AddListener(ChangeUsernameHandler);
        usernameOkButton.onClick.AddListener(OnUsernameChanged);
        singlePlayerGameButton.onClick.AddListener(SinglePlayerGameButtonHandler);
        multiPlayerGameButton.onClick.AddListener(MultiPlayerGameButtonHandler);
        quitButton.onClick.AddListener(QuitButtonHandler);
    }
    
    public void Start()
    {
        setUsernamePanel.SetActive(!PlayerPrefs.HasKey("username") ||
                                   PlayerPrefs.GetString("username") == string.Empty ||
                                   PlayerPrefs.GetString("username") == null);
    }
    
    private void OnDestroy()
    {
        setUsernameButton.onClick.RemoveListener(ChangeUsernameHandler);
        usernameOkButton.onClick.RemoveListener(OnUsernameChanged);
        singlePlayerGameButton.onClick.RemoveListener(SinglePlayerGameButtonHandler);
        multiPlayerGameButton.onClick.RemoveListener(MultiPlayerGameButtonHandler);
        quitButton.onClick.RemoveListener(QuitButtonHandler);
        startServerButton.onClick.RemoveListener(StartServerButtonHandler);
        startClientButton.onClick.RemoveListener(StartClientButtonHandler);
        serverListDropdown.onValueChanged.RemoveListener(ServerListDropdownValueChangedHandler);
    }

    private void ChangeUsernameHandler()
    {
        setUsernamePanel.SetActive(true);
        setUsernameInputField.text = PlayerPrefs.GetString("username");
    }

    private void OnUsernameChanged()
    {
        PlayerPrefs.SetString("username", setUsernameInputField.text);
        setUsernamePanel.SetActive(false);
    }

    private void SinglePlayerGameButtonHandler() => StartSinglePlayer();
    
    private void MultiPlayerGameButtonHandler()
    {
        multiPlayerPanel.SetActive(true);
        startServerButton.onClick.AddListener(StartServerButtonHandler);
        startClientButton.onClick.AddListener(StartClientButtonHandler);
        serverListDropdown.onValueChanged.AddListener(ServerListDropdownValueChangedHandler);
    }

    private void QuitButtonHandler() => Quit();

    private void StartSinglePlayer() => GameManager.Instance.StartGame(GameType.Single);
    
    private void Quit()                                     
    {
    #if UNITY_EDITOR 
        EditorApplication.isPlaying = false;
    #else 
		Application.Quit();
    #endif
    }

    
    /// <summary>
    /// Multiplayer Bolt Staff
    /// </summary>
    
    private void StartServerButtonHandler() => StartServer();
    
    private void StartClientButtonHandler() => StartClient();
    
    private void StartServer()
    {
        BoltLauncher.StartServer();
        GameManager.Instance.StartGame(GameType.Server);
    }
    
    private void StartClient()
    {
        clientPanel.SetActive(true);
        BoltLauncher.StartClient();
    }

    /// <summary>
    /// Функция-событие, когда сервер/клиент болта стартанул: будет загружать всем клиентам сцену Main
    /// </summary>
    public override void BoltStartDone()
    {
        _userName =  PlayerPrefs.GetString("username") ?? "Joe Doe";
        
        if (BoltNetwork.IsServer)
        {
            Debug.LogWarning("connections max = " + BoltMatchmaking.CurrentSession.ConnectionsMax);
            // Создать сессию (room) Третьим параметром можно передать токен
            BoltMatchmaking.CreateSession(sessionID: _userName, sceneToLoad: "Main");
        }
        
        if (BoltNetwork.IsClient)
            GameManager.Instance.StartGame(GameType.Client);
    }
    
    /// <summary>
    /// Ф-ия-событие, вызывается на клиенте, когда создается/разрушается сессия (room) и затем каждые несколько секунд
    /// </summary>
    /// <param name="sessionList"></param>
    public override void SessionListUpdated(Map<Guid, UdpSession> sessionList)
    {
        Debug.Log("SessionListUpdated");
        ClearSessionsDropdown();

        foreach (var session in sessionList)
        {
            UdpSession/*PhotonSession*/ photonSession = session.Value/*as PhotonSession*/;    
            _sessionList.Add(photonSession);
            serverListDropdown.options.Add(new Dropdown.OptionData(photonSession.HostName));
        }
        
        serverListDropdown.RefreshShownValue();
    }

    private void ServerListDropdownValueChangedHandler(int value) => JoinSession(value);

    private void JoinSession(int photonSession)
    {
        GameManager.gameType = GameType.Client;
        var clientToken = new PlayerClientToken {username = _userName};
        Debug.Log("JoinSession: me, " + clientToken.username + ", to host " + _sessionList[photonSession].HostName);
        BoltMatchmaking.JoinSession(_sessionList[photonSession], clientToken);
        /*Если надо аутентификацию, используем методы
         1 BoltNetwork.Connect(UdpEndPoint endpoint, IProtocolToken token);  клиент; с передачей в токене username, password
         2 ConnectRequest(UdpEndPoint endpoint, IProtocolToken token);       сервер; c приемом токена и формированием AuthResultToken 
         3 Connected(BoltConnection connection);                             клиент; с использованием connection.AcceptToken
         */                                                      
    }

    private void ClearSessionsDropdown()
    {
        serverListDropdown.options.Clear();
    }
}

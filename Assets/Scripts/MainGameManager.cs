using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Крутить основную петлю поединка будет ViewModel
/// А также управлять общими View, типа номера раунда
/// </summary>

public class MainGameManager : MonoBehaviour
{
    private static MainGameManager _instance;
    public static MainGameManager Instance => _instance;

    public GameObject player;
    public GameObject enemy;
    
    private PlayerClient _playerClient;
    private AIClient _aIClient;

    private void Awake() => _instance ??= this;

    private void OnDestroy() => _instance = null;
    
    private void Start()
    {
        if (!SceneManager.GetSceneByBuildIndex(1).isLoaded) 
            SceneManager.LoadScene (1, LoadSceneMode.Additive);
    }

    private void OnEnable()
    {
        if (GameManager.gameType == GameType.Single)
            StartAIClient();
        
        StartPlayerClient();
    }

    private void StartAIClient()
    {
        _aIClient = new AIClient();
        _aIClient.Init("bot");
    }
    
    private void StartPlayerClient()
    {
        _playerClient = new PlayerClient();
        _playerClient.Init(PlayerPrefs.GetString("username"));
    }
}
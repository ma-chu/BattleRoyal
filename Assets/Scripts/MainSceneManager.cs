using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Крутить основную петлю поединка будет этот класс, а не не ViewModel
/// А вот управлять общими View, типа номера раунда, возможно, должен не ViewModel, а HeroViewManager
/// </summary>

//  HERE!!!
//  1. Не уверен, что класс ViewModel нужен. Перенести его логику сюда
//  2. Из логики ViewModel убрать все GetComponent, заменить их на ссылки, хранящиеся в HeroViewManager
//  3. Сделать player и enemy не GameObject, а класс HeroViewManager
public class MainSceneManager : MonoBehaviour 
{
    public GameObject player; 
    public GameObject enemy;
    
    private PlayerClient _playerClient;
    private AIClient _aIClient;
    
    private void Start()
    {
        if (!SceneManager.GetSceneByBuildIndex(1).isLoaded) 
            SceneManager.LoadScene (1, LoadSceneMode.Additive);
    }

    private void OnEnable()
    {
        if (GameManager.GameType == GameType.Single)
            StartAIClient();
        
        StartPlayerClient();
    }

    private void StartAIClient()
    {
        _aIClient = new AIClient();
        _aIClient.Init(this, "bot");
    }
    
    private void StartPlayerClient()
    {
        _playerClient = new PlayerClient();
        _playerClient.Init(this,PlayerPrefs.GetString("username"));
    }
}
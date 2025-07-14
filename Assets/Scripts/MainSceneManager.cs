using System;
using System.Collections;
using EF.Localization;
using EF.Sounds;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Крутит основную петлю поединка
/// </summary>

public class MainSceneManager : MonoBehaviour 
{
    public const float StartDelay = 3.5f;                     
    public const float EndDelay = 5f;                         
    public const float DeathDelay = 2.5f;                     
    private const float AttackDelay = 3f;                      
    private const float ChangeDelay = 7.5f;
    
    [SerializeField] private CommonView commonView;   // общее: общий текст, кнопки ввода, салют в конце
    
    [SerializeField] private PlayerViewManager playerViewManager;     // Смена сетов оружия, инвенторий и изменение цвета/формы оружия 
    [SerializeField] private EnemyViewManager enemyViewManager;       // придумать другое название или вообще раскидать?
    
    private WaitForSeconds _deathWait;                    
    private WaitForSeconds _startWait;                                   
    private WaitForSeconds _endWait;
    private WaitForSeconds _attackWait;
    private WaitForSeconds _changeWait;
    
    private PlayerClient _playerClient;
    private AIClient _aIClient;

    private void Awake()
    {
        _deathWait = new WaitForSeconds(DeathDelay);         
        _startWait = new WaitForSeconds(StartDelay);
        _endWait = new WaitForSeconds(EndDelay);
        _attackWait = new WaitForSeconds(AttackDelay);
        _changeWait = new WaitForSeconds(ChangeDelay);
    }

    private void Start()
    {
        if (!SceneManager.GetSceneByBuildIndex(1).isLoaded) 
            SceneManager.LoadScene (1, LoadSceneMode.Additive);
        
        if (GameManager.GameType == GameType.Single)
            StartAIClient();
        
        StartPlayerClient();
        
        commonView.WeaponSetButtonsObject.SetActive(false);
        commonView.PlayersControlsCanvas.enabled = false;
        playerViewManager.WeaponSet = _playerClient.PlayerWeaponSet;    // пока не избавился от состояния WeaponSet в HeroManager'е
        enemyViewManager.WeaponSet = _playerClient.EnemyWeaponSet;
        
        SetStartPositions();
    }

    private void OnEnable()
    {
        commonView.RestartButton.onClick.AddListener(RestartPressed);
        commonView.TurnDataReady += OnTurnInDataReady;
    }

    private void OnDisable()
    {
        commonView.RestartButton.onClick.RemoveListener(RestartPressed);
        commonView.TurnDataReady -= OnTurnInDataReady;
    }

    private void StartAIClient()
    {
        _aIClient = new AIClient();
        _aIClient.Init(this, "bot", true);
    }
    
    private void StartPlayerClient()
    {
        _playerClient = new PlayerClient();
        _playerClient.Init(this,PlayerPrefs.GetString("username"), false);
    }
    
    public void ChangeResultText(string value) => commonView.ResultText = value;
    
    public void SetPlayerName(string value) => playerViewManager.SetName(value);
    
    public void SetEnemyName(string value) => enemyViewManager.SetName(value);

    public void SetPlayerSeries(int[] nums, bool[] sets)
    { 
        playerViewManager.SetSeries(nums, sets);
    }
    
    public void SetEnemySeries(int[] nums, bool[] sets)
    { 
        enemyViewManager.SetSeries(nums, sets);
    }
    
    public IEnumerator GameStarting()
    {
        commonView.ResultText = String.Format("{0} {1} {2}", "Defeat".Localize(), Client.NumRoundsToWin, "to_win".Localize());
        yield return _startWait;                        
    }
    
    public IEnumerator RoundStarting(int roundNumber, int playerStartHealth, int enemyStartHealth)                 // начало раунда
    {
        FitWeaponButtonsToWeaponSet();
        commonView.PlayersControlsCanvas.enabled = false;
        
        playerViewManager.enabled = true;
        enemyViewManager.enabled = true;

        playerViewManager.Dead = false;
        enemyViewManager.Dead = false;

        ChangeResultText("round".Localize() + roundNumber);

        playerViewManager.SetStartHealth(playerStartHealth);
        enemyViewManager.SetStartHealth(enemyStartHealth);
        
        if (_playerClient.RoundsWon > 0)
            enemyViewManager.ChangeWeaponsView(_playerClient.RoundsWon - 1);
        
        if (roundNumber != 1)
            SetStartPositions();
        
        SetPlayerSeries(new int[3], new bool[3]);
        SetEnemySeries(new int[3], new bool[3]);

        yield return _startWait;  

        ChangeResultText(string.Empty); 
        
        commonView.PlayersControlsCanvas.enabled = true;
    }
    
    private void OnTurnInDataReady(TurnInInfo turnInInfo)
    {
        _playerClient.Decision = turnInInfo.PlayerDecision;

        FitWeaponButtonsToWeaponSet();

        _playerClient.SendDataToServer(turnInInfo);
    }

    private void FitWeaponButtonsToWeaponSet()
    {
        switch (_playerClient.Decision)
        {
            case Decision.ChangeSwordShield:
                _playerClient.PlayerWeaponSet = WeaponSet.SwordShield;        
                break;
            case Decision.ChangeSwordSword:
                _playerClient.PlayerWeaponSet = WeaponSet.SwordSword;
                break;
            case Decision.ChangeTwoHandedSword:
                _playerClient.PlayerWeaponSet = WeaponSet.TwoHandedSword;
                break;
        }

        commonView.SwordShieldButton.enabled = true;
        commonView.SwordSwordButton.enabled = true;
        commonView.TwoHandedSwordButton.enabled = true;
        
        switch (_playerClient.PlayerWeaponSet)
        {
            case WeaponSet.SwordShield:
                commonView.SwordShieldButton.enabled = false;
                break;
            case WeaponSet.SwordSword:
                commonView.SwordSwordButton.enabled = false;
                break;
            case WeaponSet.TwoHandedSword:
                commonView.TwoHandedSwordButton.enabled = false;
                break;
        }

        commonView.WeaponSetButtonsObject.SetActive(false);
    }

    private void SetStartPositions()
    {
        playerViewManager.SetStartPosition();
        enemyViewManager.SetStartPosition();
    }
    
    public IEnumerator RoundPlaying(TurnOutInfo currentResults)
    {
        ChangeResultText(string.Empty);
        
        {
                // обновляем впоследствии покойный heroManager.WeaponSet
            playerViewManager.WeaponSet = _playerClient.PlayerWeaponSet;
            enemyViewManager.WeaponSet = _playerClient.EnemyWeaponSet;
            
                // основной запускатель анимаций и звуков
            playerViewManager.Exchange(currentResults.PlayerExchangeResults, currentResults.PlayerDamages, _playerClient.Decision, currentResults.PlayerHP); 
            enemyViewManager.Exchange(currentResults.EnemyExchangeResults, currentResults.EnemyDamages, currentResults.EnemyDecision, currentResults.EnemyHP); 
              
            playerViewManager.SetHealth(currentResults.PlayerHP);
            enemyViewManager.SetHealth(currentResults.EnemyHP);
               
            commonView.PlayersControlsCanvas.enabled = false;
               
            var dead = currentResults.PlayerHP < 0 || currentResults.EnemyHP < 0;
            if (dead)
            {
                yield return _deathWait;
            }
            else if (_playerClient.Decision == Decision.Attack)
            {
                if (currentResults.EnemyDecision == Decision.Attack)
                    yield return _attackWait;                       
                else yield return _changeWait;                                                         
            }
            else 
                yield return _changeWait; 
                // основной запускатель - здесь только обнуляет тексты ?можно уже избавиться от _playerManager'ов?
            playerViewManager.ExchangeEnded();
            enemyViewManager.ExchangeEnded();

            if (dead)
            {
                yield return true;
            }
            else 
            {
                commonView.PlayersControlsCanvas.enabled = true;
                yield return null;
            }
        }
    }
    
    public IEnumerator RoundEnding(int roundNumber, string winner, string prize)
    {
        playerViewManager.enabled = false;
        enemyViewManager.enabled = false;

        yield return _deathWait;
       
        var winString = !winner.Equals(string.Empty) ? winner : "nobody".Localize();
        ChangeResultText("round".Localize() + roundNumber + " " + "ended".Localize() + winString + "win".Localize());

        if (winner == _playerClient.PlayerName)
        {
            yield return _deathWait;
            
            var item = playerViewManager.AddPrize(prize);
            if (item != null) 
                ChangeResultText ("you_got".Localize() + prize.Localize());

            if ( /*_enemyUI.Name.Equals("bot")*/_aIClient != null && _playerClient.RoundsWon == 3) 
                enemyViewManager.AddPrize("ring_of_cunning", false);
        }
      
        if (winner == enemyViewManager.GetName()) 
            enemyViewManager.AddPrize(prize, false);

        yield return _endWait;
    }
    
    public IEnumerator GameOver(string matchWinner)
    {
        var gameWinner = matchWinner == _playerClient.PlayerName ? Heroes.Player : Heroes.Enemy;
        
        var res = "game_over".Localize() + matchWinner + "win".Localize();
        ChangeResultText(res);

        commonView.GameOverAnimator.SetTrigger("GameOver");
        SoundsManager.Instance.PlaySound(SoundsContainer.GetAudioClip(SoundTypes.GameOver));

        if (gameWinner == Heroes.Player)
        {
            GameSave.LastLoadedSnapshot.tournamentsWon++;
            
            yield return StartCoroutine(commonView.Salute());
        }
        
        yield return _endWait;
        
        commonView.RestartButtonGameObject.SetActive(true);
    }

    private void RestartPressed()
    {
        GameSave.Save();
        if (GameManager.GameType != GameType.Single) 
            Photon.Bolt.BoltLauncher.Shutdown();
        
        // Как-то уничтожить компонент Server
        SceneManager.UnloadSceneAsync(2, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
        SceneManager.LoadScene(0);                          
    }
}
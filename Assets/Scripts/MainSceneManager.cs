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
    
    [SerializeField] public GameObject player; 
    [SerializeField] public GameObject enemy;
    
    [SerializeField] private CommonView commonView;   // общее: общий текст, кнопки ввода, салют в конце
    
    private PlayerViewManager _playerViewManager;     // Смена сетов оружия, инвенторий и изменение цвета/формы оружия 
    private EnemyViewManager _enemyViewManager;       // придумать другое название или вообще раскидать?
    private HeroUI _playerUI;                         // тексты урона
    private HeroUI _enemyUI;
    private HPView _playerHP;
    private HPView _enemyHP;
    private HeroAnimation _playerAnimations;
    private HeroAnimation _enemyAnimations;
    private SeriesView _playerSeries;
    private SeriesView _enemySeries; 
    
    private WaitForSeconds _deathWait;                    
    private WaitForSeconds _startWait;                                   
    private WaitForSeconds _endWait;
    private WaitForSeconds _attackWait;
    private WaitForSeconds _changeWait;
    
    private PlayerClient _playerClient;
    private AIClient _aIClient;

    private void Awake()
    {
        _playerViewManager = player.GetComponent<PlayerViewManager>();
        _enemyViewManager = enemy.GetComponent<EnemyViewManager>();
        _playerUI = player.GetComponent<HeroUI>();            
        _enemyUI = enemy.GetComponent<HeroUI>();
        _playerHP = player.GetComponent<HPView>();            
        _enemyHP = enemy.GetComponent<HPView>();
        _playerAnimations = player.GetComponent<HeroAnimation>();   
        _enemyAnimations = enemy.GetComponent<HeroAnimation>();
        _playerSeries = player.GetComponent<SeriesView>();   
        _enemySeries = enemy.GetComponent<SeriesView>();

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
        _playerViewManager.weaponSet = _playerClient.PlayerWeaponSet;    // пока не избавился от состояния weaponSet в HeroManager'е
        _enemyViewManager.weaponSet = _playerClient.EnemyWeaponSet;
        
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
        _aIClient.Init(this, "bot");
    }
    
    private void StartPlayerClient()
    {
        _playerClient = new PlayerClient();
        _playerClient.Init(this,PlayerPrefs.GetString("username"));
    }
    
    public void ChangeResultText(string value) => commonView.ResultText = value;
    
    public void SetPlayerName(string value) => _playerUI.Name = value;
    
    public void SetEnemyName(string value) => _enemyUI.Name = value;
    
    public void SetPlayerSeries(int[] nums, bool[] sets)
    { 
        _playerSeries.UpdateStrongSeries(nums[0], sets[0]);
        _playerSeries.UpdateSeriesOfBlocks(nums[1], sets[1]);
        _playerSeries.UpdateSeriesOfStrikes(nums[2], sets[2]);

        _playerUI.SetRegenValues(nums[1]);
    }
    public void SetEnemySeries(int[] nums, bool[] sets)
    { 
        _enemySeries.UpdateStrongSeries(nums[0], sets[0]);
        _enemySeries.UpdateSeriesOfBlocks(nums[1], sets[1]);
        _enemySeries.UpdateSeriesOfStrikes(nums[2], sets[2]);
        
        _enemyUI.SetRegenValues(nums[1]);
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
        
        _playerViewManager.enabled = true;
        _enemyViewManager.enabled = true;

        _playerViewManager.dead = false;
        _enemyViewManager.dead = false;

        ChangeResultText("round".Localize() + roundNumber);

        _playerHP.SetStartHealth(playerStartHealth);
        _enemyHP.SetStartHealth(enemyStartHealth);
        
        if (_playerClient.RoundsWon > 0)
            _enemyViewManager.ChangeWeaponsView(_playerClient.RoundsWon - 1);
        
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
        if (!_playerAnimations.enabled) 
            _playerAnimations.enabled = true; 
        
        if (!_enemyAnimations.enabled) 
            _enemyAnimations.enabled = true; 
        
        _playerAnimations.SetStartPositions();
        _enemyAnimations.SetStartPositions();
    }
    
    public IEnumerator RoundPlaying(TurnOutInfo currentResults)
    {
        ChangeResultText(string.Empty);
        
        {
                // обновляем впоследствии покойный heroManager.weaponSet
            _playerViewManager.weaponSet = _playerClient.PlayerWeaponSet;
            _enemyViewManager.weaponSet = _playerClient.EnemyWeaponSet;
            
                // основной запускатель анимаций и звуков
            _playerViewManager.Exchange(currentResults.PlayerExchangeResults, currentResults.PlayerDamages, _playerClient.Decision, currentResults.PlayerHP); 
            _enemyViewManager.Exchange(currentResults.EnemyExchangeResults, currentResults.EnemyDamages, currentResults.EnemyDecision, currentResults.EnemyHP); 
              
            _playerHP.SetHealth(currentResults.PlayerHP);
            _enemyHP.SetHealth(currentResults.EnemyHP);
               
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
            _playerViewManager.ExchangeEnded();
            _enemyViewManager.ExchangeEnded();

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
        _playerViewManager.enabled = false;
        _enemyViewManager.enabled = false;
        _playerAnimations.enabled = false;       
        _enemyAnimations.enabled = false;  

        yield return _deathWait;
       
        var winString = !winner.Equals(string.Empty) ? winner : "nobody".Localize();
        ChangeResultText("round".Localize() + roundNumber + " " + "ended".Localize() + winString + "win".Localize());

        if (winner == _playerClient.PlayerName)
        {
            yield return _deathWait;
            
            var item = _playerViewManager.AddPrize(prize);
            if (item != null) ChangeResultText ("you_got".Localize() + prize.Localize());
            
            if (_enemyUI.Name.Equals("bot") && _playerClient.RoundsWon == 3) 
                _enemyViewManager.AddPrize("ring_of_cunning", false);
        }
      
        if (winner == _enemyUI.Name) _enemyViewManager.AddPrize(prize, false);

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
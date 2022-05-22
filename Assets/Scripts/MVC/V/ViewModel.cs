using System;
using UnityEngine;
using System.Collections; 
using EF.Localization;
using EF.Sounds;
using UnityEngine.SceneManagement;

// Интерфейс между ViewModel и view'хами. Его должен реализовать ViewModel. Ни Client, ни ViewModel не наследуют от Monobehaviour, а все View да, и висят на героях
public interface IViewModel
{
    // Интерфейс IViewModel должен описывать, cудя по DI, как общаться с ViewModel (вернее, PresenterModel) из клиента...
}

public class ViewModel : IViewModel
{
    // Хорошо бы вообще для ViewModel не знать о View'хах: HeroUI, HeroAnimation и пр.
    // И общаться с ними с помощью событий или какого-то датабиндинга (UniRx?)
    // ... Но здесь пока скорее PresenterModel, где есть ссылки на все View, и общение с View через поля

    public const float StartDelay = 3.5f;                     
    public const float EndDelay = 5f;                         
    public const float DeathDelay = 2.5f;                     
    private const float AttackDelay = 3f;                      
    private const float ChangeDelay = 7.5f;
    private WaitForSeconds _deathWait;                    
    private WaitForSeconds _startWait;                                   
    private WaitForSeconds _endWait;
    private WaitForSeconds _attackWait;
    private WaitForSeconds _changeWait;

    private Client _client;
    // Views:
    private CommonView _commonView;           // общее: общий текст, кнопки ввода, салют в конце
    private PlayerViewManager _playerViewManager;     // Смена сетов оружия, инвенторий и изменение цвета/формы оружия 
    private EnemyViewManager _enemyViewManager;       // придумать другое название или вообще раскидать?
    private HeroUI _playerUI;                 // тексты урона
    private HeroUI _enemyUI;
    private HPView _playerHP;                 // слайдеры здоровья
    private HPView _enemyHP;
    private HeroAnimation _playerAnim;        // анимации героев
    private HeroAnimation _enemyAnim;
    private SeriesView _playerSeries;         // отображение серий
    private SeriesView _enemySeries; 
    
    public void Init(Client client)
    {
        _client = client;

        _commonView = MainGameManager.Instance.GetComponent<CommonView>();
        _commonView.RestartButton.onClick.AddListener(RestartPressed);
        _commonView.SubscribeOnTurnInDataReady(OnTurnInDataReady);
        
        _playerViewManager = MainGameManager.Instance.player.GetComponent<PlayerViewManager>();
        _enemyViewManager = MainGameManager.Instance.enemy.GetComponent<EnemyViewManager>();

        _playerUI = MainGameManager.Instance.player.GetComponent<HeroUI>();            
        _enemyUI = MainGameManager.Instance.enemy.GetComponent<HeroUI>();
        _playerHP = MainGameManager.Instance.player.GetComponent<HPView>();            
        _enemyHP = MainGameManager.Instance.enemy.GetComponent<HPView>();
        _playerAnim = MainGameManager.Instance.player.GetComponent<HeroAnimation>();   
        _enemyAnim = MainGameManager.Instance.enemy.GetComponent<HeroAnimation>();
        _playerSeries = MainGameManager.Instance.player.GetComponent<SeriesView>();   
        _enemySeries = MainGameManager.Instance.enemy.GetComponent<SeriesView>();
        

        _deathWait = new WaitForSeconds(DeathDelay);         
        _startWait = new WaitForSeconds(StartDelay);
        _endWait = new WaitForSeconds(EndDelay);
        _attackWait = new WaitForSeconds(AttackDelay);
        _changeWait = new WaitForSeconds(ChangeDelay);
        
        //SetPlayerName(_client.PlayerName);
        _commonView.WeaponSetButtonsObject.SetActive(false);
        _commonView.PlayersControlsCanvas.enabled = false;

        _playerViewManager.weaponSet = _client.PlayerWeaponSet;    // пока не избавился от состояния weaponSet в HeroManager'е
        _enemyViewManager.weaponSet = _client.EnemyWeaponSet;
        
        SetStartPosition();
    }

    private void OnTurnInDataReady(TurnInInfo turnInInfo)
    {
        _client.decision = turnInInfo.PlayerDecision;

        FitWeaponButtonsToWeaponSet();

        _client.SendDataToServer(turnInInfo);
    }

    private void FitWeaponButtonsToWeaponSet()
    {
        switch (_client.decision)
        {
            case Decision.ChangeSwordShield:
                _client.PlayerWeaponSet = WeaponSet.SwordShield;        
                break;
            case Decision.ChangeSwordSword:
                _client.PlayerWeaponSet = WeaponSet.SwordSword;
                break;
            case Decision.ChangeTwoHandedSword:
                _client.PlayerWeaponSet = WeaponSet.TwoHandedSword;
                break;
        }

        _commonView.SwordShieldButton.enabled = true;                           // делаем все кнопки доступными для следующей смены
        _commonView.SwordSwordButton.enabled = true;
        _commonView.TwoHandedSwordButton.enabled = true;
        
        if (_client.PlayerWeaponSet == WeaponSet.SwordShield) _commonView.SwordShieldButton.enabled = false;    // но не даём выбрать тот же сет
        if (_client.PlayerWeaponSet == WeaponSet.SwordSword) _commonView.SwordSwordButton.enabled = false;
        if (_client.PlayerWeaponSet == WeaponSet.TwoHandedSword) _commonView.TwoHandedSwordButton.enabled = false;
        
        _commonView.WeaponSetButtonsObject.SetActive(false);
    }

    public void ChangeResultText(string value) => _commonView.ResultText = value;
    public void SetPlayerName(string value) => _playerUI.Name = value;
    public void SetEnemyName(string value) => _enemyUI.Name = value;
    public void SetPlayerHP(int value) => _playerHP.SetStartHealth(value);
    public void SetEnemyHP(int value) => _enemyHP.SetStartHealth(value);
    public void SetPlayerSeries(int[] nums, bool[] sets)
    { 
        _playerSeries.UpdateStrongSeries(nums[0], sets[0]);
        _playerSeries.UpdateSeriesOfBlocks(nums[1], sets[1]);
        _playerSeries.UpdateSeriesOfStrikes(nums[2], sets[2]);

        _playerUI.SetRegenValues(nums[1]);    // здесь же для регена
    }
    public void SetEnemySeries(int[] nums, bool[] sets)
    { 
        _enemySeries.UpdateStrongSeries(nums[0], sets[0]);
        _enemySeries.UpdateSeriesOfBlocks(nums[1], sets[1]);
        _enemySeries.UpdateSeriesOfStrikes(nums[2], sets[2]);
        
        _enemyUI.SetRegenValues(nums[1]);    // здесь же для регена
    }

    public IEnumerator GameOver(string matchWinner)
    {
        var gameWinner = matchWinner == _client.PlayerName ? Heroes.Player : Heroes.Enemy;
        
        var res = "game_over".Localize() + matchWinner + "win".Localize();
        ChangeResultText(res);

        _commonView.GameOverAnimator.SetTrigger("GameOver");
        SoundsManager.Instance.PlaySound(SoundsContainer.GetAudioClip(SoundTypes.GameOver));

        if (gameWinner == Heroes.Player)
        {
            GameSave.LastLoadedSnapshot.tournamentsWon++;
            
            yield return MainGameManager.Instance.StartCoroutine(_commonView.Salute());
        }
        yield return _endWait;
        
        _commonView.RestartButtonGameObject.SetActive(true);
    }

    private void RestartPressed()
    {
        GameSave.Save();
        if (GameManager.gameType != GameType.Single) Photon.Bolt.BoltLauncher.Shutdown();
        // Как-то уничтожить компонент Server
        SceneManager.UnloadSceneAsync(2, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
        SceneManager.LoadScene(0);                          
    }

    
    public IEnumerator GameStarting()                  // начало игры
    {
        _commonView.ResultText = String.Format("{0} {1} {2}", "Defeat".Localize(), Client.NumRoundsToWin, "to_win".Localize());
        //SetStartPosition();
        yield return _startWait;                        
    }
    
    
    public IEnumerator RoundStarting(int roundNumber, int playerStartHealth, int enemyStartHealth)                 // начало раунда
    {
        /*// Вернуться при фотоне
        if (gameType != GameType.Single)       
            _enemyState = enemyBoltEntity?.GetState<Photon.Bolt.IEFPlayerState>();
        */

        // кнопки управления выкл
        FitWeaponButtonsToWeaponSet();
        _commonView.PlayersControlsCanvas.enabled = false;
        // набор оружия в исх. щит-меч
        _playerViewManager.enabled = true;
        _enemyViewManager.enabled = true;
        // не умирать
        _playerViewManager.dead = _enemyViewManager.dead = false;
        // сообщение
        ChangeResultText("round".Localize() + roundNumber);
        // здоровье на максимум
        SetPlayerHP(playerStartHealth);
        SetEnemyHP(enemyStartHealth);
        // изменяем вид врага
        if (_client.roundsWon > 0) _enemyViewManager.ChangeWeaponsView(_client.roundsWon - 1);
        // анмацию в начало
        if (roundNumber != 1) SetStartPosition();
        // серии сбросить
        SetPlayerSeries(new int[3], new bool[3]);
        SetEnemySeries(new int[3], new bool[3]);

        yield return _startWait;  

        ChangeResultText(string.Empty); 
        // кнопки управления вкл
        _commonView.PlayersControlsCanvas.enabled = true;
    }

    private void SetStartPosition()
    {
        if (!_playerAnim.enabled) _playerAnim.enabled = true;       
        if (!_enemyAnim.enabled) _enemyAnim.enabled = true; 
        _playerAnim.SetStartPositions();
        _enemyAnim.SetStartPositions();
    }
    
    public IEnumerator RoundPlaying(TurnOutInfo currentResults)
    {
        ChangeResultText(string.Empty);                         // 1. Очистить информационное сообщение
        
        //while (!OneHeroLeft())                                  // 2. Играем раунд (пропускаем такты), пока кто-то не умрет
        //{
        //1. Разблокировать кнопки управления игроку       
        /*фотон if (!doServerExchange && !doClientExchange)*/ //_commonView.PlayersControlsCanvas.enabled = true;

        
            //2. При одиночной игре определить решение врага: удар или смена оружия
            //if (gameType == GameType.Single && enemy.decision == Decision.No)
            //{
            //    MakeSinglePlayerEnemyDecision(HeroManager.player_countRoundsWon); 
            //}
            //3. Ожидать действие игрока и врага: удара или смены оружия
            /*if (
                player.decision==Decision.No || enemy.decision==Decision.No
                || gameType == GameType.Server && !doServerExchange
                || gameType == GameType.Client && !doClientExchange)
            {
                yield return null;   // Решения еще нет - заканчиваем этот такт (и почему-то не переходим сразу к началу корутины, а проходим её тело до конца...)
                //Debug.Log("Why am I displaying?"); // А вот так работает yield return null - такт проходится до конца
            }*/
            {
                // обновляем впоследствии покойный heroManager.weaponSet
                _playerViewManager.weaponSet = _client.PlayerWeaponSet;
                _enemyViewManager.weaponSet = _client.EnemyWeaponSet;
                // основной запускатель анимаций и звуков
                _playerViewManager.Exchange(currentResults.PlayerExchangeResults, currentResults.PlayerDamages, _client.decision, currentResults.PlayerHP); 
                _enemyViewManager.Exchange(currentResults.EnemyExchangeResults, currentResults.EnemyDamages, currentResults.EnemyDecision, currentResults.EnemyHP); 
                // обновляем здоровье
                _playerHP.SetHealth(currentResults.PlayerHP);
                _enemyHP.SetHealth(currentResults.EnemyHP);
                // кнопки выкл
                _commonView.PlayersControlsCanvas.enabled = false;
                // задержка на анимацмю смерти/атаки/смены
                var dead = currentResults.PlayerHP < 0 || currentResults.EnemyHP < 0;
                if (dead) yield return _deathWait;
                else if (_client.decision == Decision.Attack)
                {
                    if (currentResults.EnemyDecision == Decision.Attack) yield return _attackWait;                       
                    else yield return _changeWait;                                                         
                }
                else yield return _changeWait;     //  точно какая-то смена
                // основной запускатель - здесь только обнуляет тексты ?можно уже избавиться от _playerManager'ов?
                _playerViewManager.ExchangeEnded();
                _enemyViewManager.ExchangeEnded();

                if (dead) yield return true;
                else 
                {
                    _commonView.PlayersControlsCanvas.enabled = true;    // кнопки вкл
                    yield return null;  // заканчиваем этот такт (и не переходим к концу корутины)
                }
            }
    }
    
    public IEnumerator RoundEnding(int roundNumber, string winner, string prize)                       // конец раунда
    {
        _playerViewManager.enabled = false;
        _enemyViewManager.enabled = false;
        _playerAnim.enabled = false;       
        _enemyAnim.enabled = false;  
        //1. ждем анимацию смерти
        yield return _deathWait;                           // используем m_DeathWait ( 2.5 сек), чтоб не плодить сущности
        //2. Вывести информационное сообщение.
        var w = !winner.Equals(string.Empty) ? winner : "nobody".Localize();
        ChangeResultText("round".Localize() + roundNumber + " " + "ended".Localize() + w + "win".Localize());
        //3. Выдать игроку пункт инвентаря за победу в раунде
        if (winner == _client.PlayerName)
        {
            yield return _deathWait;    //ждём еще одну m_DeathWait, ибо будем выдавать инвентарь
            
            var item = _playerViewManager.AddPrize(prize);
            if (item != null) ChangeResultText ("you_got".Localize() + prize.Localize());
            
            // пока что так коряво, а надо бы от сервера писать полностью, кому что
            if (_enemyUI.Name.Equals("bot") && _client.roundsWon == 3) _enemyViewManager.AddPrize("ring_of_cunning", false);
            
            /* //Добавим инвентарь в state - вернуться при фотоне
            if (gameType!=GameType.Single && a!=null)
            {
                myBoltEntity.GetState<Photon.Bolt.IEFPlayerState>().InventoryItem = a;
            }*/
        }
        //4. Врагу тоже
        if (winner == _enemyUI.Name) _enemyViewManager.AddPrize(prize, false);

        yield return _endWait;
        
        /* вернуться при фотоне
        player.m_dead = false;
        enemy.m_dead = false;

        doClientExchange = false;
        doServerExchange = false;
        */
    }
}

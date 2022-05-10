using System;
using UnityEngine;
using System.Collections; 
using EF.Localization;
using EF.Sounds;
using UnityEngine.SceneManagement;

// Интерфейс между ViewModel и view'хами. Его должен реализовать ViewModel. Ни Client, ни ViewModel не наследуют от Monobehaviour, а все View да, и висят на героях
public interface IViewModel
{
    void OnTurnInDataReady (TurnInInfo turnInInfo); // Принять входные данные
    //public event Action ExchangeEvent1;             // удар1 состоялся 
    //public event Action ExchangeEvent2;             // удар2 состоялся
    //public event Action ExchangeEndedEvent;         // весь сход закончен
    
    
    // Или интерфейс IViewModel должен описывать, как общаться с ним из клиента?
    void RestartPressed();
}

public class ViewModel : IViewModel
{
    // Вообще, статические события лучше не юзать: подписываясь на них, мы создаем ссылки в статическом объекте, который не будет уничтожен,
    // пока существует класс GameManager, то есть пока не закончится работа программы.
    
    // Хорошо бы вообще не знать о View'хах: HeroUI, HeroAnimation и пр. И общаться с ними с помощью событий...
    // ... Но пока есть ссылки на все View, и общение с View через поля

    private readonly float startDelay = 3.5f;                     
    private readonly float endDelay = 5f;                         
    private readonly float deathDelay = 2.5f;                     
    private readonly float attackDelay = 3f;                      
    private readonly float changeDelay = 7.5f;
    private WaitForSeconds _deathWait;                    // задержки понятного сопрограмме вида
    private WaitForSeconds _startWait;                    // переведение в него из секунд состоится в ф-ии Init()                 
    private WaitForSeconds _endWait;
    private WaitForSeconds _attackWait;
    private WaitForSeconds _changeWait;
    
    private string _enemyName;
    
    private Client _client;
    // Views:
    private CommonView _commonView;           // общее: общий текст, кнопки ввода, салют
    private PlayerManager _playerManager;     // Смена сетов оружия, инвенторий и изменение цвета/формы оружия 
    private EnemyManager _enemyManager;       // придумать другое название или вообще раскидать?
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
        
        _playerManager = MainGameManager.Instance.player.GetComponent<PlayerManager>();
        _enemyManager = MainGameManager.Instance.enemy.GetComponent<EnemyManager>();

        _playerUI = MainGameManager.Instance.player.GetComponent<HeroUI>();            
        _enemyUI = MainGameManager.Instance.enemy.GetComponent<HeroUI>();
        _playerHP = MainGameManager.Instance.player.GetComponent<HPView>();            
        _enemyHP = MainGameManager.Instance.enemy.GetComponent<HPView>();
        _playerAnim = MainGameManager.Instance.player.GetComponent<HeroAnimation>();   
        _enemyAnim = MainGameManager.Instance.enemy.GetComponent<HeroAnimation>();
        _playerSeries = MainGameManager.Instance.player.GetComponent<SeriesView>();   
        _enemySeries = MainGameManager.Instance.enemy.GetComponent<SeriesView>();

        //SubscribeAllViews();

        _deathWait = new WaitForSeconds(deathDelay);         // инициализируем задержки: переводим секунды в понятный сопрограмме вид. Затем будем использовать их yield-ом
        _startWait = new WaitForSeconds(startDelay);
        _endWait = new WaitForSeconds(endDelay);
        _attackWait = new WaitForSeconds(attackDelay);
        _changeWait = new WaitForSeconds(changeDelay);
        
        SetPlayerName(_client.PlayerName);
        _commonView.WeaponSetButtonsObject.SetActive(false);
        _commonView.PlayersControlsCanvas.enabled = false;

        _playerManager.weaponSet = _client.PlayerWeaponSet;    // пока не избавился от состояния weaponSet в HeroManager'е
        _enemyManager.weaponSet = _client.EnemyWeaponSet;
    }

    /*private void SubscribeAllViews()
    {
        ExchangeEvent1 += _playerManager.OnExchange1;
        ExchangeEvent1 += _enemyManager.OnExchange1;
        ExchangeEvent2 += _playerManager.OnExchange2;
        ExchangeEvent2 += _enemyManager.OnExchange2;
        ExchangeEndedEvent += _playerManager.OnExchangeEnded;
        ExchangeEndedEvent += _enemyManager.OnExchangeEnded;
    }*/
    
    public void OnTurnInDataReady(TurnInInfo turnInInfo)
    {
        _client.decision = turnInInfo.PlayerDecision;
        
        switch (_client.decision)
        {
            case Decision.ChangeSwordShield:
                _client.PlayerWeaponSet = WeaponSet.SwordShield;        // надо бы избавиться от переменной HeroManager.weaponSet
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
        
        _client.SendDataToServer(turnInInfo);
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
    }
    public void SetEnemySeries(int[] nums, bool[] sets)
    { 
        _enemySeries.UpdateStrongSeries(nums[0], sets[0]);
        _enemySeries.UpdateSeriesOfBlocks(nums[1], sets[1]);
        _enemySeries.UpdateSeriesOfStrikes(nums[2], sets[2]);
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

    public void RestartPressed()
    {
        GameSave.Save();
        if (GameManager.gameType != GameType.Single) Photon.Bolt.BoltLauncher.Shutdown();
        //SceneManager.UnloadSceneAsync(2);
        SceneManager.LoadScene(0);                          
    }

    
    // запускаем сам процесс боя как сопрограмму. Почему как сопрограмму? Потому что будем прерывать её директивой yield return
    /*public IEnumerator MatchLoop()                              // основная петля поединка
    {
        // выход из yeild-ов-функций происходит по усоловию - выдаче соотв. функциями true
        if (_roundNumber == 0) yield return StartCoroutine(GameStarting());    // начало игры - обозначить цель

        yield return StartCoroutine(RoundStarting());   // начало раунда: вывод номера раунда и количества побед у бойцов. Стартовая пауза
        yield return StartCoroutine(RoundPlaying());    // сам процесс боя
        yield return StartCoroutine(RoundEnding());     // конец раунда: вывод победителя раунда, количества побед у бойцов и имени победителя. Конечная пауза
        */
        //_gameWinner = GameWinner();    Server
        
        /*if (_gameWinner != Heroes.Nobody)
        {
            var winner = _gameWinner.ToString().Localize();
            if (GameManager.gameType != GameType.Single)
            {
                if (_gameWinner == Heroes.Player) winner = PlayerPrefs.GetString("username");
                else winner = _enemyState.Username;
            }
            resultText.text = "game_over".Localize() + winner + "win".Localize();
            
            gameOverAnimator.SetTrigger("GameOver");
            SoundsManager.Instance.PlaySound(SoundsContainer.GetAudioClip(SoundTypes.GameOver));

            if (_gameWinner == Heroes.Player)
            {
                GameSave.LastLoadedSnapshot.tournamentsWon++;
                yield return StartCoroutine(Salute());
            }
            yield return _endWait;
            
            player.restartButtonObject.SetActive(true);
        }
        else
        {        
            StartCoroutine(GameLoop());         // рекурсия: текущая GameLoop() завершается и начинается новая 
        }
    }*/

    public IEnumerator GameStarting()                  // начало игры
    {
        _commonView.ResultText = String.Format("{0} {1} {2}", "Defeat".Localize(), Client.numRoundsToWin, "to_win".Localize());
        SetStartPosition();
        yield return _startWait;                        
    }
    
    
    public IEnumerator RoundStarting(int roundNumber, int playerStartHealth, int enemyStartHealth)                 // начало раунда
    {
        /*// Вернуться при фотоне
        if (gameType != GameType.Single)       
            _enemyState = enemyBoltEntity?.GetState<Photon.Bolt.IEFPlayerState>();
        */
        
        //0. Имя врага
        //enemyNameText.text = _enemyState?.Username ?? "enemyBot";        
        //1. Увеличить номер раунда.
        //_roundNumber++;
        //2. Сформировать и вывести информационное сообщение.

        //resultText.text="round".Localize() + _roundNumber.ToString();
        //3. Установить стартовые параметры игроку и врагу: твикеры (с учетом инвентаря), здоровье, начальные позиции и пр. Заблокировать кнопки управления игроку.
        // в самом начале игры инвентарь не сработает
        // ВОЗМОЖНО, тут лучше вызвать событие, и принять его не только HeroManager-ом, но и HP и пр.
        //player.enabled = true;
        //enemy.enabled = true;
        
        // кнопки управления выкл
        _commonView.WeaponSetButtonsObject.SetActive(false);
        _commonView.PlayersControlsCanvas.enabled = false;
        // набор оружия в исх. щит-меч
        _playerManager.enabled = true;
        _enemyManager.enabled = true;
        //_playerManager.weaponSet = _client.PlayerWeaponSet;    // пока не избавился от состояния weaponSet в HeroManager
        //_enemyManager.weaponSet = _client.EnemyWeaponSet;
        
        // сообщение
        ChangeResultText("round".Localize() + roundNumber);
        // здоровье на максимум
        SetPlayerHP(playerStartHealth);
        SetEnemyHP(enemyStartHealth);
        // анмацию в начало
        if (roundNumber != 1) SetStartPosition();
        // серии сбросить
        SetPlayerSeries(new int[3], new bool[3]);
        SetEnemySeries(new int[3], new bool[3]);

        yield return _startWait;  

        ChangeResultText(string.Empty);                         // 1. Очистить информационное сообщение
        _commonView.PlayersControlsCanvas.enabled = true;
    }

    private void SetStartPosition()
    {
        if (!_playerAnim.enabled) _playerAnim.enabled = true;       
        if (!_enemyAnim.enabled) _enemyAnim.enabled = true; 
        _playerAnim.SetStartPositions(PlayerManager.zeroZposition, PlayerManager.zeroYrotation, PlayerManager.stockXposition, PlayerManager.startRotation);
        _enemyAnim.SetStartPositions(EnemyManager.zeroZposition, EnemyManager.zeroYrotation, EnemyManager.stockXposition, EnemyManager.startRotation);
    }
    
    public IEnumerator RoundPlaying(TurnOutInfo currentResults)
    {        
        
        
        // From RoundPlaying here
        ChangeResultText(string.Empty);                         // 1. Очистить информационное сообщение
        
        
        //while (!OneHeroLeft())                                  // 2. Играем раунд (пропускаем такты), пока кто-то не умрет
        //{
        //1. Разблокировать кнопки управления игроку       
        /*фотон if (!doServerExchange && !doClientExchange)*/ _commonView.PlayersControlsCanvas.enabled = true;

        
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
            //4. иначе рассчитать урон. (Плюс посмотреть, не умер ли кто. Если умер - идем на конец раунда)
            //else
            {
                // a. предварительные коэффициенты, результат схода и урон для одиночной игры (для сетевой это сделает сервер в ServerNetworkCallbacks по событию EFReadyForExchangeEvent)
                //if (gameType == GameType.Single) ExchangeResultsAndDamages();

                // b. Удар 1. Обновление коэфф. и добавление эффектов серий ударов (серия блоков ставится по событию HeroManager'а-->HeroUI-->Series).
                /*
                if ((enemy.exchangeResult[0] == ExchangeResult.GetHit) || (enemy.exchangeResult[0] == ExchangeResult.BlockVs2Handed))
                    player.AddStrongSeries(1);
                if ((player.exchangeResult[0] == ExchangeResult.GetHit) || (player.exchangeResult[0] == ExchangeResult.BlockVs2Handed))
                    enemy.AddStrongSeries(1);
                
                if ((enemy.exchangeResult[0] == ExchangeResult.GetHit) || (enemy.exchangeResult[0] == ExchangeResult.BlockVs2Handed))
                    player.AddStrikesSeries();
                if ((player.exchangeResult[0] == ExchangeResult.GetHit) || (player.exchangeResult[0] == ExchangeResult.BlockVs2Handed))
                    enemy.AddStrikesSeries();
                */
                
                // I. Удар1 состоялся  - запустить событие. 
                //ExchangeEvent1?.Invoke(currentResults); 

                _playerManager.weaponSet = _client.PlayerWeaponSet;
                _enemyManager.weaponSet = _client.EnemyWeaponSet;

                _playerManager.Exchange(currentResults.PlayerExchangeResults, currentResults.PlayerDamages, _client.decision, currentResults.PlayerHP); 
                _enemyManager.Exchange(currentResults.EnemyExchangeResults, currentResults.EnemyDamages, currentResults.EnemyDecision, currentResults.EnemyHP); 
                
                _playerHP.SetHealth(currentResults.PlayerHP);
                _enemyHP.SetHealth(currentResults.EnemyHP);

                _commonView.PlayersControlsCanvas.enabled = false;

                // c. Удар 2. Обновление коэфф. и добавление эффектов серий ударов
                /*if (enemy.exchangeResult[1] == ExchangeResult.GetHit)
                    player.AddStrongSeries(2);
                if (player.exchangeResult[1] == ExchangeResult.GetHit)
                    enemy.AddStrongSeries(2);
                
                if (enemy.exchangeResult[1] == ExchangeResult.GetHit)
                    player.AddStrikesSeries();
                if (player.exchangeResult[1] == ExchangeResult.GetHit)
                    enemy.AddStrikesSeries();
                
                // d. Коэффициенты серий ударов. Ресет.
                if ((enemy.exchangeResult[0] != ExchangeResult.GetHit) && (enemy.exchangeResult[1] != ExchangeResult.GetHit) && (enemy.exchangeResult[0] != ExchangeResult.BlockVs2Handed))                    
                    player.ResetStrikesSeries();
                if ((player.exchangeResult[0] != ExchangeResult.GetHit)&&(player.exchangeResult[1] != ExchangeResult.GetHit)&&(player.exchangeResult[0] != ExchangeResult.BlockVs2Handed))                    
                    enemy.ResetStrikesSeries();
                */
                // e. Смена оружия врага
                /*if (enemy.decision != Decision.Attack)
                {
                    switch (enemy.decision)
                    {
                        case Decision.ChangeSwordShield:
                            enemy.SetSwordShield();
                            break;
                        case Decision.ChangeSwordSword:
                            enemy.SetSwordSword();
                            break;
                        case Decision.ChangeTwoHandedSword:
                            enemy.SetTwoHandedSword();
                            break;
                    }
                }*/
                
                // II. Удар2 состоялся  - запустить событие 
                //ExchangeEvent2?.Invoke(currentResults);
                
                //5. Блокировка кнопок управления игрока и задержка на анимацию (атаки или смены)
                if (currentResults.PlayerHP < 0 || currentResults.EnemyHP < 0) yield return _attackWait;
                else if (_client.decision == Decision.Attack)
                {
                    if (currentResults.EnemyDecision == Decision.Attack) yield return _attackWait;                       
                    else yield return _changeWait;                                                         
                }
                else yield return _changeWait;     //  точно какая-то смена


                    //6. Уменьшить или обнулить задержку на тупизну
                //if ((currentResults.EnemyDecision == Decision.Attack) && (_client.decision == Decision.Attack)) stupitidyChangeDelay -= 1;
                //else if (currentResults.EnemyDecision != Decision.Attack) stupitidyChangeDelay = numRoundsToWin - HeroManager.player_countRoundsWon - 1;

                // III. Сход закончен  - запустить событие 
                //ExchangeEndedEvent?.Invoke(currentResults);
                _playerManager.ExchangeEnded();
                _enemyManager.ExchangeEnded();

                _commonView.PlayersControlsCanvas.enabled = true;
                
                //7. снять меркеры сделанного хода на клиенте и сервере - вернуться при фотоне
                /*if (gameType == GameType.Server)
                {
                    doServerExchange = false;
                }
                if (gameType == GameType.Client)
                {
                    doClientExchange = false;
                }*/
                
                yield return null;  // заканчиваем этот такт (и не переходим к концу корутины)
            }
        //}
    }
    
    public IEnumerator RoundEnding(int roundNumber, string winner, string prize)                       // конец раунда
    {
        _playerManager.enabled = false;
        _enemyManager.enabled = false;
        _playerAnim.enabled = false;       
        _enemyAnim.enabled = false;  
        
        //1. Всех на переинициализацию.
        //player.enabled = false;
        //enemy.enabled = false;
        yield return _deathWait;                           // используем m_DeathWait ( 2.5 сек), чтоб не плодить сущности
        
        //2. Вывести информационное сообщение.
        //resultText.text = "round".Localize() + _roundNumber + " " + "ended".Localize() + _roundWinner.ToString().Localize() + "win".Localize();
        ChangeResultText("round".Localize() + roundNumber + " " + "ended".Localize() + winner + "win".Localize());

        //3. ждём еще одну m_DeathWait, ибо будем выдавать инвентарь
        //if (prize != null)
        //   yield return _deathWait;
        
        //4. Выдать игроку пункт инвентаря за победу в раунде
        if (winner == _client.PlayerName)
        {
            yield return _deathWait;    //ждём еще одну m_DeathWait, ибо будем выдавать инвентарь
            
            var item = _playerManager.AddPrize(prize);
            if (item != null) ChangeResultText ("you_got".Localize() + prize.Localize());
            
            /* //Добавим инвентарь в state - вернуться при фотоне
            if (gameType!=GameType.Single && a!=null)
            {
                myBoltEntity.GetState<Photon.Bolt.IEFPlayerState>().InventoryItem = a;
            }*/
        }
        
        //5. Врагу тоже
        if (winner == _enemyName) _enemyManager.AddPrize(prize, false);
        
        //6. Изменяем вид врага
        if (_client._roundsLost > 0) _enemyManager.ChangeWeaponsView(_client._roundsLost - 1);
        
        yield return _endWait;

        //enemyNameText.text = string.Empty;    // не надо - меняем только  в начале матча
        
        /* вернуться при фотоне
        player.m_dead = false;
        enemy.m_dead = false;

        doClientExchange = false;
        doServerExchange = false;
        */
    }
}

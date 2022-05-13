using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIClient : Client
{
    private int _stupitidyChangeDelay;                                   // Задержка на тупизну бота перед сменой оружия
    private TurnInInfo _turnInInfo = new TurnInInfo()
    {
        PlayerDecision = Decision.No
    };

    protected override void CheckForSeries()
    {
        PlayerStrongStrikesSeries = currentResults.PlayerSeries[0] >= Series.StrongStrikeSeriesBeginning;
        EnemyStrongStrikesSeries = currentResults.EnemySeries[0] >= Series.StrongStrikeSeriesBeginning;
        PlayerSeriesOfBlocks = currentResults.PlayerSeries[1] >= Series.SeriesBlockBeginning;
        EnemySeriesOfBlocks = currentResults.EnemySeries[1] >= Series.SeriesBlockBeginning;
        PlayerSeriesOfStrikes = currentResults.PlayerSeries[2] >= Series.SeriesStrikeBeginning;
        EnemySeriesOfStrikes = currentResults.EnemySeries[2] >= Series.SeriesStrikeBeginning;
    }
    
        
    protected override void OnStartRound(object o, StartRoundInfo startRoundInfo)
    {
        if (!startRoundInfo.PlayerName.Equals(PlayerName)) return;
        base.OnStartRound(o, startRoundInfo);
        _stupitidyChangeDelay = NumRoundsToWin - roundsLost - 1;
        MakeTurn(roundsLost);     // решение бота на первый сход
    }

    protected override void OnResultsReady(object o, TurnOutInfo results)
    {
        if (!results.PlayerName.Equals(PlayerName)) return;
        
        // Сперва уменьшить или обнулить задержку на тупизну на основании прошлого хода AI и текущего игрока
        if (_turnInInfo.PlayerDecision == Decision.Attack && results.EnemyDecision == Decision.Attack ) 
            _stupitidyChangeDelay--;
        else if (_turnInInfo.PlayerDecision != Decision.Attack) 
            _stupitidyChangeDelay = NumRoundsToWin - roundsLost - 1;

        base.OnResultsReady(o, results);
    }

    protected override void MakeTurn (int nicety)       
    {
        /* nicety = 0: 
         * 1. на свои серии реагирует, 
         * 2. на чужие - нет,
         * 3. относительно оружия - тупит 3 удара, затем меняет
         * 
         * nicety = 1: 
         * 1. на свои серии реагирует, 
         * 2. на чужие - нет,
         * 3. относительно оружия - тупит 2 удара, затем меняет
         * 
         * nicety = 2: 
         * 1. на свои серии реагирует, 
         * 2. на чужие тоже,
         * 3. относительно оружия - тупит 1 удар, затем меняет
         * 
         * nicety = 3: 
         * 1. на свои серии реагирует, 
         * 2. на чужие тоже,
         * 3. относительно оружия - сразу меняет
        */
        
        
         // 1. Если у врага есть серия - продолжаем её (при любом nicety):
        if (PlayerSeriesOfStrikes)
        {
            _turnInInfo.PlayerDecision = Decision.Attack;
            _turnInInfo.PlayerDefencePart = 0f;
            SendDataToServer(_turnInInfo);
            return;
        }
        if (PlayerSeriesOfBlocks)
        {
            _turnInInfo.PlayerDecision = Decision.Attack;
            _turnInInfo.PlayerDefencePart = 1f;  //не забыть перемножить на MaxDefPart
            SendDataToServer(_turnInInfo);
            return;
        }
        // если нет
        // 2. Если у игрока есть серия - нейтрализуем её (при nicety > 1):
        if (nicety > 1)
        {
            if (EnemySeriesOfStrikes)
            {
                _turnInInfo.PlayerDecision = (PlayerWeaponSet != WeaponSet.SwordShield) ? Decision.ChangeSwordShield : Decision.Attack;
                _turnInInfo.PlayerDefencePart = 1f;
                SetWeaponSet(_turnInInfo.PlayerDecision);
                SendDataToServer(_turnInInfo);
                return;
            }
            if (EnemySeriesOfBlocks)
            {
                _turnInInfo.PlayerDecision = (PlayerWeaponSet != WeaponSet.TwoHandedSword) ? Decision.ChangeTwoHandedSword : Decision.Attack;
                _turnInInfo.PlayerDefencePart = 1f;
                SetWeaponSet(_turnInInfo.PlayerDecision);
                SendDataToServer(_turnInInfo);
                return;
            }
        } 
        // а если нет, то
        // 3. Варианты относительно типов оружия (с задержкой на тупизну):
        // 1 - полный рандом; 2 - оптимум - то, что сейчас; 3 - идеальное - надо рассчитывать еще необходимость смены в зависимости от оставшегося здоровья
        // еще можно использовать комбинацию вариантов. Например, оптимум с добавлением небольшого шанса на рандом
        if (_stupitidyChangeDelay > 0) _turnInInfo.PlayerDecision = Decision.Attack;
        else if ((EnemyWeaponSet == WeaponSet.SwordShield) && (PlayerWeaponSet == WeaponSet.SwordSword))
            _turnInInfo.PlayerDecision = Decision.ChangeTwoHandedSword;
        else if ((EnemyWeaponSet == WeaponSet.SwordSword) && (PlayerWeaponSet == WeaponSet.TwoHandedSword))
            _turnInInfo.PlayerDecision = Decision.ChangeSwordShield;
        else if ((EnemyWeaponSet == WeaponSet.TwoHandedSword) && (PlayerWeaponSet == WeaponSet.SwordShield))
            _turnInInfo.PlayerDecision = Decision.ChangeSwordSword;
        else _turnInInfo.PlayerDecision = Decision.Attack;

        // 4. Тактику при этом пока будем выбирать по рандому:
        var tactic = UnityEngine.Random.value;
        _turnInInfo.PlayerDefencePart = tactic < 0.33f ? 1f : 0f;
        
        // 5. Не забываем проставить weaponSet, а то понадобится в HeroAnimation
        SetWeaponSet(_turnInInfo.PlayerDecision);

        SendDataToServer(_turnInInfo);
    }

    private void SetWeaponSet(Decision decision)
    {
        if (_turnInInfo.PlayerDecision != Decision.Attack)
        {
            PlayerWeaponSet = decision switch
            {
                Decision.ChangeSwordShield => WeaponSet.SwordShield,
                Decision.ChangeSwordSword => WeaponSet.SwordSword,
                Decision.ChangeTwoHandedSword => WeaponSet.TwoHandedSword
            };
        }
    }
}

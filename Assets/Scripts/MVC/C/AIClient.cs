using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIClient : Client
{
    private int stupitidyChangeDelay;                                   // Задержка на тупизну бота перед сменой оружия

    protected override void CheckForSeries()
    {
        PlayerStrongStrikesSeries = _currentResults.PlayerSeries[0] >= Series.StrongStrikeSeriesBeginning;
        EnemyStrongStrikesSeries = _currentResults.EnemySeries[0] >= Series.StrongStrikeSeriesBeginning;
        PlayerSeriesOfBlocks = _currentResults.PlayerSeries[1] >= Series.SeriesBlockBeginning;
        EnemySeriesOfBlocks = _currentResults.EnemySeries[1] >= Series.SeriesBlockBeginning;
        PlayerSeriesOfStrikes = _currentResults.PlayerSeries[2] >= Series.SeriesStrikeBeginning;
        EnemySeriesOfStrikes = _currentResults.EnemySeries[2] >= Series.SeriesStrikeBeginning;
    }
    
        
    protected override void OnStartRound(object o, StartRoundInfo startRoundInfo)
    {
        base.OnStartRound(o, startRoundInfo);
        stupitidyChangeDelay = numRoundsToWin - _roundsLost - 1;
        MakeTurn(_roundsWon);     // решение бота на первый сход
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
        
         var turnInInfo = new TurnInInfo();

         // 1. Если у врага есть серия - продолжаем её (при любом nicety):
        if (PlayerSeriesOfStrikes)
        {
            turnInInfo.PlayerDecision = Decision.Attack;
            turnInInfo.PlayerDefencePart = /*enemy.m_Tweakers.ParryChance*/0f;
            SendDataToServer(turnInInfo);
            return;
        }
        if (PlayerSeriesOfBlocks)
        {
            turnInInfo.PlayerDecision = Decision.Attack;
            turnInInfo.PlayerDefencePart = /*enemy.m_Tweakers.MaxDefencePart + enemy.m_Tweakers.ParryChance*/1f;  /*не забыть перемножить на MaxDefPart*/
            SendDataToServer(turnInInfo);
            return;
        }
        // если нет
        // 2. Если у игрока есть серия - нейтрализуем её (при nicety > 1):
        if (nicety > 1)
        {
            if (EnemySeriesOfStrikes)
            {
                turnInInfo.PlayerDecision = (PlayerWeaponSet != WeaponSet.SwordShield) ? Decision.ChangeSwordShield : Decision.Attack;
                turnInInfo.PlayerDefencePart = /*enemy.m_Tweakers.MaxDefencePart + enemy.m_Tweakers.ParryChance*/1f;
                SendDataToServer(turnInInfo);
                return;
            }
            if (EnemySeriesOfBlocks)
            {
                turnInInfo.PlayerDecision = (PlayerWeaponSet != WeaponSet.TwoHandedSword) ? Decision.ChangeTwoHandedSword : Decision.Attack;
                turnInInfo.PlayerDefencePart = /*enemy.m_Tweakers.ParryChance*/1f;
                SendDataToServer(turnInInfo);
                return;
            }
        }
        // если нет
        // 3. Варианты относительно типов оружия (с задержкой на тупизну):
        // 1 - полный рандом; 2 - оптимум - то, что сейчас; 3 - идеальное - надо рассчитывать еще необходимость смены в зависимости от оставшегося здоровья
        // еще можно использовать комбинацию вариантов. Например, оптимум с добавлением небольшого шанса на рандом
        if (stupitidyChangeDelay > 0) turnInInfo.PlayerDecision = Decision.Attack;
        else if ((EnemyWeaponSet == WeaponSet.SwordShield) && (PlayerWeaponSet == WeaponSet.SwordSword))
            turnInInfo.PlayerDecision = Decision.ChangeTwoHandedSword;
        else if ((EnemyWeaponSet == WeaponSet.SwordSword) && (PlayerWeaponSet == WeaponSet.TwoHandedSword))
            turnInInfo.PlayerDecision = Decision.ChangeSwordShield;
        else if ((EnemyWeaponSet == WeaponSet.TwoHandedSword) && (PlayerWeaponSet == WeaponSet.SwordShield))
            turnInInfo.PlayerDecision = Decision.ChangeSwordSword;
        else turnInInfo.PlayerDecision = Decision.Attack;

        // Уменьшить или обнулить задержку на тупизну
        if (turnInInfo.PlayerDecision == Decision.Attack) stupitidyChangeDelay -= 1;
        else stupitidyChangeDelay = numRoundsToWin - _roundsLost - 1;
        
        
        // 4. Тактику при этом пока будем выбирать по рандому:
        var tactic = UnityEngine.Random.value;
        turnInInfo.PlayerDefencePart = tactic < 0.33f ? /*enemy.m_Tweakers.MaxDefencePart + enemy.m_Tweakers.ParryChance*/1f : /*enemy.m_Tweakers.ParryChance*/0f;

        // 5. Не забываем проставить weaponSet, а то понадобится в HeroAnimation
        if (turnInInfo.PlayerDecision != Decision.Attack)
        {
            PlayerWeaponSet = turnInInfo.PlayerDecision switch  
            {  
                Decision.ChangeSwordShield => WeaponSet.SwordShield,  
                Decision.ChangeSwordSword => WeaponSet.SwordSword,  
                Decision.ChangeTwoHandedSword => WeaponSet.TwoHandedSword
            }; 
        }
        
        SendDataToServer(turnInInfo);
    }
}

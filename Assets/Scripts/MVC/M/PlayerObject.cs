[System.Serializable]
public struct PreCoeffs
{
    public float damage;                                           // Возможный нанесенный урон
    public bool evade;                                             // Уворот от удара на смене     
    public bool block;                                             // Блок удара
    public bool parry;                                             // Парирование удара   
    public bool blockVs2Handed;                                    // Блок удара двуручником - половина урона
}

[System.Serializable]
public class PlayerObject
{ 
// не меняются в течении турнира
    public string name;
// не меняются в течении раунда
    public Item[] inventoryItems = new Item[3];
    public Tweakers Tweakers { get; private set; }                   // Настройки балланса боёвки
    public int roundsWon; 
    public int roundsLost;
// меняются каждый ход
    //входные
    public WeaponSet weaponSet;
    public float defencePart;                                        // Тактика боя - ориентированность на защиту: от 0 до 33% урона меняется на возможность парирования (шаги на сегодня: 0%, 33%)
    public Decision decision;
    public bool dataTaken;
    //выходные
    public PreCoeffs[] preCoeffs = new PreCoeffs[2];                 // Предв. значения для рассчета урона
    public ExchangeResult[] exchangeResults = new ExchangeResult[2]; // Результаты ударов
    public int[] gotDamages = new int[2];                            // Возможный получаемый урон на текущий уда

    public bool dead;

    public HP Hp { get; private set; }
    public Series Series { get; private set; }    

    public PlayerObject(string name)
    {
        this.name = name;
        Hp = new HP();
        Series = new Series(this);
    }

    public void Reset()
    {
        decision = Decision.No;
        weaponSet = WeaponSet.SwordShield;
        Tweakers = new Tweakers();
        if (name.Equals("bot")) Tweakers.AddLevelTweakers(roundsLost);
        Tweakers.AddInventoryTweakers(inventoryItems);
        Hp.SetStartHealth(Tweakers.StartingHealth);
        Series.ResetAll();
        dead = false;
    }
    
    public void CalculatePreCoeffs()
    {
        preCoeffs[0].parry = (UnityEngine.Random.value <= defencePart);
        preCoeffs[1].parry = (UnityEngine.Random.value <= defencePart);

        // Предварительные коэффициенты на основе текущего набора оружия
        switch (weaponSet)
        {
            case WeaponSet.SwordShield:
                preCoeffs[0].damage = UnityEngine.Random.Range(Tweakers.DamageBaseMin, Tweakers.DamageBaseMax + 1);
                preCoeffs[0].damage += Series.AddSeriesDamage();
                preCoeffs[1].damage = 0f;
                preCoeffs[0].block = (UnityEngine.Random.Range(0f, 1f) <= Tweakers.BlockChance);
                preCoeffs[1].block = (UnityEngine.Random.Range(0f, 1f) <= Tweakers.BlockChance);
                break;
            case WeaponSet.SwordSword:
                preCoeffs[0].damage = UnityEngine.Random.Range(Tweakers.DamageBaseMin, Tweakers.DamageBaseMax + 1);
                preCoeffs[0].damage += Series.AddSeriesDamage();
                preCoeffs[1].damage = UnityEngine.Random.Range(Tweakers.DamageBaseMin * Tweakers.CoefSecondSword, Tweakers.DamageBaseMax * Tweakers.CoefSecondSword);
                preCoeffs[1].damage += Series.AddSeriesDamage();
                preCoeffs[0].block = false;
                preCoeffs[1].block = false;
                preCoeffs[0].blockVs2Handed = false;
                break;
            case WeaponSet.TwoHandedSword:
                preCoeffs[0].damage = UnityEngine.Random.Range(Tweakers.DamageBaseMin * Tweakers.Coef2HandedSword, Tweakers.DamageBaseMax * Tweakers.Coef2HandedSword);
                preCoeffs[0].damage += Series.AddSeriesDamage();
                preCoeffs[1].damage = 0f;
                preCoeffs[0].block = false;
                preCoeffs[1].block = false;
                preCoeffs[0].blockVs2Handed = false;
                break;
        }
        // А также предварительные коэффициенты на основе решения
        switch (decision)
        {
            case Decision.Attack:
                preCoeffs[0].evade = false;
                preCoeffs[1].evade = false;
                break;
            default:                                                    // точно какая-то смена
                preCoeffs[0].evade = (UnityEngine.Random.Range(0f, 1f) <= Tweakers.EvadeOnChangeChance);
                preCoeffs[1].evade = (UnityEngine.Random.Range(0f, 1f) <= Tweakers.EvadeOnChangeChance);
                preCoeffs[0].block = false;
                preCoeffs[1].block = false;
                preCoeffs[0].blockVs2Handed = false;
                preCoeffs[0].parry = false;
                preCoeffs[1].parry = false;
                break;
        }
    }
    
    public ExchangeResult CalculateExchangeResult(int strikeNumber)
    {
        if (preCoeffs[strikeNumber - 1].parry) return ExchangeResult.Parry;                   // А. парирование
        if (preCoeffs[strikeNumber - 1].blockVs2Handed) return ExchangeResult.BlockVs2Handed; // Б. пробитие щита двуручником
        if (preCoeffs[strikeNumber - 1].block) return ExchangeResult.Block;                   // В. блок
        if (preCoeffs[strikeNumber - 1].evade) return ExchangeResult.Evade;                   // Г. уворот на смене
        return ExchangeResult.GetHit;                                                         // Д. принять полный первый удар
    }
    
    public void SetSwordSword() => weaponSet = WeaponSet.SwordSword;
    public void SetSwordShield() => weaponSet = WeaponSet.SwordShield;
    public void SetTwoHandedSword() => weaponSet = WeaponSet.TwoHandedSword;

    public int AddInventoryItem(Item itemToAdd)                      // добавить пункт инвентаря
    {
        for (int i = 0; i < inventoryItems.Length; i++)
        {
            if (inventoryItems[i] == itemToAdd) return -2;           // такой предмет уже есть - не добавляем
            if (inventoryItems[i] == null)                           
            {
                inventoryItems[i] = itemToAdd;                       // помещаем сам item в массив item-ов
                return i;
            }
        }
        return -1;                              // не удалось добавить по причине того, что инвенторий полон
    }
}
using UnityEngine;

/// <summary>
/// Настройки балланса боёвки 
/// </summary>
[System.Serializable]
public class Tweakers      
{
    [SerializeField] private int damageBaseMin = 5;                          // минимальный базовый урон
    [SerializeField] private int damageBaseMax = 15;                         // максимальный базовый урон
    [SerializeField] private float coef2HandedSword = 1.54f;                 // увеличение урона при двуручнике относительно базового
    [SerializeField] private float coefSecondSword = 0.7f;                   // уменьшение урона при ударе вторым мечом относительно базового
    [SerializeField] private float blockChance = 0.5f;                       // шанс блока шитом
    [SerializeField] private float part2HandedThroughShield = 0.5f;          // доля урона двурой, что проходит сквозь щит
    [SerializeField] private float evadeOnChangeChance = 0.33f;              // шанс уворота на смене
    [SerializeField] private float maxDefencePart = 0.33f;                   // процент урона, что меняется на шанс парирования при тактике-защите
    [SerializeField] private float parryChance = 0f;                         // шанс парировать удар противника
    [SerializeField] private int startingHealth = 100;                       // начальное здоровье

    public int DamageBaseMin => damageBaseMin;
    public int DamageBaseMax => damageBaseMax;                              
    public float Coef2HandedSword => coef2HandedSword;   
    public float CoefSecondSword => coefSecondSword;
    public float BlockChance => blockChance;
    public float Part2HandedThroughShield => part2HandedThroughShield;
    public float EvadeOnChangeChance => evadeOnChangeChance;
    public float MaxDefencePart => maxDefencePart;
    public float ParryChance => parryChance;
    public int StartingHealth => startingHealth;

    public void AddInventoryTweakers(Item[] items)
    {
        for (var i = 0; i < items.Length; i++)
        {
            if (items[i] == null) 
                continue;
            
            damageBaseMin += items[i].DamageModifierAdd;
            damageBaseMax += items[i].DamageModifierAdd;

            coef2HandedSword *= 1 + items[i].Coef2HandSwordModifier / 100f;

            coefSecondSword *= 1 + items[i].CoefSecondSwordModifier / 100f;

            blockChance *= 1 + items[i].BlockChanceModifier / 100f;

            part2HandedThroughShield *= 1 + items[i].Part2HandedThroughShieldModifier / 100f;

            evadeOnChangeChance *= 1 + items[i].EvadeOnChangeChanceModifier / 100f;

            parryChance += items[i].ParringChanceModifier / 100f;

            startingHealth += items[i].StartHealthModifierAdd;
            startingHealth = Mathf.RoundToInt(startingHealth*(1 + items[i].StartHealthModifierMul / 100f));
        }
    }

    public void AddLevelTweakers(int level)
    {
        switch (level)
        {
            case 1:
                damageBaseMin += 1;
                break;
            case 2:
                damageBaseMin += 2;
                damageBaseMax += 1;
                break;
            case 3:
                damageBaseMin += 2;
                damageBaseMax += 2;
                break;
        }
    }
}

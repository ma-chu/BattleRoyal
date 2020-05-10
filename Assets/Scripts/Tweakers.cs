using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tweakers                                      // Настройки балланса боёвки         
{
    [SerializeField]
    private int damageBaseMin;                          // минимальный базовый урон
    [SerializeField]
    private int damageBaseMax;                          // максимальный базовый урон
    [SerializeField]
    private float coef2HandedSword;                     // увеличение урона при двуручнике
    [SerializeField]
    private float coefSecondSword;                      // уменьшение при ударе вторым мечом
    [SerializeField]
    private float blockChance;                          // шанс блока шитом
    [SerializeField]
    private float part2HandedThroughShield;             // доля урона двурой, что проходит сквозь щит
    [SerializeField]
    private float evadeOnChangeChance;                  // шанс уворота на смене
    [SerializeField]
    private float maxDefencePart;                       // процент урона, что меняется на шанс парирования при тактике-защите
    [SerializeField]
    private float parryChance;                          // шанс парировать удар противника
    [SerializeField]
    private float startingHealth = 100f;                // начальное здоровье

    public int DamageBaseMin
    {
        get { return damageBaseMin; }
    }
    public int DamageBaseMax
    {
        get { return damageBaseMax; }
    }
    public float Coef2HandedSword
    {
        get { return coef2HandedSword; }
    }
    public float CoefSecondSword
    {
        get { return coefSecondSword; }
    }
    public float BlockChance
    {
        get { return blockChance; }
    }
    public float Part2HandedThroughShield
    {
        get { return part2HandedThroughShield; }
    }
    public float EvadeOnChangeChance
    {
        get { return evadeOnChangeChance; }
    }
    public float MaxDefencePart
    {
        get { return maxDefencePart; }
    }
    public float ParryChance
    {
        get { return parryChance; }
    }
    public float StartingHealth
    {
        get { return startingHealth; }
    }

    public Tweakers()                                  // значения по-умолчанию
    {
        damageBaseMin = 5;
        damageBaseMax = 15;
        coef2HandedSword = 1.54f;
        coefSecondSword = 0.7f;
        blockChance = 0.5f;
        part2HandedThroughShield = 0.5f;
        evadeOnChangeChance = 0.33f;
        maxDefencePart = 0.33f;
        parryChance = 0f;
        startingHealth = 100f;
    }

    public void AddInventoryTweakers(Inventory inventory)     // Пересчитать твикеры с учетом модификаторов инвентаря
    {
        for (int i = 0; i < inventory.items.Length; i++)
        {
            if (inventory.items[i] != null)
            {
                damageBaseMin += inventory.items[i].DamageModifierAdd;
                damageBaseMax += inventory.items[i].DamageModifierAdd;

                coef2HandedSword *= 1 + inventory.items[i].Coef2HandSwordModifier / 100f;

                coefSecondSword *= 1 + inventory.items[i].CoefSecondSwordModifier / 100f;

                blockChance *= 1 + inventory.items[i].BlockChanceModifier / 100f;

                part2HandedThroughShield *= 1 + inventory.items[i].Part2HandedThroughShieldModifier / 100f;

                evadeOnChangeChance *= 1 + inventory.items[i].EvadeOnChangeChanceModifier / 100f;

                parryChance += inventory.items[i].ParringChanceModifier / 100f;

                startingHealth += inventory.items[i].StartHealthModifierAdd;
                startingHealth *= 1 + inventory.items[i].StartHealthModifierMul / 100f;
            }
        }
    }

    public void AddLevelTweakers(int level)     // Пересчитать твикеры с учетом уровня
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

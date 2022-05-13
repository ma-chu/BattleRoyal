using UnityEngine;

[System.Serializable]
public class Tweakers                                      // Настройки балланса боёвки         
{
    [SerializeField]
    private int damageBaseMin;                          // минимальный базовый урон
    [SerializeField]
    private int damageBaseMax;                          // максимальный базовый урон
    [SerializeField]
    private float coef2HandedSword;                     // увеличение урона при двуручнике относительно базового
    [SerializeField]
    private float coefSecondSword;                      // уменьшение при ударе вторым мечом относительно базового
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
    private int startingHealth;                         // начальное здоровье

    public int DamageBaseMin                            // 1. Полная запись свойства (с именнованным полем)
    {
        get
        {
            return damageBaseMin;
        }
    }
    public int DamageBaseMax                               
    {
        get => damageBaseMax;
        //set => damageBaseMax=value;                    // 2. Сокращенная запись св-ва (с именнованным полем); позволяет использовать модификатор set
    }

    public float Coef2HandedSword => coef2HandedSword;   // 3. Еще более сокращенная запись readonly свойства (с именнованным полем); не позволяет использовать модификатор set
    // 4. Еще короче только автоматически реализуемое свойство: public static Language CurrentLanguage { get; private set; }  НО здесь поле неименнованное, что плохо для сериализации 

    public float CoefSecondSword => coefSecondSword;
    public float BlockChance => blockChance;
    public float Part2HandedThroughShield => part2HandedThroughShield;
    public float EvadeOnChangeChance => evadeOnChangeChance;
    public float MaxDefencePart => maxDefencePart;
    public float ParryChance => parryChance;
    public int StartingHealth => startingHealth;

    public Tweakers()                                          // значения по-умолчанию
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
        startingHealth = 100;
    }

    public void AddInventoryTweakers(Item[] items)     // Пересчитать твикеры с учетом модификаторов инвентаря
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
            {
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

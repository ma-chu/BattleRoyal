using UnityEngine;

// This simple script represents Items in the game. The inventory system is done using
// this script instead of just sprites to ensure that items are extensible.

[CreateAssetMenu]                                   // позволяет добавлять себя себя через меню редактора Assets|Create
public class Item : ScriptableObject                // наследование от ScriptableObject означает, что мы можем сохранить этот скрипт как asset (и создавать его instance-ы)
{
    [SerializeField]
    private Sprite sprite;                          // картинка
    [SerializeField]
    private string description;                     // описание

    // модификаторы предмета
    [SerializeField]
    private int damageModifierAdd;                   // модификатор значения урона, аддитивный, абс.единицы
    //public int damageModifierMul;                  // модификатор значения урона, мультипликативный, %
    [SerializeField]
    private int blockChanceModifier;                 // модификатор значения блока щитом, мультипликативный, %
    [SerializeField]
    private int evadeOnChangeChanceModifier;         // модификатор значения шанса уворота на смене, мультипликативный, %
    [SerializeField]
    private int coefSecondSwordModifier;             // модификатор значения силы удара вторым мечом относительно базового первого, мультипликативный, %
    [SerializeField]
    private int coef2HandSwordModifier;              // модификатор значения силы удара двурой, мультипликативный, %
    [SerializeField]
    private int startHealthModifierMul;              // модификатор значения начального здоровья, мультипликативный, %
    [SerializeField]
    private int startHealthModifierAdd;              // модификатор значения начального здоровья, аддитивный, абс.ед.
    [SerializeField]
    private int part2HandedThroughShieldModifier;    // модификатор значения доля урона двурой, что проходит сквозь щит, мультипликативный, %
    [SerializeField]
    private int parringChanceModifier;               // модификатор значения шанса парирования, АДДИТИВНЫЙ, %

    public Sprite Sprite
    {
        get { return sprite; }
    }
    public string Description
    {
        get { return description; }
    }
    public int DamageModifierAdd
    {
        get { return damageModifierAdd; }
    }
    public int BlockChanceModifier
    {
        get { return blockChanceModifier; }
    }
    public int EvadeOnChangeChanceModifier
    {
        get { return evadeOnChangeChanceModifier; }
    }
    public int CoefSecondSwordModifier
    {
        get { return coefSecondSwordModifier; }
    }
    public int Coef2HandSwordModifier
    {
        get { return coef2HandSwordModifier; }
    }
    public int StartHealthModifierMul
    {
        get { return startHealthModifierMul; }
    }
    public int StartHealthModifierAdd
    {
        get { return startHealthModifierAdd; }
    }
    public int Part2HandedThroughShieldModifier
    {
        get { return part2HandedThroughShieldModifier; }
    }
    public int ParringChanceModifier
    {
        get { return parringChanceModifier; }
    }
}

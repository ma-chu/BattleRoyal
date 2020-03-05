using UnityEngine;

// This simple script represents Items that can be picked
// up in the game.  The inventory system is done using
// this script instead of just sprites to ensure that items
// are extensible.

[CreateAssetMenu]                       // позволяет добавлять себя себя через меню редактора Assets|Create
public class Item : ScriptableObject    // наследование от ScriptableObject означает, что мы можем сохранить этот скрипт как asset (и создавать его instance-ы)
{
    public Sprite sprite;               // картинка
    public string description;          // описание

    // модификаторы предмета
    public int damageModifier;          // модификатор значения урона, %
    public bool damageModMulAdd;        // true - мультипликативный, false - аддитивный

    public int blockChanceModifier;     // модификатор значения блока щитом, %
    public bool blockChanceModMulAdd;   // true - мультипликативный, false - аддитивный

    public int evadeOnChangeChanceModifier;   // модификатор значения шанса уворота на смене, %
    public bool evadeOnChangeChanceModMulAdd; // true - мультипликативный, false - аддитивный

    public int koefSecondSwordModifier;  // модификатор значения силы второго удара относительно первого, %
    public bool koefSecondSwordModMulAdd;// true - мультипликативный, false - аддитивный

    public int koef2HandSwordModifier;  // модификатор значения силы удара двурой, %
    public bool koef2HandSwordModMulAdd;// true - мультипликативный, false - аддитивный

    public int startHealthModifier;     // модификатор значения начального здоровья, %
    public bool startHealthModMulAdd;   // true - мультипликативный, false - аддитивный

    public int part2HandedThroughShieldModifier;  // модификатор значения доля урона двурой, что проходит сквозь щит, %
    public bool part2HandedThroughShieldModMulAdd;// true - мультипликативный, false - аддитивный

    public int parringChanceModifier;    // модификатор значения шанса парирования, %
    public bool parringChanceModMulAdd;  // true - мультипликативный, false - аддитивный
}

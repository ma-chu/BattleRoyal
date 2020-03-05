using UnityEngine; 

public class EnemyManager : HeroManager
{
    public override void Awake()
    {
        // получаем ссылки на объекты-оружия
        m_Hero2HandedSword = GameObject.FindGameObjectWithTag("twohandedsword_enemy");
        m_HeroSword = GameObject.FindGameObjectWithTag("sword_enemy");
        m_HeroSword_2 = GameObject.FindGameObjectWithTag("sword_2_enemy");
        m_HeroShield = GameObject.FindGameObjectWithTag("shield_enemy");

        // определимся со ссылками на слоты инвентория
        for (int i = 0; i < Inventory.numItemSlots; i++)
        {
            itemSlots = GameObject.FindGameObjectsWithTag("itemSlot_enemy");
        }

        base.Awake();                                               // запустить базовую Awake из класса-родителя

    }

    public override void OnEnable()                                 // что мы делаем, когда герой снова жив (back on again, следующий раунд)
    {
        zeroZposition = 1.35f;
        zeroYrotation = -180f;                                      // противоположное игроку вращение
        stockXposition = -2.2f;

        transform.rotation = Quaternion.Euler(0f, 90f, 0f);         // установить начальное вращение
        base.OnEnable();                                            // запустить бозовую OnEnable из класса-родителя
    }

}

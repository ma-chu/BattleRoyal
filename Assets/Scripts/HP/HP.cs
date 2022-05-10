using UnityEngine;
//  ЗДОРОВЬЕ ГЕРОЯ: Логика
public class HP
{
    public float Health { get; private set; }

    public void SetStartHealth(float startHealth) => Health = startHealth;     // Установить начальное здоровье     

    public bool TakeDamage(float amount)                 // возвращает true, если после текущего удара герой умер
    {                                                  
        Health -= amount;
        if (Health <= 0f)  return true;
        //Debug.Log(Health + " ");
        return false;
    }

    public void RegenHealth(float amount) => Health += amount;     // регенерация здоровья (за серию блоков). Вызывается Series-ом  
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#pragma warning disable 0649    // убирает предупреждения компилятора о [SerializeField] private переменных, инициализируемых в редакторе   
//  ЗДОРОВЬЕ ГЕРОЯ
public class HP : MonoBehaviour
{
    [SerializeField]
    private Slider m_Slider;                             // ссылка на слайдер здоровья                
    [SerializeField]
    private Image m_FillImage;                           // будем контролировать перемещение именно fillImage  (относительно фона)            
    [SerializeField]
    private Color m_FullHealthColor = Color.green;       // цвет для 100% здоровья
    [SerializeField]
    private Color m_ZeroHealthColor = Color.red;         // цвет для 0% здоровья
    [SerializeField]
    private float m_CurrentHealth;                       // текущее количество здоровья  
    private float m_StartHealth = 100f;

    public void SetStartHealth(float startHealth)        // Установить начальное здоровье
    {
        m_StartHealth = startHealth;
        m_CurrentHealth = startHealth;
        SetHealthUI();                                   // двигаем слайдер (и корректируем его цвет)
    }
    // ПРИНЯТЬ УДАР
    public bool TakeDamage(float amount)                // public, следовательно будет вызываться другим объектом - менеджером игры
    {                                                   // возвращает true, если после текущего удара герой умер
        m_CurrentHealth -= amount;                          
        SetHealthUI();                          
        
        if (m_CurrentHealth <= 0f)  return true;
        else return false;
    }

    public void RegenHealth(float amount)        
    {
        m_CurrentHealth += amount;                            
        SetHealthUI();                                             
    }

    private void SetHealthUI()                          // двигаем слайдер (и корректируем его цвет)
    {
        m_Slider.value = m_CurrentHealth;                           
        m_FillImage.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / m_StartHealth);   // плавно меняем цвет верхней (зеленой при 100% жизни) картинки слайдера на красную при 0%
    }
}


using UnityEngine;
using UnityEngine.UI;
//  ЗДОРОВЬЕ ГЕРОЯ: Отображение
public class HPView : MonoBehaviour
{
    [SerializeField] private Slider m_Slider;                             // ссылка на слайдер здоровья                
    [SerializeField] private Image m_FillImage;                           // будем контролировать перемещение именно fillImage  (относительно фона)            
    [SerializeField] private Color m_FullHealthColor = Color.green;       // цвет для 100% здоровья
    [SerializeField] private Color m_ZeroHealthColor = Color.red;         // цвет для 0% здоровья
    
    private float _startHealth;                                           // начальное количество здоровья  
    private float _health;                                                // текущее количество здоровья  


    public void SetStartHealth(float startHealth)      // Установить начальное здоровье - нужна ли?
    {
        _startHealth = startHealth;
        SetHealth(startHealth);
    }

    public void SetHealth(float health)                // Установить текущее здоровье
    {
        _health = health;
        SetHealthUI();                                   
    }

    private void SetHealthUI()                          // двигаем слайдер (и корректируем его цвет)
    {
        m_Slider.value = _health;                           
        m_FillImage.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, _health / _startHealth);   // плавно меняем цвет верхней (зеленой при 100% жизни) картинки слайдера на красную при 0%
    }
}


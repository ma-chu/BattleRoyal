using UnityEngine;
using UnityEngine.UI;

public class HPView : MonoBehaviour
{
    [SerializeField] private Slider slider;               
    [SerializeField] private Image fillImage;            
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color zeroHealthColor = Color.red;
    
    private float _startHealth;
    private float _health;

    public void SetStartHealth(float startHealth)
    {
        _startHealth = startHealth;
        SetHealth(startHealth);
    }

    public void SetHealth(float health)
    {
        _health = health;
        SetHealthUI();                                   
    }

    private void SetHealthUI()
    {
        slider.value = _health;                           
        fillImage.color = Color.Lerp(zeroHealthColor, fullHealthColor, _health / _startHealth);
    }
}
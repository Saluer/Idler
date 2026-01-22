using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class HealthBar : MonoBehaviour
{
    private Image _fillImage;

    private int _maxHealth;

    private void Awake()
    {
        _fillImage = GetComponent<Image>();
    }

    public void Init(int maxHealth)
    {
        _maxHealth = maxHealth;
        SetHealth(maxHealth);
    }

    public void SetHealth(int currentHealth)
    {
        Debug.Log(_fillImage);
        if (_maxHealth <= 0) return;
        
        _fillImage.fillAmount = (float)currentHealth / _maxHealth;
    }
}
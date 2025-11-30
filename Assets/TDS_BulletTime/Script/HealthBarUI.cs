using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Health health;   // скрипт здоровья игрока
    [SerializeField] private Slider slider;   // сам Slider на Canvas

    private void Awake()
    {
        // если не назначил в инспекторе — ищем игрока по тегу
        if (health == null)
            health = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();
    }

    private void OnEnable()
    {
        health.OnHealthChanged.AddListener(UpdateHealthBar);
    }

    private void OnDisable()
    {
        health.OnHealthChanged.RemoveListener(UpdateHealthBar);
    }

    private void Start()
    {
        slider.minValue = 0f;
        slider.maxValue = health.GetMaxHealth();
        slider.value = health.GetCurrentHealth();
    }

    private void UpdateHealthBar(float currentHealth)
    {
        slider.value = currentHealth;
    }
}

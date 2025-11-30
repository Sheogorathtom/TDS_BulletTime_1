using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Система здоровья для игрока и врагов
/// Модульный компонент, работает с любым объектом
/// </summary>
public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Events")]
    public UnityEvent<float> OnHealthChanged; // вызывается при изменении здоровья
    public UnityEvent OnDeath; // вызывается при смерти

    private bool isDead = false;

    // НЕУЯЗВИМОСТЬ
    [Header("Invulnerability")]
    [SerializeField] private bool _isInvulnerable = false;

    public void SetInvulnerable(bool value)
    {
        _isInvulnerable = value;
    }

    public bool IsInvulnerable()
    {
        return _isInvulnerable;
    }

    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
    }

    /// <summary>
    /// Получить урон
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        if (_isInvulnerable) return; // во время инвула урон не проходит

        currentHealth -= damage;
        OnHealthChanged?.Invoke(currentHealth);

        Debug.Log($"{gameObject.name} получил {damage} урона. HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Восстановить здоровье
    /// </summary>
    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);

        Debug.Log($"{gameObject.name} восстановил {amount} HP. HP: {currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// Смерть объекта
    /// </summary>
    private void Die()
    {
        isDead = true;
        OnDeath?.Invoke();

        Debug.Log($"{gameObject.name} умер!");

        // Для игрока - можно добавить Game Over
        // Для врага - уничтожить или отключить
        if (CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
        else if (CompareTag("Player"))
        {
            // TODO: Показать Game Over экран
            Time.timeScale = 0f; // Пауза игры
        }
    }

    /// <summary>
    /// Получить текущее здоровье (0-1)
    /// </summary>
    public float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }

    /// <summary>
    /// Получить текущее здоровье (абсолютное значение)
    /// </summary>
    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    /// <summary>
    /// Получить максимальное здоровье
    /// </summary>
    public float GetMaxHealth()
    {
        return maxHealth;
    }

    /// <summary>
    /// Проверить, мёртв ли объект
    /// </summary>
    public bool IsDead()
    {
        return isDead;
    }
}

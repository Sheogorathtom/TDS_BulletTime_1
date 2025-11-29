using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Обновленный скрипт стрельбы игрока с поддержкой перезарядки
/// Интегрирует PlayerShooting, AmmoSystem и обработку input'а
/// </summary>
public class PlayerShooting : MonoBehaviour
{
    public WeaponBase currentWeapon;
    private AmmoSystem ammoSystem;

    void Awake()
    {
        currentWeapon = GetComponentInChildren<WeaponBase>();
        ammoSystem = GetComponent<AmmoSystem>();

        if (currentWeapon == null)
            Debug.LogWarning("WeaponBase не найдена!");
        if (ammoSystem == null)
            Debug.LogWarning("AmmoSystem не найдена!");
    }

    // ===== INPUT SYSTEM CALLBACKS =====

    /// <summary>
    /// Callback для стрельбы
    /// Input System путь: Action "Shoot" → LMB или какая-то другая клавиша
    /// </summary>
    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            currentWeapon?.TryShoot();
        }
    }

    /// <summary>
    /// Callback для перезарядки
    /// INPUT SYSTEM: Создай отдельное Action "Reload" с клавишей R
    /// Путь: Input Asset → Add Action "Reload" → Binding R
    /// </summary>
    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.performed) // Срабатывает при нажатии клавиши R
        {
            ammoSystem?.StartReload();
        }
    }

    // ===== МЕТОДЫ ДЛЯ UI =====

    /// <summary>
    /// Получить текущее количество патронов (для UI)
    /// </summary>
    public int GetCurrentAmmo()
    {
        return ammoSystem?.GetCurrentAmmo() ?? 0;
    }

    /// <summary>
    /// Получить резервные патроны (для UI)
    /// </summary>
    public int GetReserveAmmo()
    {
        return ammoSystem?.GetReserveAmmo() ?? 0;
    }

    /// <summary>
    /// Проверить, идёт ли перезарядка (для UI или анимаций)
    /// </summary>
    public bool IsReloading()
    {
        return ammoSystem?.IsReloading() ?? false;
    }
}
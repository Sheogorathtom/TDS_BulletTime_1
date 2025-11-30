using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Стрельба игрока + перезарядка
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

    /// <summary>
    /// Input System: Action "Shoot"
    /// </summary>
    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            bool shotDone = false;

            if (currentWeapon != null)
            {
                // если в TryShoot возвращается bool — используй:
                // shotDone = currentWeapon.TryShoot();
                // если нет — просто вызываем и считаем что выстрел есть:
                currentWeapon.TryShoot();
                shotDone = true;
            }

            if (shotDone && CameraShaker.Instance != null)
            {
                CameraShaker.Instance.Shake();        // или Shake(0.1f, 0.3f);
            }
        }
    }

    /// <summary>
    /// Input System: Action "Reload"
    /// </summary>
    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ammoSystem?.StartReload();
        }
    }

    public int GetCurrentAmmo()
    {
        return ammoSystem?.GetCurrentAmmo() ?? 0;
    }

    public int GetReserveAmmo()
    {
        return ammoSystem?.GetReserveAmmo() ?? 0;
    }

    public bool IsReloading()
    {
        return ammoSystem?.IsReloading() ?? false;
    }
}

using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Система патронов для оружия
/// Отслеживает текущие патроны, максимум и перезарядку
/// </summary>
public class AmmoSystem : MonoBehaviour
{
    [Header("Ammo Settings")]
    [SerializeField] private int maxAmmo = 30; // максимум патронов в магазине
    [SerializeField] private int maxReserveAmmo = 120; // максимум патронов в резерве
    private int currentAmmo;
    private int currentReserveAmmo;

    [Header("Reload Settings")]
    [SerializeField] private float reloadTime = 2f; // время перезарядки в секундах
    private float reloadTimer = 0f;
    private bool isReloading = false;

    [Header("Events")]
    public UnityEvent<int, int> OnAmmoChanged; // (текущие патроны, макс патроны)
    public UnityEvent<int> OnReserveAmmoChanged; // (резервные патроны)
    public UnityEvent OnReloadStart; // начало перезарядки
    public UnityEvent OnReloadComplete; // конец перезарядки

    void Start()
    {
        currentAmmo = maxAmmo;
        currentReserveAmmo = maxReserveAmmo;
        OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
        OnReserveAmmoChanged?.Invoke(currentReserveAmmo);
    }

    void Update()
    {
        // Обновляем таймер перезарядки
        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0)
            {
                CompleteReload();
            }
        }
    }

    /// <summary>
    /// Попытаться выстрелить (потратить 1 патрон)
    /// </summary>
    public bool TryUseAmmo()
    {
        if (isReloading)
        {
            Debug.Log("Идёт перезарядка!");
            return false;
        }

        if (currentAmmo > 0)
        {
            currentAmmo--;
            OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
            Debug.Log($"Выстрел! Осталось патронов: {currentAmmo}");
            return true;
        }
        else
        {
            Debug.Log("Патронов нет! Нужна перезарядка!");
            return false;
        }
    }

    /// <summary>
    /// Начать перезарядку
    /// INPUT SYSTEM: Нужно привязать эту функцию к отдельной кнопке (например, R)
    /// </summary>
    public void StartReload()
    {
        // Если уже полный магазин - не перезаряжаем
        if (currentAmmo == maxAmmo)
        {
            Debug.Log("Магазин уже полный!");
            return;
        }

        // Если нет резервных патронов - не перезаряжаем
        if (currentReserveAmmo <= 0)
        {
            Debug.Log("Нет резервных патронов!");
            return;
        }

        // Если уже идёт перезарядка - игнорируем
        if (isReloading)
        {
            Debug.Log("Уже идёт перезарядка!");
            return;
        }

        isReloading = true;
        reloadTimer = reloadTime;
        OnReloadStart?.Invoke();
        Debug.Log($"Начало перезарядки... ({reloadTime}сек)");
    }

    /// <summary>
    /// Завершить перезарядку
    /// </summary>
    private void CompleteReload()
    {
        isReloading = false;

        // Берём патроны из резерва
        int ammoNeeded = maxAmmo - currentAmmo;
        int ammoTaken = Mathf.Min(ammoNeeded, currentReserveAmmo);

        currentAmmo += ammoTaken;
        currentReserveAmmo -= ammoTaken;

        OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
        OnReserveAmmoChanged?.Invoke(currentReserveAmmo);
        OnReloadComplete?.Invoke();

        Debug.Log($"Перезарядка завершена! Патроны: {currentAmmo}/{maxAmmo}, Резерв: {currentReserveAmmo}");
    }

    /// <summary>
    /// Проверить, идёт ли перезарядка
    /// </summary>
    public bool IsReloading()
    {
        return isReloading;
    }

    /// <summary>
    /// Получить текущее количество патронов
    /// </summary>
    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }

    /// <summary>
    /// Получить резервные патроны
    /// </summary>
    public int GetReserveAmmo()
    {
        return currentReserveAmmo;
    }

    /// <summary>
    /// Добавить резервные патроны (когда найдёшь pickup)
    /// </summary>
    public void AddReserveAmmo(int amount)
    {
        currentReserveAmmo += amount;
        currentReserveAmmo = Mathf.Min(currentReserveAmmo, maxReserveAmmo);
        OnReserveAmmoChanged?.Invoke(currentReserveAmmo);
        Debug.Log($"Добавлено {amount} резервных патронов. Всего: {currentReserveAmmo}");
    }
}
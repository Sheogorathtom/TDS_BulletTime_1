using UnityEngine;

/// <summary>
/// Оружие дрона - стреляет в игрока когда парит рядом
/// </summary>
public class DroneWeapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private float damage = 5f;      // урон за выстрел (меньше чем у игрока)
    [SerializeField] private float fireRate = 1.5f;  // выстрелы каждые 1.5 сек
    [SerializeField] private float range = 20f;      // дальность стрельбы

    [Header("Shoot Points")]
    [SerializeField] private Transform[] shootPoints; // 4 точки выстрела на дроне

    [Header("Effects")]
    [SerializeField] private GameObject muzzleFlashPrefab; // эффект выстрела
    [SerializeField] private GameObject hitEffectPrefab;   // эффект попадания

    private float nextFireTime = 0f;
    private DroneAI droneAI;

    void Awake()
    {
        droneAI = GetComponent<DroneAI>();
        if (droneAI == null)
            Debug.LogWarning("DroneAI не найден на этом объекте!");

        if (shootPoints == null || shootPoints.Length == 0)
            Debug.LogWarning("ShootPoints не назначены!");
        else if (shootPoints.Length < 4)
            Debug.LogWarning("Рекомендуется 4 shootPoints, сейчас: " + shootPoints.Length);
    }

    void Update()
    {
        if (droneAI != null && droneAI.IsPlayerInAttackRange())
        {
            TryShoot();
        }
    }

    private void TryShoot()
    {
        if (Time.time >= nextFireTime)
        {
            ShootFromAllPoints();
            nextFireTime = Time.time + fireRate;
        }
    }

    /// <summary>
    /// Выстрел из всех доступных точек (до 4)
    /// </summary>
    private void ShootFromAllPoints()
    {
        if (shootPoints == null || shootPoints.Length == 0)
        {
            Debug.LogWarning("Нет точек выстрела (shootPoints)!");
            return;
        }

        // Берём основное направление на игрока один раз
        Vector3 baseAimDirection = droneAI.GetAimDirection();

        // Проходимся по всем точкам выстрела
        foreach (Transform sp in shootPoints)
        {
            if (sp == null)
                continue;

            ShootSingle(sp, baseAimDirection);
        }
    }

    /// <summary>
    /// Один выстрел из одной точки
    /// </summary>
    private void ShootSingle(Transform shootPoint, Vector3 baseAimDirection)
    {
        Vector3 origin = shootPoint.position;

        // Немного случайного разброса для каждого ствола
        Vector3 spreadDirection = baseAimDirection + Random.insideUnitSphere * 0.1f;
        spreadDirection.Normalize();

        RaycastHit hit;

        if (Physics.Raycast(origin, spreadDirection, out hit, 100f))
        {
            // Эффект выстрела
            if (muzzleFlashPrefab != null)
                Instantiate(muzzleFlashPrefab, shootPoint.position, Quaternion.identity);

            // Эффект попадания
            if (hitEffectPrefab != null)
                Instantiate(hitEffectPrefab, hit.point, Quaternion.identity);

            // Ищем Health компонент для урона
            Health targetHealth = hit.collider.GetComponent<Health>();
            if (targetHealth == null)
                targetHealth = hit.collider.GetComponentInParent<Health>();

            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
                Debug.Log($"Дрон выстрелил! Урон: {damage} | Цель: {hit.collider.name}");
            }
            else
            {
                Debug.Log("Дрон: Попадание в стену");
            }

            Debug.DrawLine(origin, hit.point, Color.red, 10f);
        }
        else
        {
            Debug.DrawRay(origin, spreadDirection * range, Color.yellow, 10f);
            Debug.Log("Дрон: Промах");
        }
    }

    /// <summary>
    /// Дрон получил урон (небольшое замедление на время)
    /// </summary>
    public void OnDroneHit()
    {
        droneAI?.TakeDamage();
    }
}

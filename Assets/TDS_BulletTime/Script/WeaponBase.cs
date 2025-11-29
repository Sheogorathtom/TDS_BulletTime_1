using UnityEngine;

/// <summary>
/// Обновленный скрипт оружия с интеграцией системы патронов
/// Проверяет патроны перед выстрелом
/// </summary>
public class WeaponBase : MonoBehaviour
{
    [Header("Weapon Settings")]
    public float damage = 10f;
    public float fireRate = 0.5f;
    public float range = 100f;

    [Header("Shoot Point")]
    public Transform shootPoint; // Позиция выстрела (на оружии)
    public Transform stableForward; // Объект с правильным forward

    [Header("Effects")]
    public GameObject Fire;
    public GameObject HitPoint;

    private float _nextFireTime;

    // Ссылка на систему патронов
    private AmmoSystem ammoSystem;

    void Awake()
    {
        // Ищем AmmoSystem на этом же игроке
        ammoSystem = GetComponentInParent<AmmoSystem>();
        if (ammoSystem == null)
        {
            Debug.LogWarning("AmmoSystem не найдена!");
        }
    }

    public void TryShoot()
    {
        // ПРОВЕРКА: Есть ли патроны? (новое условие)
        if (ammoSystem != null && !ammoSystem.TryUseAmmo())
        {
            Debug.Log("Не могу выстрелить - нет патронов или идёт перезарядка!");
            return;
        }

        if (Time.time >= _nextFireTime)
        {
            Shoot();
            _nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        if (shootPoint == null)
        {
            Debug.LogWarning("ShootPoint not assigned!");
            return;
        }

        if (stableForward == null)
        {
            Debug.LogWarning("StableForward not assigned!");
            return;
        }

        // Позиция от shootPoint, направление от stableForward
        Vector3 origin = shootPoint.position;
        Vector3 direction = stableForward.forward;
        RaycastHit hit;

        if (Physics.Raycast(origin, direction, out hit, 100f))
        {
            Debug.DrawLine(origin, hit.point, Color.red, 10f);
            
            if (Fire != null)
                Instantiate(Fire, shootPoint.position, Quaternion.identity);
            
            if (HitPoint != null)
                Instantiate(HitPoint, hit.point, Quaternion.identity);

            // НОВОЕ: Проверяем попал ли в объект с Health компонентом
            Health targetHealth = hit.collider.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
                Debug.Log($"Попадание! Урон: {damage} | Цель: {hit.collider.name}");
            }
            else
            {
                Debug.Log("Попадание в стену или объект без Health!");
            }

            Debug.Log("Hit at: " + hit.point + " | Object: " + hit.collider.name);
        }
        else
        {
            Debug.DrawRay(origin, direction * range, Color.yellow);
            Debug.Log("Missed");
        }
    }
}
using UnityEngine;

public class BossAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float warningTime = 1.5f;      // Сколько секунд показывается красная зона
    public float attackRange = 10f;       // Длина зоны
    public float attackAngle = 60f;       // Максимальный угол отклонения влево/вправо

    [Header("References")]
    public GameObject attackZonePrefab;   // Префаб треугольной зоны удара
    public Transform zoneSpawnPoint;      // Точка где ВСЕГДА появляется зона удара

    private GameObject currentZone;
    private Quaternion chosenRotation;

    private void Start()
    {
        StartCoroutine(AttackRoutine());
    }

    private System.Collections.IEnumerator AttackRoutine()
    {
        while (true)
        {
            PickRandomAttackDirection();
            ShowWarningZone();

            yield return new WaitForSeconds(warningTime);

            PerformAttack();
            HideWarningZone();

            yield return new WaitForSeconds(2f);
        }
    }

    // Выбираем случайный угол внутри сектора
    private void PickRandomAttackDirection()
    {
        float halfAngle = attackAngle * 0.5f;
        float randomAngle = Random.Range(-halfAngle, halfAngle);

        chosenRotation = Quaternion.Euler(0, randomAngle, 0);
    }

    private void ShowWarningZone()
    {
        if (attackZonePrefab == null || zoneSpawnPoint == null)
        {
            Debug.LogError("Missing prefab or spawn point!");
            return;
        }

        // Создаём зону в фиксированной позиции
        currentZone = Instantiate(attackZonePrefab);
        currentZone.transform.position = zoneSpawnPoint.position;

        // Поворот меняется, позиция НЕТ
        currentZone.transform.rotation = transform.rotation * chosenRotation;

        // Масштаб только по длине
        currentZone.transform.localScale = new Vector3(attackRange, 1, attackRange);
    }

    private void HideWarningZone()
    {
        if (currentZone != null)
            Destroy(currentZone);
    }

    private void PerformAttack()
    {
        Debug.Log("Boss Attack!");
    }
}

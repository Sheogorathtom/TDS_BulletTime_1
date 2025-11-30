using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Система управления дроном
/// Дрон подлетает к игроку и парит рядом с ним на фиксированной высоте.
/// Всегда смотрит на игрока и НЕ разворачивается по траектории движения.
/// </summary>
public class DroneAI : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float fixedHeight = 3f;          // ФИКСИРОВАННАЯ высота дрона в мировых координатах (Y)
    [SerializeField] private float hoverDistance = 5f;        // расстояние, на котором парить вокруг игрока
    [SerializeField] private float moveSpeed = 3.5f;          // скорость движения дрона
    [SerializeField] private float stoppingDistance = 0.5f;   // расстояние остановки

    [Header("Hovering Settings")]
    [SerializeField] private float hoverSmoothing = 0.1f;     // гладкость парения

    [Header("Attack Settings")]
    [SerializeField] private float attackRangeDistance = 15f; // расстояние атаки
    [SerializeField] private bool isAttacking = false;

    private Transform playerTransform;
    private Rigidbody rb;

    // Состояния дрона
    private enum DroneState { Approaching, Hovering, Dead }
    private DroneState currentState = DroneState.Approaching;

    void Start()
    {
        // Ищем игрока
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody>();

        if (rb == null)
            Debug.LogWarning($"{gameObject.name}: Rigidbody не найден!");
        if (playerTransform == null)
            Debug.LogWarning($"{gameObject.name}: Игрок не найден!");
    }

    void Update()
    {
        if (playerTransform == null)
            return;

        // Переключение между состояниями
        switch (currentState)
        {
            case DroneState.Approaching:
                UpdateApproaching();
                break;
            case DroneState.Hovering:
                UpdateHovering();
                break;
            case DroneState.Dead:
                break;
        }

        // Всегда смотрим на игрока
        LookAtPlayer();

        // Жёстко фиксируем высоту дрона каждый кадр
        Vector3 pos = transform.position;
        pos.y = fixedHeight;
        transform.position = pos;
    }

    /// <summary>
    /// Фаза подлёта к игроку
    /// </summary>
    private void UpdateApproaching()
    {
        // Целевая позиция: рядом с игроком на фиксированной высоте
        Vector3 targetPos = playerTransform.position;
        targetPos.y = fixedHeight;

        // Орбита вокруг игрока в плоскости XZ
        float angle = Mathf.Atan2(transform.position.z - playerTransform.position.z,
                                  transform.position.x - playerTransform.position.x);
        Vector3 orbitOffset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * hoverDistance;
        targetPos += orbitOffset;

        // Вычисляем расстояние до целевой позиции
        float distanceToTarget = Vector3.Distance(new Vector3(transform.position.x, 0f, transform.position.z),
                                                  new Vector3(targetPos.x, 0f, targetPos.z));

        // Если достаточно близко к точке парения → переходим в режим парения
        if (distanceToTarget < stoppingDistance)
        {
            currentState = DroneState.Hovering;
            return;
        }

        // Движение к целевой позиции (игнорируем разницу по Y, высота фиксируется отдельно)
        Vector3 moveDir = (targetPos - transform.position);
        moveDir.y = 0f;
        moveDir = moveDir.normalized;

        rb.linearVelocity = new Vector3(
            moveDir.x * moveSpeed,
            0f,                    // по Y не двигаем физикой, высота фиксируется вручную
            moveDir.z * moveSpeed
        );

        // Debug.Log($"{gameObject.name}: Подлетаю к игроку... Расстояние: {distanceToTarget:F2}");
    }

    /// <summary>
    /// Фаза парения вокруг игрока
    /// </summary>
    private void UpdateHovering()
    {
        if (playerTransform == null)
            return;

        // Орбитальное движение вокруг игрока в одной высоте
        float angle = Time.time * 0.5f; // медленная орбита вокруг игрока
        Vector3 orbitPos = new Vector3(
            Mathf.Cos(angle) * hoverDistance,
            0f,
            Mathf.Sin(angle) * hoverDistance
        );

        Vector3 targetPos = playerTransform.position + orbitPos;
        targetPos.y = fixedHeight; // фиксированная высота

        // Плавное движение к целевой позиции (в плоскости XZ)
        Vector3 moveDir = (targetPos - transform.position);
        moveDir.y = 0f;
        float distanceToTarget = moveDir.magnitude;

        // Если дрон слишком далеко отошёл → возвращаемся в режим подлёта
        if (distanceToTarget > hoverDistance + stoppingDistance + 2f)
        {
            currentState = DroneState.Approaching;
            return;
        }

        moveDir = moveDir.normalized;

        Vector3 targetVelocity = new Vector3(
            moveDir.x * moveSpeed * 0.5f,
            0f,
            moveDir.z * moveSpeed * 0.5f
        );

        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, hoverSmoothing);
    }

    /// <summary>
    /// Всегда смотреть на игрока
    /// </summary>
    private void LookAtPlayer()
    {
        if (playerTransform == null)
            return;

        Vector3 lookDirection = (playerTransform.position - transform.position);
        lookDirection.y = 0f; // не наклоняемся по вертикали

        if (lookDirection.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
            transform.rotation = targetRotation;
        }
    }

    public Vector3 GetAimDirection()
    {
        if (playerTransform == null)
            return transform.forward;

        return (playerTransform.position - transform.position).normalized;
    }

    public bool IsPlayerInAttackRange()
    {
        if (playerTransform == null)
            return false;

        float distanceToPlayer = Vector3.Distance(
            new Vector3(transform.position.x, 0f, transform.position.z),
            new Vector3(playerTransform.position.x, 0f, playerTransform.position.z)
        );

        return distanceToPlayer <= attackRangeDistance && currentState == DroneState.Hovering;
    }

    public void TakeDamage()
    {
        // Небольшой толчок при попадании (только по XZ)
        Vector3 impulse = Random.insideUnitSphere * 2f;
        impulse.y = 0f;
        rb.linearVelocity += impulse;
    }

    public void Die()
    {
        currentState = DroneState.Dead;
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, fixedHeight, transform.position.z), hoverDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, fixedHeight, transform.position.z), attackRangeDistance);

        Gizmos.color = Color.blue;
        Vector3 from = new Vector3(transform.position.x, fixedHeight, transform.position.z);
        Gizmos.DrawLine(from, from + Vector3.up * 0.5f);
    }
}

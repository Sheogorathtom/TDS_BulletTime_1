using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// DroneAI с NavMesh Pathfinding
/// Дрон летает в 3D пространстве и ищет оптимальный путь вокруг препятствий
/// </summary>
public class DroneAI_NavMesh : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float hoverHeight = 2f; // высота парения над игроком
    [SerializeField] private float hoverDistance = 5f; // расстояние на котором парить
    [SerializeField] private float moveSpeed = 3.5f; // скорость движения
    [SerializeField] private float stoppingDistance = 0.5f; // расстояние остановки

    [Header("Hovering Settings")]
    [SerializeField] private float hoverSmoothing = 0.1f;
    [SerializeField] private float hoverBobAmount = 0.3f;
    [SerializeField] private float hoverBobSpeed = 2f;

    [Header("Attack Settings")]
    [SerializeField] private float attackRangeDistance = 15f;

    private Transform playerTransform;
    private NavMeshAgent navMeshAgent;
    private Rigidbody rb;
    private float hoverTimer = 0f;

    private enum DroneState { Approaching, Hovering, Dead }
    private DroneState currentState = DroneState.Approaching;

    void Start()
    {
        // Ищем игрока
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // Получаем NavMeshAgent
        navMeshAgent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        if (navMeshAgent == null)
            Debug.LogError($"{gameObject.name}: NavMeshAgent НЕ НАЙДЕН! Добавь его в Inspector!");
        if (playerTransform == null)
            Debug.LogWarning($"{gameObject.name}: Игрок не найден!");

        // Настраиваем NavMeshAgent
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = moveSpeed;
            navMeshAgent.stoppingDistance = stoppingDistance;
            navMeshAgent.updateRotation = false; // Сам управляем поворотом
            navMeshAgent.updatePosition = true;
        }
    }

    void Update()
    {
        if (playerTransform == null)
            return;

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
    }

    /// <summary>
    /// Фаза подлёта к игроку
    /// </summary>
    private void UpdateApproaching()
    {
        if (navMeshAgent == null || !navMeshAgent.enabled)
            return;

        // Целевая позиция: рядом с игроком на определённой высоте
        Vector3 targetPos = playerTransform.position + Vector3.up * hoverHeight;

        // Добавляем орбиту вокруг игрока
        float angle = Mathf.Atan2(transform.position.z - playerTransform.position.z,
                                  transform.position.x - playerTransform.position.x);
        targetPos += new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * hoverDistance;

        // Устанавливаем цель для NavMeshAgent
        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            navMeshAgent.SetDestination(hit.position);
        }

        float distanceToTarget = Vector3.Distance(transform.position, targetPos);

        // Если достаточно близко → переходим в режим парения
        if (distanceToTarget < stoppingDistance + 1f)
        {
            currentState = DroneState.Hovering;
            hoverTimer = 0f;
            return;
        }

        // Смотрим в направлении движения
        if (navMeshAgent.velocity != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(navMeshAgent.velocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.1f);
        }

        Debug.Log($"{gameObject.name}: Подлетаю к игроку (NavMesh)... Расстояние: {distanceToTarget:F2}");
    }

    /// <summary>
    /// Фаза парения вокруг игрока
    /// </summary>
    private void UpdateHovering()
    {
        if (playerTransform == null)
            return;

        hoverTimer += Time.deltaTime;

        // Орбитальное движение вокруг игрока
        float angle = Time.time * 0.5f;
        Vector3 orbitPos = new Vector3(
            Mathf.Cos(angle) * hoverDistance,
            0,
            Mathf.Sin(angle) * hoverDistance
        );

        // Добавляем качание (вверх-вниз)
        float bobOffset = Mathf.Sin(hoverTimer * hoverBobSpeed) * hoverBobAmount;
        Vector3 targetPos = playerTransform.position + orbitPos + Vector3.up * (hoverHeight + bobOffset);

        // Проверяем позицию на NavMesh
        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                navMeshAgent.SetDestination(hit.position);
            }

            float distanceToTarget = Vector3.Distance(transform.position, targetPos);

            // Если слишком далеко отошёл → возвращаемся в режим подлёта
            if (distanceToTarget > hoverDistance + stoppingDistance + 3f)
            {
                currentState = DroneState.Approaching;
                return;
            }

            // Плавное движение
            if (navMeshAgent.velocity.magnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(navMeshAgent.velocity);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.1f);
            }
        }

        // Смотрим на игрока
        Vector3 lookDirection = (playerTransform.position - transform.position).normalized;
        if (lookDirection != Vector3.zero)
        {
            Quaternion aimRotation = Quaternion.LookRotation(lookDirection);
            // Медленнее смотрим на игрока, чтобы не спешить
            transform.rotation = Quaternion.Slerp(transform.rotation, aimRotation, 0.05f);
        }
    }

    /// <summary>
    /// Получить направление на игрока для стрельбы
    /// </summary>
    public Vector3 GetAimDirection()
    {
        if (playerTransform == null)
            return transform.forward;

        return (playerTransform.position - transform.position).normalized;
    }

    /// <summary>
    /// Проверить находится ли игрок в диапазоне атаки
    /// </summary>
    public bool IsPlayerInAttackRange()
    {
        if (playerTransform == null)
            return false;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        return distanceToPlayer <= attackRangeDistance && currentState == DroneState.Hovering;
    }

    /// <summary>
    /// Дрон получил урон
    /// </summary>
    public void TakeDamage()
    {
        if (rb != null)
            rb.linearVelocity += Random.insideUnitSphere * 2f;
    }

    /// <summary>
    /// Дрон умер
    /// </summary>
    public void Die()
    {
        currentState = DroneState.Dead;
        
        if (navMeshAgent != null)
            navMeshAgent.enabled = false;
        
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        Debug.Log($"{gameObject.name}: Дрон уничтожен!");
    }

    /// <summary>
    /// Визуализация для отладки
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // Радиус парения
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hoverDistance);

        // Радиус атаки
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRangeDistance);

        // Высота парения
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * hoverHeight);
    }
}
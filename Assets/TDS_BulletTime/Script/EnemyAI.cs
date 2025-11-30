using UnityEngine;

/// <summary>
/// ОБНОВЛЕНО: DroneAI с система избегания препятствий
/// Дрон обходит объекты используя raycast'ы вперед
/// </summary>
public class DroneAI_Avoiding : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float hoverHeight = 2f;
    [SerializeField] private float hoverDistance = 5f;
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float stoppingDistance = 0.5f;

    [Header("Hovering Settings")]
    [SerializeField] private float hoverSmoothing = 0.1f;
    [SerializeField] private float hoverBobAmount = 0.3f;
    [SerializeField] private float hoverBobSpeed = 2f;

    [Header("Obstacle Avoidance")]
    [SerializeField] private float avoidanceDistance = 2f; // на каком расстоянии избегать препятствий
    [SerializeField] private float avoidanceForce = 2f; // сила избегания
    [SerializeField] private int raycastCount = 3; // количество лучей для проверки

    [Header("Attack Settings")]
    [SerializeField] private float attackRangeDistance = 15f;

    private Transform playerTransform;
    private Rigidbody rb;
    private float hoverTimer = 0f;

    private enum DroneState { Approaching, Hovering, Dead }
    private DroneState currentState = DroneState.Approaching;

    void Start()
    {
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

    private void UpdateApproaching()
    {
        Vector3 targetPos = playerTransform.position + Vector3.up * hoverHeight;

        float angle = Mathf.Atan2(transform.position.z - playerTransform.position.z,
                                  transform.position.x - playerTransform.position.x);
        targetPos += new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * hoverDistance;

        float distanceToTarget = Vector3.Distance(transform.position, targetPos);

        if (distanceToTarget < stoppingDistance)
        {
            currentState = DroneState.Hovering;
            hoverTimer = 0f;
            return;
        }

        Vector3 moveDirection = (targetPos - transform.position).normalized;

        // НОВОЕ: Проверяем препятствия впереди
        Vector3 avoidanceDirection = GetAvoidanceDirection(moveDirection);
        moveDirection = Vector3.Lerp(moveDirection, avoidanceDirection, 0.5f).normalized;

        rb.linearVelocity = moveDirection * moveSpeed;

        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.1f);
        }

        Debug.Log($"{gameObject.name}: Подлетаю к игроку... Расстояние: {distanceToTarget:F2}");
    }

    private void UpdateHovering()
    {
        if (playerTransform == null)
            return;

        hoverTimer += Time.deltaTime;

        float angle = Time.time * 0.5f;
        Vector3 orbitPos = new Vector3(
            Mathf.Cos(angle) * hoverDistance,
            0,
            Mathf.Sin(angle) * hoverDistance
        );

        float bobOffset = Mathf.Sin(hoverTimer * hoverBobSpeed) * hoverBobAmount;

        Vector3 targetPos = playerTransform.position + orbitPos + Vector3.up * (hoverHeight + bobOffset);

        // НОВОЕ: Проверяем препятствия при парении
        Vector3 moveDirection = (targetPos - transform.position).normalized;
        Vector3 avoidanceDirection = GetAvoidanceDirection(moveDirection);
        moveDirection = Vector3.Lerp(moveDirection, avoidanceDirection, 0.3f).normalized;

        float distanceToTarget = Vector3.Distance(transform.position, targetPos);

        if (distanceToTarget > hoverDistance + stoppingDistance + 2f)
        {
            currentState = DroneState.Approaching;
            return;
        }

        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, moveDirection * moveSpeed * 0.5f, hoverSmoothing);

        Vector3 lookDirection = (playerTransform.position - transform.position).normalized;
        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.1f);
        }
    }

    /// <summary>
    /// НОВОЕ: Получить направление для избегания препятствий
    /// </summary>
    private Vector3 GetAvoidanceDirection(Vector3 currentDirection)
    {
        Vector3 avoidanceDir = Vector3.zero;
        bool obstacleDetected = false;

        // Проверяем лучи в разных направлениях
        for (int i = 0; i < raycastCount; i++)
        {
            float angle = (i - raycastCount / 2) * 30f; // распределяем лучи в стороны
            Vector3 rayDirection = Quaternion.Euler(0, angle, 0) * currentDirection;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, rayDirection, out hit, avoidanceDistance))
            {
                // Если это не игрок и не сам дрон
                if (hit.transform.gameObject != gameObject && 
                    !hit.transform.CompareTag("Player"))
                {
                    obstacleDetected = true;

                    // Отталкиваемся от препятствия
                    Vector3 awayFromObstacle = (transform.position - hit.point).normalized;
                    avoidanceDir += awayFromObstacle * (avoidanceDistance - hit.distance);

                    Debug.DrawRay(transform.position, rayDirection * hit.distance, Color.red, 0.1f);
                }
                else
                {
                    Debug.DrawRay(transform.position, rayDirection * hit.distance, Color.green, 0.1f);
                }
            }
        }

        // Если препятствие не найдено - используем исходное направление
        if (!obstacleDetected)
            return currentDirection;

        // Смешиваем исходное направление с направлением избегания
        Vector3 finalDirection = (currentDirection + avoidanceDir.normalized * avoidanceForce).normalized;
        return finalDirection;
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

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        return distanceToPlayer <= attackRangeDistance && currentState == DroneState.Hovering;
    }

    public void TakeDamage()
    {
        rb.linearVelocity += Random.insideUnitSphere * 2f;
    }

    public void Die()
    {
        currentState = DroneState.Dead;
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
    }

    void OnDrawGizmosSelected()
    {
        // Радиус парения
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hoverDistance);

        // Радиус атаки
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRangeDistance);

        // Радиус избегания
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, avoidanceDistance);

        // Высота парения
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * hoverHeight);
    }
}
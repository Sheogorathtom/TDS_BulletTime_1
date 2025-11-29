using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Система ближнего боя врага
/// Враг наносит урон игроку когда находится рядом
/// </summary>
public class EnemyMelee : MonoBehaviour
{
    [Header("Melee Settings")]
    [SerializeField] private float attackRange = 1.5f; // расстояние атаки
    [SerializeField] private float attackDamage = 15f; // урон за удар
    [SerializeField] private float attackCooldown = 1.5f; // время между атаками

    private float nextAttackTime = 0f;
    private Transform playerTransform;
    private Health playerHealth;
    private NavMeshAgent navMeshAgent;

    void Start()
    {
        // Ищем игрока
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform != null)
            playerHealth = playerTransform.GetComponent<Health>();

        navMeshAgent = GetComponent<NavMeshAgent>();

        if (playerTransform == null)
            Debug.LogWarning($"{gameObject.name}: Игрок не найден!");
        if (playerHealth == null && playerTransform != null)
            Debug.LogWarning($"{gameObject.name}: Health компонент игрока не найден!");
    }

    void Update()
    {
        if (playerTransform == null || playerHealth == null)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Проверяем, находится ли игрок в пределах атаки
        if (distanceToPlayer <= attackRange)
        {
            // Пытаемся атаковать
            TryAttack();
        }
    }

    /// <summary>
    /// Попытаться атаковать игрока
    /// </summary>
    private void TryAttack()
    {
        if (Time.time >= nextAttackTime)
        {
            AttackPlayer();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    /// <summary>
    /// Атаковать игрока
    /// </summary>
    private void AttackPlayer()
    {
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
            Debug.Log($"{gameObject.name} атаковал игрока! Урон: {attackDamage}");

            // TODO: Добавить анимацию атаки
            // animator.SetTrigger("Attack");
        }
    }

    /// <summary>
    /// Визуализация радиуса атаки в редакторе (для отладки)
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
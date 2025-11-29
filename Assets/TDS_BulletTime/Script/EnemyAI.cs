using UnityEngine;
using UnityEngine.AI;

public class EnemyAI_Move : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform target;          // цель (обычно игрок)
    public float updateRate = 0.2f;   // как часто обновлять маршрут

    private NavMeshAgent agent;
    private float nextUpdate;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (target == null)
            return;

        if (Time.time >= nextUpdate)
        {
            agent.SetDestination(target.position);
            nextUpdate = Time.time + updateRate;
        }
    }
}

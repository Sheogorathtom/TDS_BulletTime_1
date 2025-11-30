using UnityEngine;

public class BossWarningLine : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float lifeTime = 0.6f;

    [Header("Damage Prefab")]
    [SerializeField] private GameObject damagePrefab;

    void Start()
    {
        Invoke(nameof(SpawnDamageAndDestroy), lifeTime);
    }

    private void SpawnDamageAndDestroy()
    {
        if (damagePrefab != null)
        {
            Instantiate(damagePrefab, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }
}

using UnityEngine;

public class BossDamageHit : MonoBehaviour
{
    [SerializeField] private float _damage = 10f;
    public float Damage
    {
        get => _damage;
        set => _damage = value;
    }

    [SerializeField] private float existTime = 0.5f;

    private bool hasDealtDamage = false;

    void Start()
    {
        Destroy(gameObject, existTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasDealtDamage) return;

        Health h = other.GetComponent<Health>();
        if (h == null)
            h = other.GetComponentInParent<Health>();

        if (h != null && !h.IsDead())
        {
            h.TakeDamage(_damage);
            hasDealtDamage = true;
            Debug.Log($"Урон нанесён {other.name}");
        }
    }
}

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private Rigidbody rb;
    private PlayerAnimationController anim;

    // Случайное направление
    private Vector3 moveDir;

    public void Die() 
    { 
        Destroy(gameObject); 
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<PlayerAnimationController>();

        GenerateNewDirection();
    }

    void FixedUpdate()
    {
        Move();
        SendAnimationData();
    }

    // ------------------------------
    // ДВИЖЕНИЕ
    // ------------------------------
    void Move()
    {
        // Двигаем Rigidbody
        Vector3 velocity = moveDir * speed;
        rb.linearVelocity = velocity;

        // Полностью запрещаем поворот
        rb.rotation = Quaternion.identity;
    }

    // ------------------------------
    // ОБНОВЛЕНИЕ ДАННЫХ В АНИМАЦИИ
    // ------------------------------
    void SendAnimationData()
    {
        float currentSpeed = rb.linearVelocity.magnitude;
        anim.UpdateSpeed(currentSpeed);
        anim.UpdateDirection(rb.linearVelocity);
    }

    // ------------------------------
    // Меняем направление каждые 2 секунды
    // ------------------------------
    void GenerateNewDirection()
    {
        // Случайно выбираем строго 4 направления
        int r = Random.Range(0, 4);

        switch (r)
        {
            case 0: moveDir = Vector3.forward; break;
            case 1: moveDir = Vector3.back; break;
            case 2: moveDir = Vector3.right; break;
            case 3: moveDir = Vector3.left; break;
        }

        // Гарантируем что движение ровно по осям
        moveDir.Normalize();

        // Вызываем снова
        Invoke(nameof(GenerateNewDirection), 2f);
    }
}

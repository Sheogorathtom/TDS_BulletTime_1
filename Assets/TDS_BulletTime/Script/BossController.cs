using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Rotation")]
    public float rotationSpeed = 120f;

    [Header("Attack Settings")]
    [SerializeField] private GameObject damagePrefab;  // префаб урона (сфера/капсула с триггером)
    [SerializeField] private float bossAttackDamage = 10f;
    [SerializeField] private GameObject warningPrefab;

    public float warningDuration = 0.6f;

    [Header("Full Circle Attack")]
    public float fullCircleWarningDuration = 0.3f;
    public float fullCircleTimeBetweenHits = 0.1f;
    public float fullCircleAngleStep = 30f;

    [Header("Auto Attack")]
    [SerializeField] private float timeBetweenAttacks = 3f;
    [SerializeField] private int[] attackSequence = { 2, 3, 4, 5 };

    [Header("Start Delay")]
    [SerializeField] private float startAttackDelay = 3f;   // сколько секунд в начале босс не атакует

    [Header("Special Attack Timings")]
    public float delayBetweenPatterns = 1.0f;

    private bool isAttacking;
    private bool canRotateToPlayer = true;
    private int currentAttackIndex = 0;
    private float attackTimer = 0f;

    void Start()
    {
        // первая атака начнётся только через startAttackDelay секунд
        attackTimer = startAttackDelay;
    }

    void Update()
    {
        if (player != null && canRotateToPlayer)
        {
            RotateTowardsPlayer();
        }

        if (!isAttacking)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                StartCoroutine(ExecuteNextAttack());
                attackTimer = timeBetweenAttacks;
            }
        }

        if (!isAttacking)
        {
            if (Input.GetKeyDown(KeyCode.Alpha2))
                StartCoroutine(Attack_CrossThenDiagonal());
            if (Input.GetKeyDown(KeyCode.Alpha3))
                StartCoroutine(Attack_FullCircle());
            if (Input.GetKeyDown(KeyCode.Alpha4))
                StartCoroutine(Attack_Spiral());
            if (Input.GetKeyDown(KeyCode.Alpha5))
                StartCoroutine(Attack_DoubleWave());
        }
    }


    IEnumerator ExecuteNextAttack()
    {
        int attackType = attackSequence[currentAttackIndex % attackSequence.Length];
        
        if (attackType == 2)
            yield return StartCoroutine(Attack_CrossThenDiagonal());
        else if (attackType == 3)
            yield return StartCoroutine(Attack_FullCircle());
        else if (attackType == 4)
            yield return StartCoroutine(Attack_Spiral());
        else if (attackType == 5)
            yield return StartCoroutine(Attack_DoubleWave());

        currentAttackIndex++;
    }

    void RotateTowardsPlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            rotationSpeed * Time.deltaTime
        );
    }

    IEnumerator Attack_CrossThenDiagonal()
    {
        isAttacking = true;
        canRotateToPlayer = false;

        float[] crossAngles = { 0f, 90f, 180f, 270f };
        foreach (float angle in crossAngles)
        {
            SpawnWarning(angle);
        }
        yield return new WaitForSeconds(warningDuration);

        foreach (float angle in crossAngles)
        {
            SpawnDamage(angle);
        }

        yield return new WaitForSeconds(delayBetweenPatterns);

        float[] diagAngles = { 45f, 135f, 225f, 315f };
        foreach (float angle in diagAngles)
        {
            SpawnWarning(angle);
        }
        yield return new WaitForSeconds(warningDuration);

        foreach (float angle in diagAngles)
        {
            SpawnDamage(angle);
        }

        yield return new WaitForSeconds(delayBetweenPatterns);

        isAttacking = false;
        canRotateToPlayer = true;
    }

    IEnumerator Attack_FullCircle()
    {
        isAttacking = true;
        canRotateToPlayer = false;

        float angleStep = fullCircleAngleStep;
        float currentAngle = 0f;

        while (currentAngle < 360f)
        {
            SpawnWarning(currentAngle);
            yield return new WaitForSeconds(fullCircleWarningDuration);

            SpawnDamage(currentAngle);
            yield return new WaitForSeconds(fullCircleTimeBetweenHits);

            currentAngle += angleStep;
        }

        yield return new WaitForSeconds(delayBetweenPatterns);

        isAttacking = false;
        canRotateToPlayer = true;
    }

    IEnumerator Attack_Spiral()
    {
        isAttacking = true;
        canRotateToPlayer = false;

        float angleStep = 15f;
        float currentAngle = 0f;

        while (currentAngle < 360f)
        {
            SpawnWarning(currentAngle);
            yield return new WaitForSeconds(0.15f);

            SpawnDamage(currentAngle);
            yield return new WaitForSeconds(0.15f);

            currentAngle += angleStep;
        }

        yield return new WaitForSeconds(delayBetweenPatterns);

        isAttacking = false;
        canRotateToPlayer = true;
    }

    IEnumerator Attack_DoubleWave()
    {
        isAttacking = true;
        canRotateToPlayer = false;

        float[] leftWave = { -90f, -70f, -50f, -30f, -10f };
        foreach (float angle in leftWave)
        {
            SpawnWarning(angle);
        }
        yield return new WaitForSeconds(warningDuration);

        foreach (float angle in leftWave)
        {
            SpawnDamage(angle);
        }

        yield return new WaitForSeconds(0.5f);

        float[] rightWave = { 90f, 70f, 50f, 30f, 10f };
        foreach (float angle in rightWave)
        {
            SpawnWarning(angle);
        }
        yield return new WaitForSeconds(warningDuration);

        foreach (float angle in rightWave)
        {
            SpawnDamage(angle);
        }

        yield return new WaitForSeconds(delayBetweenPatterns);

        isAttacking = false;
        canRotateToPlayer = true;
    }

    void SpawnWarning(float angleDeg)
    {
        if (warningPrefab == null) return;

        Quaternion rot = Quaternion.AngleAxis(angleDeg, Vector3.up) * transform.rotation;
        Vector3 start = transform.position + Vector3.up * 0.2f;

        Instantiate(warningPrefab, start, rot);
    }

void SpawnDamage(float angleDeg)
{
    if (damagePrefab == null) return;

    Quaternion rot = Quaternion.AngleAxis(angleDeg, Vector3.up) * transform.rotation;
    Vector3 start = transform.position + Vector3.up * 0.2f;

    GameObject obj = Instantiate(damagePrefab, start, rot);
    BossDamageHit dmg = obj.GetComponent<BossDamageHit>();
    if (dmg != null)
    {
        dmg.Damage = bossAttackDamage;
    }
}

}

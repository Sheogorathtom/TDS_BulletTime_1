using UnityEngine;

public class Shooting : MonoBehaviour
{
    public Transform FirePoint;
    public GameObject Fire;
    public GameObject HitPoint;

    public void ShootingRay()
    {
        RaycastHit hit;

        if (Physics.Raycast(FirePoint.position, FirePoint.forward, out hit, 100f))
        {
            Debug.DrawLine(FirePoint.position, hit.point, Color.red);

            Instantiate(Fire,FirePoint.position, Quaternion.identity);
            Instantiate(HitPoint,hit.point, Quaternion.identity);

              Enemy enemy = hit.collider.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.Die();
        }
        }
    }
}

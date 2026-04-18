
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    // Attaque à l'épée
    public LayerMask enemyLayer;
    public float attackRange = 5;
    public PlayerStats playerStats;

    // Attaque à l'arc
    public GameObject arrowPrefab;

    

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);
            foreach (Collider collider in colliders)
            {
                Enemy enemy = collider.GetComponent<Enemy>();
                enemy.TakeDamage(playerStats.attackDamage);
                Debug.Log(enemy.name);
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 direction = (hit.point - transform.position).normalized;
                GameObject arrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);
                arrow.GetComponent<Projectile>().movementDirection = direction;
            }
        }
    }
}

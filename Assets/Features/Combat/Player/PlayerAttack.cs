using UnityEngine;

public class PlayerAttack : MonoBehaviour
{

    public LayerMask enemyLayer;
    public float attackRange = 5;
    public PlayerStats playerStats;

    

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
    }
}

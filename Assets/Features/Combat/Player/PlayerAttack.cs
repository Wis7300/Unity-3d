using UnityEngine;
using UnityEngine.InputSystem; // REQUIS pour le New Input System

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
        // Sécurité : On s'assure qu'une souris est bien détectée par le système
        if (Mouse.current == null) return;

        // --- CLIC GAUCHE : Attaque à l'épée ---
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);
            foreach (Collider collider in colliders)
            {
                Enemy enemy = collider.GetComponent<Enemy>();

                // Sécurité pour éviter un crash si l'objet n'a pas le script Enemy
                if (enemy != null)
                {
                    enemy.TakeDamage(playerStats.attackDamage);
                    Debug.Log("Ennemi touché : " + enemy.name);
                }
            }
        }
        // --- CLIC DROIT : Attaque à l'arc ---
        else if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            // Récupération de la position de la souris avec le New Input System
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ~0))
            {
                Vector3 direction = (hit.point - transform.position).normalized;
                direction.y = 0;
                direction = direction.normalized;

                Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y + 2, transform.position.z);
                GameObject arrow = Instantiate(arrowPrefab, spawnPosition, Quaternion.identity);

                Projectile projectile = arrow.GetComponent<Projectile>();
                if (projectile != null)
                {
                    projectile.movementDirection = direction;
                }
            }
            else
            {
                Debug.Log("No Raycast");
                Debug.Log("Ray origin: " + ray.origin + " direction: " + ray.direction);
            }
        }
    }
}
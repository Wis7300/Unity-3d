using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyData data;
    public GameObject player;

    private int currentHP;
    private float currentCooldown;
    private Rigidbody rb;

    private PlayerStats playerStats;

    void Start()
    {
        currentHP = data.maxHP;
        currentCooldown = data.attackCooldown;
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        
        GameObject player = GameObject.FindWithTag("Player");

        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();

            if (playerStats == null)
                Debug.LogError("PlayerStats non trouvé sur le Player !");
        }
        else
        {
            Debug.LogError("Aucun GameObject avec le tag 'Player' trouvé !");
        }
    }

    void Update()
    {
        float distance = Vector3.Distance(player.transform.position, transform.position);
        if (distance <= data.attackRange && currentCooldown <= 0f)
        {
            playerStats.TakeDamage(data.damage);
            currentCooldown = data.attackCooldown;
        }
        else if (distance <= data.detectionRange)
        {
            Vector3 direction = (player.transform.position - transform.position).normalized;
            rb.MovePosition(transform.position + direction * data.speed * Time.deltaTime);
        }

        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
        }
        
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        Debug.Log(name + " took " + amount + " damage");
        Debug.Log(name + " has " + currentHP + " HP left");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(name + " is dead");
        Destroy(gameObject);
    }
}
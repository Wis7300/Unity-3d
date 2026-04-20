using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyData data;
    public LootTableData lootTable;

    private int currentHP;
    private float currentCooldown;
    private Rigidbody rb;

    private PlayerStats playerStats;
    private GameObject player;

    void Start()
    {
        currentHP = data.maxHP;
        currentCooldown = data.attackCooldown;
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        
        player = GameObject.FindWithTag("Player");

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
        currentHP -= (amount - data.defense);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        DropLoot();
        playerStats.AddXP(data.xpDropped);
        Destroy(gameObject);
    }

    void DropLoot()
    {
        for (int i = 0; i < lootTable.items.Length; i++)
        {
            if (Random.value <= lootTable.dropChance[i])
            {
                GameObject obj = Instantiate(lootTable.items[i].prefab, transform.position, Quaternion.identity);
            }
        }
    }
}
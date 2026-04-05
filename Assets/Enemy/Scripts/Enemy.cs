using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyData data;
    public GameObject player;

    private int currentHP;
    private Rigidbody rb;

    void Start()
    {
        currentHP = data.maxHP;
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        Vector3 direction = (player.transform.position - transform.position).normalized;
        transform.Translate(direction * data.speed * Time.deltaTime);
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
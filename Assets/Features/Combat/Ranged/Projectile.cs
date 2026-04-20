using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage;
    public int speed;
    public Vector3 movementDirection;
    
    void Start()
    {
        Physics.IgnoreCollision(GetComponent<Collider>(), GameObject.FindWithTag("Player").GetComponent<Collider>(), true);
        Collider playerCollider = GameObject.FindWithTag("Player").GetComponent<Collider>();
        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            Physics.IgnoreCollision(col, playerCollider);
        }
        transform.forward = movementDirection;
    }

    void Update()
    {
        transform.Translate(movementDirection * speed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null )
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}

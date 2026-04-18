using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage;
    public int speed;
    public Vector3 movementDirection;
        
    void Update()
    {
        transform.Translate(movementDirection * speed * Time.deltaTime);
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

using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public PlayerStatsData statsData; 
    public int currentHealth;
    public int attackDamage;

    void Start()
    {
        currentHealth = statsData.maxHealth;
        attackDamage = statsData.damage;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0); 
        Debug.Log("Player HP: " + currentHealth);

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        Debug.Log("Le Player est mort !");
    }
}
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public PlayerStatsData statsData;
    public int maxHealth;
    public int currentHealth;
    public float attackDamageFloat;
    public int attackDamage;
    public int level;
    public int xp;
    private int xpRequired;

    void Start()
    {
        maxHealth = statsData.maxHealth;
        currentHealth = statsData.maxHealth;
        attackDamage = statsData.damage;
        attackDamageFloat = statsData.damage;
        level = statsData.level;
        xp = statsData.xp;
        xpRequired = 10;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0); 
        Debug.Log("Player HP: " + currentHealth);

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        Debug.Log("Le Player est mort !");
    }

    public void AddXP(int nbr)
    {
        if (currentHealth > 0)
        {
            xp += nbr;
            LevelUp();
        }
    }
    void LevelUp()
    {
        while (xpRequired <= xp)
        {   
            xp = xp - xpRequired;
            xpRequired = Mathf.RoundToInt(xpRequired * statsData.xpMult);
            level += 1;

            maxHealth = Mathf.RoundToInt(maxHealth * statsData.healthMult);
            attackDamageFloat *= statsData.damageMult;
            attackDamage = Mathf.RoundToInt(attackDamageFloat);

            currentHealth = maxHealth;

            Debug.Log("Le joueur est monté au niveau " + level);
        }
    }
}
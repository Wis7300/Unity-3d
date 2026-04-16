using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public PlayerStatsData statsData;
    public int maxHealth;
    public int currentHealth;
    public int attackDamage;
    public int level;
    public int xp;
    private int[] levelList;

    void Start()
    {
        maxHealth = statsData.maxHealth;
        currentHealth = statsData.maxHealth;
        attackDamage = statsData.damage;
        level = statsData.level;
        xp = statsData.xp;
        levelList = new int[] {10, 15, 20, 25, 30, 35, 40 };
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
        while (levelList[level] <= xp)
        {   
            xp = xp - levelList[level];
            level += 1;

            maxHealth = Mathf.RoundToInt(maxHealth * 1.05f);
            attackDamage = Mathf.RoundToInt(attackDamage * 1.05f);

            currentHealth = maxHealth;

            Debug.Log("Le joueur est monté au niveau " + level);
        }
    }
}
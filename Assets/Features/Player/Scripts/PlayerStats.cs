using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public PlayerStatsData statsData;
    private PlayerSaveData saveData;

    // Stats "normales"
    public int maxHealth;
    public int currentHealth;
    public float attackDamageFloat;
    public int attackDamage;
    public int level;
    public int xp;
    private int xpRequired;

    // Stats Spéciales (elles ont un ptt truc en plus mdr)
    public int toxicity;
    public int freeze;
    public int warm;


    void Start()
    {
        saveData = GameManager.instance.PlayerSaveData;

        if (saveData.level > 0)
        {
            maxHealth = saveData.maxHealth;
            currentHealth = saveData.currentHealth;
            attackDamage = saveData.attackDamage;
            attackDamageFloat = saveData.attackDamageFloat;
            level = saveData.level;
            xp = saveData.xp;
            xpRequired = saveData.xpRequired;

            toxicity = saveData.toxicity;
            freeze = saveData.freeze;
            warm = saveData.warm;

        }
        else
        {
            maxHealth = statsData.maxHealth;
            currentHealth = statsData.maxHealth;
            attackDamage = statsData.damage;
            attackDamageFloat = statsData.damage;
            level = statsData.level;
            xp = statsData.xp;
            xpRequired = 10;

            toxicity = 0;
            freeze = 0;
            warm = 0;
        }
    }

    private void SaveStats()
    {
        saveData.maxHealth = maxHealth;
        saveData.currentHealth = currentHealth;
        saveData.attackDamage = attackDamage;
        saveData.attackDamageFloat = attackDamageFloat;
        saveData.level = level;
        saveData.xp = xp;
        saveData.xpRequired = xpRequired;

        saveData.toxicity = toxicity;
        saveData.freeze = freeze;
        saveData.warm = warm;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        saveData.currentHealth = currentHealth;
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
        SaveStats();
    }
}
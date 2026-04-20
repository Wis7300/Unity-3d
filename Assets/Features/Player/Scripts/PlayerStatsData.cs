using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Player")]
public class PlayerStatsData : ScriptableObject
{
    public int maxHealth;
    public int damage;
    public int level;
    public int xp;

    public float xpMult;
    public float damageMult;
    public float healthMult;
}
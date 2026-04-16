using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Player")]
public class PlayerStatsData : ScriptableObject
{
    public int maxHealth;
    public int damage;
    public int level;
    public int xp;
}
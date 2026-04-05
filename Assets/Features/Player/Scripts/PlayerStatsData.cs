using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Player")]
public class PlayerStatsData : ScriptableObject
{
    public int maxHealth;
    public int damage;
}
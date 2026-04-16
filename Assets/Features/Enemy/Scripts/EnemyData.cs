using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy")]
public class EnemyData : ScriptableObject
{
    public int maxHP;
    public int damage;
    public float speed;
    public int defense;
    public float attackRange;
    public float detectionRange;
    public float attackCooldown;
    public int xpDropped;
}
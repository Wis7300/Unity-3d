using UnityEngine;

[CreateAssetMenu(fileName = "NewLootTable", menuName = "LootTable")]
public class LootTableData : ScriptableObject
{
    public ItemData[] items;
    public float[] dropChance;
}

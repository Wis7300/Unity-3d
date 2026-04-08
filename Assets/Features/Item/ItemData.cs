using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Item")]
public class ItemData : ScriptableObject
{
    public ItemID id;

    public string itemName;
    public int rarety;  // 1 = common, 2 = rare, 3 = epic, 4 = legendary, 5 = mythic
    public GameObject prefab;
}

using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public Inventory playerInventory;

    void Start()
    {
        playerInventory = new Inventory();
        playerInventory.AddItem(ItemID.IronSword);

        int qty = playerInventory.GetQuantity(ItemID.IronSword);
        Debug.Log("Iron Sword quantity: " + qty);

        Item swordData = ItemDatabase.items[ItemID.IronSword];
        Debug.Log("Iron Sword damage: " + swordData.damage);
    }
}
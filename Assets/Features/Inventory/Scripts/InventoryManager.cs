using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public Inventory playerInventory;

    void Start()
    {
        playerInventory = GameManager.instance.PlayerInventory;

        playerInventory.AddItem(ItemID.IronSword);
        Debug.Log("sword added to inventory");
    }
}
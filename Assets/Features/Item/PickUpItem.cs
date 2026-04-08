using System.Runtime.CompilerServices;
using UnityEngine;

public class PickUpItem : MonoBehaviour
{
    public LayerMask itemLayer;
    public InventoryManager inventoryManager;
    public float range = 2;

    private float delayBetweenChecks = 0.2f;
    private float time = 0f;
    
    void Update()
    {

        if (time <= 0f)
        {
            PickUp();
            time = delayBetweenChecks;
        }
        else if (time <= delayBetweenChecks && time >= 0)
        {
            time -= Time.deltaTime;
        }
    }

    void PickUp()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, range, itemLayer);
        foreach (Collider collider in colliders)
        {
            ItemWorld itemWorld = collider.GetComponent<ItemWorld>();
            if (itemWorld != null)
            {
                inventoryManager.playerInventory.AddItem(itemWorld.itemData.id);
                Destroy(collider.gameObject);
            }
        }
    }
}

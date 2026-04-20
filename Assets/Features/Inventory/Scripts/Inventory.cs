using System.Collections.Generic;

public class Inventory
{
    private Dictionary<ItemID, int> items = new Dictionary<ItemID, int>();
    private int capacity = 24;
    public event System.Action OnInventoryChanged;

    public void AddItem(ItemID id)
    {
        if (items.ContainsKey(id))
        { 
            items[id]++;
            OnInventoryChanged?.Invoke();
        }
        else if (items.Count < capacity)
        { 
            items[id] = 1;
            OnInventoryChanged?.Invoke();
        }
        
    }

    public void RemoveItem(ItemID id)
    {
        if (items.ContainsKey(id))
        { 
            items[id]--; 
            if (items[id] <= 0)
            {  
                items.Remove(id);
                
            }
            OnInventoryChanged?.Invoke();
        }
    }

    public int GetQuantity(ItemID id)
    {
        return items.ContainsKey(id) ? items[id] : 0;
    }
    
    public Dictionary<ItemID, int> GetItems()
    {
        return items;
    }


}
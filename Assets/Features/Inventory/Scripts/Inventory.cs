using System.Collections.Generic;

public class Inventory
{
    private Dictionary<ItemID, int> items = new Dictionary<ItemID, int>();

    public void AddItem(ItemID id)
    {
        if (items.ContainsKey(id))
        { items[id]++; }
        else
        { items[id] = 1; }
    }

    public void RemoveItem(ItemID id)
    {
        if (items.ContainsKey(id))
        { 
            items[id]--; 
            if (items[id] <= 0)
            {  items.Remove(id); }
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
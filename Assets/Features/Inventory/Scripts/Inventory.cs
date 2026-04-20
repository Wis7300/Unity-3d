using System.Collections.Generic;

public class ItemStack
{
    public ItemID id;
    public int quantity;

    public ItemStack(ItemID id, int quantity)
    {
        this.id = id;
        this.quantity = quantity;
    }
}

public class Inventory
{
    private Dictionary<int, ItemStack> slots = new Dictionary<int, ItemStack>();
    private int capacity = 24;
    private ItemDatabase database;
    public event System.Action OnInventoryChanged;

    public Inventory(ItemDatabase database)
    {
        this.database = database;
    }

    public void AddItem(ItemID id)
    {
        ItemData data = database.GetItem(id);
        bool isArmor = data.type == ItemType.Armor;

        if (isArmor)
        {
            for (int i = 4; i < 8; i++)
            {
                if (slots.ContainsKey(i) && slots[i].id == id)
                {
                    slots[i].quantity++;
                    OnInventoryChanged?.Invoke();
                    return;
                }
                if (!slots.ContainsKey(i))
                {
                    slots[i] = new ItemStack(id, 1);
                    OnInventoryChanged?.Invoke();
                    return;
                }
            }
        }
        else
        {
            for (int i = 0; i < capacity; i++)
            {
                if (i >= 4 && i < 8) continue; // saute les slots armure
                if (slots.ContainsKey(i) && slots[i].id == id)
                {
                    slots[i].quantity++;
                    OnInventoryChanged?.Invoke();
                    return;
                }
                if (!slots.ContainsKey(i))
                {
                    slots[i] = new ItemStack(id, 1);
                    OnInventoryChanged?.Invoke();
                    return;
                }
            }
        }
    }

    public void RemoveItem(ItemID id)
    {
        for (int i = 0; i < capacity; i++)
        {
            if (slots.ContainsKey(i) && slots[i].id == id)
            {
                slots[i].quantity--;
                if (slots[i].quantity <= 0)
                    slots.Remove(i);
                OnInventoryChanged?.Invoke();
                return;
            }
        }
    }

    public int GetQuantity(ItemID id)
    {
        int count = 0;
        foreach (var slot in slots.Values)
            if (slot.id == id) count += slot.quantity;
        return count;
    }

    public Dictionary<int, ItemStack> GetSlots()
    {
        return slots;
    }
}
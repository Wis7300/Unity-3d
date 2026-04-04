using System.Collections.Generic;


public static class ItemDatabase
{
    public static Dictionary<ItemID, Item> items = new Dictionary<ItemID, Item>()
    {
        { ItemID.Coin, new Item { name = "Steel Sword", damage = 0, type = "Usable", rarety = "common" } },
        { ItemID.Potion, new Item { name = "Health Potion", damage = 0, type="Consumable", rarety = "common" } },
        { ItemID.IronSword, new Item { name = "Iron Sword", damage = 10, type="Weapon", rarety = "common" } }
    };
}
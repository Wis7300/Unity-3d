using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    private Image[] slots;
    private float refreshDelay = 0.5f;
    void Start()
    {
        slots = GetComponentsInChildren<Image>();
        Refresh();
    }

    void Update()
    {
        if (refreshDelay > 0)
        {
            refreshDelay -= Time.deltaTime;
        }
        if (refreshDelay <= 0)
        {
            Refresh();
            refreshDelay = 0.5f;
        }
    }

    void Refresh()
    {
        var itemList = new List<ItemID>(GameManager.instance.PlayerInventory.GetItems().Keys);
        for (int i = 1; i < slots.Length; i++)
        {
            if (i - 1 < itemList.Count)
                slots[i].color = Color.gray;
            else
                slots[i].color = Color.white;
        }
    }
}

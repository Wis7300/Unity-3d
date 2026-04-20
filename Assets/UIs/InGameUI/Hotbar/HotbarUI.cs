using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class HotbarUI : MonoBehaviour
{
    private Image[] slots;
    

    void Awake()
    {
        slots = GetComponentsInChildren<Image>(true);
        GameManager.instance.PlayerInventory.OnInventoryChanged += Refresh;
    }

    void Start()
    {
        Refresh();
    }

    void OnDestroy()
    {
        GameManager.instance.PlayerInventory.OnInventoryChanged -= Refresh;
    }

    void Refresh()
    {
        var itemList = new List<ItemID>(GameManager.instance.PlayerInventory.GetItems().Keys);
        Debug.Log(itemList.Count);
        if (slots == null || slots.Length == 0) return;
        for (int i = 0; i < 8; i++)
        {
            if (i < itemList.Count)
            {
                slots[i].color = Color.gray;
                TextMeshProUGUI textSlot = slots[i].GetComponentInChildren<TextMeshProUGUI>();
                textSlot.text = GameManager.instance.PlayerInventory.GetQuantity(itemList[i]).ToString();
            }
            else
            {
                slots[i].color = Color.white;
            }
        }
    }
}

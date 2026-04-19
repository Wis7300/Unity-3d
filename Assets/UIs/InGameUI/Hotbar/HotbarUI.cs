using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class HotbarUI : MonoBehaviour
{
    private Image[] slots;
    

    void Awake()
    {
        slots = GetComponentsInChildren<Image>();
    }

    void Start()
    {
        GameManager.instance.PlayerInventory.OnInventoryChanged += Refresh;
        Refresh();
    }

    void Refresh()
    {
        var itemList = new List<ItemID>(GameManager.instance.PlayerInventory.GetItems().Keys);
        Debug.Log(itemList.Count);
        for (int i = 1; (i < itemList.Count && i < 9); i++)
        {
            if (i - 1 < itemList.Count)
            {
                slots[i].color = Color.gray;
                TextMeshProUGUI textSlot = slots[i].GetComponentInChildren<TextMeshProUGUI>();
                textSlot.text = GameManager.instance.PlayerInventory.GetQuantity(itemList[i - 1]).ToString();
            }
            else
            {
                slots[i].color = Color.white;
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    private Image[] slots;
    void Start()
    {
        slots = GetComponentsInChildren<Image>();
        Refresh();
    }

    public void Refresh()
    {
        var slots_data = GameManager.instance.PlayerInventory.GetSlots();
        for (int i = 1; i < slots.Length; i++)
        {
            if (slots_data.ContainsKey(i - 1))
            {
                slots[i].color = Color.gray;
                TextMeshProUGUI textSlot = slots[i].GetComponentInChildren<TextMeshProUGUI>();
                textSlot.text = slots_data[i - 1].quantity.ToString();
            }
            else
            {
                slots[i].color = Color.white;
                TextMeshProUGUI textSlot = slots[i].GetComponentInChildren<TextMeshProUGUI>();
                textSlot.text = "";
            }
        }
    }
}

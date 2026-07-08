using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public List<Image> slots; // assignés dans l’inspecteur

    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        // SÉCURITÉ : On s'assure que l'inventaire existe avant de demander ses slots
        if (GameManager.instance == null || GameManager.instance.PlayerInventory == null) return;

        var slots_data = GameManager.instance.PlayerInventory.GetSlots();
        if (slots_data == null) return;

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == null) continue;
            TextMeshProUGUI textSlot = slots[i].GetComponentInChildren<TextMeshProUGUI>();

            if (slots_data.ContainsKey(i))
            {
                slots[i].color = Color.gray;
                if (textSlot != null)
                    textSlot.text = slots_data[i].quantity.ToString();
            }
            else
            {
                slots[i].color = Color.white;
                if (textSlot != null)
                    textSlot.text = "";
            }
        }
    }
}
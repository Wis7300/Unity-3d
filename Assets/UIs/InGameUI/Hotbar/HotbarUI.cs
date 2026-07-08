using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HotbarUI : MonoBehaviour
{
    private Image[] slots;
    private int selectedSlot = 0;

    void Awake()
    {
        slots = GetComponentsInChildren<Image>(true);
    }

    void Start()
    {
        // SÉCURITÉ : On vérifie que tout le GameManager est bien prêt
        if (GameManager.instance != null && GameManager.instance.PlayerInventory != null)
        {
            GameManager.instance.PlayerInventory.OnInventoryChanged += Refresh;
        }
        Refresh();
    }

    void OnDestroy()
    {
        if (GameManager.instance != null && GameManager.instance.PlayerInventory != null)
        {
            GameManager.instance.PlayerInventory.OnInventoryChanged -= Refresh;
        }
    }

    void Update()
    {
        for (int i = 0; i < 8; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                selectedSlot = i;
                Refresh();
            }
        }
    }

    void Refresh()
    {
        // SÉCURITÉ : Si l'inventaire n'est pas encore instancié
        if (GameManager.instance == null || GameManager.instance.PlayerInventory == null) return;
        if (slots == null || slots.Length == 0) return;

        var slots_data = GameManager.instance.PlayerInventory.GetSlots();
        if (slots_data == null) return;

        for (int i = 0; i < Mathf.Min(8, slots.Length); i++)
        {
            TextMeshProUGUI textSlot = slots[i].GetComponentInChildren<TextMeshProUGUI>();

            if (slots_data.ContainsKey(i))
            {
                slots[i].color = Color.gray;
                if (textSlot != null) textSlot.text = slots_data[i].quantity.ToString();
            }
            else
            {
                slots[i].color = Color.white;
                if (textSlot != null) textSlot.text = "";
            }

            if (i == selectedSlot)
                slots[i].color = Color.yellow;
        }
    }
}
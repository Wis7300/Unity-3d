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
        GameManager.instance.PlayerInventory.OnInventoryChanged += Refresh;
    }

    void Start()
    {
        Refresh();
    }

    void OnDestroy()
    {
        // On vérifie si l'instance existe encore avant de se désabonner
        if (GameManager.instance != null && GameManager.instance.PlayerInventory != null)
        {
            GameManager.instance.PlayerInventory.OnInventoryChanged -= Refresh;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedSlot = 0;
            Refresh();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedSlot = 1;
            Refresh();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            selectedSlot = 2;
            Refresh();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            selectedSlot = 3;
            Refresh();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            selectedSlot = 4;
            Refresh();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            selectedSlot = 5;
            Refresh();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            selectedSlot = 6;
            Refresh();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            selectedSlot = 7;
            Refresh();
        }
    }

    void Refresh()
    {
        var slots_data = GameManager.instance.PlayerInventory.GetSlots();
        if (slots == null || slots.Length == 0) return;
        for (int i = 0; i < 8; i++)
        {
            if (slots_data.ContainsKey(i))
            {
                slots[i].color = Color.gray;
                TextMeshProUGUI textSlot = slots[i].GetComponentInChildren<TextMeshProUGUI>();
                textSlot.text = slots_data[i].quantity.ToString();
            }
            else
            {
                slots[i].color = Color.white;
                TextMeshProUGUI textSlot = slots[i].GetComponentInChildren<TextMeshProUGUI>();
                textSlot.text = "";
            }
            if (i == selectedSlot)
                slots[i].color = Color.yellow;
        }
    }
}

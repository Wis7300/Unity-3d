using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    public GameObject inventoryGrid;
    public GameObject inventoryTitle;

    void Start()
    {
        inventoryGrid.SetActive(false);
        inventoryTitle.SetActive(false);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            inventoryGrid.SetActive(!inventoryGrid.activeSelf);
            inventoryTitle.SetActive(!inventoryTitle.activeSelf);
        }
    }
}
using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    public GameObject inventoryGrid;
    public GameObject inventoryTitle;

    void Start()
    {
        inventoryGrid.SetActive(false);
        inventoryTitle.SetActive(false);
        Time.timeScale = 1;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            inventoryGrid.SetActive(!inventoryGrid.activeSelf);
            inventoryTitle.SetActive(!inventoryTitle.activeSelf);
            if (Time.timeScale == 1)
            {
                inventoryGrid.GetComponent<InventoryUI>().Refresh();
                Time.timeScale = 0;
            }
            else
            {
                Time.timeScale = 1;
            }
        }
    }
}
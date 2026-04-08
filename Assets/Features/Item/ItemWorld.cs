using UnityEngine;

public class ItemWorld : MonoBehaviour
{
    public ItemData itemData;

    void Start()
    {
        if (itemData == null)
        {
            Debug.LogError("ItemData manquant sur " + gameObject.name);
        }
    }
}
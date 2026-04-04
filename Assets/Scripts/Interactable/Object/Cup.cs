using UnityEngine;

public class Cup : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("Tasse intéragie");
        gameObject.SetActive(false);
    }
}

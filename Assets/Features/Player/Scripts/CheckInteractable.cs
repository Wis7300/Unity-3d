using UnityEngine;

public class CheckInteractable : MonoBehaviour
{
    public float range = 2;
    public LayerMask interactableLayer;

    // On stocke directement l'interface, pas le GameObject
    IInteractable currentInteractable;

    void Update()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range, interactableLayer);

        IInteractable closest = null;
        float minDistance = range;

        foreach (Collider hit in hits)
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();

            if (interactable != null)
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = interactable;
                }
            }
        }
        currentInteractable = closest;

        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            currentInteractable.Interact();
        }
    }
}
using UnityEngine;

public class CheckInteractable : MonoBehaviour
{
    public float range = 2;
    public LayerMask interactableLayer;

    // Update is called once per frame
    void Update()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 2f, interactableLayer);

        foreach (Collider col in hits)
        {
            Debug.Log("Objet proche : " + col.name);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Objet proche :" + other.name);
    }
}

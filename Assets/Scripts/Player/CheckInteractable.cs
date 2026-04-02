using UnityEngine;

public class CheckInteractable : MonoBehaviour
{
    public float range = 2;
    public 

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, range))
        {
            Debug.Log("Touché");
        }
        Debug.DrawRay(transform.position, transform.forward * range, Color.red, 2.0f);
        Debug.DrawRay(Vector3.zero, Vector3.forward * 5f, Color.red);
    }
}

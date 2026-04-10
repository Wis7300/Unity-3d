using UnityEngine;

public class DebugScript : MonoBehaviour
{

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameManager.instance.GoToNextFloor();
        }
    }
}

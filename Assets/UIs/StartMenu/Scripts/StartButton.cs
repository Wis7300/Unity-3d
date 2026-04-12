using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    public void Play()
    {
        SceneManager.LoadScene("Floor1");
    }

    public void Credit()
    {

    }

    public void Exit()
    {
        //Application.Quit();       Quand on importera le jeu et qu'on le lancera pas dans Unity 
        UnityEditor.EditorApplication.isPlaying = false;    // En attendant
    }
}

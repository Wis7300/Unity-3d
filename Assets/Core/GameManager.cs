using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static GameManager instance;

    private int currentFloor;
    private int currentBiome;
    private GameState currentState;

    public int CurrentFloor => currentFloor;
    public int CurrentBiome => currentBiome;
    public GameState CurrentState => currentState;


    void Awake()
    {
        if (instance is null)
        {
            instance = this; 
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        currentBiome = GetCurrentBiome();
        DontDestroyOnLoad(gameObject);
        currentState = GameState.Playing;
    }

    private int GetCurrentBiome()
    {
        return currentFloor / 10;
    }

    public void GoToNextFloor()
    {
        currentFloor++;
        currentBiome = GetCurrentBiome();
        currentState = GameState.Playing;
        string floorName = $"Floor{currentFloor}";
        SceneManager.LoadScene(floorName);
    }

    public void SetGameState(GameState newState)
    {
        currentState = newState;
    }
}

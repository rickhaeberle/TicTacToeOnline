using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Server Server { get; private set; }

    public event Action OnQuitMatchEvent;

    private EGameMode? _gameMode;
    private EDifficulty? _difficulty;
    private ESide? _side;

    private void Awake()
    {
        if (Instance != null)
            return;

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(Constants.Scene.MENU);
    }

    // Offline
    public void StartOfflineGame(EGameMode? gameMode, EDifficulty? difficulty, ESide? side)
    {
        _gameMode = gameMode;
        _difficulty = difficulty;
        _side = side;

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(Constants.Scene.GAME);

    }

    // Online
    public void CreateServer()
    {
        Server = new Server();
    }

    public void QuitOnlineMatch()
    {
        OnQuitMatchEvent?.Invoke();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        BoardController.Instance.CreateGame(_gameMode.Value, _side != null ? _side.Value : ESide.First, _difficulty);

    }
}

using System;
using UnityEngine;

public enum GameState
{
    Init,
    FirstLevel,
    Started
}

public class GameManager : Singleton<GameManager>
{
    public delegate void OnLevelCreated();
    public static event OnLevelCreated onNewLevel;

    [Header("Joystick")]
    public Joystick joystick;
    public static GameState CurrentGameState { get { return currentGameState; } }

    static GameState currentGameState;

    public static Player Clone { get { return clone; } }
    static Player clone;

    static Player currentPlayer;

    LevelManager levelManager;
    EnemySpawner enemySpawner;
    ObstacleSpawner obstacleSpawner;

    LevelInfoAssetSO levelInfoAsset;

    private void OnEnable()
    {
        UIManager.OnVariableChange += SelectPlayer;
        LevelManager.onGameStateChange += SetGameState;
    }

    public void SetGameState(GameState newVal)
    {
        currentGameState = newVal;
        Debug.Log(currentGameState);
    }

    private void OnDisable()
    {
        UIManager.OnVariableChange -= SelectPlayer;
        LevelManager.onGameStateChange -= SetGameState;
    }

    private new void Awake()
    {
        base.Awake();

        if (levelManager == null)
            levelManager = FindObjectOfType<LevelManager>();
        if (enemySpawner == null)
            enemySpawner = FindObjectOfType<EnemySpawner>();
        if (obstacleSpawner == null)
            obstacleSpawner = FindObjectOfType<ObstacleSpawner>();

        levelInfoAsset = Resources.Load<LevelInfoAssetSO>("Level/LevelInfoAsset");
    }

    void Start()
    {
        switch (currentGameState)
        {
            case GameState.Init:
                if (currentPlayer == null)
                    currentPlayer = Resources.Load<Player>("Gunner").GetComponent<Player>();
                break;
            case GameState.Started:
                if (currentPlayer != null)
                    clone = Instantiate(currentPlayer);
                if (onNewLevel != null)
                    onNewLevel?.Invoke();
                enemySpawner.SpawnEnemies(levelInfoAsset.levelInfos[levelManager.CurrentLevelIndex]);
                obstacleSpawner.SpawnObstacles(levelInfoAsset.levelInfos[levelManager.CurrentLevelIndex]);
                break;
            case GameState.FirstLevel:
                if (currentPlayer != null)
                    clone = Instantiate(currentPlayer);
                clone.fillHp();
                enemySpawner.SpawnEnemies(levelInfoAsset.levelInfos[levelManager.CurrentLevelIndex]);
                obstacleSpawner.SpawnObstacles(levelInfoAsset.levelInfos[levelManager.CurrentLevelIndex]);
                break;
        }
    }

    public void Restart()
    {
        levelManager.RestartGame();
        currentGameState = GameState.Init;
    }

    public void SelectPlayer(string characterIndexName)
    {
        currentPlayer = Resources.Load<Player>(characterIndexName).GetComponent<Player>();
    }
}


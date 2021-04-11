using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Init,
    Started
}

public class GameManager : Singleton<GameManager>
{
    [Header("Joystick")]
    public Joystick joystick;
    public static GameState CurrentGameState { get { return currentGameState; } }

    static GameState currentGameState;

    public Player Clone { get { return clone; } }
    Player clone;

    static Player currentPlayer;

    LevelManager levelManager;
    EnemySpawner enemySpawner;
    ObstacleSpawner obstacleSpawner;

    LevelInfoAssetSO levelInfoAsset;

    private new void Awake()
    {
        //currentGameState = GameState.Started; // FOR TEST

        base.Awake();

        levelManager = FindObjectOfType<LevelManager>();
        enemySpawner = FindObjectOfType<EnemySpawner>();
        obstacleSpawner = FindObjectOfType<ObstacleSpawner>();

        levelInfoAsset = Resources.Load<LevelInfoAssetSO>("Level/LevelInfoAsset");
    }

    void Start()
    {
        switch (currentGameState)
        {
            case GameState.Init:
                if (currentPlayer == null)
                    currentPlayer = Resources.Load<Player>("Archer").GetComponent<Player>();       
                break;
            case GameState.Started:
                if (currentPlayer != null)
                    clone = Instantiate(currentPlayer);
                enemySpawner.SpawnEnemies(levelInfoAsset.levelInfos[levelManager.CurrentLevelIndex]);
                obstacleSpawner.SpawnObstacles(levelInfoAsset.levelInfos[levelManager.CurrentLevelIndex]);
                break;
        }
    }

    public void Restart()
    {
        currentGameState = GameState.Init;
        //TODO restart level 
    }

    public void StartGame()
    {
        levelManager.HandleCreateNextLevel();
        currentGameState = GameState.Started;
    }

    private void Update()
    {
        // For Test
        if (Input.GetKeyDown(KeyCode.F))
        {
            enemySpawner.SpawnEnemies(levelInfoAsset.levelInfos[levelManager.CurrentLevelIndex]);
            obstacleSpawner.SpawnObstacles(levelInfoAsset.levelInfos[levelManager.CurrentLevelIndex]);
        }
    }

    public void SelectPlayer(Player playerType)
    {
        currentPlayer = playerType.GetComponent<Player>();
    }
}


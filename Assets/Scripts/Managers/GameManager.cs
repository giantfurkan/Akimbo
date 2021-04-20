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

    private void OnEnable()
    {
        UIManager.OnVariableChange += SelectPlayer;
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
                enemySpawner.SpawnEnemies(levelInfoAsset.levelInfos[levelManager.CurrentLevelIndex]);
                obstacleSpawner.SpawnObstacles(levelInfoAsset.levelInfos[levelManager.CurrentLevelIndex]);
                break;
        }
    }

    public void Restart()
    {
        currentGameState = GameState.Init;
    }

    public void StartGame()
    {
        levelManager.HandleCreateNextLevel();
        currentGameState = GameState.Started;
    }

    public void SelectPlayer(string characterIndexName)
    {
        Debug.Log("yep beybi");

        currentPlayer = Resources.Load<Player>(characterIndexName).GetComponent<Player>();
    }
}


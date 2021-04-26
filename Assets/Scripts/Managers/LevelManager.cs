using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager>
{
    public delegate void OnGameStateChange(GameState newVal);
    public static event OnGameStateChange onGameStateChange;

    public int CurrentLevelIndex { get { return currentLevelIndex; } }

    [SerializeField] Animator transition;
    [SerializeField] float transitionTime = .7f;

    private int currentLevelIndex = 0;

    LevelInfoAssetSO levelInfoAsset;

    private void OnEnable()
    {
        Gate.handleNextLevel += HandleCreateNextLevel;
    }

    private void OnDisable()
    {
        Gate.handleNextLevel -= HandleCreateNextLevel;
    }

    private new void Awake()
    {
        base.Awake();

        currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
        levelInfoAsset = Resources.Load<LevelInfoAssetSO>("Level/LevelInfoAsset");
        transition = GetComponentInChildren<Animator>();
    }

    public void HandleCreateNextLevel()
    {
        ++currentLevelIndex;

        if (levelInfoAsset.levelInfos.Count -1 >= currentLevelIndex)
        {
            LoadNextLevel(currentLevelIndex);

            if (onGameStateChange != null && GameManager.CurrentGameState != GameState.Started)
                onGameStateChange(GameState.Started);
        }
        else
        {
            RestartGame();
        }
    }

    public void LoadNextLevel(int level)
    {
        StartCoroutine(LoadLevel(level));
    }

    public void StartGame()
    {
        StartCoroutine(LoadLevel(1));

        if (onGameStateChange != null)
            onGameStateChange(GameState.FirstLevel);
    }

    public void RestartGame()
    {
        StartCoroutine(LoadLevel(0));
    }

    IEnumerator LoadLevel(int level)
    {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(level);
    }
}

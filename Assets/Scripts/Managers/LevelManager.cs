using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private int currentLevelIndex = 0;
    public int CurrentLevelIndex { get { return currentLevelIndex; } }

    LevelInfoAssetSO levelInfoAsset;

    private void OnEnable()
    {
        Gate.handleNextLevel += HandleCreateNextLevel;
    }
    private void OnDisable()
    {
        Gate.handleNextLevel -= HandleCreateNextLevel;
    }
    private void Awake()
    {
        currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
        levelInfoAsset = Resources.Load<LevelInfoAssetSO>("Level/LevelInfoAsset");
    }

    public void HandleCreateNextLevel()
    {
        ++currentLevelIndex;
        if (levelInfoAsset.levelInfos.Count >= currentLevelIndex)
        {
            CreateNextLevel();
        }
    }

    private void CreateNextLevel()
    {
        SceneManager.LoadScene(currentLevelIndex);
    }
}

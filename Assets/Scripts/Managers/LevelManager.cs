using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public int CurrentLevelIndex { get { return currentLevelIndex; } }

    [SerializeField] Animator transition;
    [SerializeField] float transitionTime = 1f;

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
    private void Awake()
    {
        currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
        levelInfoAsset = Resources.Load<LevelInfoAssetSO>("Level/LevelInfoAsset");
        transition = GetComponentInChildren<Animator>();
    }

    public void HandleCreateNextLevel()
    {
        ++currentLevelIndex;
        if (levelInfoAsset.levelInfos.Count >= currentLevelIndex)
        {
            LoadNextLevel(currentLevelIndex);
        }
    }

    public void LoadNextLevel(int level)
    {
        StartCoroutine(LoadLevel(level));
    }

    IEnumerator LoadLevel(int level)
    {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(level);

    }
}

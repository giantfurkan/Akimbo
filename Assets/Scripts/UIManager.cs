using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class CharacterInfos
{
    public int index;
    public GameObject model;
    public string charName;
}

public class UIManager : MonoBehaviour
{
    public string currentCharName;

    [Space]
    [SerializeField] Image bar;
    [SerializeField] TextMeshProUGUI levelText;

    [Space]
    [SerializeField] Button nextBtn, previousBtn, playBtn, restartBtn;

    [SerializeField] GameObject deathCanvas;

    private int currentIndex = 0;

    private float offset = 7.5f;

    [SerializeField] List<CharacterInfos> characterInfos;

    #region Variable Changed Event
    public delegate void OnVariableChangeDelegate(string newVal);
    public static event OnVariableChangeDelegate OnVariableChange;
    #endregion

    private void OnEnable()
    {
        Player.onPlayerDeath += DeathCanvas;
    }
    private void OnDisable()
    {
        Player.onPlayerDeath -= DeathCanvas;
    }

    private void Start()
    {
        if (deathCanvas != null)
        {
            deathCanvas.SetActive(false);
            restartBtn.onClick.AddListener(() => GameManager.Instance.Restart());
        }

        if (GameManager.CurrentGameState == GameState.Init)
        {
            foreach (var model in characterInfos)
            {
                model.model.gameObject.SetActive(false);
            }

            characterInfos[currentIndex].model.SetActive(true);
            nextBtn.onClick.AddListener(() => NextButton());
            previousBtn.onClick.AddListener(() => PreviousButton());
            playBtn.onClick.AddListener(() => LevelManager.Instance.StartGame());
        }

        if (GameManager.CurrentGameState == GameState.Started && GameManager.CurrentGameState == GameState.FirstLevel)
        {

        }
    }

    void Update()
    {
        if (levelText != null)
            levelText.text = "Lv" + " " + Player.level.ToString();
        if (bar != null)
            bar.fillAmount = GameManager.Clone.GetExperienceNormalized();
    }

    private void NextButton()
    {
        if (currentIndex < characterInfos.Count - 1)
        {
            currentIndex++;
            currentCharName = characterInfos[currentIndex].charName;

            if (OnVariableChange != null)
                OnVariableChange(currentCharName);

            characterInfos[currentIndex - 1].model.gameObject.SetActive(false);
            characterInfos[currentIndex].model.gameObject.SetActive(true);

            if (currentIndex == characterInfos.Count - 1) nextBtn.interactable = false;
            if (!previousBtn.interactable) previousBtn.interactable = true;
        }
    }

    private void PreviousButton()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            currentCharName = characterInfos[currentIndex].charName;

            if (OnVariableChange != null)
                OnVariableChange(currentCharName);

            characterInfos[currentIndex + 1].model.gameObject.SetActive(false);
            characterInfos[currentIndex].model.gameObject.SetActive(true);

            if (currentIndex == 0) previousBtn.interactable = false;
            if (!nextBtn.interactable) nextBtn.interactable = true;
        }
    }

    public void DeathCanvas()
    {
        deathCanvas.SetActive(true);
    }
}

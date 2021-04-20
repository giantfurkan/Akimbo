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
    [SerializeField] Button nextBtn, previousBtn;

    private int currentIndex = 0;


    private float offset = 7.5f;

    [SerializeField] List<CharacterInfos> asd;

    #region Variable Changed Event
    public delegate void OnVariableChangeDelegate(string newVal);
    public static event OnVariableChangeDelegate OnVariableChange;
    #endregion

    private void Start()
    {
        if (GameManager.CurrentGameState == GameState.Init)
        {
            nextBtn.onClick.AddListener(() => NextButton());
            previousBtn.onClick.AddListener(() => PreviousButton());
        }
    }

    void Update()
    {
        if (levelText != null)
            levelText.text = "Lv" + " " + Player.level.ToString();
        if (bar != null)
            bar.fillAmount = Player.GetExperienceNormalized();
    }

    private void NextButton()
    {
        if (currentIndex < asd.Count - 1)
        {
            currentIndex++;
            currentCharName = asd[currentIndex].charName;

            if (OnVariableChange != null)
                OnVariableChange(currentCharName);

            var temp = Camera.main.transform;
            Camera.main.transform.position = new Vector3(temp.position.x - offset, temp.position.y, temp.position.z);

            if (currentIndex == asd.Count - 1) nextBtn.interactable = false;
            if (!previousBtn.interactable) previousBtn.interactable = true;
        }
    }

    private void PreviousButton()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            currentCharName = asd[currentIndex].charName;

            if (OnVariableChange != null)
                OnVariableChange(currentCharName);

            var temp = Camera.main.transform;
            Camera.main.transform.position = new Vector3(temp.position.x + offset, temp.position.y, temp.position.z);

            if (currentIndex == 0) previousBtn.interactable = false;
            if (!nextBtn.interactable) nextBtn.interactable = true;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Space]
    [SerializeField] Image bar;
    [SerializeField] TextMeshProUGUI levelText;
    
    [Space]
    [SerializeField] Button nextBtn, previousBtn;

    [SerializeField] GameObject[] models;

    private int currentIndex = 0;

    private float offset = 7.5f;

    private void Start()
    {
        nextBtn.onClick.AddListener(() => NextButton());
        previousBtn.onClick.AddListener(() => PreviousButton());
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
        if (currentIndex < models.Length - 1)
        {
            currentIndex++;

            var temp = Camera.main.transform;
            Camera.main.transform.position = new Vector3(temp.position.x - offset, temp.position.y, temp.position.z);

            if (currentIndex == models.Length - 1) nextBtn.interactable = false;
            if (!previousBtn.interactable) previousBtn.interactable = true;

        }
    }

    private void PreviousButton()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            if (currentIndex == 0) previousBtn.interactable = false;
            if (!nextBtn.interactable) nextBtn.interactable = true;

            var temp = Camera.main.transform;
            Camera.main.transform.position = new Vector3(temp.position.x + offset, temp.position.y, temp.position.z);
        }
    }
}

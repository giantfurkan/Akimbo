using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] Image bar;
    [SerializeField] TextMeshProUGUI levelText;

    void Update()
    {
        levelText.text = "Lv" + " " + Player.level.ToString();
        bar.fillAmount = Player.GetExperienceNormalized();
    }
}

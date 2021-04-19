using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] Image bar;
    [SerializeField] TextMeshProUGUI levelText;

    private void OnEnable()
    {
        Player.onLevelUp += SetActiveTrue;
    }
   private void OnDisable()
    {
        Player.onLevelUp -= SetActiveTrue;
    }
    private void Start()
    {
  
    }
    void Update()
    {
        levelText.text = "Lv" + " " + Player.level.ToString();
        bar.fillAmount = Player.GetExperienceNormalized();
    }

    void SetActiveTrue()
    {
     
    }

}

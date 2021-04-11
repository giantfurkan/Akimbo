using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] Image bar;
    Entity entity;

    Quaternion iniRot;

    private void Awake()
    {
        iniRot = transform.rotation;
        entity = GetComponentInParent<Entity>();
    }
    private void Update()
    {
        bar.fillAmount = Mathf.Lerp(bar.fillAmount, entity.Hp / entity.MaxHp, 0.2f);
    }
    private void LateUpdate()
    {
        transform.rotation = iniRot;
    }
}
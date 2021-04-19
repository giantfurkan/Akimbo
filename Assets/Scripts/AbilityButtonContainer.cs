using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using DefaultNamespace.Managers;
using UnityEngine;
using UnityEngine.UI;

public class AbilityButtonContainer : MonoBehaviour
{
   [HideInInspector]public AbilityData abilityData;

   public Image myImage;

   public Text myName;

   public Text valueText;

   public void SetButton(AbilityData target)
   {
      abilityData = target;
      myImage.sprite = abilityData.abilitySprite;
      myName.text = abilityData.name;
      valueText.text = abilityData.value.ToString();
   }

   public void OnButtonDown()
   {
      AbilityManager.manager.player.UseAbility(abilityData);
      AbilityManager.manager.ClearAbilities();
   }
}

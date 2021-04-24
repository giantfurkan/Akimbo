using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    [CreateAssetMenu(fileName = "Ability", menuName = "Ability", order = 0)]
    public class AbilitySO : ScriptableObject
    {
        public List<AbilityData> abilityDataList;
    }
    [Serializable]
    public class AbilityData
    {
        public string name;
        public enum AbilityType
        {
            AttackSpeed,
            AttackDamage,
            Health,
            MoveSpeed,
            Fire,
            Electric,
            Poison
        }

        public AbilityType myType;
        public float value;
        public Sprite abilitySprite;
        public GameObject ammo;

    }
}
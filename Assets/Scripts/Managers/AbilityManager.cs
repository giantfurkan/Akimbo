using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace.Managers
{
    public class AbilityManager : MonoBehaviour
    {
        public static AbilityManager manager;

        public AbilitySO abilitySo;

        public AbilityButtonContainer abilityPrefab;

        public Transform abilityParent;

        private List<AbilityData> _builtAbility = new List<AbilityData>();

        public Player player;
        private void Awake()
        {
            manager = this;
        }

        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            player = GameManager.Clone;
        }
        
        public void BuildRandomAbility()
        {
            var selectedAbility = abilitySo.abilityDataList[Random.Range(0, abilitySo.abilityDataList.Count)];
            while (_builtAbility.Contains(selectedAbility))
            {
                if (_builtAbility.Count>=abilitySo.abilityDataList.Count)
                {
                    break;
                }
                selectedAbility = abilitySo.abilityDataList[Random.Range(0, abilitySo.abilityDataList.Count)];
            }
            _builtAbility.Add(selectedAbility);
            var clone = Instantiate(abilityPrefab, abilityParent);
            clone.SetButton(selectedAbility);
        }

        public void ClearAbilities()
        {
            var children = abilityParent.transform.GetComponentsInChildren<AbilityButtonContainer>();
            foreach (AbilityButtonContainer child in children)
            {
                Destroy(child.gameObject);
            }
            _builtAbility?.Clear();
        }
    }
}
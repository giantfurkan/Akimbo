using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace.Managers
{
    public class AbilityManager : MonoBehaviour
    {
        public delegate void OnAbilitySelect();
        public static event OnAbilitySelect onAbilitySelect;

        public static AbilityManager manager;

        public AbilitySO abilitySo;

        public AbilityButtonContainer abilityPrefab;

        public Transform abilityParent;

        public GameObject abilityCanvas;
        public Animator anim;

        private List<AbilityData> _builtAbility = new List<AbilityData>();

        public Player player;
        private void Awake()
        {
            manager = this;
            abilityCanvas.SetActive(false);
            anim = abilityCanvas.GetComponent<Animator>();
        }

        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            player = GameManager.Clone;
        }

        public void BuildRandomAbility()
        {
            StartCoroutine(ChoseAbility());
        }

        IEnumerator ChoseAbility()
        {
            abilityCanvas.SetActive(true);
            yield return new WaitForSeconds(.66f);
            for (int i = 0; i < 3; i++)
            {
                var selectedAbility = abilitySo.abilityDataList[Random.Range(0, abilitySo.abilityDataList.Count)];
                while (_builtAbility.Contains(selectedAbility))
                {
                    if (_builtAbility.Count >= abilitySo.abilityDataList.Count)
                    {
                        break;
                    }
                    selectedAbility = abilitySo.abilityDataList[Random.Range(0, abilitySo.abilityDataList.Count)];
                }
                _builtAbility.Add(selectedAbility);
                var clone = Instantiate(abilityPrefab, abilityParent);
                clone.SetButton(selectedAbility);
            }
        }

        IEnumerator CloseAbilityCanvas()
        {
            anim.SetTrigger("Close");
            yield return new WaitForSeconds(1f);
            abilityCanvas.SetActive(false);
            onAbilitySelect?.Invoke();
        }

        public void ClearAbilities()
        {
            StartCoroutine(CloseAbilityCanvas());
            var children = abilityParent.transform.GetComponentsInChildren<AbilityButtonContainer>();
            foreach (AbilityButtonContainer child in children)
            {
                Destroy(child.gameObject);
            }
            _builtAbility?.Clear();
        }


    }
}
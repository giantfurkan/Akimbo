using System;
using UnityEngine;
using UnityEngine.AI;

namespace DefaultNamespace
{       
    
    public class PetController : MonoBehaviour
    {
        [HideInInspector]public Player targetPlayer;

        public NavMeshAgent myAgent;

        
        private void Update()
        {
            
            myAgent.SetDestination(targetPlayer.transform.position);
        }
    }
}
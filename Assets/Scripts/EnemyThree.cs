using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyThree : RandomWalkingEnemy
{
    private new void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        if (!agent.hasPath)
        {
            agent.SetDestination(GetRandomPoint(enemyHandler, transform, radius));
        }
    }


}

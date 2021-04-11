using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : Shell
{
    protected override void OnObstacleCollision(Transform obstacle) { }

    protected override void OnEnemyCollision(Entity entity)
    {
        shooter.DeactiveShell(gameObject);
    }

    protected override void OnPlayerCollision(Entity entity)
    {
        entity.TakeDamage(damageReport);
        shooter.DeactiveShell(gameObject);
    }

}

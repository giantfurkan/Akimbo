using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : Shell
{
    public GameObject effect;

    protected override void OnEnemyCollision(Entity entity)
    {
    }

    protected override void OnObstacleCollision(Transform obstacle)
    {
        shooter.DeactiveShell(gameObject);
    }

    protected override void OnPlayerCollision(Entity entity)
    {
        entity.TakeDamage(damageReport);
        Instantiate(effect, transform.position, Quaternion.identity);
        shooter.DeactiveShell(gameObject);
    }

}

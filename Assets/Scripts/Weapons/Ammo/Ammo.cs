using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo : Shell
{
    [SerializeField] private GameObject effect;
    protected override void OnEnemyCollision(Entity entity)
    {
        entity.TakeDamage(damageReport);
        Instantiate(effect, transform.position, Quaternion.identity);
        shooter.DeactiveShell(gameObject);
    }

    protected override void OnObstacleCollision(Transform obstacle)
    {
        GetComponent<Collider>().enabled = false;
        rigidbody.velocity = Vector3.zero;
        flyingState = FlyingState.Stuck;
        stuckTime = Time.time;
    }

    private void FixedUpdate()
    {
        if (flyingState == FlyingState.Stuck && Time.time - stuckTime >= timeToDissapear)
        {
            flyingState = FlyingState.Flying;
            shooter.DeactiveShell(gameObject);
        }
    }

    protected override void OnPlayerCollision(Entity entity) { }

}



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class Shell : MonoBehaviour
{
    protected const float timeToDissapear = 2f;

    protected enum FlyingState
    {
        Flying,
        Stuck
    }

    [SerializeField] protected float speed = 1;
    [SerializeField] protected Rigidbody rigidbody;
    protected DamageReport damageReport;
    protected Shooter shooter;
    protected FlyingState flyingState = FlyingState.Flying;
    protected float stuckTime; // Time, that represents Time.time when shell stuck in wall

    private void Awake()
    {
        if (rigidbody == null)
            rigidbody = GetComponent<Rigidbody>();

        speed = Random.Range(speed - .2f, speed + .7f);
    }

    public void Shoot(Shooter shooter, DamageReport damageReport)
    {
        this.damageReport = damageReport;
        this.shooter = shooter;
        rigidbody.velocity = transform.forward * speed;
    }

    public void DeactiveShell()
    {
        shooter.DeactiveShell(gameObject);
    }

    protected void OnTriggerEnter(Collider other)
    {
        string tag = other.tag;
        switch (tag)
        {
            default:
            case Tags.obstacleTag:
                OnObstacleCollision(other.transform);
                break;
            case Tags.enemyTag:
                OnEnemyCollision(other.GetComponent<Entity>());
                break;
            case Tags.playerTag:
                OnPlayerCollision(other.GetComponent<Entity>());
                break;
        }
    }
    protected abstract void OnObstacleCollision(Transform obstacle);
    protected abstract void OnEnemyCollision(Entity entity);
    protected abstract void OnPlayerCollision(Entity entity);
}

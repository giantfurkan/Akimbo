using UnityEngine;

public class EnemyTwo : WalkingEnemy
{
    EnemyAimer aimer;
    Shooter shooter;
    [SerializeField] private float lastShootTime;

    private new void Awake()
    {
        base.Awake();

        attackSpeed = Random.Range(attackSpeed - .25f, attackSpeed + .25f);

        if (aimer == null)
            aimer = GetComponentInChildren<EnemyAimer>();
        if (shooter == null)
            shooter = GetComponentInChildren<Shooter>();
    }

    private void Update()
    {
        if (aimer.Target != null)
        {
            walkingState = MovingState.Staying;
            aimer.FollowTarget();

            if (Time.time - lastShootTime >= (1 / attackSpeed))
            {
                lastShootTime = Time.time;
                shooter.Shoot(new DamageReport(damage, this));
            }
        }
    }

    protected new void FixedUpdate()
    {
        base.FixedUpdate();
        if (walkingState == MovingState.Staying)
        {
            if (aimer.Target == null)
                aimer.Aim();
            else if (!aimer.IsVisible())
                aimer.ResetTarget();
        }
    }
}

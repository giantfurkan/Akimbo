using UnityEngine;
using UnityEngine.Events;

public class Archer : Player
{
    Shooter shooter;
    float lastShootTime;

    private new void Awake()
    {
        base.Awake();

        if (shooter == null)
        {
            shooter = GetComponentInChildren<Shooter>();
        }
    }

    private new void Update()
    {
        base.Update();

        if (walkingState == MovingState.Staying)
        {
            if (aimer.Target != null)
            {
                aimer.FollowTarget();
                if (Time.time - lastShootTime >= (1 / attackSpeed))
                {
                    lastShootTime = Time.time;
                    shooter.Shoot(new DamageReport(damage, this));
                }
            }
        }
        else
        {
            if (aimer.Target != null)
            {
                aimer.ResetTarget();
            }
        }
    }
}


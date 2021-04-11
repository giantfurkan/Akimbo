using UnityEngine;

public class Gunner : Player
{
    public static bool inRange;

    [SerializeField] float range;
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

        if (aimer.Target != null && aimer.DistanceToTarget(aimer.Target) < range)
        {
            aimer.FollowTarget();
            inRange = true;
            if (Time.time - lastShootTime >= (1 / attackSpeed))
            {
                lastShootTime = Time.time;
                shooter.Shoot(new DamageReport(damage, this));
            }
        }

        else if (aimer.Target != null && aimer.DistanceToTarget(aimer.Target) > range + 1 || aimer.Target == null || !aimer.IsVisible())
        {
            inRange = false;
        }


    }
}

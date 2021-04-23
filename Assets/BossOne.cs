using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossOne : MultiShooterBoss
{
    private void Update()
    {
        if (aimer.Target != null)
        {
            aimer.FollowTarget();

            if (Time.time - shootTimer >= reloadTimer)
            {
                foreach (Shooter shooters in shooter)
                {
                    shooters.Shoot(new DamageReport(damage, this));
                    shootTimer = Time.time;
                }
            }
        }
    }
}


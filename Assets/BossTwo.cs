using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTwo : MultiShooterBoss
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
                    shooters.transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y + Random.Range(100f, 260f), transform.rotation.z);
                }
                shootTimer = Time.time;
            }
        }
    }
}


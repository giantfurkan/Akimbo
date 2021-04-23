using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFour : Enemy
{
    [SerializeField] List<Shooter> shooter;
    [SerializeField] float reloadTimer = 1;
    float shootTimer;

    private new void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        if (Time.time - shootTimer >= reloadTimer)
        {
            foreach (Shooter shooters in shooter)
            {
                shooters.Shoot(new DamageReport(damage, this));
            }
            shootTimer = Time.time;
        }
    }
}

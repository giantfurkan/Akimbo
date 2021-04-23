using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossOne : Enemy
{
    [SerializeField] List<Shooter> shooter;
    [SerializeField] EnemyAimer aimer;

    [SerializeField] float reloadTimer = 1;
    float shootTimer;

    private new void Awake()
    {
        base.Awake();

        if (aimer == null)
            aimer = GetComponentInChildren<EnemyAimer>();
    }

    private void Start()
    {
        transform.position = Vector3.zero;
    }

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
                }
                shootTimer = Time.time;
            }
        }
    }

    protected void FixedUpdate()
    {
        if (aimer.Target == null)
            aimer.Aim();
        else if (!aimer.IsVisible())
            aimer.ResetTarget();
        aimer.FollowTarget();
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiShooterBoss : Enemy
{
    [SerializeField] protected List<Shooter> shooter;
    [SerializeField] protected EnemyAimer aimer;

    [SerializeField] protected float reloadTimer = 1;
    protected float shootTimer;

    protected new void Awake()
    {
        base.Awake();

        if (aimer == null)
            aimer = GetComponentInChildren<EnemyAimer>();
    }

    protected void Start()
    {
        transform.position = Vector3.zero;
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


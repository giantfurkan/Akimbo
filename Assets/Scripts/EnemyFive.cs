using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFive : RandomWalkingEnemy
{
    EnemyAimer aimer;
    Shooter shooter;

    private float shootTime;
    private float reloadTime;
    private float respawnTime;

    [SerializeField] GameObject shadow;

    private new void Awake()
    {
        base.Awake();

        if (shooter == null)
            shooter = GetComponentInChildren<Shooter>();
        if (aimer == null)
            aimer = GetComponentInChildren<EnemyAimer>();

        StartCoroutine(Cycle());
    }
    IEnumerator Cycle()
    {
        while (hp > 0)
        {
            yield return new WaitForSeconds(Random.Range(.5f, 1f));
            if (aimer.Target != null)
            {
                shooter.Shoot(new DamageReport(damage, this));
            }
            yield return new WaitForSeconds(Random.Range(2,2.5f));
            Vector3 newPos = GetRandomPoint(transform, radius);
            var asd = Instantiate(shadow);
            asd.transform.position = newPos;
            yield return new WaitForSeconds(Random.Range(.5f,1f));
            Destroy(asd);
            transform.position = newPos;
        }
    }

    protected void FixedUpdate()
    {
        if (aimer.Target == null)
        {
            aimer.Aim();
        }
        else if (!aimer.IsVisible())
            aimer.ResetTarget();
        aimer.FollowTarget();

    }
}

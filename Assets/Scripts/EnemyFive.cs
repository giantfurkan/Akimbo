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
    GameObject temp;

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
            yield return new WaitForSeconds(Random.Range(.25f, .5f));
            if (aimer.Target != null)
            {
                shooter.Shoot(new DamageReport(damage, this));
            }

            yield return new WaitForSeconds(Random.Range(1.2f, 2f));
            Vector3 newPos = GetRandomPoint(enemyHandler, transform, 12);
            temp = Instantiate(shadow);
            temp.transform.position = newPos;

            yield return new WaitForSeconds(Random.Range(.6f, 1.1f));
            Destroy(temp);
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

    private void OnDestroy()
    {
        Destroy(temp);
    }
}

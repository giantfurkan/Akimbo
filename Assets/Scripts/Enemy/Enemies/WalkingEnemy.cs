using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class WalkingEnemy : Enemy
{
    [SerializeField] float movingStateTimer = 0;
    [SerializeField] NavMeshAgent agent;

    protected new void Awake()
    {
        base.Awake();
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        agent.speed = Speed;
    }

    protected void FixedUpdate()
    {
        switch (walkingState)
        {
            case MovingState.Moving:
                if (Time.time - movingStateTimer >= movingTime)
                {
                    walkingState = MovingState.Staying;
                    movingStateTimer = Time.time;
                }
                else
                {
                    agent.destination = player.transform.position;
                }
                break;
            case MovingState.Staying:
                if (Time.time - movingStateTimer >= waitingTime)
                {
                    if (touchingPlayer != null)
                    {
                        if (touchingPlayer.TakeDamage(new DamageReport(damage, this)))
                        {
                            touchingPlayer = null;
                        }
                        movingStateTimer = Time.time;
                    }
                    else
                    {
                        walkingState = MovingState.Moving;
                        movingStateTimer = Time.time + Random.Range(0, randomTime);
                    }
                }
                else
                {
                    agent.destination = transform.position;
                }
                break;
            default:
                break;
        }
    }

    protected new void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        if (other.tag == Tags.playerTag)
        {
            walkingState = MovingState.Staying;
            movingStateTimer = Time.time;
        }
    }
}

using UnityEngine;
using UnityEngine.AI;

public class RandomWalkingEnemy : Enemy
{
    [SerializeField] private protected float radius;
    private protected NavMeshAgent agent;

    protected new void Awake()
    {
        base.Awake();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        agent.speed = Speed;
    }

    protected bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        result = Vector3.zero;

        return false;
    }

    protected Vector3 GetRandomPoint(Transform point, float radius)
    {
        Vector3 _point;

        if (RandomPoint(point.position, radius, out _point))
        {
            return _point;
        }

        return point == null ? Vector3.zero : point.position;
    }
    
}
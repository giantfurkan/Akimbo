using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Aimer : MonoBehaviour
{
    private float distanceToTarget;

    [SerializeField] private Transform target;
    [SerializeField] private LayerMask obstacleMask;

    protected float minDistance;

    GameObject owner;

    public Transform Target
    {
        get { return target; }
        set { target = value; }
    }

    public float DistanceToTarget(Transform target)
    {
        return distanceToTarget = Vector3.Distance(transform.position, target.position);
    }

    protected bool IsVisible(Transform target)
    {
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (minDistance == -1 || minDistance > distanceToTarget)
        {
            if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask))
            {
                minDistance = distanceToTarget;
                this.target = target;
                return true;
            }
        }
        return false;
    }

    public bool IsVisible()
    {
        Vector3 directionToTarget = (target.position - transform.position.normalized);
        distanceToTarget = Vector3.Distance(transform.position, target.position);
        return !Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask);
    }

    public void ResetTarget()
    {
        target = null;
    }

    public void FollowTarget()
    {
        if (owner == null)
            owner = transform.parent.gameObject;
        owner.transform.LookAt(target);
    }

}

using UnityEngine;

public class PlayerAimer : Aimer
{
    [SerializeField] EnemyHandler enemyHandler;

    private void Start()
    {
        enemyHandler = FindObjectOfType<EnemyHandler>();
    }

    public bool Aim()
    {
        if (enemyHandler == null)
        {
            enemyHandler = FindObjectOfType<EnemyHandler>();
        }
        bool success = false;
        minDistance = -1;
        foreach (var enemies in enemyHandler.Enemies)
        {
            success = IsVisible(enemies.transform);
        }
        return success;
    }

    public Transform ClosestTarget()
    {
        Transform closestTarget = null;

        float minDistance = float.MaxValue;
        for (int i = 0; i < enemyHandler.Enemies.Count; i++)
        {
            if (DistanceToTarget(enemyHandler.Enemies[i].transform) < minDistance)
            {
                minDistance = DistanceToTarget(enemyHandler.Enemies[i].transform);
                closestTarget = enemyHandler.Enemies[i].transform;
                IsVisible(enemyHandler.Enemies[i].transform);
            }
        }
        return closestTarget;
    }
}

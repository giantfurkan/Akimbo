using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyHandler))]
public class EnemySpawner : MonoBehaviour
{
    private const float overlapSphereRadius = 1f;

    private EnemyHandler enemyHandler;
    
    private void Awake()
    {
        if(enemyHandler == null)
        {
            enemyHandler = GetComponent<EnemyHandler>();
        }
    }

    public void SpawnEnemies(LevelInfo levelInfo)
    {
        if(levelInfo.enemies.Count == 0)
        {
            throw new System.ArgumentNullException("enemies list are null");
        }
        for(int i = 0; i < levelInfo.enemyCount; i++)
        {
            Vector3 spawnPos;
            int loopBreaker = 0;

            do
            {
                loopBreaker++;
                spawnPos = new Vector3(Random.Range(-4.5f, 4.5f), 0f, Random.Range(-2f, 9.5f)); // If each level has a different scale, variables can take from SO or Prefabs
            } while (CheckCollisions(spawnPos) && loopBreaker < 100); // If there is no spawn position, it maybe try to endless forever

            if(loopBreaker < 100)
            {
                var newEnemy = Instantiate(levelInfo.enemies[Random.Range(0, levelInfo.enemies.Count)], transform);
                newEnemy.transform.position = spawnPos;
                enemyHandler.AddEnemy(newEnemy.GetComponent<Enemy>());
            }
        }
    }

    private bool CheckCollisions(Vector3 pos)
    {
        Collider[] hitColliders = Physics.OverlapSphere(pos, overlapSphereRadius);
        return hitColliders.Length > 0;
    }

    private bool CheckDist(Vector3 pos)
    { 
        return true;
    }
}

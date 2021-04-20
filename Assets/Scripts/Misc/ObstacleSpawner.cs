using UnityEngine;
using UnityEngine.AI;

public class ObstacleSpawner : MonoBehaviour
{
    public void SpawnObstacles(LevelInfo levelInfo)
    {
       GameObject obstaclesParent= Instantiate(levelInfo.obstacleDesign, transform);
       var obstaclesP=obstaclesParent.GetComponentsInChildren<NavMeshObstacle>();
       foreach (var obstacle in obstaclesP)
       {
           obstacle.carving = true;
       }
    }
}

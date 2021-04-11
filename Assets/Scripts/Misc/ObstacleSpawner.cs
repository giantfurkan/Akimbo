using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public void SpawnObstacles(LevelInfo levelInfo)
    {
        Instantiate(levelInfo.obstacleDesign, transform);
    }
}

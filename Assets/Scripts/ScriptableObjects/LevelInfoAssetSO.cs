using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelInfoAsset", menuName = "Level/LevelInfoAsset")]
public class LevelInfoAssetSO : ScriptableObject
{
    [Space]
    public List<LevelInfo> levelInfos = new List<LevelInfo>();
}

[System.Serializable]
public struct LevelInfo
{
    public int enemyCount;
    public List<Enemy> enemies;
    public GameObject obstacleDesign;
}
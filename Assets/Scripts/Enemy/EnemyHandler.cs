using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyHandler : MonoBehaviour
{
    [SerializeField] private List<Enemy> enemies;

    public delegate void OnLevelCleared();
    public static event OnLevelCleared levelCleared;

    public List<Enemy> Enemies
    {
        get { return enemies; }
        private set { enemies = value; }
    }
    public void AddEnemy(Enemy enemy)
    {
        enemies.Add(enemy);
    }

    public void RemoveEnemy(Enemy enemy)
    {
        enemies.Remove(enemy);
        if (enemies.Count == 0)
        {
            levelCleared?.Invoke();
        }
    }
}

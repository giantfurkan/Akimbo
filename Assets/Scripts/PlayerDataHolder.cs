using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DefaultNamespace.Managers;
public class PlayerDataHolder : MonoBehaviour
{
    public static float speed;
    public static float maxHp;
    public static float hp;
    public static float attackSpeed;
    public static float damage;
    public static GameObject bulletPrefab;

    private void OnEnable()
    {
        EnemyHandler.levelCleared += SaveData;
        GameManager.onNewLevel += LoadData;
        AbilityManager.onAbilitySelect += SaveData;
    }

    private void OnDisable()
    {
        EnemyHandler.levelCleared -= SaveData;
        AbilityManager.onAbilitySelect -= SaveData;
        GameManager.onNewLevel -= LoadData;
    }

    public void SaveData()
    {
        hp = GameManager.Clone.Hp;
        maxHp = GameManager.Clone.MaxHp;
        speed = GameManager.Clone.Speed;
        attackSpeed = GameManager.Clone.AttackSpeed;
        damage = GameManager.Clone.Damage;
        Debug.Log("saved");
    }

    public void LoadData()
    {
        GameManager.Clone.Hp = hp;
        GameManager.Clone.MaxHp = maxHp;
        GameManager.Clone.Speed = speed;
        GameManager.Clone.AttackSpeed = attackSpeed;
        GameManager.Clone.Damage = damage;
        Debug.Log("loaded");
    }
}
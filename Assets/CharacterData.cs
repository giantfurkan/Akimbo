using UnityEngine;

[CreateAssetMenu]
public class CharacterData : ScriptableObject
{
    public CharacterInfo[] charInfos;
}

[System.Serializable]
public class CharacterInfo
{
    public int index;
    public GameObject model;
    public string charName;
    public float speed;
    public float maxHp;
    public float hp;
    public float attackSpeed;
    public float damage;
}


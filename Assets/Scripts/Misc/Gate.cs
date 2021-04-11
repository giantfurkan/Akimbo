using System;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
    [SerializeField]List<GameObject> gateDoor;

    public delegate void NextLevel();
    public static event NextLevel handleNextLevel;

    private void OnEnable()
    {
        EnemyHandler.levelCleared += GateOpen;
    }

    private void OnDisable()
    {
        EnemyHandler.levelCleared -= GateOpen;
    }
    private void Awake()
    {
        foreach(Transform child in transform)
        {
            gateDoor.Add(child.gameObject);
        }
    }
    private void GateOpen()
    {
       foreach(var door in gateDoor)
        {
            door.SetActive(false);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
            handleNextLevel?.Invoke();

    }
}

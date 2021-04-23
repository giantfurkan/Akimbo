using System;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
    public delegate void NextLevel();
    public static event NextLevel handleNextLevel;

    Collider col;
    Animator anim;

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
        col = GetComponent<Collider>();
        anim = GetComponentInChildren<Animator>();
    }

    private void GateOpen()
    {
        col.isTrigger = true;
        anim.SetTrigger("GateOpen");
    }

    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
            handleNextLevel?.Invoke();
    }
}

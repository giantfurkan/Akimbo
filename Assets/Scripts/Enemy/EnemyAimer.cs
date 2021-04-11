using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAimer : Aimer
{
    Player player;
    private void Awake()
    {
        player = Player.instance;
    }
    public bool Aim()
    {
        bool success = false;
        minDistance = -1;
        success = IsVisible(player.transform);
        return success;
    }
}

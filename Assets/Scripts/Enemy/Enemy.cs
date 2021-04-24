using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity
{
    [SerializeField] protected Player player;
    [SerializeField] protected EnemyHandler enemyHandler;
    [SerializeField] protected float movingTime;
    [SerializeField] protected float waitingTime;
    [SerializeField] protected float randomTime;
    [SerializeField] protected int coinsToDrop = 100;
    [SerializeField] protected int experienceDrop = 100;
    protected SpriteRenderer aimSprite;

    protected Player touchingPlayer;

    protected new void Awake()
    {
        base.Awake();

        hp = maxHp;
        player = GameManager.Clone;
        enemyHandler = FindObjectOfType<EnemyHandler>();
    }

    protected override void Death(Entity killer)
    {
        Player player = killer as Player;
        if (player != null)
        {
            player.AddCoins(coinsToDrop);
        }
        enemyHandler.RemoveEnemy(this);
        Destroy(gameObject);

        player.AddExp(experienceDrop);
    }

    private void ResetTouchingPlayer()
    {
        touchingPlayer = null;
    }

    protected void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<Player>();
        if (player != null)
        {
            touchingPlayer = player;
            player.TakeDamage(new DamageReport(damage, this));
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        if (other.tag == Tags.playerTag)
        {
            ResetTouchingPlayer();
        }
    }
}

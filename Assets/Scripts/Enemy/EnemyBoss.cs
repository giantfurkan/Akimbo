using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyBoss : MonoBehaviour
{
    private Player player;
    private float _attackTimer;
    
    public GameObject bulletPrefab;
    public int multiShot = 1;
    public float attackRate=.5f;
    
    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        
        player =FindObjectOfType<Player>();
        
    }

    

    private void FixedUpdate()
    {
        FireJelly();
        
    }

    
    
    private void Update()
    {
        transform.rotation=Quaternion.LookRotation(transform.position-player.transform.position);
    }

    private void FireJelly()
    {
        _attackTimer += Time.fixedDeltaTime * attackRate;
        if (_attackTimer >= 1f)
        {
            _attackTimer = 0f;

            for (int i = 0; i < multiShot; i++)
            {

                var bullet = Instantiate(this.bulletPrefab, transform.position, Quaternion.identity); // todo buraya bullet koy 

                var fireIndex = (i * Mathf.Pow(-1, i)) / 2;
                fireIndex = Mathf.FloorToInt(fireIndex);
                var fireDir = Quaternion.Euler(0, 20 * fireIndex, 0) * transform.forward;
                bullet.transform.rotation = Quaternion.LookRotation(fireDir);
            }

        }
    }
}

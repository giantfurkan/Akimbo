using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    public GameObject shellPrefab;
    [SerializeField] int poolSize = 20;
    Queue<GameObject> pool;

    private void Awake()
    {
        pool = new Queue<GameObject>(poolSize);
        ResizePool(poolSize);
    }

    #region Pool
    public void ResizePool(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            var obj = Instantiate(shellPrefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public void Dispose()
    {
        while(pool.Count > 0 )
        {
            Destroy(pool.Dequeue());
        }
    }
    #endregion

    public void DeactiveShell(GameObject shell)
    {
        shell.SetActive(false);
        shell.GetComponent<Collider>().enabled = true;
        try
        {
            shell.transform.position = transform.position;
            pool.Enqueue(shell);
        }
        catch
        {
            Destroy(shell);
        }
    }

    public void Shoot(DamageReport damageReport)
    {
        var newShell = pool.Dequeue();
        newShell.transform.position = transform.position;
        newShell.transform.rotation = transform.rotation;
        newShell.SetActive(true);
        newShell.GetComponent<Shell>().Shoot(this, damageReport);
        if(pool.Count <= 1)
        {
            ResizePool(poolSize);
        }
    }

    public void Shoot(DamageReport damageReport,Gunner target)
    {
        Shoot(damageReport);
        target.IncreaseBonus();
    }
}

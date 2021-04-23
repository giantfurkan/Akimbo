using UnityEngine;

public class Gunner : Player
{
    public static bool inRange;

    [SerializeField] float range;
    Shooter shooter;

    float lastShootTime;

    [Header("Gunner Bonus Radius")]
    [HideInInspector] public float bonusRadius;
    public SpriteRenderer bonusRadiusSprite;
    public float maxRadius;
    public float increaseValuePerBullet;
    public float maxSpriteScale;

    private new void Awake()
    {
        base.Awake();

        if (shooter == null)
        {
            shooter = GetComponentInChildren<Shooter>();
        }
    }

    private new void Update()
    {
        base.Update();

        if (aimer.Target != null && aimer.DistanceToTarget(aimer.Target) < range)
        {
            aimer.FollowTarget();
            inRange = true;
            if (Time.time - lastShootTime >= (1 / attackSpeed))
            {
                lastShootTime = Time.time;
                shooter.Shoot(new DamageReport(Random.Range(damage-5,Damage+5), this), this);
            }
        }

        else if (aimer.Target != null && aimer.DistanceToTarget(aimer.Target) > range + 1 || aimer.Target == null || !aimer.IsVisible())
        {
            inRange = false;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (bonusRadius > .5)
            {
                UseBonus();
                bonusRadius = 0;
                bonusRadiusSprite.transform.localScale = new Vector3(.25f, .25f, .25f);
            }
        }
    }

    public void IncreaseBonus()
    {
        if (walkingState == MovingState.Moving)
        {
            bonusRadius += increaseValuePerBullet;
            if (bonusRadius > maxRadius)
            {
                bonusRadius = maxRadius;
            }
            var inverseValue = Mathf.InverseLerp(0, maxRadius, bonusRadius);
            bonusRadiusSprite.transform.localScale = Mathf.Lerp(0, maxSpriteScale, inverseValue) * Vector3.one;
        }
    }

    //todo elini cektigin yere yada kullanmak istedigin yerde bir seferligine calistir. UseBonus();
    public void OnDrawGizmos()
    {
        var center = transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, bonusRadius);
    }

    public void UseBonus()
    {
        var center = transform.position;
        var RaycastHit = Physics.SphereCastAll(center, bonusRadius, Vector3.one, 100);

        foreach (RaycastHit hit in RaycastHit)
        {
            var bullet = hit.collider.GetComponent<Bullet>(); //todo kendi mermin yok olursa buraya bak
            if (bullet)
            {
                Destroy(bullet.gameObject);
                Instantiate(bullet.effect, hit.collider.transform.position, Quaternion.identity);
            }
        }
    }


}

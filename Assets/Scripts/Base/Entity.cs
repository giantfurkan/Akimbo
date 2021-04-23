using UnityEngine;

public abstract class Entity : MonoBehaviour
{
   protected enum MovingState
    {
        Moving,
        Staying
    }

    [SerializeField] protected float speed;
    [SerializeField] protected float maxHp;
    [SerializeField] protected float hp;
    [SerializeField] protected float attackSpeed;
    [SerializeField] protected float damage;

    [SerializeField] GameObject damagePopUp;

    protected MovingState walkingState = MovingState.Staying;

    public float Speed
    {
        get { return speed; }
        set { speed = value; }
    }
    public float MaxHp
    {
        get { return maxHp; }
        set { maxHp = value; }
    }
    public float Hp
    {
        get { return hp; }
        set { hp=value; }
    }
    public float AttackSpeed
    {
        get { return attackSpeed; }
        set { attackSpeed = value; }
    }
    public float Damage
    {
        get { return damage; }
        set { damage = value; }
    }
    protected void Awake()
    {
        damagePopUp = Resources.Load<GameObject>("DamagaPopUp");
    }

    public bool TakeDamage(DamageReport damageReport)
    {
        hp -= damageReport.damage;

        if (hp <= 0)
        {
            Death(damageReport.attacker);
            return true;
        }

        DamageIndicator indicator = Instantiate(damagePopUp, transform.position, Quaternion.identity).GetComponent<DamageIndicator>();
        indicator.SetDamageText((int)damageReport.damage);

        return false;
    }

    public void fillHp()
    {
        hp = MaxHp;
    }

    protected abstract void Death(Entity killer);
}

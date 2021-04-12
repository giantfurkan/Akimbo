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
    protected MovingState walkingState = MovingState.Staying;

    public float Speed
    {
        get { return speed; }
    }
    public float MaxHp
    {
        get { return maxHp; }
    }
    public float Hp
    {
        get { return hp; }
    }
    public float AttackSpeed
    {
        get { return attackSpeed; }
    }
    public float Damage
    {
        get { return damage; }
    }
    protected void Awake()
    {
        hp = MaxHp;
    }

    public bool TakeDamage(DamageReport damageReport)
    {
        hp -= damageReport.damage;
        if (hp <= 0)
        {
            Death(damageReport.attacker);
            return true;
        }
        return false;
    }

    protected abstract void Death(Entity killer);
}
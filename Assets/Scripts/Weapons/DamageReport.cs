[System.Serializable]
public struct DamageReport 
{
    public float damage;
    public Entity attacker;

    public DamageReport(float damage, Entity attacker)
    {
        this.damage = damage;
        this.attacker = attacker;
    }
}

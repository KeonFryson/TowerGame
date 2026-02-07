using UnityEngine;

public class ProjectileEffects
{
    private readonly float damage;
    private readonly Transform projectileTransform;

    public ProjectileEffects(float damage, Transform projectileTransform)
    {
        this.damage = damage;
        this.projectileTransform = projectileTransform;
    }

    public void ApplyHeavyImpact(Enemy enemy)
    {
        if (enemy != null)
            enemy.ApplyPushback((enemy.transform.position - projectileTransform.position).normalized, 2f);
    }

    public void ApplyArmorBreak(Enemy enemy, int amount)
    {
        if (enemy != null)
            enemy.ApplyArmorBreak(amount, 3f);
    }

    public void ApplySlow(Enemy enemy, float slowAmount, float duration)
    {
        if (enemy != null)
            enemy.ApplySlow(slowAmount, duration);
    }

    public void ApplyAoESlow(Vector3 position, float slowAmount, float duration, float radius)
    {
        Enemy.ApplyAoEEffect(position, radius, e => e.ApplySlow(slowAmount, duration));
    }

    public void ApplyStun(Enemy enemy, float duration)
    {
        if (enemy != null)
            enemy.ApplyStun(duration);
    }

    public void ApplyAoEStun(Vector3 position, float duration, float radius)
    {
        Enemy.ApplyAoEEffect(position, radius, e => e.ApplyStun(duration));
    }

    public void ApplyMicroShrapnel(Vector3 position, float radius, float aoeDamage)
    {
        Enemy.ApplyAoEEffect(position, radius, e => e.TakeDamage(aoeDamage));
         
    }

    public void ApplyPoison(Enemy enemy)
    {
        if (enemy != null)
            enemy.ApplyPoison(damage * 0.1f, 5, 0.5f);
    }

    public void ApplyFire(Enemy enemy)
    {
        if (enemy != null)
            enemy.ApplyBurn(damage * 0.2f, 3, 0.5f);
    }

    public void ApplyFreeze(Enemy enemy)
    {
        if (enemy != null)
            enemy.ApplyFreeze(2f, 0.5f);
    }

    public void ApplyPushback(Enemy enemy)
    {
        if (enemy != null)
            enemy.ApplyPushback((enemy.transform.position - projectileTransform.position).normalized, 1f);
    }
}
using System.Collections.Generic;
using UnityEngine;

public enum ProjectileEffectType
{
    //  Impact Effects
    Impact,         // Basic heavy hit. No special effect.
    HeavyImpact,    // Stronger hit, increased knockback or hit feeling.

    // Damage & Armor Effects
    ArmorBreak1,    // Reduces enemy armor by 1 for a set duration.
    ArmorBreak2,    // Reduces enemy armor by 2 for a set duration.
    Pierce,        // Projectile pierces.

    //  Crowd-Control (CC) Effects
    Slow20_1s,      // Slows enemy by 20% for 1 second.
    AoESlow20_1_5s, // Slows all enemies in small radius by 20% for 1.5 seconds.
    MiniStun_0_2s,  // Stuns hit enemy for 0.2 seconds.
    AoEStun_0_3s,   // Stuns all enemies in radius for 0.3 seconds.

    //  AoE / Utility Effects
    MicroShrapnel,  // Tiny explosion or small particle burst, light AoE damage.
    AoESlow30_2s,   // Slows enemies near impact by 30% for 2 seconds.

    // Legacy/Existing Effects (for compatibility)
    None,
    Poison,
    Fire,
    Freeze,
    Pushback,
    
}

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float maxLifetime = 5f;
    [SerializeField] private bool followTarget = false;

    private Enemy target;
    private float damage;
    private float lifetime = 0f;
    private Vector3 moveDirection;
    private bool lostTarget = false;

    private float sizeMultiplier = 1f;
    [SerializeField] private List<ProjectileEffectType> effectTypes = new List<ProjectileEffectType>(); // Multiple effects
    private Sprite customSprite = null;

    [SerializeField] private int pierceCount = 0; // Number of enemies to pierce through (0 = no pierce)
    private int piercedEnemies = 0;
    private HashSet<Enemy> hitEnemies = new HashSet<Enemy>(); // Track unique enemies hit

    public void SetProjectileSize(float size)
    {
        sizeMultiplier = size;
        transform.localScale = Vector3.one * sizeMultiplier;
        Debug.Log($"Projectile size set to {sizeMultiplier}");
    }

    public void SetProjectileEffects(IEnumerable<ProjectileEffectType> effects)
    {
        effectTypes.Clear();
        if (effects != null)
            effectTypes.AddRange(effects);
    }

    public void AddProjectileEffect(ProjectileEffectType effect)
    {
        if (!effectTypes.Contains(effect) && effect != ProjectileEffectType.None)
            effectTypes.Add(effect);
    }

    public void SetProjectileSprite(Sprite sprite)
    {
        customSprite = sprite;
        if (sprite != null)
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.sprite = sprite;
        }
    }

    public void SetPierceCount(int count)
    {
        pierceCount = count;
    }

    public void SetTarget(Enemy targetEnemy, float damageAmount, bool follow = true)
    {
        target = targetEnemy;
        damage = damageAmount;
        followTarget = follow;

        if (target != null)
            moveDirection = (target.GetPosition() - transform.position).normalized;
        else
            moveDirection = Vector3.right;
    }

    public void SetInitialDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;
    }

    private void Update()
    {
        lifetime += Time.deltaTime;

        if (lifetime >= maxLifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (followTarget && !lostTarget)
        {
            if (target == null)
            {
                lostTarget = true;
            }
            else
            {
                Vector3 direction = (target.GetPosition() - transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                }
                moveDirection = direction;
                transform.position += direction * speed * Time.deltaTime;
                return;
            }
        }

        if (moveDirection != Vector3.zero)
        {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null && !hitEnemies.Contains(enemy))
        {
            hitEnemies.Add(enemy);
            

            foreach (var effectType in effectTypes)
            {
                switch (effectType)
                {
                    // Impact Effects
                    case ProjectileEffectType.Impact:
                        // No special effect
                        break;
                    case ProjectileEffectType.HeavyImpact:
                        ApplyHeavyImpact(enemy);
                        break;

                    // Damage & Armor Effects
                    case ProjectileEffectType.ArmorBreak1:
                        ApplyArmorBreak(enemy, 1);
                        break;
                    case ProjectileEffectType.ArmorBreak2:
                        ApplyArmorBreak(enemy, 2);
                        break;
                    case ProjectileEffectType.Pierce:
                        // Pierce logic handled below
                        break;

                    // Crowd-Control Effects
                    case ProjectileEffectType.Slow20_1s:
                        ApplySlow(enemy, 0.2f, 1f);
                        break;
                    case ProjectileEffectType.AoESlow20_1_5s:
                        ApplyAoESlow(transform.position, 0.2f, 1.5f, 2f);
                        break;
                    case ProjectileEffectType.MiniStun_0_2s:
                        ApplyStun(enemy, 0.2f);
                        break;
                    case ProjectileEffectType.AoEStun_0_3s:
                        ApplyAoEStun(transform.position, 0.3f, 2f);
                        break;

                    // AoE / Utility Effects
                    case ProjectileEffectType.MicroShrapnel:
                        ApplyMicroShrapnel(transform.position, 1f, damage * 0.3f);
                        break;
                    case ProjectileEffectType.AoESlow30_2s:
                        ApplyAoESlow(transform.position, 0.3f, 2f, 2.5f);
                        break;

                    // Legacy/Existing Effects
                    case ProjectileEffectType.Poison:
                        ApplyPoison(enemy);
                        break;
                    case ProjectileEffectType.Fire:
                        ApplyFire(enemy);
                        break;
                    case ProjectileEffectType.Freeze:
                        ApplyFreeze(enemy);
                        break;
                    case ProjectileEffectType.Pushback:
                        ApplyPushback(enemy);
                        break;
                    case ProjectileEffectType.None:
                    default:
                        break;
                }
            }

            enemy.TakeDamage(damage);

            piercedEnemies++;
            if (piercedEnemies > pierceCount)
            {
                Destroy(gameObject);
            }
        }
    }

    // --- Effect Implementations ---

    private void ApplyHeavyImpact(Enemy enemy)
    {
        // Example: Stronger pushback
        if (enemy != null)
            enemy.ApplyPushback((enemy.transform.position - transform.position).normalized, 2f);
    }

    private void ApplyArmorBreak(Enemy enemy, int amount)
    {
        if (enemy != null)
            enemy.ApplyArmorBreak(amount, 3f); // 3 seconds duration (example)
    }

    private void ApplySlow(Enemy enemy, float slowAmount, float duration)
    {
        if (enemy != null)
            enemy.ApplySlow(slowAmount, duration);
    }

    private void ApplyAoESlow(Vector3 position, float slowAmount, float duration, float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, radius);
        foreach (var hit in hits)
        {
            Enemy e = hit.GetComponent<Enemy>();
            if (e != null)
                e.ApplySlow(slowAmount, duration);
        }
    }

    private void ApplyStun(Enemy enemy, float duration)
    {
        if (enemy != null)
            enemy.ApplyStun(duration);
    }

    private void ApplyAoEStun(Vector3 position, float duration, float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, radius);
        foreach (var hit in hits)
        {
            Enemy e = hit.GetComponent<Enemy>();
            if (e != null)
                e.ApplyStun(duration);
        }
    }

    private void ApplyMicroShrapnel(Vector3 position, float radius, float aoeDamage)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, radius);
        foreach (var hit in hits)
        {
            Enemy e = hit.GetComponent<Enemy>();
            if (e != null)
                e.TakeDamage(aoeDamage);
        }
        // Optionally: spawn particle effect here
    }

    private void ApplyPoison(Enemy enemy)
    {
        if (enemy != null)
            enemy.ApplyPoison(damage * 0.1f, 5, 0.5f);
    }

    private void ApplyFire(Enemy enemy)
    {
        if (enemy != null)
            enemy.ApplyBurn(damage * 0.2f, 3, 0.5f);
    }

    private void ApplyFreeze(Enemy enemy)
    {
        if (enemy != null)
            enemy.ApplyFreeze(2f, 0.5f);
    }

    private void ApplyPushback(Enemy enemy)
    {
        if (enemy != null)
            enemy.ApplyPushback((enemy.transform.position - transform.position).normalized, 1f);
    }
}
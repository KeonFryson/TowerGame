using System.Collections.Generic;
using UnityEngine;

public enum ProjectileEffectType
{
    Impact,
    HeavyImpact,
    ArmorBreak1,
    ArmorBreak2,
    Pierce,
    Slow20_1s,
    AoESlow20_1_5s,
    MiniStun_0_2s,
    AoEStun_0_3s,
    MicroShrapnel,
    AoESlow30_2s,
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
    [SerializeField] private List<ProjectileEffectType> effectTypes = new List<ProjectileEffectType>();
    private Sprite customSprite = null;

    [SerializeField] private int pierceCount = 0;
    private int piercedEnemies = 0;
    private HashSet<Enemy> hitEnemies = new HashSet<Enemy>();

    private ProjectileEffects effects;

    public void SetProjectileSize(float size)
    {
        sizeMultiplier = size;
        transform.localScale = Vector3.one * sizeMultiplier;
    }

    public void SetProjectileEffects(IEnumerable<ProjectileEffectType> effectsList)
    {
        effectTypes.Clear();
        if (effectsList != null)
            effectTypes.AddRange(effectsList);
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

        effects = new ProjectileEffects(damage, transform);

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
                    case ProjectileEffectType.Impact:
                        break;
                    case ProjectileEffectType.HeavyImpact:
                        effects.ApplyHeavyImpact(enemy);
                        break;
                    case ProjectileEffectType.ArmorBreak1:
                        effects.ApplyArmorBreak(enemy, 1);
                        break;
                    case ProjectileEffectType.ArmorBreak2:
                        effects.ApplyArmorBreak(enemy, 2);
                        break;
                    case ProjectileEffectType.Pierce:
                        break;
                    case ProjectileEffectType.Slow20_1s:
                        effects.ApplySlow(enemy, 0.2f, 1f);
                        break;
                    case ProjectileEffectType.AoESlow20_1_5s:
                        effects.ApplyAoESlow(transform.position, 0.2f, 1.5f, 2f);
                        break;
                    case ProjectileEffectType.MiniStun_0_2s:
                        effects.ApplyStun(enemy, 0.2f);
                        break;
                    case ProjectileEffectType.AoEStun_0_3s:
                        effects.ApplyAoEStun(transform.position, 0.3f, 2f);
                        break;
                    case ProjectileEffectType.MicroShrapnel:
                        effects.ApplyMicroShrapnel(transform.position, 1f, damage * 0.3f);
                        break;
                    case ProjectileEffectType.AoESlow30_2s:
                        effects.ApplyAoESlow(transform.position, 0.3f, 2f, 2.5f);
                        break;
                    case ProjectileEffectType.Poison:
                        effects.ApplyPoison(enemy);
                        break;
                    case ProjectileEffectType.Fire:
                        effects.ApplyFire(enemy);
                        break;
                    case ProjectileEffectType.Freeze:
                        effects.ApplyFreeze(enemy);
                        break;
                    case ProjectileEffectType.Pushback:
                        effects.ApplyPushback(enemy);
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
}
using System.Collections;
using UnityEngine;

public class EnemyEffects
{
    private readonly Enemy enemy;
    private readonly SpriteRenderer spriteRenderer;
    private readonly GameObject armorBreakEffectPrefab;

    public EnemyEffects(Enemy enemy, SpriteRenderer spriteRenderer, GameObject armorBreakEffectPrefab)
    {
        this.enemy = enemy;
        this.spriteRenderer = spriteRenderer;
        this.armorBreakEffectPrefab = armorBreakEffectPrefab;
    }

    public void ApplyPoison(float damagePerSecond, int ticks, float interval)
    {
        enemy.StartCoroutine(PoisonCoroutine(damagePerSecond, ticks, interval));
        enemy.StartCoroutine(ColorCoroutine(Color.green, interval));
    }

    private IEnumerator PoisonCoroutine(float damagePerSecond, int ticks, float interval)
    {
        for (int i = 0; i < ticks; i++)
        {
            enemy.TakeDamage(damagePerSecond * interval);
            yield return new WaitForSeconds(interval);
        }
    }

    public void ApplyBurn(float damagePerSecond, int ticks, float interval)
    {
        enemy.StartCoroutine(PoisonCoroutine(damagePerSecond, ticks, interval));
        enemy.StartCoroutine(ColorCoroutine(Color.red, interval));
    }

    private IEnumerator ColorCoroutine(Color color, float duration)
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = color;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            spriteRenderer.color = originalColor;
        }
    }

    public void ApplyFreeze(float slowPercentage, float duration)
    {
        enemy.StartCoroutine(SlowCoroutine(slowPercentage, duration));
        enemy.StartCoroutine(ColorCoroutine(Color.lavenderBlush, duration));
    }

    public void ApplySlow(float slowAmount, float duration)
    {
        enemy.StartCoroutine(SlowCoroutine(slowAmount, duration));
        enemy.StartCoroutine(ColorCoroutine(Color.cyan, duration));
    }

    private IEnumerator SlowCoroutine(float slowAmount, float duration)
    {
        enemy.SetSlowMultiplier(enemy.GetSlowMultiplier() * (1f - slowAmount));
        enemy.UpdateMoveSpeed();
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        enemy.SetSlowMultiplier(enemy.GetSlowMultiplier() / (1f - slowAmount));
        enemy.UpdateMoveSpeed();
    }

    public void ApplyStun(float duration)
    {
        enemy.StartCoroutine(StunCoroutine(duration));
        enemy.StartCoroutine(ColorCoroutine(Color.yellow, duration));
    }

    private IEnumerator StunCoroutine(float duration)
    {
        enemy.SetStunned(true);
        enemy.UpdateMoveSpeed();
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        enemy.SetStunned(false);
        enemy.UpdateMoveSpeed();
    }

    public void ApplyArmorBreak(int amount, float duration)
    {
        enemy.StartCoroutine(ArmorBreakCoroutine(amount, duration));
    }

    private IEnumerator ArmorBreakCoroutine(int amount, float duration)
    {
        if (enemy.Armor <= -1)
            yield break;

        enemy.SetArmor(enemy.Armor - amount);
        enemy.UpdateArmorVisual();

        if (enemy.Armor < 0 && armorBreakEffectPrefab != null)
        {
            Object.Instantiate(armorBreakEffectPrefab, enemy.transform.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(duration);
    }
}
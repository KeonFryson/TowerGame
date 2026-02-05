using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private int health = 1;
    [SerializeField] private int armor = 0;
    [SerializeField] private int moneyReward = 10;
    [SerializeField] private int damage = 1;
    [SerializeField] private int cost = 1;

    [Header("Tier")]
    [SerializeField] private EnemyTier tier;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private int currentNodeIndex = 0;
    private float currentHealth;

    public int Cost => cost;
    public EnemyTier Tier => tier;

    private void Start()
    {
        currentHealth = health;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (PathManager.Instance != null && PathManager.Instance.GetWaypointCount() > 0)
        {
            transform.position = PathManager.Instance.GetWaypoint(0);
            currentNodeIndex = 0;
        }
    }

    private void Update()
    {
        MoveAlongPath();
    }

    private void MoveAlongPath()
    {
        if (PathManager.Instance == null)
        {
            ReachEnd();
            return;
        }

        PathNodeData currentNode = PathManager.Instance.GetNode(currentNodeIndex);
        if (currentNode == null)
        {
            ReachEnd();
            return;
        }

        Vector3 targetPosition = currentNode.position;
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            if (currentNode.nextNodeIndices != null && currentNode.nextNodeIndices.Length > 0)
            {
                int nextIdx = currentNode.nextNodeIndices[Random.Range(0, currentNode.nextNodeIndices.Length)];
                currentNodeIndex = nextIdx;
            }
            else
            {
                ReachEnd();
            }
        }
    }

    public void TakeDamage(float damageAmount)
    {
        // Subtract armor from incoming damage
        float effectiveDamage = damageAmount - armor;
        if (effectiveDamage < 0f)
            effectiveDamage = 0f; // Prevent healing from negative damage

        currentHealth -= effectiveDamage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    private void Die()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(moneyReward);
        }

        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnEnemyDefeated();
        }

        Destroy(gameObject);
    }

    private void ReachEnd()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseLife(damage);
        }
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnEnemyDefeated();
        }
        Destroy(gameObject);
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void ApplyPoison(float damagePerSecond, int ticks, float interval)
    {
        StartCoroutine(PoisonCoroutine(damagePerSecond, ticks, interval));
        StartCoroutine(ColorCoroutine(Color.green, interval));

    }

    private  IEnumerator PoisonCoroutine(float damagePerSecond, int ticks, float interval)
    {
        for (int i = 0; i < ticks; i++)
        {
            TakeDamage(damagePerSecond * interval);
            yield return new WaitForSeconds(interval);
        }
    }


    public void ApplyBurn(float damagePerSecond, int ticks, float interval)
    {
        StartCoroutine(PoisonCoroutine(damagePerSecond, ticks, interval));
        StartCoroutine(ColorCoroutine(Color.red, interval));
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
        StartCoroutine(SlowCoroutine(slowPercentage, duration));
        StartCoroutine(ColorCoroutine(Color.lavenderBlush, duration));
    }
    private  IEnumerator SlowCoroutine(float slowPercentage, float duration)
    {
        float originalSpeed = moveSpeed;
        moveSpeed *= (1f - slowPercentage);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        moveSpeed = originalSpeed;
    }

    public void ApplyPushback(Vector3 direction, float force)
    {
        transform.position += direction * force;
    }

    public void SetColor (Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }

    public void ApplyArmorBreak(int amount, float duration)
    {
        StartCoroutine(ArmorBreakCoroutine(amount, duration));
    }

    private IEnumerator ArmorBreakCoroutine(int amount, float duration)
    {
        // Directly modify the armor field since it now exists in this class
        armor -= amount;
        StartCoroutine(ColorCoroutine(Color.gray, duration));
        yield return new WaitForSeconds(duration);
        armor += amount;
    }

    // Slows the enemy by a percentage for a duration (0.2f = 20% slow)
    public void ApplySlow(float slowAmount, float duration)
    {
        StartCoroutine(SlowCoroutine(slowAmount, duration));
        StartCoroutine(ColorCoroutine(Color.cyan, duration));
    }

    // Stuns the enemy for a duration (sets moveSpeed to 0)
    public void ApplyStun(float duration)
    {
        StartCoroutine(StunCoroutine(duration));
        StartCoroutine(ColorCoroutine(Color.yellow, duration));
    }

    private IEnumerator StunCoroutine(float duration)
    {
        float originalSpeed = moveSpeed;
        moveSpeed = 0f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        moveSpeed = originalSpeed;
    }

    // Utility for AoE effects: can be called from Projectile
    public static void ApplyAoEEffect(Vector3 position, float radius, System.Action<Enemy> effect)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, radius);
        foreach (var hit in hits)
        {
            Enemy e = hit.GetComponent<Enemy>();
            if (e != null)
                effect(e);
        }
    }
}

public enum EnemyTier
{
    Baby,
    Weak,
    Mid,
    Strong,
    Elite,
    Boss
}
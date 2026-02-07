using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public abstract class Enemy : MonoBehaviour
{
    // --- Stats ---
    [Header("Stats")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private int health = 1;
    [SerializeField] private int armor = 0;
    [SerializeField] private int moneyReward = 10;
    [SerializeField] private int damage = 1;
    [SerializeField] private int cost = 1;

    // --- Effects ---
    [Header("Effects")]
    [SerializeField] private GameObject armorBreakEffectPrefab;

    // --- Visual ---
    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Material armorMaterial;
    [SerializeField] private Material defaultMaterial;

    // --- Description ---
    [Header("Description")]
    [TextArea]
    [SerializeField] private string description;

    // --- Tier ---
    [Header("Tier")]
    [SerializeField] private EnemyTier tier;

    // --- State ---
    private bool isDead = false;
    private int currentNodeIndex = 0;
    [SerializeField] private float currentHealth;
    private float baseMoveSpeed;
    private bool isStunned = false;
    private float slowMultiplier = 1f;
    private EnemyEffects effects;

    // --- Properties ---
    public bool IsDead => isDead;
    public int Health => health;
    public float MoveSpeed => moveSpeed;
    public int Armor => armor;
    public int MoneyReward => moneyReward;
    public int Damage => damage;
    public float CurrentHealth => currentHealth;
    public int Cost => cost;
    public EnemyTier Tier => tier;

    public string GetDescription() => description;

    // --- Unity Lifecycle ---
    protected virtual void Start()
    {
        baseMoveSpeed = moveSpeed;
        currentHealth = health;
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        effects = new EnemyEffects(this, spriteRenderer, armorBreakEffectPrefab);
        UpdateArmorVisual();

        if (PathManager.Instance != null && PathManager.Instance.GetWaypointCount() > 0)
        {
            transform.position = PathManager.Instance.GetWaypoint(0);
            currentNodeIndex = 0;
        }
    }

    protected virtual void Update()
    {
        MoveAlongPath();
    }

    // --- Path Movement ---
    protected virtual void MoveAlongPath()
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

        if (Vector3.Distance(transform.position, targetPosition) < 0.2f)
        {
            transform.position = targetPosition;
            if (currentNode.nextNodeIndices != null && currentNode.nextNodeIndices.Length > 0)
            {
                int nextIdx = currentNode.nextNodeIndices[Random.Range(0, currentNode.nextNodeIndices.Length)];
                nextIdx = Mathf.Clamp(nextIdx, 0, PathManager.Instance.GetWaypointCount() - 1);
                currentNodeIndex = nextIdx;
            }
            else
            {
                ReachEnd();
            }
        }
    }

    // --- Damage & Death ---
    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        if (armor > 0)
        {
            if (damageAmount > armor)
            {
                armor -= 1;
                if (armor < 0) armor = 0;
                UpdateArmorVisual();
                Debug.Log($"[Enemy] Damage ({damageAmount}) > armor. Armor reduced by 1. New Armor: {armor}");
            }
            else
            {
                // Damage is less than or equal to armor, do nothing
                Debug.Log($"[Enemy] Damage ({damageAmount}) <= armor ({armor}). No effect.");
            }
        }
        else
        {
            currentHealth -= damageAmount;
            Debug.Log($"[Enemy] No armor. Damage to health: {damageAmount}, New Health: {currentHealth}");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        GameManager.Instance?.AddMoney(moneyReward);
        WaveManager.Instance?.OnEnemyDefeated();

        Destroy(gameObject);
    }

    protected virtual void ReachEnd()
    {
        if (isDead) return;
        isDead = true;

        GameManager.Instance?.LoseLife(damage);
        WaveManager.Instance?.OnEnemyDefeated();

        Destroy(gameObject);
    }

    // --- Utility ---
    public Vector3 GetPosition() => transform.position;

    // --- Effects API ---
    public void ApplyPoison(float damagePerSecond, int ticks, float interval) => effects.ApplyPoison(damagePerSecond, ticks, interval);
    public virtual void ApplyBurn(float damagePerSecond, int ticks, float interval) => effects.ApplyBurn(damagePerSecond, ticks, interval);
    public void ApplyFreeze(float slowPercentage, float duration) => effects.ApplyFreeze(slowPercentage, duration);
    public virtual void ApplySlow(float slowAmount, float duration) => effects.ApplySlow(slowAmount, duration);
    public virtual void ApplyStun(float duration) => effects.ApplyStun(duration);
    public virtual void ApplyArmorBreak(int amount, float duration) => effects.ApplyArmorBreak(amount, duration);

    // --- MoveSpeed Coordination ---
    public void UpdateMoveSpeed()
    {
        moveSpeed = isStunned ? 0f : baseMoveSpeed * slowMultiplier;
    }

    public float GetSlowMultiplier() => slowMultiplier;
    public void SetSlowMultiplier(float value) => slowMultiplier = value;
    public void SetStunned(bool value) => isStunned = value;
    public void SetArmor(int value) => armor = value;
    public virtual void SetCurrentNodeIndex(int nodeIndex) => currentNodeIndex = nodeIndex;

    // --- Pushback ---
    public virtual void ApplyPushback(Vector3 direction, float force)
    {
        transform.position += direction * force;
        ClampToPath();
    }

    // --- Visual ---
    public virtual void SetColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }

    public void UpdateArmorVisual()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.material = (armor > 0 && armorMaterial != null) ? armorMaterial : defaultMaterial;
        }
    }

    // --- AoE Effect ---
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

    public void ClampToPath()
    {
        if (PathManager.Instance == null)
            return;

        int nodeCount = PathManager.Instance.GetWaypointCount();
        if (nodeCount < 2)
            return;

        Vector3 closestPoint = transform.position;
        float closestDist = float.MaxValue;

        // Find the closest point on any path segment
        for (int i = 0; i < nodeCount - 1; i++)
        {
            Vector3 a = PathManager.Instance.GetWaypoint(i);
            Vector3 b = PathManager.Instance.GetWaypoint(i + 1);
            Vector3 projected = ProjectPointOnLineSegment(a, b, transform.position);
            float dist = Vector3.Distance(transform.position, projected);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestPoint = projected;
            }
        }

        transform.position = closestPoint;
    }

    private Vector3 ProjectPointOnLineSegment(Vector3 a, Vector3 b, Vector3 p)
    {
        Vector3 ap = p - a;
        Vector3 ab = b - a;
        float ab2 = ab.sqrMagnitude;
        float ap_ab = Vector3.Dot(ap, ab);
        float t = Mathf.Clamp01(ap_ab / ab2);
        return a + ab * t;
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
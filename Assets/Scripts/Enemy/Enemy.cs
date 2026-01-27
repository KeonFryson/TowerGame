using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private int health = 1;
    [SerializeField] private int moneyReward = 10;
    [SerializeField] private int damage = 1;
    [SerializeField] private int cost = 1;

    [Header("Tier")]
    [SerializeField] private EnemyTier tier; // Add this line

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private int currentNodeIndex = 0;
    private float currentHealth;

    public int Cost => cost;
    public EnemyTier Tier => tier; // Add this property

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

        // Check if reached node
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            // If there are next nodes, pick one (randomly)
            if (currentNode.nextNodeIndices != null && currentNode.nextNodeIndices.Length > 0)
            {
                // Pick a random next node index
                int nextIdx = currentNode.nextNodeIndices[Random.Range(0, currentNode.nextNodeIndices.Length)];
                currentNodeIndex = nextIdx;
            }
            else
            {
                // No next node, reached end
                ReachEnd();
            }
        }
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;

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
        Destroy(gameObject);
    }

    public Vector3 GetPosition()
    {
        return transform.position;
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

public static class EnemyTierHelper
{
    public static EnemyTier GetTierByWave(int wave)
    {
        if (wave >= 1 && wave < 10)
            return EnemyTier.Baby;
        if (wave >= 10 && wave <= 20)
            return EnemyTier.Weak;
        if (wave > 20 && wave <= 30)
            return EnemyTier.Mid;
        if (wave > 30 && wave <= 50)
            return EnemyTier.Strong;
        if (wave > 50 && wave <= 80)
            return EnemyTier.Elite;
        if (wave > 80 && wave <= 100)
            return EnemyTier.Boss;
        return EnemyTier.Baby; // Default/fallback for waves < 1
    }

    public static float GetTierProgress(int wave)
    {
        // Returns a value from 0.1 to 1.0 representing how far into the current tier we are
        // e.g., at the start of a tier range, 0.1; at the end, 1.0
        if (wave >= 1 && wave < 10)
            return (wave - 1) / 9f;
        if (wave >= 10 && wave <= 20)
            return (wave - 10) / 10f;
        if (wave > 20 && wave <= 30)
            return (wave - 20) / 10f;
        if (wave > 30 && wave <= 50)
            return (wave - 30) / 20f;
        if (wave > 50 && wave <= 80)
            return (wave - 50) / 30f;
        if (wave > 80 && wave <= 100)
            return (wave - 80) / 20f;
        return 1f;
    }
}
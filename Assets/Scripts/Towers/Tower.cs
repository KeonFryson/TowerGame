using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#region Upgrade Data Structures
[System.Serializable]
public class TowerUpgrade
{
    public string upgradeName;
    public string description;
    public int cost;
    public Sprite icon;

    // Stat bonuses
    public float rangeBonus;
    public float fireRateBonus;
    public float damageBonus;
    public int projectilesPerShotBonus; 
    public float spreadAngleBonus;
    // Projectile bonuses/effects
    public float projectileSizeBonus;
 
    public List<ProjectileEffectType> projectileEffects = new List<ProjectileEffectType>(); // Multiple effects
    public Sprite projectileSprite;
    public int pierceBonus; // Add this if you want upgrades to add pierce
}
#endregion

public class Tower : MonoBehaviour
{
    #region Fields & Properties
    [Header("Tower Stats")]
    [SerializeField] private float range = 5f;
    [SerializeField] private float baseFireRate = 1f;
    [SerializeField] private float fireRateBonus = 0f;
    [SerializeField] private float damage = 1f;
    [SerializeField] private int cost = 100;
    [SerializeField] private int baseProjectilesPerShot = 1;
    private int projectilesPerShotBonus = 0;
    [SerializeField] private float spreadAngle = 30f;

    [Header("Upgrade System")]
    [Tooltip("Current tier for each path (e.g., [2,1,0] means path 0 is tier 2, path 1 is tier 1, path 2 is tier 0)")]
    public int[] upgradeTiers = new int[3];
    [Tooltip("Maximum tier for each path.")]
    public int[] maxPathLevels = new int[3] { 5, 5, 5 };
    [Tooltip("Upgrade data for each path. Each path is an array of TowerUpgrade objects.")]
    public TowerUpgradePath[] upgradePaths = new TowerUpgradePath[3];

    [Header("Abilities")]
    [SerializeField] private TowerAbility[] abilities;

    [Header("Targeting")]
    public TargetMode targetMode = TargetMode.Close;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject projectilePrefab;
    private GameObject rangeIndicator;

    [Header("Debug")]
    [SerializeField] private bool showRange = true;

    private float fireTimer = 0f;
    private Enemy currentTarget;
    private LineRenderer rangeLineRenderer;
    public bool IsPlaced { get; private set; } = false;

    // Ability runtime state
    private float fireRateMultiplier = 1f;
    private float projectileSizeBonus = 0f;
    [SerializeField] private List<ProjectileEffectType> projectileEffects = new List<ProjectileEffectType>();
    private Sprite projectileSprite = null;

    private int pierceBonus = 0;
    #endregion

    #region Unity Methods
    public void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        fireTimer += Time.deltaTime;
        float effectiveFireRate = (baseFireRate + fireRateBonus) * fireRateMultiplier;
        if (effectiveFireRate < 0.1f) effectiveFireRate = 0.1f;
        if (fireTimer >= 1f / effectiveFireRate)
        {
            FindAndAttackTarget();
            fireTimer = 0f;
        }
    }
    #endregion

    #region Targeting & Attacking
    public enum TargetMode { First, Last, Strong, Weak, Close }

    private void FindAndAttackTarget()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        List<Enemy> enemiesInRange = new List<Enemy>();
        foreach (Enemy enemy in enemies)
        {
            if (Vector3.Distance(transform.position, enemy.GetPosition()) <= range)
                enemiesInRange.Add(enemy);
        }
        if (enemiesInRange.Count > 0)
        {
            currentTarget = SelectTarget(enemiesInRange);
            Attack(currentTarget);
        }
    }

    private Enemy SelectTarget(List<Enemy> enemies)
    {
        if (enemies == null || enemies.Count == 0)
            return null;
        switch (targetMode)
        {
            case TargetMode.First:
                return enemies[enemies.Count - 1];
            case TargetMode.Last:
                return enemies[0];
            case TargetMode.Close:
                Enemy closest = enemies[0];
                float closestDist = Vector3.Distance(transform.position, closest.GetPosition());
                for (int i = 1; i < enemies.Count; i++)
                {
                    float dist = Vector3.Distance(transform.position, enemies[i].GetPosition());
                    if (dist < closestDist)
                    {
                        closest = enemies[i];
                        closestDist = dist;
                    }
                }
                return closest;
            case TargetMode.Strong:
                Enemy strongest = enemies[0];
                for (int i = 1; i < enemies.Count; i++)
                {
                    if ((int)enemies[i].Tier > (int)strongest.Tier)
                        strongest = enemies[i];
                }
                return strongest;
            case TargetMode.Weak:
                Enemy weakest = enemies[0];
                for (int i = 1; i < enemies.Count; i++)
                {
                    if ((int)enemies[i].Tier < (int)weakest.Tier)
                        weakest = enemies[i];
                }
                return weakest;
            default:
                return enemies[enemies.Count - 1];
        }
    }

    private void Attack(Enemy target)
    {
        if (target == null) return;
        int totalProjectiles = baseProjectilesPerShot + projectilesPerShotBonus;
        if (totalProjectiles < 1) totalProjectiles = 1;
        if (projectilePrefab != null && projectileSpawnPoint != null)
        {
            float angleStep = totalProjectiles > 1 ? spreadAngle / (totalProjectiles - 1) : 0f;
            float startAngle = -spreadAngle / 2f;
            Vector3 toTarget = (target.GetPosition() - projectileSpawnPoint.position).normalized;
            for (int i = 0; i < totalProjectiles; i++)
            {
                float angle = startAngle + i * angleStep;
                Quaternion spreadRot = Quaternion.AngleAxis(angle, Vector3.forward);
                Vector3 direction = spreadRot * toTarget;
                float aimAssistThreshold = 0.1f;
                Vector3 exactToTarget = (target.GetPosition() - projectileSpawnPoint.position).normalized;
                if (Vector3.Distance(direction, exactToTarget) <= aimAssistThreshold)
                    direction = exactToTarget;
                GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
                Projectile proj = projectile.GetComponent<Projectile>();
                if (proj != null)
                {
                    proj.SetTarget(target, damage, false);
                    proj.SetInitialDirection(direction);
                    proj.SetProjectileSize(0.5f + projectileSizeBonus);

                    // Set all effects
                    proj.SetProjectileEffects(projectileEffects);
                    if (pierceBonus > 0)
                        proj.SetPierceCount(pierceBonus);

                    if (projectileSprite != null)
                        proj.SetProjectileSprite(projectileSprite);
                }
            }
        }
        else
        {
            target.TakeDamage(damage);
        }
    }
    #endregion

    #region Abilities
    public TowerAbility[] GetAbilities() => abilities;

    public void ActivateAbility(int index)
    {
        if (abilities != null && index >= 0 && index < abilities.Length)
            abilities[index].Activate(this);
    }

    public void AddFireRateMultiplier(float multiplier)
    {
        fireRateMultiplier *= multiplier;
    }

    public void RemoveFireRateMultiplier(float multiplier)
    {
        if (multiplier != 0f)
            fireRateMultiplier /= multiplier;
    }

    public void AddDamageBonus(float bonus)
    {
        damage += bonus;
    }

    public void RemoveDamageBonus(float bonus)
    {
        damage -= bonus;
    }

    public void AddRangeBonus(float bonus)
    {
        range += bonus;
        UpdateRangeIndicator();
    }

    public void RemoveRangeBonus(float bonus)
    {
        range -= bonus;
        UpdateRangeIndicator();
    }

    public void AddProjectilesPerShotBonus(int bonus)
    {
        projectilesPerShotBonus += bonus;
    }

    public void RemoveProjectilesPerShotBonus(int bonus)
    {
        projectilesPerShotBonus -= bonus;
    }

    

    #endregion

    #region Upgrade System
    public bool CanUpgrade(int path)
    {
        if (upgradePaths == null || path < 0 || path >= upgradePaths.Length || upgradePaths[path].upgrades == null)
            return false;
        if (upgradeTiers[path] >= maxPathLevels[path])
            return false;
        if (upgradeTiers[path] >= upgradePaths[path].upgrades.Length)
            return false;
        int upgradedOtherPaths = 0;
        for (int i = 0; i < upgradeTiers.Length; i++)
        {
            if (i != path && upgradeTiers[i] > 0)
                upgradedOtherPaths++;
        }
        if (upgradedOtherPaths >= 2)
            return false;
        int highTierCount = 0;
        for (int i = 0; i < upgradeTiers.Length; i++)
        {
            if (upgradeTiers[i] >= 3)
                highTierCount++;
        }
        if (upgradeTiers[path] == 2 && highTierCount > 0)
            return false;
        if (upgradeTiers[path] >= 3 && highTierCount > 1)
            return false;
        if (upgradeTiers[path] >= 4)
            return false;
        return true;
    }

    public TowerUpgrade GetNextUpgrade(int path)
    {
        if (upgradePaths == null || path < 0 || path >= upgradePaths.Length || upgradePaths[path].upgrades == null)
            return null;
        int tier = upgradeTiers[path];
        if (tier < upgradePaths[path].upgrades.Length)
            return upgradePaths[path].upgrades[tier];
        return null;
    }

    public bool ApplyUpgrade(int path)
    {
        if (!CanUpgrade(path)) return false;
        TowerUpgrade upgrade = GetNextUpgrade(path);
        if (upgrade == null) return false;
        range += upgrade.rangeBonus;
        fireRateBonus += upgrade.fireRateBonus;
        damage += upgrade.damageBonus;
        projectilesPerShotBonus += upgrade.projectilesPerShotBonus;
        spreadAngle += upgrade.spreadAngleBonus;
        projectileSizeBonus += upgrade.projectileSizeBonus;
        

        // Add all new effects from upgrade
        foreach (var effect in upgrade.projectileEffects)
        {
            if (!projectileEffects.Contains(effect) && effect != ProjectileEffectType.None)
                projectileEffects.Add(effect);
        }

        if (upgrade.pierceBonus > 0)
            pierceBonus += upgrade.pierceBonus;

        if (upgrade.projectileSprite != null)
            projectileSprite = upgrade.projectileSprite;
        if (upgrade.icon != null && spriteRenderer != null)
            spriteRenderer.sprite = upgrade.icon;
        upgradeTiers[path]++;
        return true;
    }

    #endregion

    #region Utility & Visuals
    public void MarkAsPlaced() => IsPlaced = true;
    public int GetCost() => cost;
    public float GetRange() => range;
    public void SetShowRange(bool value) => showRange = value;

    public void ShowRangeIndicator()
    {
        if (rangeIndicator != null) return;
        rangeIndicator = new GameObject("RangeIndicator");
        rangeLineRenderer = rangeIndicator.AddComponent<LineRenderer>();
        rangeLineRenderer.useWorldSpace = false;
        rangeLineRenderer.startWidth = 0.1f;
        rangeLineRenderer.endWidth = 0.1f;
        rangeLineRenderer.startColor = new Color(1f, 1f, 1f, 0.5f);
        rangeLineRenderer.endColor = new Color(1f, 1f, 1f, 0.5f);
        rangeLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        rangeLineRenderer.sortingOrder = 0;
        rangeIndicator.transform.SetParent(transform);
        rangeIndicator.transform.localPosition = Vector3.zero;
        UpdateRangeIndicator();
    }

    public void UpdateRangeIndicator()
    {
        if (rangeLineRenderer == null) return;
        int segments = 64;
        rangeLineRenderer.positionCount = segments + 1;
        float r = GetRange();
        float angle = 0f;
        for (int i = 0; i <= segments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * r;
            float y = Mathf.Cos(Mathf.Deg2Rad * angle) * r;
            rangeLineRenderer.SetPosition(i, new Vector3(x, y, 0f));
            angle += 360f / segments;
        }
    }

    public void HideRangeIndicator()
    {
        if (rangeIndicator != null)
        {
            Destroy(rangeIndicator);
            rangeIndicator = null;
        }
    }

    public int GetSellPrice()
    {
        int totalSpent = cost;
        for (int path = 0; path < upgradeTiers.Length; path++)
        {
            for (int tier = 0; tier < upgradeTiers[path]; tier++)
            {
                TowerUpgrade upgrade = upgradePaths[path].upgrades[tier];
                if (upgrade != null)
                    totalSpent += upgrade.cost;
            }
        }
        return totalSpent / 2;
    }
    #endregion
}
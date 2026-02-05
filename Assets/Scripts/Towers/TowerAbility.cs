using UnityEngine;

public abstract class TowerAbility : ScriptableObject
{
    public string abilityName;
    public string description;
    public Sprite icon;
    public float cooldown = 5f;

    protected float lastUsedTime = -Mathf.Infinity;

    public virtual bool CanActivate()
    {
        return Time.time >= lastUsedTime + cooldown;
    }

    public void Activate(Tower tower)
    {
        if (CanActivate())
        {
            lastUsedTime = Time.time;
            OnActivate(tower);
        }
    }

    protected abstract void OnActivate(Tower tower);
}

[CreateAssetMenu(menuName = "TowerAbilities/RapidFire")]
public class RapidFireAbility : TowerAbility
{
    public float fireRateMultiplier = 2f;
    public float duration = 3f;

    protected override void OnActivate(Tower tower)
    {
        tower.StartCoroutine(ApplyRapidFire(tower));
    }

    private System.Collections.IEnumerator ApplyRapidFire(Tower tower)
    {
        tower.AddFireRateMultiplier(fireRateMultiplier);
        yield return new WaitForSeconds(duration);
        tower.AddFireRateMultiplier(1f / fireRateMultiplier);
    }
}
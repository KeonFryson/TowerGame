using UnityEngine;

public class Tank_Enemy : Enemy
{
    [SerializeField] private ParticleSystem armorBreakEffect;
 

    protected override void Start()
    {
        base.Start();
 
    }

    protected override void Update()
    {
        base.Update();

        Debug.Log($"[Tank_Enemy] Health: {Health}, Armor: {Armor}, IsDead: {IsDead}, CurrentHealth: {CurrentHealth}");


    }

    public override void ApplyPushback(Vector3 direction, float force)
    {
        float reducedForce = force * 0.5f;
        base.ApplyPushback(direction, reducedForce);
    }

   
}
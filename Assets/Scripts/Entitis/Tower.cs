using UnityEngine;

public class Tower : BaseEntity
{
    private CodingManager codingManager;

    protected override void Awake()
    {
        base.Awake();
        codingManager = Object.FindFirstObjectByType<CodingManager>();
    }

    public override void Die()
    {
        if (codingManager != null)
        {
            codingManager.EndGame(this.teamID);
        }
        base.Die();
    }
}
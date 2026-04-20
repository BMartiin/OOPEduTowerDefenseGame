using UnityEngine;
using UnityEngine.UI;

public class Tower : BaseEntity
{
    private CodingManager codingManager;

    protected override void Awake()
    {
        base.Awake();
        codingManager = Object.FindFirstObjectByType<CodingManager>();
    }

    private void Start()
    {
        Transform fillTransform = transform.Find("HealthBarBackground/Fill") ?? transform.Find("HealthBarBackground(Clone)/Fill");

        if (fillTransform != null)
        {
            healthFillImage = fillTransform.GetComponent<Image>();
        }
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
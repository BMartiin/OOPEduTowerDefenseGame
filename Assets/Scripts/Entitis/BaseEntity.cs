using UnityEngine;
using System.Collections;

public abstract class BaseEntity : MonoBehaviour, IDamageable
{
    [SerializeField] protected float maxHealth; //SerializedField -> Hogy mutassa az inspectorban
    protected float currentHealth;
    [SerializeField]  protected byte teamID;

    //Property for var access
    public byte TeamID
    {
        get { return teamID; }
    }

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    public virtual void Die()
    {
        Destroy(gameObject);
    }

    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        if (currentHealth <= 0)
            Die();
    }
}

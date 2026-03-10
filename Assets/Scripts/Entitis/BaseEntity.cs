using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public abstract class BaseEntity : MonoBehaviour, IDamageable
{
    [SerializeField] protected float maxHealth; //SerializedField -> Hogy mutassa az inspectorban
    protected float currentHealth;
    [SerializeField]  protected byte teamID;
    [SerializeField] protected Image healthFillImage;

    protected Animator animator;

    //Property for var access
    public byte TeamID
    {
        get { return teamID; }
    }

    protected virtual void Awake()
    {
        currentHealth = maxHealth;

        animator = GetComponent<Animator>();
    }

    public virtual void Die()
    {
        Destroy(gameObject);
    }

    public bool IsDead
    {
        get { return currentHealth <= 0; }
    }

    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = currentHealth / maxHealth;
        }
        if(currentHealth <= 0)
        {
            if (healthFillImage != null)
            {
                //disable health bar when the entity is exterminated :)
                healthFillImage.transform.parent.gameObject.SetActive(false);
            }
            //disable collision when the entity is ext. :)
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;

            animator.SetBool("isAlive", false);
            Invoke(nameof(Die), 1f);
        }
    
    }
}

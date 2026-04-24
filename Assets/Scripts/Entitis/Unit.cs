using UnityEngine;
using System.Collections;
using System;

public enum AttackType
{
    Melee,
    Ranged
}

public class Unit : BaseEntity
{
    [SerializeField] private float moveSpeed;
    private float originalMoveSpeed;
    private float moveDirection;

    [SerializeField] private float attackDamage;
    [SerializeField] private float attackSpeed;
    [SerializeField] BaseEntity currentTarget;

    [SerializeField] private AttackType unitAttackType;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();

        if (teamID == 0)
            moveDirection = 1.0f;
        else
            moveDirection = -1.0f;

        //this var is needed so if the speed of the unit changes do to combat or something else we can change it back later to the original
        originalMoveSpeed = moveSpeed;

        animator.speed = attackSpeed;
    }

    void Update()
    {
        transform.Translate(Vector2.right * Time.deltaTime * moveSpeed * moveDirection, Space.World);

        if(moveSpeed > 0.0f)
        {
            animator.SetBool("isMoving", true);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckForCombat(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if(moveSpeed > 0)
        {
            CheckForCombat(other);
        }
    }

    private void CheckForCombat(Collider2D other)
    {
        if (other is CircleCollider2D && other.isTrigger)
            return;

        BaseEntity target = other.GetComponent<BaseEntity>();

        if (target != null && target.TeamID != this.teamID && moveSpeed > 0)
        {
            moveSpeed = 0;
            currentTarget = target;
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        while(currentTarget != null && !currentTarget.IsDead)
        {
            animator.SetBool("isAttacking", true);

            yield return new WaitForSeconds(1.0f / attackSpeed);
        }

        animator.SetBool("isAttacking", false);
        moveSpeed = originalMoveSpeed;
        currentTarget = null;
    }

    public void ExecuteHit()
    {
        if (currentTarget != null && !currentTarget.IsDead)
        {
            if(unitAttackType == AttackType.Melee)
            {
                currentTarget.TakeDamage(attackDamage);
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.swordSwing);
                }
            }
            else if(unitAttackType == AttackType.Ranged)
            {
                ShootProjectile();
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.arrowShoot);
                }
            }
        }
    }

    private void ShootProjectile()
    {
        if (projectilePrefab != null && projectileSpawnPoint != null)
        {
            GameObject proj = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
            Projectile projectileScript = proj.GetComponent<Projectile>();

            if (projectileScript != null)
            {
                projectileScript.Initialize(currentTarget, attackDamage);
            }
        }
    }

}

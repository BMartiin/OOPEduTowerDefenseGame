using UnityEngine;
using System.Collections;
using System;

public class Unit : BaseEntity
{
    //movement vars
    [SerializeField] private float moveSpeed;
    private float originalMoveSpeed;
    private float moveDirection;
    
    //animation vars
    private Animator animator;

    //combat vars
    [SerializeField] private float attackDamage;
    [SerializeField] private float attackSpeed;
    [SerializeField] BaseEntity currentTarget;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (teamID == 0)
            moveDirection = 1.0f;
        else
            moveDirection = -1.0f;

        //this var is needed so if the speed of the unit changes do to combat or something else we can change it back later to the original
        originalMoveSpeed = moveSpeed;

        //for the animation
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector2.right * Time.deltaTime * moveSpeed * moveDirection, Space.World);

        //to set animation for walking
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
        //if the unit is moving, but it's already in the trigger range of an enemy
        if(moveSpeed > 0)
        {
            CheckForCombat(other);
        }
    }

    private void CheckForCombat(Collider2D other)
    {
        BaseEntity target = other.GetComponent<BaseEntity>();

        //teamID is needed so the Knight can indentify which tower is the enemies and i check move speed so the unit doesn't start to fight multiple enemies at once
        if (target != null && target.TeamID != this.teamID && moveSpeed > 0)
        {
            //after meeting an enemy stops and starts the combat sequence
            moveSpeed = 0;
            currentTarget = target;
            StartCoroutine(AttackRoutine());
        }
    }



    //combat logic
    IEnumerator AttackRoutine()
    {
        while(currentTarget != null)
        {
            animator.SetBool("isAttacking", true);

            //wait so the animation matches the damage
            yield return new WaitForSeconds(0.2f);

            //have to check if the enemy is alive again, because it can die while the unit is waiting
            if(currentTarget != null)
            {
                currentTarget.TakeDamage(attackDamage);
            }
            else
            {
                break;
            }
            //attackSpeed can't be 0! 
            yield return new WaitForSeconds(1 / attackSpeed);

        }

        //ending the attacking animation after the enemy is dead
        animator.SetBool("isAttacking", false);
        moveSpeed = originalMoveSpeed;
        currentTarget = null; //just to be sure
    }
}

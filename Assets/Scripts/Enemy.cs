using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity
{
    public enum State { Idle, Chasing, Attacking }

    State currentState;

    LivingEntity targetEntity;

    NavMeshAgent pathfinder;
    Transform target;
    Material material;

    Color originalColor;

    float attackDistanceTreshold = 0.5f;
    float timeBetweenAttacks = 1f;

    float nextAttackTime;

    float myCollisionRadius;
    float targetCollisionRadius;
    bool hasTarget;

    float damage = 1f;


    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        pathfinder = GetComponent<NavMeshAgent>();

        material = GetComponent<Renderer>().material;
        originalColor = material.color;

        if(GameObject.FindGameObjectWithTag("Player") != null)
        {
            currentState = State.Chasing;
            hasTarget = true;

            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>();
            targetEntity.OnDeath += OnTargetDeath;

            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;

            StartCoroutine(UpdatePath());
        }
    }

    private void OnTargetDeath()
    {
        hasTarget = false;
        currentState = State.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        if (hasTarget)
        {
            if (Time.time > nextAttackTime)
            {
                float sqrDistanceToTarget = (target.position - transform.position).sqrMagnitude;

                if (sqrDistanceToTarget <= Mathf.Pow(attackDistanceTreshold + myCollisionRadius + targetCollisionRadius, 2))
                {
                    nextAttackTime = Time.time + timeBetweenAttacks;
                    StartCoroutine(Attack());
                }
            }
        }
    }
    IEnumerator Attack()
    {
        currentState = State.Attacking;
        material.color = Color.red;

        Vector3 originalPosition = transform.position;
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        Vector3 attackPosition = target.position - directionToTarget * (myCollisionRadius);

        float attackSpeed = 3f;
        float percent = 0f;

        bool hasAppliedDamage = false;

        while (percent <= 1f)   
        {
            if(percent >= 0.5f && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                targetEntity.TakeDamage(damage);
            }
            percent += Time.deltaTime * attackSpeed;
            float interpolation = 4 * (-Mathf.Pow(percent, 2) + percent);
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);

            yield return null;
        }

        material.color = originalColor;
        currentState = State.Chasing;
    }

    IEnumerator UpdatePath() {
        float refreshRate = 0.25f; // seconds

        while(hasTarget)
        {
            if(currentState == State.Chasing)
            {
                Vector3 directionToTarget = (target.position - transform.position).normalized;
                Vector3 targetPosition = target.position - directionToTarget * 
                    (myCollisionRadius + targetCollisionRadius + attackDistanceTreshold / 2);

                if (!dead)
                {
                    pathfinder.SetDestination(targetPosition);
                }
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }
}

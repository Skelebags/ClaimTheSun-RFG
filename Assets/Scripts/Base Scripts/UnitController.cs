using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class UnitController : BaseController
{
    [SerializeField]
    [Tooltip("The amount of damage dealt by this unit")]
    private float attackDamage = 5f;

    [SerializeField]
    [Tooltip("The amount of damage dealt by this unit")]
    private float armourPen = 0f;

    [SerializeField]
    [Tooltip("The time in seconds between attacks")]
    private float attackRate = 0.5f;
    private float attackTimer = 0f;

    [SerializeField]
    [Tooltip("The range of this unit's attack")]
    private float attackRange = 2f;

    [SerializeField]
    [Tooltip("How close to it's maximum range will the unit path to its target")][Range(0.1f, 1f)]
    private float idealRange = 0.95f;

    [SerializeField]
    [Tooltip("Can this unit attack while moving")]
    private bool attackMove = false;

    private NavMeshAgent agent;

    private GameObject attackTarget;

    public enum State { idle, attacking}
    public State state;
    
    new protected virtual void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
        state = State.idle;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch(state)
        {
            case State.idle:
                break;

            case State.attacking:
                if(attackTarget == null)
                {
                    state = State.idle;
                }
                else
                {

                    Debug.Log("Attack");
                    if ((attackTarget.transform.position - transform.position).magnitude <= attackRange)
                    {
                        if((agent.desiredVelocity == Vector3.zero) || attackMove)
                        {
                            attackTimer += Time.deltaTime;
                            if (attackTimer >= attackRate)
                            {
                                attackTarget.GetComponent<BaseController>().Damage(attackDamage, armourPen);
                                attackTimer = 0f;
                            }
                        }
                    }
                    else
                    {
                        agent.SetDestination(attackTarget.transform.position - (attackTarget.transform.position - transform.position).normalized * (attackRange * idealRange));
                        //NavMeshPath path = new NavMeshPath();
                        //agent.CalculatePath(attackTarget.transform.position, path);
                        //agent.path = path;
                        //path = null;
                        attackTimer = 0f; 
                    }
                }
                break;
        }

        if(!agent.pathPending)
        {
            if(agent.remainingDistance <= agent.stoppingDistance)
            {
                if(agent.hasPath)
                {
                    agent.ResetPath();
                }
            }
        }
    }

    public void MoveOrder(Vector3 targetPosition)
    {
        state = State.idle;
        //agent.SetDestination(targetPosition);
        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(targetPosition, path);
        agent.path = path;
        path = null;
        attackTarget = null;
    }

    public void AttackOrder(GameObject target)
    {
        state = State.attacking;
        attackTarget = target;
    }

    public void UpdateUI()
    {
        if(uiPanel)
        {
            uiPanel.transform.Find("CURRENT_HEALTH").GetComponent<Text>().text = currentHealth.ToString("#.#") + " / " + maxHealth.ToString();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    public NavMeshAgent agent;
    public Transform player;
    public Transform attackVFX;
    public LayerMask whatIsGround, whatIsPlayer;
    public int Health;
    public Animator anim;

    [Header("Patroling")]
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    [Header("Attacking")]
    public float timeBetweenAttacks;
    bool alreadyAttacked;

    [Header("States")]
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    public List<Transform> players = new List<Transform>();
    AudioManager audioManager;

    public void Awake()
    {
        //audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();

        // Hopefully the enemy will go and search for both "Player" and "Player2" tags
        players.Clear();
        foreach (var go in GameObject.FindGameObjectsWithTag("Player1"))
        {
            players.Add(go.transform);
        }

        foreach (var go in GameObject.FindGameObjectsWithTag("Player2"))
        {
            players.Add(go.transform);
        }

        // And here the lil bastard can choose to pick the closest player as its target
        if (players.Count > 0)
        {
            player = players[0]; // there is a more techy way to do this but Im gonna tweak if i read more fucking code
        } //ths is the bullshit i get for doing capture the flag in multiplayer... bloddy hell

        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        //In update to constantly check for the player
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange)
        {
            Patroling();
        }

        if (playerInSightRange && !playerInAttackRange)
        {
            ChasePlayer();
        }

        if (playerInAttackRange && playerInSightRange)
        {
            AttackPlayer();
        }
    }

    public void Patroling()
    {
        anim.SetBool("isPatrolling", true);
        if (!walkPointSet) SearchWalkPoint();
        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
        {
            walkPointSet = false;
        }
        anim.SetBool("isChasing", false);
        anim.SetBool("isAttackingPlayer", false);
        //audioManager.PlayEnemySFX(audioManager.enemygrunt);
    }

    private void SearchWalkPoint()
    {
        //Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
        {
            walkPointSet = true;
        }
    }

    private void ChasePlayer()
    {
        anim.SetBool("isChasing", true);
        anim.SetBool("isPatrolling", false);
        anim.SetBool("isAttackingPlayer", false);
        agent.SetDestination(player.position);
        //audioManager.PlayEnemySFX(audioManager.enemyFoundPlayer);
    }

    private void AttackPlayer()
    {
        //Make sure enemy doesn't move
        agent.SetDestination(transform.position);

        transform.LookAt(player);
        anim.SetBool("isAttackingPlayer", true);
        anim.SetBool("isChasing", false);
        anim.SetBool("isPatrolling", false);
        //audioManager.PlayEnemySFX(audioManager.enemyAttack);

        if (!alreadyAttacked)
        {
            ///Attack logic here
            Debug.Log("Enemy Attacked");
            if (attackVFX != null)
            {
                attackVFX.gameObject.SetActive(true);
                Invoke(nameof(DisableAttackVFX), 0.5f);
            }

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void DisableAttackVFX()
{
    if (attackVFX != null)
    {
        attackVFX.gameObject.SetActive(false);
    }
}

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("PlayerAttack"))
        {
            Debug.Log("Player hit by enemy");
            TakeDamage(20); // Enemy takes damage when colliding with player
        }
        if (collision.gameObject.CompareTag("Player1"))
        {
            Debug.Log("Player hit by enemy");
            TakeDamage(20); // Enemy takes damage when colliding with player
        }
        if (collision.gameObject.CompareTag("Player2"))
        {
            Debug.Log("Player hit by enemy");
            TakeDamage(20); // Enemy takes damage when colliding with player
        }
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;

        if (Health <= 0)
        {
            anim.SetBool("isChasing", false);
            anim.SetBool("isPatrolling", false);
            anim.SetBool("isAttackingPlayer", false);
            anim.SetBool("isDead", true);
            Invoke(nameof(DestroyEnemy), 0.5f);
        }
    }
    
    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }
}

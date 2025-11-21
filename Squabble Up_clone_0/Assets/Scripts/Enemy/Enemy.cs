using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public class Enemy : NetworkBehaviour
{
    [Header("Enemy Settings")]
    public NavMeshAgent agent;
    public Transform player;
    public Transform attackVFX;
    public LayerMask whatIsGround, whatIsPlayer;
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

    [Header("Spawning")]
    public float spawnCooldown = 2f; // Time before enemy starts chasing
    private bool canChasePlayers = false;
    private Vector3 originalSpawnPosition; // Store original position

    public List<Transform> players = new List<Transform>();
    AudioManager audioManager;

    [Header("Network Stuff")] //things that need to be synced over the network
    private NetworkVariable<int> networkHealth = new NetworkVariable<int>(100);
    private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> networkRotation = new NetworkVariable<Quaternion>();
    private NetworkVariable<bool> networkCanChase = new NetworkVariable<bool>(false);

    public void Awake()
    {
        //audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        originalSpawnPosition = transform.position;
        
    }

    public int Health 
    { 
        get => networkHealth.Value; 
        private set => networkHealth.Value = value; 
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer) // Only the server should handle the enemy logic otherwise it wont be streamlined with the other instances
        {

            transform.position = originalSpawnPosition;
            networkPosition.Value = originalSpawnPosition;

            // Hopefully the enemy will go and search for both "Player" and "Player2" tags
            
            networkCanChase.Value = false; // Start with chase disabled
            // cause these fucking bastards would swarm the player as they spawn

            // Start cooldown coroutine
            StartCoroutine(SpawnCooldown());

            agent = GetComponent<NavMeshAgent>();

            // Re-enable NavMeshAgent after position reset
            if (agent != null)
            {
                agent.enabled = false; // Disable temporarily to warp position
                agent.Warp(originalSpawnPosition);
                agent.enabled = true;
            }
        }

        // All clients subscribe to health changes
        networkHealth.OnValueChanged += OnHealthChanged;
        networkCanChase.OnValueChanged += OnChaseStateChanged;
        
        // Sync initial position/rotation
        if (!IsServer)
        {
            transform.position = networkPosition.Value;
            transform.rotation = networkRotation.Value;
        }
    }

    private IEnumerator SpawnCooldown()
    {
        yield return new WaitForSeconds(spawnCooldown);
        
        // After cooldown, find players and enable chasing
        FindAllPlayers();
        networkCanChase.Value = true;
    }

    private void FindAllPlayers()
    {
        if (!IsServer) return;

        players.Clear();
        
        // Find all player network objects more reliably
        foreach (var playerObj in FindObjectsByType<Player>(FindObjectsSortMode.None))
        {
            if (playerObj.IsSpawned && playerObj.IsOwner)
            {
                players.Add(playerObj.transform);
            }
        }

        // Fallback to tag-based search
        if (players.Count == 0)
        {
            foreach (var go in GameObject.FindGameObjectsWithTag("Player1"))
            {
                players.Add(go.transform);
            }
            foreach (var go in GameObject.FindGameObjectsWithTag("Player2"))
            {
                players.Add(go.transform);
            }
        }

        if (players.Count > 0)
        {
            player = GetClosestPlayer();
        }
    }

    private void OnChaseStateChanged(bool oldValue, bool newValue)
    {
        canChasePlayers = newValue;
    }

    private Transform GetClosestPlayer()
    {
        Transform closestPlayer = null;
        float closestDistance = Mathf.Infinity;

        foreach (Transform playerTransform in players)
        {
            if (playerTransform == null) continue;
            
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = playerTransform;
            }
        }

        return closestPlayer;
    }

    void Update()
    {
        if (!IsServer) return;

        // Server updates network variables
        if (Vector3.Distance(transform.position, networkPosition.Value) > 0.1f)
        {
            networkPosition.Value = transform.position;
        }
        
        if (Quaternion.Angle(transform.rotation, networkRotation.Value) > 1f)
        {
            networkRotation.Value = transform.rotation;
        }

        // Only run AI logic if chase is enabled
        if (!networkCanChase.Value)
        {
            // Just patrol during cooldown
            Patroling();
            return;
        }

        // Update player reference periodically
        if (player == null || Time.frameCount % 60 == 0) // Update every ~1 second
        {
            FindAllPlayers();
        }

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

    void LateUpdate()
    {
        // Clients sync position/rotation from network variables
        if (!IsServer)
        {
            if (Vector3.Distance(transform.position, networkPosition.Value) > 0.1f)
            {
                transform.position = Vector3.Lerp(transform.position, networkPosition.Value, Time.deltaTime * 10f);
            }
            
            if (Quaternion.Angle(transform.rotation, networkRotation.Value) > 1f)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation.Value, Time.deltaTime * 10f);
            }
        }
    }

    private void OnHealthChanged(int oldHealth, int newHealth)
    {
        // Handle health changes on all clients
        if (newHealth <= 0)
        {
            HandleDeath();
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

        // Don't chase if no valid player target
        if (player == null) 
        {
            Patroling();
            return;
        }
        
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

    private void HandleDeath()
    {
        anim.SetBool("isChasing", false);
        anim.SetBool("isPatrolling", false);
        anim.SetBool("isAttackingPlayer", false);
        anim.SetBool("isDead", true);
        
        if (IsServer)
        {
            Invoke(nameof(DestroyEnemy), 0.5f);
        }
    }
    
    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    public override void OnNetworkDespawn()
    {
        networkHealth.OnValueChanged -= OnHealthChanged;
    }
}

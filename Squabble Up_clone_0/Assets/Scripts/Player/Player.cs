using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]

public class Player : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float speed = 12f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundLayer;

    [Header("Enemy Settings")]
    public Enemy enemy;                       // Reference to the enemy script
    public Transform lightAttackVFX;          // Reference to the light attack VFX transform

    [Header("Camera Settings")]
    public Camera playerCamera;
    public Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float controllerSensitivity = 200f;
    public float smoothTime = 0.05f;        // How quickly the camera lerps
    public float minLookY = -60f;           // Clamping the vertical look (up) 
    public float maxLookY = 60f;            // Clamping the vertical look (down)

    [Header("Player Stuff")] //Fuck off, idk im gettng tired of writing these headers
    public int maxHealth = 100;
    private int currentHealth;
    private static int playerCount = 0; //this is this track how many players have spawned 
    public bool death;
    public int deathCount = 0;
    public event Action<int, int> OnHealthChanged; // currentHealth, maxHealth
    public int playerNumber = 0; // Added to identify player (1 or 2), this is for my hpBar script
    public bool isP1;  //bools to check if player1
    public bool isP2; //bools to check if player2
    public Transform deathScreen1;
    public Transform deathScreen2;
    private bool isRespawning = false;
    private float respawnTimer = 0f;
    public float respawnDelay = 2f; // Delay before respawning
    private bool deathTriggered = false; // Prevent multiple death triggers cause this lil turd went up to 57 deaths after dying like three times
    public event Action<int> OnPlayerDeath; // so that the game manager can pick up whenever the player dies and also uses the player number
    public bool isLocalPlayer = false;

    [Header("Components")] //cause i genuinely dont know what to call this part and its annoying that its not fucking organised
    public Rigidbody rb;
    public Animator anim;
    private Vector2 currentInput;           // Current input from keyboard/gamepad
    private Vector2 currentLook;            // Current input from mouse/gamepad
    private Vector2 smoothLook;             // Smoothed look direction
    private Vector2 lookVelocity;           // Velocity used by SmoothDamp
    private Vector3 velocity;               // Jump velocity
    private bool isGrounded;
    private float camRotationX;             // Vertical camera rotation
    public Transform spawnPoint;            //Spawn Point

    [Header("Input Actions")]
    public InputAction moveAction;         // Input action for movement
    public InputAction lookAction;         // Input action for looking around
    public InputAction jumpAction;         // Input action for jumping
    public InputAction attackAction;  // Input action for attack

    //NetworkVariable<int> netHealth = new NetworkVariable<int>(); //this is used to sync health across the network, might not need it tho

    public override void OnNetworkSpawn()
    {
        playerCamera = GetComponentInChildren<Camera>(); // Get the camera component from children
        anim = GetComponentInChildren<Animator>(); // Get the animator component from children

        Awake();
        if (!IsOwner)
        {
            // Disable components for non-local players
            rb.isKinematic = true; // Make Rigidbody kinematic for non-local players
            GetComponent<PlayerInput>().enabled = false; // Disable PlayerInput for non-local players
            anim.enabled = false; // Disable Animator for non-local players
            this.enabled = false; // Disable this script for non-local players
            playerCamera.enabled = false; // this is for my camera functionality and to check if this little bastard of a player is the owner of this device
        }

        if (playerCamera != null) //Using this method to help seperate my displays. ALWAYS read the Unity API.
        {
            playerCamera.enabled = true;
            isLocalPlayer = true;
        } //You better work you whore

    }


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        //To assign player tags and shit, im starting to get fucking annyed with this bs in the fucking background
        if (playerCount % 2 == 0)
        {
            gameObject.tag = "Player1";
            isP1 = true;
            isP2 = false;
        }
        else
        {
            gameObject.tag = "Player2";
            isP1 = false;
            isP2 = true;
        }
        playerCount++;
        playerNumber = playerCount;

        if (playerCamera != null & !isLocalPlayer)
        {
            playerCamera.enabled = false;
        }

        // if (isP2 = true && isP1 = false) 
        // {
        //     Display.Activate();
        // } //trying to make the game switch displays whenever the player spawns on the 

        // Initialize health
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        //Get the player's input actions
        var playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        jumpAction = playerInput.actions["Jump"];
        attackAction = playerInput.actions["Attack"];
    }

    void OnEnable()     // Subscribe to input actions when the script is enabled
    {
        moveAction.Enable();
        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;

        lookAction.Enable();
        lookAction.performed += OnLook;
        lookAction.canceled += OnLook;

        jumpAction.Enable();
        jumpAction.performed += OnJump;

        attackAction.Enable();
        attackAction.performed += OnAttack;
    }

    void OnDisable()   // Unsubscribe from input actions when the script is disabled
    {
        moveAction.performed -= OnMove;
        moveAction.canceled -= OnMove;

        lookAction.performed -= OnLook;
        lookAction.canceled -= OnLook;

        jumpAction.performed -= OnJump;

        attackAction.performed -= OnAttack;
    }

    // Called whenever Move input changes
    public void OnMove(InputAction.CallbackContext context)
    {
        currentInput = context.ReadValue<Vector2>();
    }

    // Called whenever Look input changes
    public void OnLook(InputAction.CallbackContext context)
    {
        currentLook = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            // Jump velocity based on physics equation
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            anim.SetBool("isJumping", true);
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Trigger light attack animation
            anim.SetBool("isAttacking", true);

            // Show light attack VFX
            if (lightAttackVFX != null)
            {
                lightAttackVFX.gameObject.SetActive(true);
                Invoke(nameof(DisableLightAttackVFX), 0.5f); // Adjust delay as needed
            }

            // Check if enemy is in range and apply damage
            if (enemy != null)
            {
                float distanceToEnemy = Vector3.Distance(lightAttackVFX.position, enemy.transform.position);

                if (distanceToEnemy <= 2f) // Assuming 2 units is the attack range
                {
                    enemy.TakeDamage(10); // Doing damage to enemy
                }
            }
        }
    }

    private void DisableLightAttackVFX()
    {
        if (lightAttackVFX != null)
        {
            lightAttackVFX.gameObject.SetActive(false);
            anim.SetBool("isAttacking", false);
        }
        
    }

    void Update()
    {
        if (!IsOwner)
        {
            return; // Ensure only the local player processes input and movement
        }

        // Groundcheck lol
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);

        // Reset vertical velocity if grounded and falling
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // small downward force keeps player grounded
            anim.SetBool("isJumping", false);
        }

        HandleCameraLook(); //Calling this in Update for smoother camera movement

        //Gonna call death here for now cause I genuinely dont give a flying fuck rn
        //after so many fucking attemps hopefully this bullshit-ass respawn will fucking work, bloody poes
        if (isRespawning)
        {
            respawnTimer += Time.deltaTime;
            if (respawnTimer >= respawnDelay)
            {
                Respawn(spawnPoint.position);
                isRespawning = false;
                respawnTimer = 0f;
            }
        }
    }

    void FixedUpdate()
    {
        HandleMovement();//Rather call this in FixedUpdate for physics-based movement (and Im lowkey experimenting here)
    }

    void HandleMovement()
    {
        // real-time movement cause the orignal one was fucking out and made me tweak a bit....
        Vector3 move = (transform.right * currentInput.x + transform.forward * currentInput.y).normalized * speed;

        // movement along x with the rb, if this fucks up I'm gonna tweak cause it was working before
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);

        // Apply gravity & jump (velocity.y is modified in Update or OnJump)
        velocity.y += gravity * Time.fixedDeltaTime;
        rb.AddForce(Vector3.up * velocity.y, ForceMode.Acceleration);

        // Update animations
        bool isRunning = currentInput.magnitude > 0.1f;
        anim.SetBool("isRunning", isRunning);
    }

    void HandleCameraLook()
    {
        // Detect if the player is using mouse input
        bool usingMouse = Mouse.current != null && Mouse.current.delta.ReadValue() != Vector2.zero;

        // Scale look input depending on input device
        Vector2 scaledLook = usingMouse
        ? currentLook * mouseSensitivity
        : currentLook * controllerSensitivity * Time.deltaTime;

        // Smooth input with Lerp (or SmoothDamp for extra smoothness)
        smoothLook = Vector2.SmoothDamp(smoothLook, currentLook, ref lookVelocity, smoothTime); //using the ref to keep track of the velocity to make the smoothing work

        // Horizontal rotation (rotate the player body)
        transform.Rotate(Vector3.up * smoothLook.x);

        // Vertical rotation (rotate camera only)
        camRotationX -= smoothLook.y;
        camRotationX = Mathf.Clamp(camRotationX, minLookY, maxLookY);

        cameraTransform.localRotation = Quaternion.Euler(camRotationX, 0f, 0f);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Notify health bar of the change
        OnHealthChanged?.Invoke(currentHealth, maxHealth); //istg, ive been beefing with this for hours now, fuck code honestly...

        if (currentHealth <= 0) Invoke(nameof(Death), 0.5f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("PlayerAttack"))
        {
            Debug.Log("Player hit by enemy");
            TakeDamage(5); // Player takes damage when colliding with enemy
        }

        if (collision.gameObject.CompareTag("Player2") && isP1)
        {
            Debug.Log("P2 is chowing you dude");
            TakeDamage(15); // Player takes damage when colliding with enemy

        }
        else if (collision.gameObject.CompareTag("Player2") && isP2)
        {
            Debug.Log("P1 is frying you");
            TakeDamage(15); // Player takes damage when colliding with enemy
        }

        if (collision.gameObject.CompareTag("DeathFloor"))
            {
                Debug.Log("Player fell off the map and died like a bitch");
                Death(); // Player dies instantly when hitting the death floor
            }
    }

    public void Respawn(Vector3 spawnPoint) //fuck you respawn
    {
        // Reset physics state
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        velocity = Vector3.zero;

        // Position the player slightly above the spawn point to avoid ground collision issues
        Vector3 respawnPosition = spawnPoint + Vector3.up * 0.5f;
        transform.position = respawnPosition;

        death = false;
        deathTriggered = false; // Reset death trigger
        currentHealth = maxHealth; // Reset health or any other necessary stats
        Debug.Log("Player Respawned");

        // Notify about health change
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (isP1 == true && death == false)
        {
            deathScreen1.gameObject.SetActive(false);
        }
        else if (isP2 == true && death == false)
        {
            deathScreen2.gameObject.SetActive(false);
        }

        // Force ground check update
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);
        anim.SetBool("isDead", false);
    }

    public void Death()
    {
        if (deathTriggered) return; // trying to avoid this getting called too much

        deathTriggered = true;
        anim.SetBool("isDead", true);
        death = true;
        deathCount++;
        OnPlayerDeath?.Invoke(playerNumber); //the player dies and their playernumber is linked and calling the event
        Debug.Log("Player Died");
        //SceneManager.LoadScene("Death Scene");
        if (isP1 == true && death == true)
        {
            deathScreen1.gameObject.SetActive(true);
        }
        else if (isP2 == true && death == true)
        {
            deathScreen2.gameObject.SetActive(true);
        }
        
        // Start respawn timer instead of immediately respawning
        // isRespawning = true;
        // respawnTimer = 0f;

    }

     // Public method to get current health
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    // Public method to get max health
    public int GetMaxHealth()
    {
        return maxHealth;
    }
}

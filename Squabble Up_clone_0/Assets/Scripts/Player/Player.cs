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
    private bool deathTriggered = false; // Prevent multiple death triggers cause this lil turd went up to 57 deaths after dying
    public event Action<int> OnPlayerDeath; // so that the game manager can pick up whenever the player dies and also uses the player number
    public bool isLocalPlayer = false;
    public bool dmgBlock = false;
    public Transform dmgDisplay;

    [Header("Combo Shenanigans")]
    public int comboLevel = 0; //There's levels to the combo string
    public float inputWindow = 3f; //time frame for when the player can make an input for the combo
    public float comboResetTime = 2f; //The time before the whole combo resets
    private float lastAttackTime = 0f; //time since the last attack for the combo
    private bool canAcceptInput = true; //this stops the player from spamming like a fucking turd, goddamn button mashing bastards
    public float inputCooldown = 0.3f; //the minimun time between attacks

    [Header("Network Stuff")]
    // Network variables for combo state
    private NetworkVariable<int> networkComboStage = new NetworkVariable<int>(0);
    private NetworkVariable<bool> networkIsAttacking = new NetworkVariable<bool>(false);
    //you could never pay enough fucking money to make a combo system again, this is Satan's work ong
    // Network-synced player number assigned by the server
    private NetworkVariable<int> networkPlayerNumber = new NetworkVariable<int>(
        0, readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Server);

    // Server-side counter used to assign player numbers (increment only on server)
    private static int serverPlayerCount = 0;

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
    private bool controlsEnabled = true;

    [Header("Input Actions")]
    public InputAction moveAction;         // Input action for movement
    public InputAction lookAction;         // Input action for looking around
    public InputAction jumpAction;         // Input action for jumping
    public InputAction attackAction;       // Input action for attack
    public InputAction blockAction;        // Input action for blocking

    //NetworkVariable<int> netHealth = new NetworkVariable<int>(); //this is used to sync health across the network, might not need it tho

    public override void OnNetworkSpawn()
    {
        //moving these two here since it'll be needed when the player spawns in
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        playerCamera = GetComponentInChildren<Camera>(); // Get the camera component from children
        anim = GetComponentInChildren<Animator>(); // Get the animator component from children

        if (!IsOwner)
        {
            // Disable components for non-local players
            rb.isKinematic = true; // Make Rigidbody kinematic for non-local players
            GetComponent<PlayerInput>().enabled = false; // Disable PlayerInput for non-local players

            this.enabled = false; // Disable this script for non-local players

            if (playerCamera != null)
            {
                playerCamera.enabled = false; // this is for my camera functionality and to check if this little bastard of a player is the owner of this device
            }
        }

        // Subscribe to network variable changes
        networkComboStage.OnValueChanged += OnComboStageChanged;
        networkIsAttacking.OnValueChanged += OnIsAttackingChanged;
        networkPlayerNumber.OnValueChanged += OnNetworkPlayerNumberChanged;
        
        isLocalPlayer = true; // Mark this player as the local player

        if (playerCamera != null) //Using this method to help seperate my displays. ALWAYS read the Unity API.
        {
            playerCamera.enabled = true;
            isLocalPlayer = true;
        } //You better work you whore

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

        if (IsServer && IsClient)
        {
            gameObject.tag = "Player1";
            isP1 = true;
            isP2 = false;
        }
        else if (!IsServer && IsClient)
        {
            gameObject.tag = "Player2";
            isP1 = false;
            isP2 = true;
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
        blockAction = playerInput.actions["Block"];
    }

    void Awake()
    {
        //Only keep absolutely essential non-network initialization here
        //For example, getting component references that don't depend on network state
        rb = GetComponent<Rigidbody>();
        //Don't do player numbering or input setup here  
        //hopefulyy my animations will display across screens 
    }

    private void OnComboStageChanged(int oldValue, int newValue)
    {
        comboLevel = newValue;
        // You can add visual/audio feedback for combo changes here
    }

    private void OnIsAttackingChanged(bool oldValue, bool newValue)
    {
        // Sync attack state if needed
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

        blockAction.Enable();
        blockAction.performed += OnBlock;
    }

    void OnDisable()   // Unsubscribe from input actions when the script is disabled
    {
        moveAction.performed -= OnMove;
        moveAction.canceled -= OnMove;

        lookAction.performed -= OnLook;
        lookAction.canceled -= OnLook;

        jumpAction.performed -= OnJump;

        attackAction.performed -= OnAttack;

        blockAction.performed -= OnBlock;

        // Unsubscribe network var handlers
        networkComboStage.OnValueChanged -= OnComboStageChanged;
        networkIsAttacking.OnValueChanged -= OnIsAttackingChanged;
        networkPlayerNumber.OnValueChanged -= OnNetworkPlayerNumberChanged;
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

    public void OnBlock(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            dmgBlock = true;
            anim.SetBool("isBlocking", true);
        }
        else if (context.canceled)
        {
            dmgBlock = false;
            anim.SetBool("isBlocking", false);
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.performed || !canAcceptInput || !IsOwner)
        {
            return;
        }

        float timeSinceLastAttack = Time.time - lastAttackTime; //checking if it's within the combo window

        if (timeSinceLastAttack > comboResetTime)
        {
            comboLevel = 0; //reset combo if too much time has passed
        }

        // to advance the combo level
        comboLevel++;
        lastAttackTime = Time.time;

        StartCoroutine(InputCooldownCoroutine()); //Start input cooldown, cause we dont want those whackass button mashers to have a winning chance lol

        HandleComboAttack(); //Handles the combo logic

        if (IsServer) //syncs this over the network
        {
            networkComboStage.Value = comboLevel;
            networkIsAttacking.Value = true;
        }
        //Idea, maybe make a counter that increases every time you press a button, then it resets after a certain interval.
        //So maybe I should go fuck myself cause this a lot of bullshit lol

    }

    private void OnNetworkPlayerNumberChanged(int oldValue, int newValue)
    {
        playerNumber = newValue;
        isP1 = (newValue == 1);
        isP2 = (newValue == 2);

        // Set tag for this object on all instances (this runs on everyone via the network change)
        gameObject.tag = isP1 ? "Player1" : "Player2";

        // // Optionally enable/disable UI per player here (owner-specific UI should check IsOwner)
        // if (IsOwner)
        // {
        //     // example: enable local camera already handled elsewhere, but you can toggle displays here
        // }
    }

    private void HandleComboAttack()
    {
        // Trigger appropriate animation based on combo stage
        switch (comboLevel)
        {
            case 1:
                anim.SetBool("isAttacking", true);
                Debug.Log("Combo: First Attack");
                break;
            case 2:
                anim.SetBool("hit2", true);
                Debug.Log("Combo: Second Attack");
                break;
            case 3:
                anim.SetBool("hit3", true);
                Debug.Log("Combo: Third Attack");
                break;
            case 4:
                anim.SetBool("hit4", true);
                Debug.Log("Combo: Fourth Attack");
                break;
            case 5:
                anim.SetBool("finalhit", true);
                Debug.Log("Combo: Finisher!");
                // Reset combo after finisher
                comboLevel = 0;
                if (IsServer) networkComboStage.Value = 0;
                break;
            default: // Safety reset
                comboLevel = 0;
                if (IsServer) networkComboStage.Value = 0;
                break;
        }

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
                enemy.TakeDamage(10 + (comboLevel * 2)); // Doing damage to enemy
            }
        }
    }
    
    private System.Collections.IEnumerator InputCooldownCoroutine()
    {
        canAcceptInput = false;
        yield return new WaitForSeconds(inputCooldown);
        canAcceptInput = true;
    }

    private void DisableLightAttackVFX()
    {
        if (lightAttackVFX != null)
        {
            lightAttackVFX.gameObject.SetActive(false);
            anim.SetBool("isAttacking", false);
        }

        if (IsServer) // Reset attacking state on the network
        {
            networkIsAttacking.Value = false;
        }
        
    }

    void Update()
    {
        if (!IsOwner)
        {
            return; // Ensure only the local player processes input and movement
        }

        // Combo reset timer
        if (comboLevel > 0 && (Time.time - lastAttackTime) > comboResetTime)
        {
            comboLevel = 0;
            if (IsServer) networkComboStage.Value = 0;
            Debug.Log("Combo Reset");
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

    public void SetControlsEnabled(bool enabled)
    {
        controlsEnabled = enabled;

        // disable/enable the input actions (safe if they are already initialized)
        if (moveAction != null) { if (enabled) moveAction.Enable(); else moveAction.Disable(); }
        if (jumpAction != null) { if (enabled) jumpAction.Enable(); else jumpAction.Disable(); }
        if (attackAction != null) { if (enabled) attackAction.Enable(); else attackAction.Disable(); }

        // optionally stop physics movement while disabled
        if (rb != null) rb.isKinematic = !enabled;

        // stop accepting combo input
        canAcceptInput = enabled;
    }

    public void DisableControls(float seconds)
    {
        if (!IsOwner) return; // only affect local owner
        StartCoroutine(DisableControlsCoroutine(seconds));
    }
    
    private System.Collections.IEnumerator DisableControlsCoroutine(float seconds)
    {
        SetControlsEnabled(false);
        yield return new WaitForSeconds(seconds);
        SetControlsEnabled(true);
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
        dmgDisplay.gameObject.SetActive(true);
        Invoke(nameof(DisableDmgScreen), 0.5f);

        // Notify health bar of the change
        OnHealthChanged?.Invoke(currentHealth, maxHealth); //istg, ive been beefing with this for hours now, fuck code honestly...

        if (currentHealth <= 0) Invoke(nameof(Death), 0.5f);
    }

    public void DisableDmgScreen()
    {
        dmgDisplay.gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("PlayerAttack"))
        {
            Debug.Log("Player hit by enemy");
            TakeDamage(5); // Player takes damage when colliding with enemy

            if (dmgBlock)
            {
                TakeDamage(0);
            }
        }

        if (collision.gameObject.CompareTag("Player2") && isP1)
        {
            Debug.Log("P2 is chowing you dude");
            TakeDamage(15); // Player takes damage when colliding with enemy

            if (dmgBlock)
            {
                TakeDamage(0);
            }

        }
        else if (collision.gameObject.CompareTag("Player1") && isP2)
        {
            Debug.Log("P1 is frying you");
            TakeDamage(15); // Player takes damage when colliding with enemy

            if (dmgBlock)
            {
                TakeDamage(0);
            }
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

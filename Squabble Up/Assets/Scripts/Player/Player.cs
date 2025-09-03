using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]

public class Player : MonoBehaviour
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
    public Transform cameraTransform;
    public float lookSensitivity = 3f;      // Controling the look sensitivity
    public float smoothTime = 0.05f;        // How quickly the camera lerps
    public float minLookY = -60f;           // Clamping the vertical look (up) 
    public float maxLookY = 60f;            // Clamping the vertical look (down)

    [Header("Player Stuff")] //Fuck off, idk im gettng tired of writing these headers
    public int health;
    private static int playerCount = 0; //this is this track how many players have spawned 
    public bool death;
    public int deathCount = 0;

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
    private InputAction moveAction;         // Input action for movement
    private InputAction lookAction;         // Input action for looking around
    private InputAction jumpAction;         // Input action for jumping
    private InputAction attackAction;  // Input action for attack


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        //To assign player tags and shit, im starting to get fucking annyed with this bs in the fucking background
        if (playerCount % 2 == 0)
        {
            gameObject.tag = "Player1";
        }
        else
        {
            gameObject.tag = "Player2";
        }
        playerCount++;

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
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Trigger light attack animation
            //anim.SetTrigger("lightAttack");

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
        }
    }

    void Update()
    {
        // Groundcheck lol
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);

        // Reset vertical velocity if grounded and falling
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // small downward force keeps player grounded
        }

        HandleCameraLook(); //Calling this in Update for smoother camera movement

        //Gonna call death here for now cause I genuinely dont give a flying fuck rn
        if (health <= 0)
        {
            Death();
        }

        // if (deathCount <= 4) //if you havent died at least 4 times, then you revive
        // {
        //     Respawn(spawnPoint.position);
        // }
        // else if (deathCount >= 4) //if you die 4 times then game over.
        // {
        //     //load death scene, rn we gonna quit apllicationor whatever
        // }
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
        // Smooth input with Lerp (or SmoothDamp for extra smoothness)
        smoothLook = Vector2.SmoothDamp(smoothLook, currentLook, ref lookVelocity, smoothTime); //using the ref to keep track of the velocity to make the smoothing work

        // Horizontal rotation (rotate the player body)
        transform.Rotate(Vector3.up * smoothLook.x * lookSensitivity * Time.deltaTime);

        // Vertical rotation (rotate camera only)
        camRotationX -= smoothLook.y * lookSensitivity * Time.deltaTime;
        camRotationX = Mathf.Clamp(camRotationX, minLookY, maxLookY);

        cameraTransform.localRotation = Quaternion.Euler(camRotationX, 0f, 0f);
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0) Invoke(nameof(Death), 0.5f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Player hit by enemy");
            TakeDamage(10); // Player takes damage when colliding with enemy
        }

        if (collision.gameObject.CompareTag("DeathFloor"))
        {
            Debug.Log("Player fell off the map and died like a bitch");
            Death(); // Player dies instantly when hitting the death floor
        }
    }

    // public void Respawn(Vector3 spawnPoint) //fuck you respawn
    // {
    //     death = false;
    //     gameObject.SetActive(true);
    //     transform.position = spawnPoint;
    //     health = 100; // Reset health or any other necessary stats
    //     Debug.Log("Player Respawned");
    // }

    public void Death()
    {
        gameObject.SetActive(false);
        death = true;
        deathCount++;
        Debug.Log("Player Died");
        SceneManager.LoadScene("Death Scene");
    }
}

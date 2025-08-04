using UnityEngine;

public class PlayerTemp : MonoBehaviour
{
    public CharacterController controller;

    [Header("Movement Settings")]
    public float speed = 12f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    
    Vector3 velocity;
    public bool isGrounded;
    public bool disableGravity = false;
    public Transform cameraTransform;



    void Start()
    {
        if (cameraTransform == null)
        cameraTransform = Camera.main.transform;
    }


    public void Update()
    {
        // Ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // Standard movement input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Use camera's forward and right, but ignore y to keep movement horizontal (crossing my fingers and hoping this works lol)
        Vector3 camForward = cameraTransform.forward;
        camForward.y = 0;
        camForward.Normalize();
        Vector3 camRight = cameraTransform.right;
        camRight.y = 0;
        camRight.Normalize();

        Vector3 move = camRight * x + camForward * z; //it works, arghfffsdssceeeassdsffdffd
        
        controller.Move(move * speed * Time.deltaTime);

        // Handle jumping and gravity
        if (!disableGravity)
        {
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            velocity.y += gravity * Time.deltaTime;
        }
        else
        {
            velocity.y = 0f;
        }

        // Apply velocity (gravity + jump + impulse)
        controller.Move(velocity * Time.deltaTime);
    }

    
}

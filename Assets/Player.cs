using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float lookSpeed = 2f;
    public float jumpForce = 5f;
    public float runSpeedMultiplier = 1.5f;

    private Camera playerCamera;
    private float verticalRotation = 0f;
    private Rigidbody rb;
    private bool isGrounded = false;
    private bool flyingMode = false;
    private bool jumpKeyPressedOnce = false;

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked; // Lock cursor to center
    }

    void Update()
    {
        MovePlayer();
        LookAround();
        HandleJumpAndFlying();
    }

    private void MovePlayer()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;

        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? runSpeedMultiplier : 1f);
        Vector3 moveDirection = playerCamera.transform.TransformDirection(direction);
        moveDirection.y = 0; // No vertical movement unless jumping or flying

        rb.MovePosition(rb.position + moveDirection * speed * Time.deltaTime);
    }

    private void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -80f, 80f); // Limit vertical look

        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX); // Rotate player horizontally
    }

    private void HandleJumpAndFlying()
    {
        // Detect double space press for toggling flying mode
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!jumpKeyPressedOnce)
            {
                jumpKeyPressedOnce = true;
                Invoke("ResetJumpKeyPress", 0.3f); // Small delay to detect double press
            }
            else
            {
                flyingMode = !flyingMode;
                jumpKeyPressedOnce = false;
                rb.useGravity = !flyingMode; // Disable gravity in flying mode
            }
        }

        if (flyingMode)
        {
            // Control vertical movement in flying mode
            float flyVertical = Input.GetAxis("Jump") - Input.GetAxis("Fire3"); // Jump and Fire3 for up and down
            Vector3 flyDirection = new Vector3(0, flyVertical, 0);
            rb.MovePosition(rb.position + flyDirection * moveSpeed * Time.deltaTime);
        }
        else if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            // Jump if grounded and space is pressed
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false; // Prevent multiple jumps
        }
    }

    private void ResetJumpKeyPress()
    {
        jumpKeyPressedOnce = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if player has landed on a ground object
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
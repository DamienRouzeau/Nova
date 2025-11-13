using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Crouch Settings")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchTransitionSpeed = 10f;

    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    // Components
    private CharacterController controller;
    private Camera playerCamera;
    private PlayerInput playerInput;

    // Movement variables
    private Vector3 velocity;
    private bool isGrounded;
    private bool isCrouching;
    private bool isSprinting;
    private float currentSpeed;
    private float targetHeight;
    private float currentHeight;
    private Vector2 moveInput;
    private Vector2 lookInput;

    // Camera rotation
    private float xRotation = 0f;
    private float initialCameraHeight; // Nouvelle variable

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        playerInput = GetComponent<PlayerInput>();

        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Start()
    {
        // Initialize heights
        currentHeight = standingHeight;
        targetHeight = standingHeight;
        controller.height = standingHeight;

        // Sauvegarder la position initiale de la caméra
        initialCameraHeight = playerCamera.transform.localPosition.y;

        // Create ground check if not assigned
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -controller.height / 2, 0);
            groundCheck = groundCheckObj.transform;
        }
    }

    void OnEnable()
    {
        // S'abonner aux événements du PlayerInput
        if (playerInput != null)
        {
            playerInput.onActionTriggered += OnActionTriggered;
        }
    }

    void OnDisable()
    {
        // Se désabonner
        if (playerInput != null)
        {
            playerInput.onActionTriggered -= OnActionTriggered;
        }
    }

    // Gestionnaire centralisé pour tous les inputs
    private void OnActionTriggered(InputAction.CallbackContext context)
    {
        switch (context.action.name)
        {
            case "Move":
                moveInput = context.ReadValue<Vector2>();
                break;

            case "Look":
                lookInput = context.ReadValue<Vector2>();
                break;

            case "Jump":
                if (context.performed && isGrounded && !isCrouching)
                {
                    velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                }
                break;

            case "Sprint":
                if (context.performed)
                {
                    if (isCrouching)
                    {
                        // Check if there's space to stand up
                        if (!Physics.Raycast(transform.position, Vector3.up, standingHeight - crouchHeight + 0.1f, groundMask))
                        {
                            isCrouching = false;
                            targetHeight = standingHeight;
                        }
                    }
                    isSprinting = true;
                }
                else if (context.canceled)
                {
                    isSprinting = false;
                }
                break;

            case "Crouch":
                if (context.performed)
                {
                    if (isCrouching)
                    {
                        // Check if there's space to stand up
                        if (!Physics.Raycast(transform.position, Vector3.up, standingHeight - crouchHeight + 0.1f, groundMask))
                        {
                            isCrouching = false;
                            targetHeight = standingHeight;
                        }
                    }
                    else
                    {
                        isCrouching = true;
                        targetHeight = crouchHeight;
                    }
                }
                break;
        }
    }

    void Update()
    {
        HandleGroundCheck();
        HandleMouseLook();
        HandleMovement();
        HandleCrouchTransition();
        ApplyGravity();
    }

    void HandleGroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // Reset velocity when grounded
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    void HandleMouseLook()
    {
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        // Rotate camera up/down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotate player left/right
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        // Determine current speed
        if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        }
        else if (isSprinting)
        {
            currentSpeed = sprintSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }

        // Calculate move direction
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    void HandleCrouchTransition()
    {
        // Smooth height transition
        if (Mathf.Abs(currentHeight - targetHeight) > 0.01f)
        {
            currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * crouchTransitionSpeed);
            controller.height = currentHeight;

            // Calculer la hauteur de caméra proportionnellement
            float heightRatio = currentHeight / standingHeight; // 0.5 quand accroupi, 1.0 debout
            float crouchCameraHeight = initialCameraHeight * 0.5f; // Hauteur caméra accroupi
            float targetCameraHeight = Mathf.Lerp(crouchCameraHeight, initialCameraHeight, heightRatio);

            // Adjust camera position
            Vector3 cameraPos = playerCamera.transform.localPosition;
            cameraPos.y = targetCameraHeight;
            playerCamera.transform.localPosition = cameraPos;

            // Adjust controller center
            controller.center = new Vector3(0, currentHeight / 2, 0);
        }
    }

    void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void OnDrawGizmosSelected()
    {
        // Visualise le ground check
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
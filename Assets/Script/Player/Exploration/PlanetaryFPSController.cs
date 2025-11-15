using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlanetaryFPSController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Sprint System")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 20f; // Par seconde
    [SerializeField] private float staminaRegenRate = 15f;
    [SerializeField] private float staminaRegenDelay = 2f; // Délai avant regen

    [Header("Crouch Settings")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchTransitionSpeed = 10f;

    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float aimSensitivityMultiplier = 0.5f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private Light flashlight;

    // Components
    private CharacterController controller;
    private PlayerInput playerInput;
    private PlayerHealth healthSystem;
    private PlayerOxygen oxygenSystem;
    private WeaponSystem weaponSystem;
    private PlayerInventory inventory;
    private InteractionSystem interactionSystem;

    // Movement variables
    private Vector3 velocity;
    private bool isGrounded;
    private bool isCrouching;
    private bool isSprinting;
    private bool isAiming;
    private float currentSpeed;
    private float targetHeight;
    private float currentHeight;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool canControl = true;

    // Stamina
    private float currentStamina;
    private float lastSprintTime;

    // Camera rotation
    private float xRotation = 0f;
    private float initialCameraHeight;

    // UI Display
    private bool showOxygenUI = false;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        // Get or add systems
        healthSystem = GetComponent<PlayerHealth>();
        oxygenSystem = GetComponent<PlayerOxygen>();
        weaponSystem = GetComponent<WeaponSystem>();
        inventory = GetComponent<PlayerInventory>();
        interactionSystem = GetComponent<InteractionSystem>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentStamina = maxStamina;
    }

    void Start()
    {
        currentHeight = standingHeight;
        targetHeight = standingHeight;
        controller.height = standingHeight;

        if (playerCamera != null)
        {
            initialCameraHeight = playerCamera.transform.localPosition.y;
        }

        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -controller.height / 2, 0);
            groundCheck = groundCheckObj.transform;
        }

        if (flashlight != null)
        {
            flashlight.enabled = false;
        }
    }

    void OnEnable()
    {
        if (playerInput != null)
        {
            playerInput.onActionTriggered += OnActionTriggered;
        }
    }

    void OnDisable()
    {
        if (playerInput != null)
        {
            playerInput.onActionTriggered -= OnActionTriggered;
        }
    }

    private void OnActionTriggered(InputAction.CallbackContext context)
    {
        if (!canControl) return;

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
                if (context.performed && currentStamina > 0)
                {
                    if (isCrouching)
                    {
                        if (!Physics.Raycast(transform.position, Vector3.up, standingHeight - crouchHeight + 0.1f, groundMask))
                        {
                            isCrouching = false;
                            targetHeight = standingHeight;
                        }
                    }
                    isSprinting = true;
                    lastSprintTime = Time.time;
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
                        isSprinting = false;
                    }
                }
                break;

            case "Fire":
                if (weaponSystem != null)
                {
                    if (context.performed)
                        weaponSystem.StartFiring();
                    else if (context.canceled)
                        weaponSystem.StopFiring();
                }
                break;

            case "Aim":
                if (weaponSystem != null)
                {
                    isAiming = context.performed;
                    weaponSystem.SetAiming(isAiming);
                }
                break;

            case "Reload":
                if (context.performed && weaponSystem != null)
                {
                    weaponSystem.Reload();
                }
                break;

            case "Interact":
                if (context.performed)
                {
                    // PRIORITÉ 1 : Si on regarde un objet interactible, interact
                    if (interactionSystem != null && interactionSystem.CanInteract())
                    {
                        interactionSystem.TryInteract();
                    }
                    // PRIORITÉ 2 : Sinon, recharge l'arme
                    else if (weaponSystem != null)
                    {
                        weaponSystem.Reload();
                    }
                }
                break;

            case "Flashlight":
                if (context.performed && flashlight != null)
                {
                    flashlight.enabled = !flashlight.enabled;
                }
                break;

            case "ShowOxygen":
                showOxygenUI = context.performed;
                if (oxygenSystem != null)
                {
                    oxygenSystem.SetUIVisible(showOxygenUI);
                }
                break;

            case "WeaponSwitch":
                if (context.performed && weaponSystem != null)
                {
                    float value = context.ReadValue<float>();

                    if (value > 0.1f)
                    {
                        weaponSystem.SwitchToNextWeapon();
                    }
                    else if (value < -0.1f)
                    {
                        weaponSystem.SwitchToPreviousWeapon();
                    }
                }
                break;

            case "OpenInventory":
                if (context.performed && inventory != null)
                {
                    inventory.ToggleInventoryUI();
                }
                break;
        }
    }

    void Update()
    {
        if (!canControl) return;

        HandleGroundCheck();
        HandleMouseLook();
        HandleMovement();
        HandleStamina();
        HandleCrouchTransition();
        ApplyGravity();
    }

    void HandleGroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    void HandleMouseLook()
    {
        float sensitivity = isAiming ? mouseSensitivity * aimSensitivityMultiplier : mouseSensitivity;
        float mouseX = lookInput.x * sensitivity;
        float mouseY = lookInput.y * sensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        // Determine current speed
        if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        }
        else if (isSprinting && currentStamina > 0)
        {
            currentSpeed = sprintSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
            if (isSprinting && currentStamina <= 0)
            {
                isSprinting = false; // Stop sprint si plus de stamina
            }
        }

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    void HandleStamina()
    {
        if (isSprinting && moveInput.magnitude > 0.1f)
        {
            // Drain stamina
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(0, currentStamina);
            lastSprintTime = Time.time;
        }
        else if (Time.time - lastSprintTime > staminaRegenDelay)
        {
            // Regen stamina
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Min(maxStamina, currentStamina);
        }
    }

    void HandleCrouchTransition()
    {
        if (Mathf.Abs(currentHeight - targetHeight) > 0.01f)
        {
            currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * crouchTransitionSpeed);
            controller.height = currentHeight;

            float heightRatio = currentHeight / standingHeight;
            float crouchCameraHeight = initialCameraHeight * 0.5f;
            float targetCameraHeight = Mathf.Lerp(crouchCameraHeight, initialCameraHeight, heightRatio);

            if (playerCamera != null)
            {
                Vector3 cameraPos = playerCamera.transform.localPosition;
                cameraPos.y = targetCameraHeight;
                playerCamera.transform.localPosition = cameraPos;
            }

            controller.center = new Vector3(0, currentHeight / 2, 0);
        }
    }

    void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // Getters publics
    public float GetStaminaPercentage() => currentStamina / maxStamina;
    public bool IsSprinting() => isSprinting;
    public bool IsAiming() => isAiming;

    public void SetCanControl(bool can)
    {
        canControl = can;
        if (!can)
        {
            moveInput = Vector2.zero;
            lookInput = Vector2.zero;
            isSprinting = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}
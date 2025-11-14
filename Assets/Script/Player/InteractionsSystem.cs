using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class InteractionSystem : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("UI Settings")]
    [SerializeField] private GameObject interactionUI;
    [SerializeField] private TextMeshProUGUI interactionText;

    [Header("Input")]
    [SerializeField] private PlayerInput playerInput;

    [SerializeField] private Camera playerCamera;
    private InputAction interactAction;
    private IInteractable currentInteractable;
    private bool canInteract = false;

    void Awake()
    {
        //playerCamera = GetComponentInChildren<Camera>();

        // Get interact action from Input System
        if (playerInput != null)
        {
            interactAction = playerInput.actions["Interact"];
        }
    }

    void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.performed += OnInteract;
        }
    }

    void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.performed -= OnInteract;
        }
    }

    void Update()
    {
        CheckForInteractable();
    }

    void CheckForInteractable()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        // Debug ray (visible dans Scene view)
        Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.yellow);

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            // Vérifie si l'objet a le component IInteractable
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                // Nouvel objet interactable détecté
                if (currentInteractable != interactable)
                {
                    currentInteractable = interactable;
                    ShowInteractionUI(interactable.GetInteractionPrompt());
                }

                canInteract = true;
                return;
            }
        }

        // Aucun objet interactable détecté
        if (canInteract)
        {
            HideInteractionUI();
            currentInteractable = null;
            canInteract = false;
        }
    }

    void OnInteract(InputAction.CallbackContext context)
    {
        if (canInteract && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    void ShowInteractionUI(string prompt)
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(true);

            if (interactionText != null)
            {
                interactionText.text = prompt;
            }
        }
    }

    void HideInteractionUI()
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }

    // Visualisation du raycast dans l'éditeur
    void OnDrawGizmos()
    {
        if (playerCamera != null)
        {
            Gizmos.color = canInteract ? Color.green : Color.red;
            Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactionDistance);
        }
    }
}
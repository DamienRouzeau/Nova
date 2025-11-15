using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class InteractionSystem : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("UI")]
    [SerializeField] private GameObject interactionUI;
    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private Image progressBar;
    [SerializeField] private GameObject progressBarUI;

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PlayerInventory inventory;

    private IInteractable currentInteractable;
    private bool canInteract = false;
    private bool isInteracting = false;
    private Coroutine interactionCoroutine;

    void Update()
    {
        if (!isInteracting)
        {
            CheckForInteractable();
        }
    }

    void CheckForInteractable()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.yellow);

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                if (currentInteractable != interactable)
                {
                    currentInteractable = interactable;
                    ShowInteractionUI(interactable.GetInteractionPrompt());
                }

                canInteract = true;
                return;
            }
        }

        if (canInteract)
        {
            HideInteractionUI();
            currentInteractable = null;
            canInteract = false;
        }
    }

    public void TryInteract()
    {
        if (!canInteract || currentInteractable == null || isInteracting)
            return;

        // Vérifie le type d'interaction
        if (currentInteractable is WorldItem)
        {
            PickupItem(currentInteractable as WorldItem);
        }
        else if (currentInteractable is MineableResource)
        {
            StartMining(currentInteractable as MineableResource);
        }
        else if (currentInteractable is LootChest)
        {
            StartOpeningChest(currentInteractable as LootChest);
        }
        else
        {
            // Interaction instantanée générique
            currentInteractable.Interact();
        }
    }

    void PickupItem(WorldItem worldItem)
    {
        if (inventory != null && worldItem.itemData != null)
        {
            if (inventory.AddItem(worldItem.itemData, worldItem.quantity))
            {
                Destroy(worldItem.gameObject);
                HideInteractionUI();
            }
            else
            {
                Debug.Log("Inventaire plein!");
            }
        }
    }

    void StartMining(MineableResource resource)
    {
        if (interactionCoroutine != null)
        {
            StopCoroutine(interactionCoroutine);
        }

        interactionCoroutine = StartCoroutine(MiningCoroutine(resource));
    }

    IEnumerator MiningCoroutine(MineableResource resource)
    {
        isInteracting = true;
        ShowProgressBar();

        float miningTime = resource.GetMiningTime();
        float elapsed = 0f;

        while (elapsed < miningTime)
        {
            elapsed += Time.deltaTime;
            UpdateProgressBar(elapsed / miningTime);

            // Annule si le joueur bouge trop ou regarde ailleurs
            if (!IsLookingAt(resource.transform, interactionDistance))
            {
                CancelInteraction();
                yield break;
            }

            yield return null;
        }

        // Minage terminé
        ItemData minedResource = resource.GetResourceData();
        int quantity = resource.GetResourceQuantity();

        if (inventory != null && minedResource != null)
        {
            inventory.AddItem(minedResource, quantity);
        }

        Destroy(resource.gameObject);

        HideProgressBar();
        isInteracting = false;
    }

    void StartOpeningChest(LootChest chest)
    {
        if (interactionCoroutine != null)
        {
            StopCoroutine(interactionCoroutine);
        }

        interactionCoroutine = StartCoroutine(OpenChestCoroutine(chest));
    }

    IEnumerator OpenChestCoroutine(LootChest chest)
    {
        isInteracting = true;
        ShowProgressBar();

        float openTime = chest.GetOpenTime();
        float elapsed = 0f;

        while (elapsed < openTime)
        {
            elapsed += Time.deltaTime;
            UpdateProgressBar(elapsed / openTime);

            if (!IsLookingAt(chest.transform, interactionDistance))
            {
                CancelInteraction();
                yield break;
            }

            yield return null;
        }

        // Ouverture terminée
        HideProgressBar();
        chest.Open();
        isInteracting = false;
    }

    bool IsLookingAt(Transform target, float maxDistance)
    {
        if (target == null || playerCamera == null) return false;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, interactableLayer))
        {
            return hit.transform == target || hit.transform.IsChildOf(target);
        }

        return false;
    }

    void CancelInteraction()
    {
        if (interactionCoroutine != null)
        {
            StopCoroutine(interactionCoroutine);
            interactionCoroutine = null;
        }

        HideProgressBar();
        isInteracting = false;
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

    void ShowProgressBar()
    {
        if (progressBarUI != null)
        {
            progressBarUI.SetActive(true);
        }

        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }

    void HideProgressBar()
    {
        if (progressBarUI != null)
        {
            progressBarUI.SetActive(false);
        }
    }

    void UpdateProgressBar(float progress)
    {
        if (progressBar != null)
        {
            progressBar.fillAmount = progress;
        }
    }

    // Méthode publique pour vérifier si on peut interagir (pour reload/interact contextuel)
    public bool CanInteract()
    {
        return canInteract && currentInteractable != null && !isInteracting;
    }

    void OnDrawGizmos()
    {
        if (playerCamera != null)
        {
            Gizmos.color = canInteract ? Color.green : Color.red;
            Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactionDistance);
        }
    }
}

// ==================== INTERACTABLE INTERFACE ====================
// (Cette interface est déjà définie dans ton premier script, ne la duplique pas si elle existe déjà)
/*
public interface IInteractable
{
    void Interact();
    string GetInteractionPrompt();
}
*/
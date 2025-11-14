using UnityEngine;

/// <summary>
/// Exemple d'objet interactable - Duplique ce script pour créer tes propres objets
/// </summary>
public class Computer : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private string interactionPrompt = "E - Open";

    [Header("Optional - Object Behavior")]
    [SerializeField] private bool destroyOnInteract = false;
    [SerializeField] private GameObject visualFeedback;

    [SerializeField] Camera playerCam;

    public void Interact()
    {
        CameraSwitcher.instance.ToggleView();
        if (visualFeedback != null)
        {
            Instantiate(visualFeedback, transform.position, Quaternion.identity);
        }

        // Exemple : ajouter à l'inventaire
        // InventoryManager.Instance.AddItem(itemData);

        // Exemple : ouvrir une porte
        // DoorController door = GetComponent<DoorController>();
        // door.Open();

        // Exemple : activer un mécanisme
        // StationModule module = GetComponent<StationModule>();
        // module.Activate();

        // Détruire l'objet si nécessaire
        if (destroyOnInteract)
        {
            Destroy(gameObject);
        }

    }

    public string GetInteractionPrompt()
    {
        return interactionPrompt;
    }

    // Visualisation dans l'éditeur
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
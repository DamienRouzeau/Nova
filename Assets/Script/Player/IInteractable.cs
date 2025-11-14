using UnityEngine;

/// <summary>
/// Interface pour tous les objets interactables
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Appelé quand le joueur interagit avec l'objet
    /// </summary>
    void Interact();

    /// <summary>
    /// Retourne le texte à afficher dans l'UI (ex: "E - Ramasser", "E - Ouvrir")
    /// </summary>
    string GetInteractionPrompt();
}
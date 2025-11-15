using UnityEngine;
using System.Collections.Generic;

// ==================== WORLD ITEM (Objets au sol à ramasser) ====================
public class WorldItem : MonoBehaviour, IInteractable
{
    public ItemData itemData;
    public int quantity = 1;

    [Header("Visual")]
    [SerializeField] private GameObject visualMesh;
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private bool rotateInWorld = true;

    void Update()
    {
        if (rotateInWorld && visualMesh != null)
        {
            visualMesh.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }

    public void Interact()
    {
        // Géré par InteractionSystem.PickupItem()
    }

    public string GetInteractionPrompt()
    {
        if (itemData != null)
        {
            return $"E - Ramasser {itemData.itemName} ({quantity})";
        }
        return "E - Ramasser";
    }
}
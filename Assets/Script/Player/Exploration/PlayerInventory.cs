using UnityEngine;
using System.Collections.Generic;

// ==================== INVENTORY SLOT ====================
[System.Serializable]
public class InventorySlot
{
    public ItemData item;
    public int quantity;
    public int gridX; // Position dans la grille
    public int gridY;

    public InventorySlot(int x, int y)
    {
        gridX = x;
        gridY = y;
        item = null;
        quantity = 0;
    }

    public bool IsEmpty() => item == null || quantity <= 0;

    public bool CanAddItem(ItemData itemToAdd, int amount = 1)
    {
        if (IsEmpty()) return true;
        if (item != itemToAdd) return false;
        return quantity + amount <= item.maxStackSize;
    }
}

// ==================== PLAYER INVENTORY ====================
public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private int inventoryWidth = 8;
    [SerializeField] private int inventoryHeight = 6;
    [SerializeField] private float maxWeight = 50f;

    [Header("UI")]
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private Transform inventoryGrid;
    [SerializeField] private GameObject slotPrefab;

    [Header("Drop Settings")]
    [SerializeField] private Transform dropPoint;
    [SerializeField] private float dropForce = 3f;

    private InventorySlot[,] inventory;
    private float currentWeight;
    private bool isInventoryOpen = false;

    void Start()
    {
        InitializeInventory();

        if (inventoryUI != null)
        {
            inventoryUI.SetActive(false);
        }
    }

    void InitializeInventory()
    {
        inventory = new InventorySlot[inventoryWidth, inventoryHeight];

        for (int x = 0; x < inventoryWidth; x++)
        {
            for (int y = 0; y < inventoryHeight; y++)
            {
                inventory[x, y] = new InventorySlot(x, y);
            }
        }
    }

    public bool AddItem(ItemData item, int quantity = 1)
    {
        if (item == null) return false;

        // Vérifie le poids
        float itemWeight = item.weight * quantity;
        if (currentWeight + itemWeight > maxWeight)
        {
            Debug.Log("Inventaire trop lourd!");
            return false;
        }

        // Cherche un slot existant pour stacker
        if (item.maxStackSize > 1)
        {
            for (int x = 0; x < inventoryWidth; x++)
            {
                for (int y = 0; y < inventoryHeight; y++)
                {
                    InventorySlot _slot = inventory[x, y];
                    if (!_slot.IsEmpty() && _slot.item == item && _slot.quantity < item.maxStackSize)
                    {
                        int spaceLeft = item.maxStackSize - _slot.quantity;
                        int amountToAdd = Mathf.Min(spaceLeft, quantity);

                        _slot.quantity += amountToAdd;
                        quantity -= amountToAdd;
                        currentWeight += item.weight * amountToAdd;

                        if (quantity <= 0)
                        {
                            UpdateInventoryUI();
                            return true;
                        }
                    }
                }
            }
        }

        // Cherche un espace vide assez grand
        Vector2Int freeSpace = FindFreeSpace(item.width, item.height);

        if (freeSpace.x == -1)
        {
            Debug.Log("Pas assez de place dans l'inventaire!");
            return false;
        }

        // Place l'item
        InventorySlot slot = inventory[freeSpace.x, freeSpace.y];
        slot.item = item;
        slot.quantity = quantity;
        currentWeight += itemWeight;

        // Marque les slots occupés par cet item
        OccupySlots(freeSpace.x, freeSpace.y, item.width, item.height, item);

        UpdateInventoryUI();
        return true;
    }

    Vector2Int FindFreeSpace(int width, int height)
    {
        for (int x = 0; x <= inventoryWidth - width; x++)
        {
            for (int y = 0; y <= inventoryHeight - height; y++)
            {
                if (CanPlaceAt(x, y, width, height))
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return new Vector2Int(-1, -1);
    }

    bool CanPlaceAt(int x, int y, int width, int height)
    {
        for (int dx = 0; dx < width; dx++)
        {
            for (int dy = 0; dy < height; dy++)
            {
                if (x + dx >= inventoryWidth || y + dy >= inventoryHeight)
                    return false;

                if (!inventory[x + dx, y + dy].IsEmpty())
                    return false;
            }
        }
        return true;
    }

    void OccupySlots(int x, int y, int width, int height, ItemData item)
    {
        for (int dx = 0; dx < width; dx++)
        {
            for (int dy = 0; dy < height; dy++)
            {
                if (dx == 0 && dy == 0) continue; // Slot principal déjà occupé

                InventorySlot slot = inventory[x + dx, y + dy];
                slot.item = item; // Référence à l'item principal
                slot.quantity = -1; // Marque comme slot secondaire
            }
        }
    }

    public void RemoveItem(int x, int y, int quantity = 1)
    {
        if (x < 0 || x >= inventoryWidth || y < 0 || y >= inventoryHeight)
            return;

        InventorySlot slot = inventory[x, y];
        if (slot.IsEmpty()) return;

        ItemData item = slot.item;
        int widthToFree = item.width;
        int heightToFree = item.height;

        slot.quantity -= quantity;
        currentWeight -= item.weight * quantity;

        if (slot.quantity <= 0)
        {
            slot.item = null;
            slot.quantity = 0;

            // Libère les slots secondaires
            FreeSlots(x, y, widthToFree, heightToFree);
        }

        UpdateInventoryUI();
    }

    void FreeSlots(int x, int y, int width, int height)
    {
        for (int dx = 0; dx < width; dx++)
        {
            for (int dy = 0; dy < height; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                InventorySlot slot = inventory[x + dx, y + dy];
                slot.item = null;
                slot.quantity = 0;
            }
        }
    }

    public void DropItem(int x, int y)
    {
        if (x < 0 || x >= inventoryWidth || y < 0 || y >= inventoryHeight)
            return;

        InventorySlot slot = inventory[x, y];
        if (slot.IsEmpty() || slot.quantity < 0) return; // Pas un slot principal

        ItemData item = slot.item;
        int quantity = slot.quantity;

        // Spawn l'item dans le monde
        if (item.worldPrefab != null && dropPoint != null)
        {
            GameObject droppedItem = Instantiate(item.worldPrefab, dropPoint.position, Quaternion.identity);

            // Ajoute une force pour l'éjecter
            Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(dropPoint.forward * dropForce + Vector3.up * 2f, ForceMode.Impulse);
            }

            // Stocke la quantité dans le WorldItem component
            WorldItem worldItem = droppedItem.GetComponent<WorldItem>();
            if (worldItem != null)
            {
                worldItem.itemData = item;
                worldItem.quantity = quantity;
            }
        }

        RemoveItem(x, y, quantity);
    }

    void UpdateInventoryUI()
    {
        // À implémenter : rafraîchir l'affichage visuel
        // Tu peux utiliser un InventoryUI manager pour ça
    }

    public void ToggleInventoryUI()
    {
        isInventoryOpen = !isInventoryOpen;

        if (inventoryUI != null)
        {
            inventoryUI.SetActive(isInventoryOpen);
        }

        // Bloque/débloque les contrôles
        PlanetaryFPSController controller = GetComponent<PlanetaryFPSController>();
        if (controller != null)
        {
            controller.SetCanControl(!isInventoryOpen);
        }

        Cursor.lockState = isInventoryOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isInventoryOpen;
    }

    public bool HasItem(ItemData item, int quantity = 1)
    {
        int count = 0;
        for (int x = 0; x < inventoryWidth; x++)
        {
            for (int y = 0; y < inventoryHeight; y++)
            {
                InventorySlot slot = inventory[x, y];
                if (!slot.IsEmpty() && slot.item == item && slot.quantity > 0)
                {
                    count += slot.quantity;
                }
            }
        }
        return count >= quantity;
    }

    public float GetWeightPercentage() => currentWeight / maxWeight;
    public float GetCurrentWeight() => currentWeight;
    public InventorySlot[,] GetInventory() => inventory;
}


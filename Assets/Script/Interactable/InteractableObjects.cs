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

// ==================== MINEABLE RESOURCE ====================
public class MineableResource : MonoBehaviour, IInteractable
{
    [Header("Resource Settings")]
    [SerializeField] private ItemData resourceData;
    [SerializeField] private int minQuantity = 1;
    [SerializeField] private int maxQuantity = 3;
    [SerializeField] private float miningTime = 3f;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject brokenEffect;
    [SerializeField] private AudioClip miningSound;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && miningSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void Interact()
    {
        // Géré par InteractionSystem avec coroutine
        if (miningSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(miningSound);
        }
    }

    public string GetInteractionPrompt()
    {
        if (resourceData != null)
        {
            return $"E - Miner {resourceData.itemName} ({miningTime:F1}s)";
        }
        return $"E - Miner ({miningTime:F1}s)";
    }

    public ItemData GetResourceData() => resourceData;

    public int GetResourceQuantity()
    {
        return Random.Range(minQuantity, maxQuantity + 1);
    }

    public float GetMiningTime() => miningTime;

    void OnDestroy()
    {
        // Spawn effet de destruction
        if (brokenEffect != null)
        {
            Instantiate(brokenEffect, transform.position, Quaternion.identity);
        }
    }
}

// ==================== LOOT CHEST ====================
public class LootChest : MonoBehaviour, IInteractable
{
    [Header("Loot Settings")]
    [SerializeField] private ItemData[] possibleLoot;
    [SerializeField] private int minItems = 1;
    [SerializeField] private int maxItems = 3;
    [SerializeField] private float openTime = 2f;

    [Header("Visual")]
    [SerializeField] private Animator chestAnimator;
    [SerializeField] private string openAnimationTrigger = "Open";
    [SerializeField] private AudioClip openSound;

    [Header("UI (Optional)")]
    [SerializeField] private GameObject chestUI; // Panel UI pour voir le contenu

    private bool isOpen = false;
    private List<ItemData> lootItems = new List<ItemData>();
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && openSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void Interact()
    {
        // Géré par InteractionSystem avec coroutine
    }

    public void Open()
    {
        if (isOpen) return;

        isOpen = true;
        GenerateLoot();

        // Animation
        if (chestAnimator != null)
        {
            chestAnimator.SetTrigger(openAnimationTrigger);
        }

        // Son
        if (openSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(openSound);
        }

        // Ajoute directement à l'inventaire du joueur
        AddLootToPlayerInventory();

        // OU ouvre une UI si tu veux un système de chest UI
        // OpenChestUI();
    }

    void GenerateLoot()
    {
        lootItems.Clear();

        if (possibleLoot.Length == 0)
        {
            Debug.LogWarning("No possible loot configured for this chest!");
            return;
        }

        int itemCount = Random.Range(minItems, maxItems + 1);

        for (int i = 0; i < itemCount; i++)
        {
            ItemData randomItem = possibleLoot[Random.Range(0, possibleLoot.Length)];
            lootItems.Add(randomItem);
        }
    }

    void AddLootToPlayerInventory()
    {
        PlayerInventory inventory = FindObjectOfType<PlayerInventory>();

        if (inventory == null)
        {
            Debug.LogWarning("No PlayerInventory found!");
            return;
        }

        foreach (var item in lootItems)
        {
            if (item != null)
            {
                bool added = inventory.AddItem(item, 1);
                if (added)
                {
                    Debug.Log($"Added {item.itemName} to inventory");
                }
                else
                {
                    // Inventaire plein, drop au sol
                    DropItemNearChest(item);
                }
            }
        }
    }

    void DropItemNearChest(ItemData item)
    {
        if (item.worldPrefab != null)
        {
            Vector3 dropPos = transform.position + Random.insideUnitSphere * 2f;
            dropPos.y = transform.position.y + 1f;

            GameObject droppedItem = Instantiate(item.worldPrefab, dropPos, Quaternion.identity);

            WorldItem worldItem = droppedItem.GetComponent<WorldItem>();
            if (worldItem != null)
            {
                worldItem.itemData = item;
                worldItem.quantity = 1;
            }
        }
    }

    void OpenChestUI()
    {
        if (chestUI != null)
        {
            chestUI.SetActive(true);

            // Populate UI avec lootItems
            // À implémenter selon ton système d'UI

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public string GetInteractionPrompt()
    {
        if (isOpen)
        {
            return ""; // Pas d'interaction si déjà ouvert
        }
        return $"E - Ouvrir coffre ({openTime:F1}s)";
    }

    public float GetOpenTime() => openTime;
    public List<ItemData> GetLoot() => lootItems;
    public bool IsOpen() => isOpen;
}
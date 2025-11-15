using UnityEngine;
using System.Collections.Generic;

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
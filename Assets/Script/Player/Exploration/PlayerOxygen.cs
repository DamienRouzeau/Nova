
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PlayerOxygen : MonoBehaviour
{
    [Header("Oxygen Settings")]
    [SerializeField] private float maxOxygen = 300f; // 5 minutes
    [SerializeField] private float oxygenDrainRate = 1f; // Par seconde
    [SerializeField] private float lowOxygenThreshold = 60f; // 1 minute

    [Header("Damage")]
    [SerializeField] private float damageWhenEmpty = 5f; // Dégâts par seconde sans O2

    [Header("UI")]
    [SerializeField] private GameObject oxygenUI; // Panel entier
    [SerializeField] private Image oxygenBar;
    [SerializeField] private Image oxygenWarningIcon;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip lowOxygenSound;
    [SerializeField] private AudioClip breathingSound;

    private float currentOxygen;
    private PlayerHealth healthSystem;
    private bool isLowOxygen = false;
    private float nextDamageTime;

    void Start()
    {
        currentOxygen = maxOxygen;
        healthSystem = GetComponent<PlayerHealth>();

        if (oxygenUI != null)
        {
            oxygenUI.SetActive(false); // Caché par défaut
        }

        UpdateOxygenUI();
    }

    void Update()
    {
        // Drain oxygen
        currentOxygen -= oxygenDrainRate * Time.deltaTime;
        currentOxygen = Mathf.Max(0, currentOxygen);

        UpdateOxygenUI();

        // Low oxygen warning
        if (currentOxygen <= lowOxygenThreshold && !isLowOxygen)
        {
            isLowOxygen = true;
            if (audioSource != null && lowOxygenSound != null)
            {
                audioSource.PlayOneShot(lowOxygenSound);
            }
        }
        else if (currentOxygen > lowOxygenThreshold && isLowOxygen)
        {
            isLowOxygen = false;
        }

        // Damage when no oxygen
        if (currentOxygen <= 0 && Time.time >= nextDamageTime)
        {
            if (healthSystem != null)
            {
                healthSystem.TakeDamage(damageWhenEmpty);
            }
            nextDamageTime = Time.time + 1f;
        }

        // Warning icon blink
        if (oxygenWarningIcon != null && isLowOxygen)
        {
            float alpha = Mathf.PingPong(Time.time * 2f, 1f);
            Color color = oxygenWarningIcon.color;
            color.a = alpha;
            oxygenWarningIcon.color = color;
        }
    }

    void UpdateOxygenUI()
    {
        if (oxygenBar != null)
        {
            oxygenBar.fillAmount = currentOxygen / maxOxygen;

            // Change color based on level
            if (currentOxygen <= lowOxygenThreshold)
            {
                oxygenBar.color = Color.Lerp(Color.red, Color.yellow, currentOxygen / lowOxygenThreshold);
            }
            else
            {
                oxygenBar.color = Color.cyan;
            }
        }

        if (oxygenWarningIcon != null)
        {
            oxygenWarningIcon.gameObject.SetActive(isLowOxygen);
        }
    }

    public void RefillOxygen(float amount)
    {
        currentOxygen += amount;
        currentOxygen = Mathf.Min(maxOxygen, currentOxygen);
        UpdateOxygenUI();
    }

    public void SetUIVisible(bool visible)
    {
        if (oxygenUI != null)
        {
            oxygenUI.SetActive(visible);
        }
    }

    public float GetOxygenPercentage() => currentOxygen / maxOxygen;
    public float GetRemainingTime() => currentOxygen / oxygenDrainRate;
}
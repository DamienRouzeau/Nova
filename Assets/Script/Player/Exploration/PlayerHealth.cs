using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("UI")]
    [SerializeField] private Image healthBar;
    [SerializeField] private Image damageVignette; // Effet rouge sur les bords

    [Header("Effects")]
    [SerializeField] private float vignetteSpeed = 2f;

    [Header("Events")]
    public UnityEvent onDeath;
    public UnityEvent onDamageTaken;

    private float targetVignetteAlpha = 0f;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    void Update()
    {
        // Fade vignette
        if (damageVignette != null)
        {
            Color color = damageVignette.color;
            color.a = Mathf.Lerp(color.a, targetVignetteAlpha, Time.deltaTime * vignetteSpeed);
            damageVignette.color = color;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        UpdateHealthUI();
        onDamageTaken?.Invoke();

        // Flash damage vignette
        targetVignetteAlpha = 0.5f;
        Invoke(nameof(ResetVignette), 0.2f);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        UpdateHealthUI();
    }

    void ResetVignette()
    {
        targetVignetteAlpha = 0f;
    }

    void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = currentHealth / maxHealth;
        }
    }

    void Die()
    {
        onDeath?.Invoke();
        Debug.Log("Player died!");
        // Gérer la mort (respawn, game over, etc.)
    }

    public float GetHealthPercentage() => currentHealth / maxHealth;
    public float GetCurrentHealth() => currentHealth;
}
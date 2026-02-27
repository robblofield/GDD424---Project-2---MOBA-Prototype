using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 50f; // Starting / maximum health
    [SerializeField] private bool debugLogs = true; // Toggle debug output
    [SerializeField] private bool invulnerable = false; // If true, ignores all damage

    [Header("Hit Flash")]
    [SerializeField] private Renderer targetRenderer; // Renderer to flash red when hit
    [SerializeField] private float flashDuration = 0.1f; // How long the flash lasts

    public float MaxHealth => maxHealth;
    public float CurrentHealth { get; private set; }

    private Material runtimeMat; // Instance material (so we don't edit shared project materials)
    private Color originalColor; // Color we revert to after flashing
    private float flashTimer; // Countdown for the flash effect

    // Simple public setter used by GameManager to lock/unlock the monument
    public void SetInvulnerable(bool value) => invulnerable = value;

    private void Awake()
    {
        // Initialise health
        CurrentHealth = maxHealth;

        // Try to find a renderer automatically if one isn't assigned
        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<Renderer>();
        }

        // Cache a runtime material for flashing
        if (targetRenderer != null)
        {
            runtimeMat = targetRenderer.material;

            if (runtimeMat.HasProperty("_Color"))
                originalColor = runtimeMat.color;
        }
    }

    private void Update()
    {
        // Only run flash logic when we actually have a material and a timer running
        if (runtimeMat == null) return;
        if (flashTimer <= 0f) return;

        flashTimer -= Time.deltaTime;

        // When flash ends, restore original color
        if (flashTimer <= 0f)
        {
            if (runtimeMat.HasProperty("_Color"))
                runtimeMat.color = originalColor;
        }
    }

    public void TakeDamage(float amount)
    {
        // Overload: damage with no attacker info
        if (invulnerable) return;

        TakeDamage(amount, null);
    }

    public void TakeDamage(float amount, Transform attacker)
    {
        // Core damage function
        if (invulnerable) return;
        if (amount <= 0f) return;
        if (CurrentHealth <= 0f) return;

        float before = CurrentHealth;
        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);

        if (debugLogs)
        {
            Debug.Log($"[Health] {name} took {amount:0.0}, now {CurrentHealth:0.0}/{MaxHealth:0.0}", this);
        }

        // Flash feedback
        FlashRed();

        // If this object is the player, allow auto-retaliate when hit
        PlayerBrain pb = GetComponent<PlayerBrain>();
        if (pb != null && attacker != null)
        {
            pb.NotifyDamagedBy(attacker);
        }

        // If we just hit zero, die
        if (CurrentHealth <= 0f && before > 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (debugLogs)
        {
            Debug.Log($"[Health] {name} died", this);
        }

        // Keep it simple for teaching:
        Destroy(gameObject);
    }

    private void FlashRed()
    {
        if (runtimeMat == null) return;
        if (!runtimeMat.HasProperty("_Color")) return;

        runtimeMat.color = Color.red;
        flashTimer = flashDuration;
    }
}
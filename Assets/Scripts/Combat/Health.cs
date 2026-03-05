using System.Runtime.InteropServices;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 50f; 
    [SerializeField] private bool debugLogs = true; // [V3] Toggle debug output
    [SerializeField] private bool invulnerable = false; // [V3] If true, ignores all damage

    [Header("Hit Flash")]
    [SerializeField] private Renderer targetRenderer; // [V3] Renderer to flash red when hit
    [SerializeField] private float flashDuration = 0.1f; // [V3] How long the flash lasts

    public float MaxHealth => maxHealth;  // [V3] "=>" is a shorthand way of making public "Getter" so any script can get the max health
    public float CurrentHealth { get; private set; } // [V3] updated from regular float to a get/private set

    private Material runtimeMat; // [V3] Instance material (so we don't edit shared project materials)
    private Color originalColor; // [V3] Color we revert to after flashing
    private float flashTimer; // [V3] Countdown for the flash effect

    // [V3] Simple public setter used by GameManager to lock/unlock the monument
    public void SetInvulnerable(bool value) => invulnerable = value;

    private void Awake()
    {
        // Initialise health
        CurrentHealth = maxHealth; // [V3] updated to new `CurrentHealth` not `currentHealth`

        // [V3] Try to find a renderer automatically if one isn't assigned
        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<Renderer>();
        }

        // [V3] Cache a runtime material for flashing
        if (targetRenderer != null)
        {
            runtimeMat = targetRenderer.material;

            if (runtimeMat.HasProperty("_Color"))
                originalColor = runtimeMat.color;
        }
    }

    private void Update()
    {
        // [V3] Only run flash logic when we actually have a material and a timer running
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

    public void TakeDamage(float amount) // [V3] changed from single method to overload version which takes an amound and the transform of who hit us
    {
        if (invulnerable) return;

        TakeDamage(amount, null);
    }

    public void TakeDamage(float amount, Transform attacker) // [V3] Overload version with 2 arguments
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

    private void Die() // [V3] was previously in old TakeDamage(float amount) method
    {
        if (debugLogs)
        {
            Debug.Log($"[Health] {name} died", this);
        }

        // Keep it simple for teaching:
        Destroy(gameObject);
    }

    private void FlashRed() // [V3] Flash red when we take damage
    {
        if (runtimeMat == null) return;
        if (!runtimeMat.HasProperty("_Color")) return;

        runtimeMat.color = Color.red;
        flashTimer = flashDuration;
    }
}
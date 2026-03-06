using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private bool debugLogs = true;
    [SerializeField] private bool invulnerable = false;

    [Header("Hit Flash")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private float flashDuration = 0.1f;

    [Header("UI")]
    [SerializeField] private bool isMonument = false;
    [SerializeField] private Vector3 damageNumberOffset = new Vector3(0f, 2f, 0f);

    public float MaxHealth => maxHealth;
    public float CurrentHealth { get; private set; }

    private Material runtimeMat;
    private Color originalColor;
    private float flashTimer;

    private GameUI gameUI;

    public void SetInvulnerable(bool value) => invulnerable = value;

    private void Awake()
    {
        CurrentHealth = maxHealth;

        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<Renderer>();
        }

        if (targetRenderer != null)
        {
            runtimeMat = targetRenderer.material;

            if (runtimeMat.HasProperty("_Color"))
                originalColor = runtimeMat.color;
        }

        gameUI = FindFirstObjectByType<GameUI>();
    }

    private void Update()
    {
        if (runtimeMat == null) return;
        if (flashTimer <= 0f) return;

        flashTimer -= Time.deltaTime;

        if (flashTimer <= 0f)
        {
            if (runtimeMat.HasProperty("_Color"))
                runtimeMat.color = originalColor;
        }
    }

    public void TakeDamage(float amount)
    {
        if (invulnerable) return;
        TakeDamage(amount, null);
    }

    public void TakeDamage(float amount, Transform attacker)
    {
        if (invulnerable) return;
        if (amount <= 0f) return;
        if (CurrentHealth <= 0f) return;

        float before = CurrentHealth;
        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);

        if (debugLogs)
        {
            Debug.Log($"[Health] {name} took {amount:0.0}, now {CurrentHealth:0.0}/{MaxHealth:0.0}", this);
        }

        FlashRed();

        if (gameUI != null)
        {
            gameUI.SpawnDamageNumber(transform.position + damageNumberOffset, amount);
        }

        PlayerBrain pb = GetComponent<PlayerBrain>();
        if (pb != null && attacker != null)
        {
            pb.NotifyDamagedBy(attacker);
        }

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

        if (isMonument && gameUI != null)
        {
            gameUI.ShowVictory();
        }

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
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class DamageZone : MonoBehaviour
{
    [Header("Hit Settings")]
    [SerializeField] private float damagePerHit = 5f; // [V3] used to be damagePersecond, but now we have per hit and attacks persecond to calculate dynamically

    [SerializeField, Range(0.5f, 5f)]
    private float attacksPerSecond = 2f; // [V3] How many damage ticks per second

    [Header("Target Filter")]
    [SerializeField] private LayerMask validTargets; // [V3] Only objects on these layers can be damaged

    private SphereCollider sphere; // Trigger collider used as the hit zone

    private readonly HashSet<Health> targetsInZone = new HashSet<Health>(); // [V3] All valid Health targets currently inside
    private float hitTimer; // [V3] Countdown until the next damage tick

    // [V3] Public helpers (used by brains to know whether we're actually overlapping a target)
    public bool HasTargets => targetsInZone.Count > 0;
    public float Radius => sphere != null ? sphere.radius : 0f;

    private void Awake()
    {
        // Setup trigger collider
        sphere = GetComponent<SphereCollider>();
        sphere.isTrigger = true;

        // [V3] using a method rather than setting directly
        SetActive(false);
    }

    private void Update()
    {
        // If the zone is not active, do nothing.
        if (!sphere.enabled) return;

        // [V3] Clean up destroyed references (Unity "fake null" can leave dead entries behind)
        targetsInZone.RemoveWhere(t => t == null);

        // [V3] Countdown to next hit
        hitTimer -= Time.deltaTime;
        if (hitTimer > 0f) return;

        // [V3] Deal damage to everything currently overlapping
        if (targetsInZone.Count > 0)
        {
            foreach (Health h in targetsInZone)
            {
                if (h == null) continue;

                // [V3] Attacker is the root object (e.g. Player / Enemy parent)
                h.TakeDamage(damagePerHit, transform.root);
            }
        }

        // [V3] Reset timer based on attacks-per-second
        hitTimer = GetInterval();
    }

    public void SetActive(bool active)
    {
        // [V3] Only do work if the state actually changes.
        if (sphere.enabled == active) return;

        sphere.enabled = active;

        if (!active)
        {
            // [V3] When we stop attacking, clear any stored targets.
            targetsInZone.Clear();

            // [V3] Timer resets while disabled (not running)
            hitTimer = GetInterval();
        }
        else
        {
            // [V3] When we start attacking, allow an immediate first hit (feels responsive)
            hitTimer = 0f;
        }
    }

    private float GetInterval()
    {
        // [V3] Convert attacks-per-second into a time interval between ticks.
        float aps = Mathf.Clamp(attacksPerSecond, 0.5f, 5f);
        return 1f / aps;
    }

    private void OnTriggerEnter(Collider other) // [V3] Used to be OnTriggerStay(), but now we handle entry and exit trigger separately
    {
        // Ignore triggers when inactive.
        if (!sphere.enabled) return;

        // Don't hit ourselves.
        if (other.transform.root == transform.root) return;

        // Only accept targets on specific layers (prevents friendly fire etc.) - Thanks ChatGPT! Documentation didn't help us here!
        if (((1 << other.gameObject.layer) & validTargets) == 0) return;

        // Damage is applied to objects with Health on the root object.
        Health h = other.transform.root.GetComponent<Health>();
        if (h == null) return;

        targetsInZone.Add(h);
    }

    private void OnTriggerExit(Collider other) // [V3] Used to be OnTriggerStay(), but now we handle entry and exit trigger separately
    {
        // Remove target when it leaves the trigger.
        Health h = other.transform.root.GetComponent<Health>();
        if (h == null) return;

        targetsInZone.Remove(h);
    }
}
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private bool debugLogs = true; // Toggle debug logging
    [SerializeField] private Health monumentHealth; // The monument's Health component (unlocked when all enemies die)

    private readonly List<EnemyBrain> enemies = new List<EnemyBrain>(); // Track living enemies

    private void Start()
    {
        // At the start of the game, the monument cannot be damaged.
        if (monumentHealth != null)
            monumentHealth.SetInvulnerable(true);
    }

    public void RegisterEnemy(EnemyBrain enemy)
    {
        // Called by EnemyBrain.Awake()
        if (enemy == null) return;
        if (enemies.Contains(enemy)) return;

        enemies.Add(enemy);

        if (debugLogs)
            Debug.Log($"[GameManager] Registered enemy. Total = {enemies.Count}", this);
    }

    public void UnregisterEnemy(EnemyBrain enemy)
    {
        // Called by EnemyBrain.OnDestroy()
        if (enemy == null) return;
        if (!enemies.Remove(enemy)) return;

        if (debugLogs)
            Debug.Log($"[GameManager] Unregistered enemy. Remaining = {enemies.Count}", this);

        // If that was the last enemy, unlock the monument.
        if (enemies.Count == 0 && monumentHealth != null)
        {
            UnlockMonument();
        }
    }

    private void UnlockMonument()
    {
        // Safety check: only unlock if there really are no enemies left.
        if (enemies.Count == 0 && monumentHealth != null)
        {
            monumentHealth.SetInvulnerable(false);
            Debug.Log("[GameManager] Monument unlocked", this);
        }
    }
}
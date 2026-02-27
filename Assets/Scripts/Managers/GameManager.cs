using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private bool debugLogs = true;
    [SerializeField] private Health monumentHealth;

    private readonly List<EnemyBrain> enemies = new List<EnemyBrain>();

    private void Start()
    {
        if (monumentHealth != null)
            monumentHealth.SetInvulnerable(true);
    }

    public void RegisterEnemy(EnemyBrain enemy)
    {
        if (enemy == null) return;
        if (enemies.Contains(enemy)) return;

        enemies.Add(enemy);

        if (debugLogs) Debug.Log($"[GameManager] Registered enemy. Total = {enemies.Count}", this);
    }

    public void UnregisterEnemy(EnemyBrain enemy)
    {
        if (enemy == null) return;
        if (!enemies.Remove(enemy)) return;

        if (debugLogs) Debug.Log($"[GameManager] Unregistered enemy. Remaining = {enemies.Count}", this);

        if (enemies.Count == 0 && monumentHealth != null)
        {
            UnlockMonument();
        }
    }

    

    private void UnlockMonument()
    {
        if (enemies.Count == 0 && monumentHealth != null)
        {
            monumentHealth.SetInvulnerable(false);
            Debug.Log("[GameManager] Monument unlocked", this);
        }
    }
}
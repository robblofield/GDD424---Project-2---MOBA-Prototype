using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private bool debugLogs = true;

    private readonly List<EnemyBrain> enemies = new List<EnemyBrain>();

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

        // Section 3: when count hits 0, unlock monument.
    }
}
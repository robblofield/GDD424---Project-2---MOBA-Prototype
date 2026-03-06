using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private bool debugLogs = true;
    [SerializeField] private Health monumentHealth;
    [SerializeField] private GameUI gameUI;

    [Header("UI Prompt Timings")]
    [SerializeField] private float startPromptDuration = 3f;
    [SerializeField] private float monumentPromptDuration = 3f;

    private readonly List<EnemyBrain> enemies = new List<EnemyBrain>();

    private void Start()
    {
        // At the start of the game, the monument cannot be damaged.
        if (monumentHealth != null)
            monumentHealth.SetInvulnerable(true);

        if (gameUI != null)
        {
            StartCoroutine(ShowStartPromptRoutine());
            gameUI.UpdateEnemiesRemaining(enemies.Count);
        }
    }

    public void RegisterEnemy(EnemyBrain enemy)
    {
        // Called by EnemyBrain.Awake()
        if (enemy == null) return;
        if (enemies.Contains(enemy)) return;

        enemies.Add(enemy);

        if (debugLogs)
            Debug.Log($"[GameManager] Registered enemy. Total = {enemies.Count}", this);

        if (gameUI != null)
            gameUI.UpdateEnemiesRemaining(enemies.Count);
    }

    public void UnregisterEnemy(EnemyBrain enemy)
    {
        // Called by EnemyBrain.OnDestroy()
        if (enemy == null) return;
        if (!enemies.Remove(enemy)) return;

        if (debugLogs)
            Debug.Log($"[GameManager] Unregistered enemy. Remaining = {enemies.Count}", this);

        if (gameUI != null)
            gameUI.UpdateEnemiesRemaining(enemies.Count);

        if (enemies.Count == 0 && monumentHealth != null)
            UnlockMonument();
    }

    private void UnlockMonument()
    {
        monumentHealth.SetInvulnerable(false);

        if (debugLogs)
            Debug.Log("[GameManager] All enemies defeated. Monument is now vulnerable!", this);

        if (gameUI != null)
            StartCoroutine(ShowMonumentPromptRoutine());
    }

    private IEnumerator ShowStartPromptRoutine()
    {
        gameUI.ShowStartPrompt();

        yield return new WaitForSeconds(startPromptDuration);

        gameUI.HideStartPrompt();
        gameUI.ShowEnemiesRemaining();
    }

    private IEnumerator ShowMonumentPromptRoutine()
    {
        gameUI.HideEnemiesRemaining() ;
        gameUI.ShowMonumentPrompt();

        yield return new WaitForSeconds(monumentPromptDuration);

        gameUI.HideMonumentPrompt();
        
    }

    public void TriggerVictory()
    {
        if (debugLogs)
            Debug.Log("[GameManager] Monument destroyed. Victory!", this);

        if (gameUI != null)
            gameUI.ShowVictory();
    }
}
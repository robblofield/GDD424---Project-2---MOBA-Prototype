using UnityEngine;

public class Monument : MonoBehaviour
{
    private GameManager gameManager;

    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.TriggerVictory();
        }
    }
}
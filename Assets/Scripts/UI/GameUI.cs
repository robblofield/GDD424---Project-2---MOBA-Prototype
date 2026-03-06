using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    [Header("Static UI")]
    [SerializeField] private TMP_Text startPromptText;
    [SerializeField] private TMP_Text enemiesRemainingText;
    [SerializeField] private TMP_Text monumentPromptText;
    [SerializeField] private TMP_Text victoryText;

    [Header("Damage Numbers")]
    [SerializeField] private GameObject damageNumberPrefab;
    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private Vector3 damageNumberOffset = new Vector3(0f, 2f, 0f);

    private void Start()
    {
        HideStartPrompt();
        HideMonumentPrompt();
        HideVictory();
    }

    public void ShowStartPrompt()
    {
        if (startPromptText != null)
            startPromptText.gameObject.SetActive(true);
    }

    public void HideStartPrompt()
    {
        if (startPromptText != null)
            startPromptText.gameObject.SetActive(false);
    }

    public void ShowEnemiesRemaining()
    {
        if (enemiesRemainingText != null)
            enemiesRemainingText.gameObject.SetActive(true);
    }

    public void HideEnemiesRemaining()
    {
        if (enemiesRemainingText != null)
            enemiesRemainingText.gameObject.SetActive(false);
    }

    public void UpdateEnemiesRemaining(int count)
    {
        if (enemiesRemainingText != null)
            enemiesRemainingText.text = $"Enemies Remaining: {count}";
    }

    public void ShowMonumentPrompt()
    {
        if (monumentPromptText != null)
            monumentPromptText.gameObject.SetActive(true);
    }

    public void HideMonumentPrompt()
    {
        if (monumentPromptText != null)
            monumentPromptText.gameObject.SetActive(false);
    }

    public void ShowVictory()
    {
        if (victoryText != null)
            victoryText.gameObject.SetActive(true);
    }

    public void HideVictory()
    {
        if (victoryText != null)
            victoryText.gameObject.SetActive(false);
    }

    public void SpawnDamageNumber(Vector3 worldPosition, float amount)
    {
        if (damageNumberPrefab == null || worldCanvas == null) return;

        GameObject instance = Instantiate(
            damageNumberPrefab,
            worldPosition + damageNumberOffset,
            Quaternion.identity,
            worldCanvas.transform
        );

        TMP_Text tmp = instance.GetComponentInChildren<TMP_Text>();
        if (tmp != null)
            tmp.text = $"-{amount:0}";
    }
}
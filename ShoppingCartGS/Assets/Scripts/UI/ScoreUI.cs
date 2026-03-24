using TMPro;
using UnityEngine;

/// <summary>
/// Displays the current total score during gameplay.
/// </summary>
public class ScoreUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private TextMeshProUGUI scoreText;

    private void Awake()
    {
        ResolveScoreManager();
    }

    private void Start()
    {
        UpdateScoreText(0);
    }

    private void Update()
    {
        if (scoreText == null)
        {
            return;
        }

        if (scoreManager == null)
        {
            ResolveScoreManager();
        }

        UpdateScoreText(scoreManager != null ? scoreManager.CurrentScore : 0);
    }

    /// <summary>
    /// Finds the score manager in the scene when no direct reference is assigned.
    /// </summary>
    private void ResolveScoreManager()
    {
        if (scoreManager == null)
        {
            scoreManager = FindFirstObjectByType<ScoreManager>();
        }
    }

    private void UpdateScoreText(int score)
    {
        scoreText.text = $"Score : {score}";
    }
}

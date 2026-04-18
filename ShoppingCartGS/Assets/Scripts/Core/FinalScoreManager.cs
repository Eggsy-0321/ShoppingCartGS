using UnityEngine;

/// <summary>
/// Manages the final score calculated when the run ends.
/// The value remains accessible for future result handling.
/// </summary>
public class FinalScoreManager : MonoBehaviour
{
    private float _currentFinalScore;

    /// <summary>
    /// Gets the latest calculated final score.
    /// </summary>
    public float CurrentFinalScore => _currentFinalScore;

    /// <summary>
    /// Calculates and stores the final score.
    /// Formula: FinalScore = Distance * (1 + TotalScore / 200)
    /// Negative inputs are clamped to zero.
    /// </summary>
    public float CalculateFinalScore(float distance, int totalScore)
    {
        float safeDistance = Mathf.Max(0f, distance);
        int safeTotalScore = Mathf.Max(0, totalScore);

        _currentFinalScore = safeDistance * (1f + (safeTotalScore / 200f));
        return _currentFinalScore;
    }

    /// <summary>
    /// Resets the stored final score for a new run.
    /// </summary>
    public void ResetFinalScore()
    {
        _currentFinalScore = 0f;
    }
}

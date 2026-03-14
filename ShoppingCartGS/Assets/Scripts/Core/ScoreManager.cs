using UnityEngine;

/// <summary>
/// Keeps track of the current total score during a run.
/// Also stores the score captured at time up for future result handling.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    private int _currentScore;
    private int _finalScore;

    /// <summary>
    /// Current total score collected in the active run.
    /// </summary>
    public int CurrentScore => _currentScore;

    /// <summary>
    /// Score captured when the run ends.
    /// </summary>
    public int FinalScore => _finalScore;

    /// <summary>
    /// Adds a positive score value to the current total.
    /// </summary>
    public void AddScore(int value)
    {
        if (value < 0)
        {
            Debug.LogWarning("[ScoreManager] Negative score is not allowed.");
            return;
        }

        if (value == 0)
        {
            return;
        }

        if (_currentScore > int.MaxValue - value)
        {
            _currentScore = int.MaxValue;
            return;
        }

        _currentScore += value;
    }

    /// <summary>
    /// Resets the score state for a new run.
    /// </summary>
    public void ResetScore()
    {
        _currentScore = 0;
        _finalScore = 0;
    }

    /// <summary>
    /// Stores the current score as the final score at time up.
    /// </summary>
    public void CaptureFinalScore()
    {
        _finalScore = _currentScore;
    }
}

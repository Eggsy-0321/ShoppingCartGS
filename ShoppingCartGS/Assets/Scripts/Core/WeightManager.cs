using UnityEngine;

/// <summary>
/// Keeps track of the current total weight during a run.
/// </summary>
public class WeightManager : MonoBehaviour
{
    private int _currentWeight;

    /// <summary>
    /// Current total weight collected in the active run.
    /// </summary>
    public int CurrentWeight => _currentWeight;

    /// <summary>
    /// Adds a positive weight value to the current total.
    /// </summary>
    public void AddWeight(int value)
    {
        if (value < 0)
        {
            Debug.LogWarning("[WeightManager] Negative weight is not allowed.");
            return;
        }

        _currentWeight += value;
    }

    /// <summary>
    /// Resets the total weight for a new run.
    /// </summary>
    public void ResetWeight()
    {
        _currentWeight = 0;
    }
}

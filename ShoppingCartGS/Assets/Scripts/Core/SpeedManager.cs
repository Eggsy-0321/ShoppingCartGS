using UnityEngine;

/// <summary>
/// Calculates the current scroll speed from the current total weight.
/// </summary>
public class SpeedManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeightManager weightManager;
    [SerializeField] private SegmentLoopScroller segmentLoopScroller;

    [Header("Speed Settings")]
    [SerializeField] private float baseSpeed = 12f;
    [SerializeField] private float slowdownPerWeight = 0.20f;
    [SerializeField] private float minSpeed = 6.5f;
    [SerializeField] private float weightCurvePower = 0.7f;

    private float _currentSpeed = 12f;

    /// <summary>
    /// Current calculated speed after weight slowdown.
    /// </summary>
    public float CurrentSpeed => _currentSpeed;

    private void Awake()
    {
        ValidateSettings();
        ResetSpeed();
    }

    private void OnValidate()
    {
        ValidateSettings();
    }

    private void Update()
    {
        RecalculateSpeed();
        ApplySpeedToScroller();
    }

    /// <summary>
    /// Resets the speed back to its initial state for a new run.
    /// </summary>
    public void ResetSpeed()
    {
        ValidateSettings();
        _currentSpeed = baseSpeed;
        ApplySpeedToScroller();
    }

    private void RecalculateSpeed()
    {
        int currentWeight = weightManager != null ? weightManager.CurrentWeight : 0;

        // Keep the early slowdown gentle, then let the weight penalty build up later in the run.
        float curvedWeight = Mathf.Pow(currentWeight, weightCurvePower);
        float calculatedSpeed = baseSpeed - (curvedWeight * slowdownPerWeight);
        _currentSpeed = Mathf.Max(minSpeed, calculatedSpeed);
    }

    private void ApplySpeedToScroller()
    {
        if (segmentLoopScroller == null)
        {
            return;
        }

        segmentLoopScroller.BaseSpeed = _currentSpeed;
    }

    private void ValidateSettings()
    {
        baseSpeed = Mathf.Max(0f, baseSpeed);
        slowdownPerWeight = Mathf.Max(0f, slowdownPerWeight);
        minSpeed = Mathf.Max(0f, minSpeed);
        weightCurvePower = Mathf.Max(0.01f, weightCurvePower);

        if (minSpeed > baseSpeed)
        {
            minSpeed = baseSpeed;
        }
    }
}

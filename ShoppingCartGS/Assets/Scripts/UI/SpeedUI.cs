using TMPro;
using UnityEngine;

/// <summary>
/// Displays the current scroll speed during gameplay.
/// </summary>
public class SpeedUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpeedManager speedManager;
    [SerializeField] private TextMeshProUGUI speedText;

    private void Start()
    {
        if (speedText == null)
        {
            return;
        }

        UpdateSpeedText(speedManager != null ? speedManager.CurrentSpeed : 0f);
    }

    private void Update()
    {
        if (speedManager == null || speedText == null)
        {
            return;
        }

        UpdateSpeedText(speedManager.CurrentSpeed);
    }

    private void UpdateSpeedText(float speed)
    {
        speedText.text = $"Speed : {speed:0.00}";
    }
}

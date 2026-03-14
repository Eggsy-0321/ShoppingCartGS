using TMPro;
using UnityEngine;

/// <summary>
/// Displays the current total weight during gameplay.
/// </summary>
public class WeightUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeightManager weightManager;
    [SerializeField] private TextMeshProUGUI weightText;

    private void Start()
    {
        UpdateWeightText(0);
    }

    private void Update()
    {
        if (weightManager == null || weightText == null)
        {
            return;
        }

        UpdateWeightText(weightManager.CurrentWeight);
    }

    private void UpdateWeightText(int weight)
    {
        weightText.text = $"Weight : {weight}";
    }
}

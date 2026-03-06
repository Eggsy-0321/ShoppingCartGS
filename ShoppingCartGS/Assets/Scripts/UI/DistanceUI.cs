using TMPro;
using UnityEngine;

/// <summary>
/// 累計距離をUI表示する。
/// 表示形式：Distance : 123m
/// </summary>
public class DistanceUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SegmentLoopScroller segmentLoopScroller;
    [SerializeField] private TextMeshProUGUI distanceText;

    private void Start()
    {
        UpdateDistanceText(0f);
    }

    private void Update()
    {
        if (segmentLoopScroller == null || distanceText == null)
        {
            return;
        }

        UpdateDistanceText(segmentLoopScroller.Distance);
    }

    private void UpdateDistanceText(float distance)
    {
        int displayDistance = Mathf.FloorToInt(distance);
        distanceText.text = $"Distance : {displayDistance}m";
    }
}
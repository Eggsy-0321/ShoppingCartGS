using System;
using TMPro;
using UnityEngine;

/// <summary>
/// 60秒固定タイマー（GDD準拠）
/// - 残り時間をカウントダウン
/// - 0秒でタイムアップイベント発火
/// - UI(TextMeshPro)へ表示
/// </summary>
public class GameTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("ゲーム時間（秒）。GDDは60秒固定。")]
    [SerializeField] private float gameDurationSeconds = 60f;

    [Header("UI")]
    [Tooltip("残り時間表示（TextMeshProUGUI）")]
    [SerializeField] private TextMeshProUGUI timeText;

    public float RemainingSeconds { get; private set; }
    public float ElapsedSeconds => Mathf.Max(0f, gameDurationSeconds - RemainingSeconds);

    public event Action OnTimeUp;

    private bool _isRunning;

    private void Awake()
    {
        ResetTimer();
        UpdateTimeUI(RemainingSeconds);
    }

    private void Update()
    {
        if (!_isRunning) return;

        RemainingSeconds -= Time.deltaTime;

        if (RemainingSeconds <= 0f)
        {
            RemainingSeconds = 0f;
            UpdateTimeUI(RemainingSeconds);

            _isRunning = false;
            OnTimeUp?.Invoke();
            return;
        }

        UpdateTimeUI(RemainingSeconds);
    }

    public void StartTimer()
    {
        _isRunning = true;
    }

    public void StopTimer()
    {
        _isRunning = false;
    }

    public void ResetTimer()
    {
        RemainingSeconds = Mathf.Max(0f, gameDurationSeconds);
        _isRunning = false;
    }

    private void UpdateTimeUI(float seconds)
    {
        if (timeText == null) return;

        // MM:SS 表示
        int sec = Mathf.CeilToInt(seconds);
        int m = sec / 60;
        int s = sec % 60;
        timeText.text = $"{m:00}:{s:00}";
    }
}
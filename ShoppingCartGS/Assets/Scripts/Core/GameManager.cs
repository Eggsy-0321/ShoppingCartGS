using UnityEngine;

/// <summary>
/// ゲーム進行の最小骨格
/// - 開始時にタイマー開始
/// - タイムアップでワールド停止＆操作停止
/// - 開始時に距離もリセット
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameTimer gameTimer;

    [Tooltip("B案：ワールドスクロール担当（SegmentLoopScroller）")]
    [SerializeField] private SegmentLoopScroller worldScroller;

    [Tooltip("プレイヤー操作（PlayerLaneController）")]
    [SerializeField] private MonoBehaviour playerLaneController;

    [SerializeField] private WeightManager weightManager;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private SpeedManager speedManager;
    [SerializeField] private FinalScoreManager finalScoreManager;

    [Header("Debug")]
    [SerializeField] private bool autoStartOnPlay = true;

    private bool _isGameOver;

    private void Awake()
    {
        ResolveScoreManager();
        ResolveFinalScoreManager();

        if (gameTimer != null)
        {
            gameTimer.OnTimeUp += HandleTimeUp;
        }
    }

    private void Start()
    {
        if (autoStartOnPlay)
        {
            StartGame();
        }
    }

    private void OnDestroy()
    {
        if (gameTimer != null)
        {
            gameTimer.OnTimeUp -= HandleTimeUp;
        }
    }

    public void StartGame()
    {
        _isGameOver = false;
        ResolveScoreManager();
        ResolveFinalScoreManager();

        if (weightManager != null)
        {
            weightManager.ResetWeight();
        }

        if (scoreManager != null)
        {
            scoreManager.ResetScore();
        }

        if (speedManager != null)
        {
            speedManager.ResetSpeed();
        }

        if (finalScoreManager != null)
        {
            finalScoreManager.ResetFinalScore();
        }

        if (playerLaneController != null)
        {
            playerLaneController.enabled = true;
        }

        if (worldScroller != null)
        {
            worldScroller.ResetDistance();
            worldScroller.StartScrolling();

            if (worldScroller.BaseSpeed <= 0f)
            {
                worldScroller.BaseSpeed = 12f;
            }
        }

        if (gameTimer != null)
        {
            gameTimer.ResetTimer();
            gameTimer.StartTimer();
        }

        Debug.Log("[GameManager] Game Start");
    }

    private void HandleTimeUp()
    {
        if (_isGameOver) return;
        _isGameOver = true;
        ResolveScoreManager();
        ResolveFinalScoreManager();

        Debug.Log("[GameManager] TIME UP");

        if (worldScroller != null)
        {
            worldScroller.StopScrolling();
        }

        if (playerLaneController != null)
        {
            playerLaneController.enabled = false;
        }

        if (finalScoreManager != null)
        {
            float distance = worldScroller != null ? worldScroller.Distance : 0f;
            int totalScore = scoreManager != null ? scoreManager.CurrentScore : 0;
            float finalScore = finalScoreManager.CalculateFinalScore(distance, totalScore);

            Debug.Log($"[GameManager] Final Score Calculated: {finalScore}");
        }
    }

    private void ResolveScoreManager()
    {
        if (scoreManager == null)
        {
            scoreManager = FindFirstObjectByType<ScoreManager>();
        }
    }

    private void ResolveFinalScoreManager()
    {
        if (finalScoreManager != null)
        {
            return;
        }

        finalScoreManager = FindFirstObjectByType<FinalScoreManager>();

        if (finalScoreManager == null)
        {
            GameObject finalScoreManagerObject = new GameObject("FinalScoreManager");
            finalScoreManager = finalScoreManagerObject.AddComponent<FinalScoreManager>();
        }
    }
}

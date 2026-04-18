using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Result panel UI controller.
/// Handles display updates and lightweight result animations only.
/// </summary>
public class ResultPanelUI : MonoBehaviour
{
    private const string DefaultTitleSceneName = "Title";
    private const string GoodRankLabel = "GOOD";
    private const string GreatRankLabel = "GREAT";
    private const string AmazingRankLabel = "AMAZING";

    [Header("Panel Root")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private RectTransform panelRectTransform;
    [SerializeField] private CanvasGroup panelCanvasGroup;

    [Header("Buttons")]
    [SerializeField] private GameObject retryButtonRoot;
    [SerializeField] private GameObject titleButtonRoot;

    [Header("Result Text")]
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI weightText;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI rankText;

    [Header("Display Labels")]
    [SerializeField] private string distanceLabel = "Distance";
    [SerializeField] private string weightLabel = "Weight";
    [SerializeField] private string finalScoreLabel = "FinalScore";

    [Header("Rank Thresholds")]
    [SerializeField] private int greatThreshold = 800;
    [SerializeField] private int amazingThreshold = 1500;

    [Header("Panel Animation")]
    [SerializeField] private float panelPopDuration = 0.2f;
    [SerializeField] private float panelStartScale = 0.92f;
    [SerializeField] private float panelEndScale = 1f;

    [Header("Score Count Animation")]
    [SerializeField] private float scoreCountDuration = 1f;

    [Header("Score Emphasis Animation")]
    [SerializeField] private float scorePunchDuration = 0.18f;
    [SerializeField] private float scorePunchScale = 1.08f;

    [Header("References")]
    [SerializeField] private SegmentLoopScroller segmentLoopScroller;
    [SerializeField] private WeightManager weightManager;
    [SerializeField] private FinalScoreManager finalScoreManager;
    [SerializeField] private GameManager gameManager;

    [Header("Scene Flow")]
    [SerializeField] private string titleSceneName = DefaultTitleSceneName;

    private RectTransform _finalScoreRectTransform;
    private Vector3 _panelBaseScale = Vector3.one;
    private Vector3 _finalScoreBaseScale = Vector3.one;

    private float _panelAnimationTimer;
    private float _scoreAnimationTimer;
    private float _scorePunchTimer;

    private bool _isPanelAnimating;
    private bool _isScoreCounting;
    private bool _isScorePunchAnimating;

    private int _targetDistance;
    private int _targetWeight;
    private int _targetFinalScore;

    private void Awake()
    {
        ResolveReferences();
        CacheAnimationTargets();
        HideResult();
    }

    private void Update()
    {
        float deltaTime = Time.unscaledDeltaTime;

        UpdatePanelAnimation(deltaTime);
        UpdateScoreCountAnimation(deltaTime);
        UpdateScorePunchAnimation(deltaTime);
    }

    /// <summary>
    /// Reads the latest values from existing managers and shows the result panel.
    /// </summary>
    public void ShowResult()
    {
        ResolveReferences();

        int distance = segmentLoopScroller != null ? Mathf.FloorToInt(segmentLoopScroller.Distance) : 0;
        int weight = weightManager != null ? weightManager.CurrentWeight : 0;
        int finalScore = finalScoreManager != null ? Mathf.FloorToInt(finalScoreManager.CurrentFinalScore) : 0;

        ShowResult(distance, weight, finalScore);
    }

    /// <summary>
    /// Shows the result panel with explicitly provided values.
    /// </summary>
    public void ShowResult(int distance, int weight, int finalScore)
    {
        ResolveReferences();
        CacheAnimationTargets();

        _targetDistance = Mathf.Max(0, distance);
        _targetWeight = Mathf.Max(0, weight);
        _targetFinalScore = Mathf.Max(0, finalScore);

        SetPanelActive(true);
        SetActionButtonsActive(true);
        ResetAnimationState();
        ApplyStaticTexts();
        ApplyRankText(_targetFinalScore);
        ApplyInitialVisualState();
        StartAnimations();
        WarnForMissingReferences();
    }

    /// <summary>
    /// Hides the result panel and resets lightweight animation state.
    /// </summary>
    public void HideResult()
    {
        StopAnimations();
        ResetVisualScale();
        SetActionButtonsActive(false);
        SetPanelActive(false);
    }

    /// <summary>
    /// Called from the Retry button to restart the current game scene state.
    /// </summary>
    public void OnClickRetry()
    {
        ResolveReferences();

        if (gameManager == null)
        {
            Debug.LogWarning("[ResultPanelUI] GameManager is not assigned.");
            return;
        }

        gameManager.RestartGame();
    }

    /// <summary>
    /// Called from the Title button to return to the title scene.
    /// </summary>
    public void OnClickBackToTitle()
    {
        if (string.IsNullOrWhiteSpace(titleSceneName))
        {
            Debug.LogWarning("[ResultPanelUI] Title scene name is not assigned.");
            return;
        }

        SceneManager.LoadScene(titleSceneName);
    }

    private void ResolveReferences()
    {
        if (panelRoot == null)
        {
            panelRoot = gameObject;
        }

        if (panelRectTransform == null && panelRoot != null)
        {
            panelRectTransform = panelRoot.GetComponent<RectTransform>();
        }

        if (panelCanvasGroup == null && panelRoot != null)
        {
            panelCanvasGroup = panelRoot.GetComponent<CanvasGroup>();
        }

        if (segmentLoopScroller == null)
        {
            segmentLoopScroller = FindFirstObjectByType<SegmentLoopScroller>();
        }

        if (weightManager == null)
        {
            weightManager = FindFirstObjectByType<WeightManager>();
        }

        if (finalScoreManager == null)
        {
            finalScoreManager = FindFirstObjectByType<FinalScoreManager>();
        }

        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
    }

    private void CacheAnimationTargets()
    {
        if (panelRectTransform != null)
        {
            _panelBaseScale = panelRectTransform.localScale;
        }

        if (finalScoreText != null)
        {
            _finalScoreRectTransform = finalScoreText.rectTransform;
        }

        if (_finalScoreRectTransform != null)
        {
            _finalScoreBaseScale = _finalScoreRectTransform.localScale;
        }
    }

    private void ApplyStaticTexts()
    {
        if (distanceText != null)
        {
            distanceText.text = $"{distanceLabel} : {_targetDistance}m";
        }
        else
        {
            Debug.LogWarning("[ResultPanelUI] Distance text is not assigned.");
        }

        if (weightText != null)
        {
            weightText.text = $"{weightLabel} : {_targetWeight}";
        }
        else
        {
            Debug.LogWarning("[ResultPanelUI] Weight text is not assigned.");
        }

        UpdateFinalScoreText(0);
    }

    private void ApplyRankText(int finalScore)
    {
        if (rankText == null)
        {
            Debug.LogWarning("[ResultPanelUI] Rank text is not assigned.");
            return;
        }

        rankText.text = GetRankLabel(finalScore);
    }

    private string GetRankLabel(int finalScore)
    {
        if (finalScore >= amazingThreshold)
        {
            return AmazingRankLabel;
        }

        if (finalScore >= greatThreshold)
        {
            return GreatRankLabel;
        }

        return GoodRankLabel;
    }

    private void ApplyInitialVisualState()
    {
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 1f;
        }

        if (panelRectTransform != null)
        {
            panelRectTransform.localScale = _panelBaseScale * panelStartScale;
        }

        if (_finalScoreRectTransform != null)
        {
            _finalScoreRectTransform.localScale = _finalScoreBaseScale;
        }
    }

    private void StartAnimations()
    {
        _isPanelAnimating = panelRectTransform != null && panelPopDuration > 0f;
        _isScoreCounting = finalScoreText != null && scoreCountDuration > 0f;
        _isScorePunchAnimating = false;

        _panelAnimationTimer = 0f;
        _scoreAnimationTimer = 0f;
        _scorePunchTimer = 0f;

        if (!_isPanelAnimating && panelRectTransform != null)
        {
            panelRectTransform.localScale = _panelBaseScale * panelEndScale;
        }

        if (!_isScoreCounting)
        {
            UpdateFinalScoreText(_targetFinalScore);
            StartScorePunchAnimation();
        }
    }

    private void StopAnimations()
    {
        _isPanelAnimating = false;
        _isScoreCounting = false;
        _isScorePunchAnimating = false;

        _panelAnimationTimer = 0f;
        _scoreAnimationTimer = 0f;
        _scorePunchTimer = 0f;
    }

    private void ResetAnimationState()
    {
        StopAnimations();
        ResetVisualScale();
    }

    private void ResetVisualScale()
    {
        if (panelRectTransform != null)
        {
            panelRectTransform.localScale = _panelBaseScale;
        }

        if (_finalScoreRectTransform != null)
        {
            _finalScoreRectTransform.localScale = _finalScoreBaseScale;
        }
    }

    private void UpdatePanelAnimation(float deltaTime)
    {
        if (!_isPanelAnimating || panelRectTransform == null)
        {
            return;
        }

        _panelAnimationTimer += deltaTime;
        float progress = Mathf.Clamp01(_panelAnimationTimer / Mathf.Max(0.0001f, panelPopDuration));
        float easedProgress = EaseOutBack(progress);
        float scale = Mathf.LerpUnclamped(panelStartScale, panelEndScale, easedProgress);

        panelRectTransform.localScale = _panelBaseScale * scale;

        if (progress >= 1f)
        {
            _isPanelAnimating = false;
            panelRectTransform.localScale = _panelBaseScale * panelEndScale;
        }
    }

    private void UpdateScoreCountAnimation(float deltaTime)
    {
        if (!_isScoreCounting)
        {
            return;
        }

        _scoreAnimationTimer += deltaTime;
        float progress = Mathf.Clamp01(_scoreAnimationTimer / Mathf.Max(0.0001f, scoreCountDuration));
        float easedProgress = EaseOutCubic(progress);
        int displayScore = Mathf.RoundToInt(Mathf.Lerp(0f, _targetFinalScore, easedProgress));

        UpdateFinalScoreText(displayScore);

        if (progress >= 1f)
        {
            _isScoreCounting = false;
            UpdateFinalScoreText(_targetFinalScore);
            StartScorePunchAnimation();
        }
    }

    private void StartScorePunchAnimation()
    {
        if (_finalScoreRectTransform == null || scorePunchDuration <= 0f)
        {
            return;
        }

        _isScorePunchAnimating = true;
        _scorePunchTimer = 0f;
        _finalScoreRectTransform.localScale = _finalScoreBaseScale;
    }

    private void UpdateScorePunchAnimation(float deltaTime)
    {
        if (!_isScorePunchAnimating || _finalScoreRectTransform == null)
        {
            return;
        }

        _scorePunchTimer += deltaTime;
        float progress = Mathf.Clamp01(_scorePunchTimer / Mathf.Max(0.0001f, scorePunchDuration));
        float wave = Mathf.Sin(progress * Mathf.PI);
        float scaleMultiplier = Mathf.Lerp(1f, scorePunchScale, wave);

        _finalScoreRectTransform.localScale = _finalScoreBaseScale * scaleMultiplier;

        if (progress >= 1f)
        {
            _isScorePunchAnimating = false;
            _finalScoreRectTransform.localScale = _finalScoreBaseScale;
        }
    }

    private void UpdateFinalScoreText(int scoreValue)
    {
        if (finalScoreText == null)
        {
            Debug.LogWarning("[ResultPanelUI] Final score text is not assigned.");
            return;
        }

        finalScoreText.text = $"{finalScoreLabel} : {Mathf.Max(0, scoreValue)}";
    }

    private void WarnForMissingReferences()
    {
        if (segmentLoopScroller == null)
        {
            Debug.LogWarning("[ResultPanelUI] SegmentLoopScroller is not assigned.");
        }

        if (weightManager == null)
        {
            Debug.LogWarning("[ResultPanelUI] WeightManager is not assigned.");
        }

        if (finalScoreManager == null)
        {
            Debug.LogWarning("[ResultPanelUI] FinalScoreManager is not assigned.");
        }

        if (gameManager == null)
        {
            Debug.LogWarning("[ResultPanelUI] GameManager is not assigned.");
        }
    }

    private void SetActionButtonsActive(bool isActive)
    {
        if (retryButtonRoot != null)
        {
            retryButtonRoot.SetActive(isActive);
        }

        if (titleButtonRoot != null)
        {
            titleButtonRoot.SetActive(isActive);
        }
    }

    private void SetPanelActive(bool isActive)
    {
        if (panelRoot == null)
        {
            panelRoot = gameObject;
        }

        panelRoot.SetActive(isActive);
    }

    private float EaseOutCubic(float t)
    {
        float inverse = 1f - Mathf.Clamp01(t);
        return 1f - (inverse * inverse * inverse);
    }

    private float EaseOutBack(float t)
    {
        float clamped = Mathf.Clamp01(t);
        const float s = 1.70158f;
        float adjusted = clamped - 1f;
        return 1f + ((s + 1f) * adjusted * adjusted * adjusted) + (s * adjusted * adjusted);
    }
}

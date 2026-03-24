using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the in-scene result panel display.
/// Responsible only for showing/hiding the panel and reflecting result values.
/// </summary>
public class ResultPanelUI : MonoBehaviour
{
    private const string DefaultTitleSceneName = "Title";

    [Header("Panel Root")]
    [SerializeField] private GameObject panelRoot;

    [Header("Buttons")]
    [SerializeField] private GameObject retryButtonRoot;
    [SerializeField] private GameObject titleButtonRoot;

    [Header("Result Text")]
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI weightText;
    [SerializeField] private TextMeshProUGUI finalScoreText;

    [Header("References")]
    [SerializeField] private SegmentLoopScroller segmentLoopScroller;
    [SerializeField] private WeightManager weightManager;
    [SerializeField] private FinalScoreManager finalScoreManager;
    [SerializeField] private GameManager gameManager;

    [Header("Scene Flow")]
    [SerializeField] private string titleSceneName = DefaultTitleSceneName;

    private void Awake()
    {
        ResolveReferences();
        HideResult();
    }

    /// <summary>
    /// Updates all result labels with the latest values, then shows the panel.
    /// </summary>
    public void ShowResult()
    {
        ResolveReferences();
        UpdateResultTexts();
        SetPanelActive(true);
        SetActionButtonsActive(true);
    }

    /// <summary>
    /// Hides the result panel.
    /// </summary>
    public void HideResult()
    {
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

    private void UpdateResultTexts()
    {
        float distance = segmentLoopScroller != null ? segmentLoopScroller.Distance : 0f;
        int weight = weightManager != null ? weightManager.CurrentWeight : 0;
        float finalScore = finalScoreManager != null ? finalScoreManager.CurrentFinalScore : 0f;

        if (distanceText != null)
        {
            distanceText.text = $"Distance : {Mathf.FloorToInt(distance)}m";
        }
        else
        {
            Debug.LogWarning("[ResultPanelUI] Distance text is not assigned.");
        }

        if (weightText != null)
        {
            weightText.text = $"Weight : {weight}";
        }
        else
        {
            Debug.LogWarning("[ResultPanelUI] Weight text is not assigned.");
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = $"FinalScore : {Mathf.FloorToInt(finalScore)}";
        }
        else
        {
            Debug.LogWarning("[ResultPanelUI] Final score text is not assigned.");
        }

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
}

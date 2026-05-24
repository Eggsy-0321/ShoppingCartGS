using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls pause UI visibility and the pause/resume flow without owning core game logic.
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    [Header("Pause UI")]
    [SerializeField] private GameObject pauseButtonRoot;
    [SerializeField] private Button pauseButton;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button topButton;

    [Header("Gameplay References")]
    [SerializeField] private ResultPanelUI resultPanelUI;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PlayerLaneController playerLaneController;
    [SerializeField] private PlayerFlickInputController playerFlickInputController;
    [SerializeField] private PlayerAnimationController playerAnimationController;

    [Header("Desktop Debug")]
    [SerializeField] private bool enableEscapeToggle = true;

    private bool _isPaused;

    private void Awake()
    {
        RestoreTimeScale();
        ResolveReferences();
        SetPausePanelVisible(false);
        UpdatePauseButtonState();
    }

    private void OnEnable()
    {
        ResolveReferences();
        RegisterButtonListeners();
        UpdatePauseButtonState();
    }

    private void Update()
    {
        ResolveReferences();

        if (IsResultShowing() && _isPaused)
        {
            ClearPauseState(false);
        }

        UpdatePauseButtonState();
        HandleEscapeToggle();
    }

    private void OnDisable()
    {
        UnregisterButtonListeners();
        ClearPauseState(false);
    }

    private void OnDestroy()
    {
        RestoreTimeScale();
    }

    public void OnPauseButtonPressed()
    {
        PauseGame();
    }

    public void OnResumeButtonPressed()
    {
        ResumeGame();
    }

    public void OnRetryButtonPressed()
    {
        PrepareForSceneAction();

        Button resultRetryButton = resultPanelUI != null ? resultPanelUI.RetryButton : null;
        if (resultRetryButton != null)
        {
            resultRetryButton.onClick.Invoke();
            return;
        }

        if (resultPanelUI != null)
        {
            resultPanelUI.OnClickRetry();
            return;
        }

        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
    }

    public void OnTopButtonPressed()
    {
        PrepareForSceneAction();

        Button resultTitleButton = resultPanelUI != null ? resultPanelUI.TitleButton : null;
        if (resultTitleButton != null)
        {
            resultTitleButton.onClick.Invoke();
            return;
        }

        if (resultPanelUI != null)
        {
            resultPanelUI.OnClickBackToTitle();
        }
    }

    private void PauseGame()
    {
        if (_isPaused || !CanPause())
        {
            return;
        }

        _isPaused = true;
        Time.timeScale = 0f;
        SetPlayerControlEnabled(false);
        StopPlayerAnimation();
        SetPausePanelVisible(true);
        SetPauseButtonVisible(false);
    }

    private void ResumeGame()
    {
        if (!_isPaused)
        {
            return;
        }

        ClearPauseState(true);
    }

    private void PrepareForSceneAction()
    {
        ClearPauseState(false);
    }

    private void ClearPauseState(bool resumeGameplay)
    {
        RestoreTimeScale();

        if (!_isPaused && pausePanel != null && !pausePanel.activeSelf)
        {
            UpdatePauseButtonState();
            return;
        }

        _isPaused = false;
        SetPausePanelVisible(false);

        if (resumeGameplay && ShouldAllowGameplayInput())
        {
            SetPlayerControlEnabled(true);
            ResumePlayerAnimation();
        }

        UpdatePauseButtonState();
    }

    private bool CanPause()
    {
        return !IsResultShowing() && !IsGameOver();
    }

    private bool ShouldAllowGameplayInput()
    {
        return !IsResultShowing() && !IsGameOver();
    }

    private bool IsResultShowing()
    {
        return resultPanelUI != null && resultPanelUI.IsVisible;
    }

    private bool IsGameOver()
    {
        return gameManager != null && gameManager.IsGameOver;
    }

    private void HandleEscapeToggle()
    {
        if (!enableEscapeToggle)
        {
            return;
        }

        if (!IsDesktopPauseToggleAvailable())
        {
            return;
        }

        if (!Input.GetKeyDown(KeyCode.Escape))
        {
            return;
        }

        if (_isPaused)
        {
            ResumeGame();
            return;
        }

        PauseGame();
    }

    private bool IsDesktopPauseToggleAvailable()
    {
        return Application.isEditor
            || Application.platform == RuntimePlatform.WindowsPlayer
            || Application.platform == RuntimePlatform.OSXPlayer
            || Application.platform == RuntimePlatform.LinuxPlayer;
    }

    private void SetPausePanelVisible(bool isVisible)
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(isVisible);
        }
    }

    private void UpdatePauseButtonState()
    {
        bool shouldShowPauseButton = !_isPaused && !IsResultShowing();
        SetPauseButtonVisible(shouldShowPauseButton);

        if (pauseButton != null)
        {
            pauseButton.interactable = shouldShowPauseButton;
        }
    }

    private void SetPauseButtonVisible(bool isVisible)
    {
        if (pauseButtonRoot != null)
        {
            pauseButtonRoot.SetActive(isVisible);
        }
    }

    private void SetPlayerControlEnabled(bool isEnabled)
    {
        if (playerLaneController != null)
        {
            playerLaneController.enabled = isEnabled;
        }

        if (playerFlickInputController != null)
        {
            playerFlickInputController.enabled = isEnabled;
        }
    }

    private void StopPlayerAnimation()
    {
        if (playerAnimationController != null)
        {
            playerAnimationController.StopRunAnimation();
        }
    }

    private void ResumePlayerAnimation()
    {
        if (playerAnimationController != null)
        {
            playerAnimationController.PlayRunAnimation();
        }
    }

    private void RegisterButtonListeners()
    {
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(OnPauseButtonPressed);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(OnResumeButtonPressed);
        }

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryButtonPressed);
        }

        if (topButton != null)
        {
            topButton.onClick.AddListener(OnTopButtonPressed);
        }
    }

    private void UnregisterButtonListeners()
    {
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(OnPauseButtonPressed);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(OnResumeButtonPressed);
        }

        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(OnRetryButtonPressed);
        }

        if (topButton != null)
        {
            topButton.onClick.RemoveListener(OnTopButtonPressed);
        }
    }

    private void ResolveReferences()
    {
        if (pauseButtonRoot == null)
        {
            pauseButtonRoot = FindChildGameObject("PauseButton");
        }

        if (pauseButton == null && pauseButtonRoot != null)
        {
            pauseButton = pauseButtonRoot.GetComponent<Button>();
        }

        if (pausePanel == null)
        {
            pausePanel = FindChildGameObject("PausePanel");
        }

        if (resumeButton == null)
        {
            resumeButton = FindChildComponentUnderParent<Button>(pausePanel, "ResumeButton");
        }

        if (retryButton == null)
        {
            retryButton = FindChildComponentUnderParent<Button>(pausePanel, "RetryButton");
        }

        if (topButton == null)
        {
            topButton = FindChildComponentUnderParent<Button>(pausePanel, "TopButton");
        }

        if (resultPanelUI == null)
        {
            resultPanelUI = FindFirstObjectByType<ResultPanelUI>(FindObjectsInactive.Include);
        }

        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        if (playerLaneController == null)
        {
            playerLaneController = FindFirstObjectByType<PlayerLaneController>();
        }

        if (playerFlickInputController == null)
        {
            playerFlickInputController = FindFirstObjectByType<PlayerFlickInputController>();
        }

        if (playerAnimationController == null)
        {
            playerAnimationController = FindFirstObjectByType<PlayerAnimationController>();
        }
    }

    private GameObject FindChildGameObject(string objectName)
    {
        Transform child = FindChildTransform(objectName);
        return child != null ? child.gameObject : null;
    }

    private T FindChildComponentUnderParent<T>(GameObject parentObject, string objectName) where T : Component
    {
        if (parentObject == null)
        {
            return null;
        }

        Transform[] children = parentObject.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].name == objectName)
            {
                return children[i].GetComponent<T>();
            }
        }

        return null;
    }

    private Transform FindChildTransform(string objectName)
    {
        Transform root = transform;
        Transform[] children = root.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].name == objectName)
            {
                return children[i];
            }
        }

        return null;
    }

    private void RestoreTimeScale()
    {
        Time.timeScale = 1f;
    }
}

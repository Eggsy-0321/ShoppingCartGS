using UnityEngine;

/// <summary>
/// Detects horizontal flick input and forwards lane move requests to PlayerLaneController.
/// </summary>
public class PlayerFlickInputController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerLaneController laneController;

    [Header("Flick Settings")]
    [SerializeField] private float minFlickDistance = 100f;
    [SerializeField] private float horizontalDominanceRatio = 1.2f;
    [SerializeField] private bool enableMouseInputInEditor = true;

    private Vector2 _dragStartPosition;
    private bool _isTrackingInput;

    private void Awake()
    {
        ResolveLaneController();
    }

    private void Update()
    {
        HandleTouchInput();
        HandleEditorMouseInput();
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount <= 0)
        {
            return;
        }

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                BeginTracking(touch.position);
                break;
            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                EndTracking(touch.position);
                break;
        }
    }

    private void HandleEditorMouseInput()
    {
        if (!Application.isEditor || !enableMouseInputInEditor)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            BeginTracking(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndTracking(Input.mousePosition);
        }
    }

    private void BeginTracking(Vector2 startPosition)
    {
        _dragStartPosition = startPosition;
        _isTrackingInput = true;
    }

    private void EndTracking(Vector2 endPosition)
    {
        if (!_isTrackingInput)
        {
            return;
        }

        _isTrackingInput = false;
        TryHandleFlick(endPosition - _dragStartPosition);
    }

    private void TryHandleFlick(Vector2 delta)
    {
        if (laneController == null)
        {
            ResolveLaneController();
            if (laneController == null)
            {
                return;
            }
        }

        float absDeltaX = Mathf.Abs(delta.x);
        float absDeltaY = Mathf.Abs(delta.y);

        if (absDeltaX < minFlickDistance)
        {
            return;
        }

        if (absDeltaX < absDeltaY * horizontalDominanceRatio)
        {
            return;
        }

        if (delta.x < 0f)
        {
            laneController.RequestMoveLeft();
        }
        else if (delta.x > 0f)
        {
            laneController.RequestMoveRight();
        }
    }

    private void ResolveLaneController()
    {
        if (laneController == null)
        {
            laneController = GetComponent<PlayerLaneController>();
        }
    }
}

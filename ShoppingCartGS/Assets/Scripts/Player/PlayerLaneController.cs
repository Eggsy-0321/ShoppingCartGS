using UnityEngine;

/// <summary>
/// Moves the player across the fixed 3-lane layout.
/// Keyboard input remains available for PC-side verification.
/// </summary>
public class PlayerLaneController : MonoBehaviour
{
    [Header("Lane Settings")]
    [Tooltip("Lane X positions for left, center, and right.")]
    [SerializeField] private float[] laneX = new float[] { -1.4f, 0f, 1.4f };

    [Tooltip("Starting lane index. 0 = left, 1 = center, 2 = right.")]
    [SerializeField] private int startLaneIndex = 1;

    [Header("Movement Settings")]
    [Tooltip("Horizontal movement speed when changing lanes.")]
    [SerializeField] private float laneMoveSpeed = 12f;

    [Tooltip("Snap to the lane when the remaining distance is below this value.")]
    [SerializeField] private float arriveThreshold = 0.01f;

    private int _currentLaneIndex;
    private float _targetX;

    private void Awake()
    {
        if (laneX == null || laneX.Length != 3)
        {
            Debug.LogError("[PlayerLaneController] laneX must contain exactly 3 lane positions.");
            laneX = new float[] { -1.4f, 0f, 1.4f };
        }

        _currentLaneIndex = Mathf.Clamp(startLaneIndex, 0, 2);
        _targetX = laneX[_currentLaneIndex];

        Vector3 position = transform.position;
        position.x = _targetX;
        transform.position = position;
    }

    private void Update()
    {
        HandleInput();
        MoveToTargetLane();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            RequestMoveLeft();
        }

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            RequestMoveRight();
        }
    }

    public void RequestMoveLeft()
    {
        if (!CanAcceptLaneChangeRequest())
        {
            return;
        }

        ChangeLane(-1);
    }

    public void RequestMoveRight()
    {
        if (!CanAcceptLaneChangeRequest())
        {
            return;
        }

        ChangeLane(+1);
    }

    private void ChangeLane(int delta)
    {
        int nextLaneIndex = Mathf.Clamp(_currentLaneIndex + delta, 0, 2);
        if (nextLaneIndex == _currentLaneIndex)
        {
            return;
        }

        _currentLaneIndex = nextLaneIndex;
        _targetX = laneX[_currentLaneIndex];
    }

    private void MoveToTargetLane()
    {
        Vector3 position = transform.position;
        float newX = Mathf.MoveTowards(position.x, _targetX, laneMoveSpeed * Time.deltaTime);
        position.x = newX;
        transform.position = position;

        if (Mathf.Abs(transform.position.x - _targetX) <= arriveThreshold)
        {
            position = transform.position;
            position.x = _targetX;
            transform.position = position;
        }
    }

    private bool CanAcceptLaneChangeRequest()
    {
        return isActiveAndEnabled;
    }

    public int CurrentLaneIndex => _currentLaneIndex;
}

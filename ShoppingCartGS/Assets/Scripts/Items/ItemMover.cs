using UnityEngine;

/// <summary>
/// Moves a spawned item toward the player using the same speed as the world scroll.
/// </summary>
public class ItemMover : MonoBehaviour
{
    [SerializeField] private float despawnBackwardOffset = 10f;
    [SerializeField] private SegmentLoopScroller segmentLoopScroller;
    [SerializeField] private Transform playerTransform;

    private bool _hasLoggedMissingReference;

    /// <summary>
    /// Injects required references right after spawn.
    /// </summary>
    public void Initialize(SegmentLoopScroller segmentLoopScroller, Transform playerTransform)
    {
        this.segmentLoopScroller = segmentLoopScroller;
        this.playerTransform = playerTransform;
        _hasLoggedMissingReference = false;
    }

    private void OnValidate()
    {
        if (despawnBackwardOffset < 0f)
        {
            despawnBackwardOffset = 0f;
        }
    }

    private void Update()
    {
        if (segmentLoopScroller == null || playerTransform == null)
        {
            if (!_hasLoggedMissingReference)
            {
                Debug.LogWarning("[ItemMover] Missing SegmentLoopScroller or Player reference.", this);
                _hasLoggedMissingReference = true;
            }

            return;
        }

        float currentSpeed = segmentLoopScroller.CurrentSpeed;
        if (currentSpeed <= 0f)
        {
            return;
        }

        // Move in world space so the item always travels toward the camera on -Z.
        float dz = currentSpeed * Time.deltaTime;
        transform.position += Vector3.back * dz;

        // Remove items after they pass behind the player.
        if (transform.position.z < playerTransform.position.z - despawnBackwardOffset)
        {
            Destroy(gameObject);
        }
    }
}

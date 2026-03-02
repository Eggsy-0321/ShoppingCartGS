using UnityEngine;

/// <summary>
/// 3レーン(-1.4, 0, +1.4)を左右入力で移動するコントローラー（初心者向け）
/// - PCテスト用に A/D または ←/→ で操作
/// - レーン間はスムーズに補間移動
/// </summary>
public class PlayerLaneController : MonoBehaviour
{
    [Header("Lane Settings")]
    [Tooltip("左・中央・右レーンのX座標。GDD: -1.4 / 0 / +1.4")]
    [SerializeField] private float[] laneX = new float[] { -1.4f, 0f, 1.4f };

    [Tooltip("開始レーン (0=左, 1=中央, 2=右)")]
    [SerializeField] private int startLaneIndex = 1;

    [Header("Movement Settings")]
    [Tooltip("レーン変更の移動速度（大きいほどキビキビ）")]
    [SerializeField] private float laneMoveSpeed = 12f;

    [Tooltip("移動完了と見なす距離")]
    [SerializeField] private float arriveThreshold = 0.01f;

    private int _currentLaneIndex;
    private float _targetX;

    private void Awake()
    {
        // レーン配列の最低限チェック
        if (laneX == null || laneX.Length != 3)
        {
            Debug.LogError("[PlayerLaneController] laneX は要素3つ（左/中/右）で設定してください。");
            laneX = new float[] { -1.4f, 0f, 1.4f };
        }

        _currentLaneIndex = Mathf.Clamp(startLaneIndex, 0, 2);
        _targetX = laneX[_currentLaneIndex];

        // 初期位置をレーンに合わせる（Xだけ合わせる）
        Vector3 p = transform.position;
        p.x = _targetX;
        transform.position = p;
    }

    private void Update()
    {
        HandleInput();
        MoveToTargetLane();
    }

    private void HandleInput()
    {
        // 左入力：A または ←
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeLane(-1);
        }

        // 右入力：D または →
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeLane(+1);
        }
    }

    private void ChangeLane(int delta)
    {
        int next = Mathf.Clamp(_currentLaneIndex + delta, 0, 2);
        if (next == _currentLaneIndex) return;

        _currentLaneIndex = next;
        _targetX = laneX[_currentLaneIndex];
    }

    private void MoveToTargetLane()
    {
        Vector3 pos = transform.position;
        float newX = Mathf.MoveTowards(pos.x, _targetX, laneMoveSpeed * Time.deltaTime);
        pos.x = newX;
        transform.position = pos;

        // ピタッと止める（微小揺れ防止）
        if (Mathf.Abs(transform.position.x - _targetX) <= arriveThreshold)
        {
            pos = transform.position;
            pos.x = _targetX;
            transform.position = pos;
        }
    }

    // いまどのレーンか外部から参照したくなった時用（後で使う可能性あり）
    public int CurrentLaneIndex => _currentLaneIndex;
}
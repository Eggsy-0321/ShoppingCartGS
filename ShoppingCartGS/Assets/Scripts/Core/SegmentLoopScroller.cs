using UnityEngine;

/// <summary>
/// B案（プレイヤー固定）用：床などのセグメントを後ろに流し、
/// 後ろに行ききったセグメントを前方へ回して無限に見せる。
/// さらに、現在速度をもとに移動距離を積算する。
/// </summary>
public class SegmentLoopScroller : MonoBehaviour
{
    [Header("References")]
    [Tooltip("固定プレイヤー（位置判定に使う）")]
    [SerializeField] private Transform player;

    [Tooltip("ループさせるセグメント（Segment_0, Segment_1 ...）をここに入れる")]
    [SerializeField] private Transform[] segments;

    [Header("Speed")]
    [SerializeField] private float baseSpeed = 12f;

    [Header("Segment Settings")]
    [Tooltip("1セグメントの長さ。GroundがPlaneでScaleZ=20なら 10*20=200")]
    [SerializeField] private float segmentLength = 200f;

    [Tooltip("完全に後ろへ抜けた判定に使う余裕")]
    [SerializeField] private float recycleBuffer = 5f;

    [Header("Distance")]
    [Tooltip("距離の表示倍率。基本は1のままでOK")]
    [SerializeField] private float distanceMultiplier = 1f;

    private float _distance;
    private bool _isScrolling = true;

    /// <summary>
    /// 現在の累計距離
    /// </summary>
    public float Distance => _distance;

    /// <summary>
    /// 現在の基礎速度
    /// </summary>
    public float BaseSpeed
    {
        get => baseSpeed;
        set => baseSpeed = Mathf.Max(0f, value);
    }

    /// <summary>
    /// スクロール中かどうか
    /// </summary>
    public bool IsScrolling => _isScrolling;

    private void Awake()
    {
        if (player == null)
        {
            Debug.LogError("[SegmentLoopScroller] Player が未設定です。InspectorでPlayerを入れてください。");
        }

        if (segments == null || segments.Length < 2)
        {
            Debug.LogError("[SegmentLoopScroller] Segments は最低2つ必要です（Segment_0, Segment_1）。");
        }
    }

    private void Update()
    {
        if (!_isScrolling) return;

        float speed = baseSpeed;
        float dz = speed * Time.deltaTime;

        // 距離積算
        _distance += dz * distanceMultiplier;

        // 全セグメントを手前へ流す
        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i] == null) continue;

            Vector3 p = segments[i].position;
            p.z -= dz;
            segments[i].position = p;
        }

        // 後ろに抜けたセグメントを前方へ回す
        float playerZ = player != null ? player.position.z : 0f;
        float half = segmentLength * 0.5f;

        float frontMostZ = float.MinValue;
        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i] == null) continue;

            if (segments[i].position.z > frontMostZ)
            {
                frontMostZ = segments[i].position.z;
            }
        }

        for (int i = 0; i < segments.Length; i++)
        {
            Transform seg = segments[i];
            if (seg == null) continue;

            float segFrontEdgeZ = seg.position.z + half;

            if (segFrontEdgeZ < playerZ - recycleBuffer)
            {
                Vector3 p = seg.position;
                p.z = frontMostZ + segmentLength;
                seg.position = p;

                frontMostZ = p.z;
            }
        }
    }

    /// <summary>
    /// スクロール開始
    /// </summary>
    public void StartScrolling()
    {
        _isScrolling = true;
    }

    /// <summary>
    /// スクロール停止
    /// </summary>
    public void StopScrolling()
    {
        _isScrolling = false;
    }

    /// <summary>
    /// 距離をリセット
    /// </summary>
    public void ResetDistance()
    {
        _distance = 0f;
    }
}
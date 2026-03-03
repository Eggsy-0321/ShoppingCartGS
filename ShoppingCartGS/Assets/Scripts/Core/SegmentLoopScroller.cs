using UnityEngine;

/// <summary>
/// B案（プレイヤー固定）用：床などの「セグメント」を後ろに流し、
/// 後ろに行ききったセグメントを前方へ回して無限に見せる。
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

    [Tooltip("完全に後ろへ抜けた判定に使う余裕（小さくてOK）")]
    [SerializeField] private float recycleBuffer = 5f;

    private float _distance;

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
        float speed = baseSpeed;
        float dz = speed * Time.deltaTime;

        // 距離（GDD準拠の積算）
        _distance += dz;

        // 全セグメントを手前へ流す（-Z方向へ）
        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i] == null) continue;
            Vector3 p = segments[i].position;
            p.z -= dz;
            segments[i].position = p;
        }

        // 後ろに抜けたセグメントを前方へ回す
        // 条件：そのセグメントの「前端」(center + length/2) がプレイヤーより十分後ろ
        float playerZ = player != null ? player.position.z : 0f;
        float half = segmentLength * 0.5f;

        // 今一番前にいるセグメントのZを探す（回す先を決めるため）
        float frontMostZ = float.MinValue;
        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i] == null) continue;
            if (segments[i].position.z > frontMostZ) frontMostZ = segments[i].position.z;
        }

        for (int i = 0; i < segments.Length; i++)
        {
            Transform seg = segments[i];
            if (seg == null) continue;

            float segFrontEdgeZ = seg.position.z + half;

            // セグメント全体がプレイヤーより後ろに抜けたら、最前方のさらに前へ回す
            if (segFrontEdgeZ < playerZ - recycleBuffer)
            {
                Vector3 p = seg.position;
                p.z = frontMostZ + segmentLength;
                seg.position = p;

                // frontMostZ 更新（連続で回す可能性に備える）
                frontMostZ = p.z;
            }
        }
    }

    public float Distance => _distance;

    public float BaseSpeed
    {
        get => baseSpeed;
        set => baseSpeed = Mathf.Max(0f, value);
    }
}
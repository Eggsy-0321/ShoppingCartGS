using UnityEngine;

/// <summary>
/// B案：プレイヤー固定で「世界」を後ろに流す。
/// - WorldRoot を -Z 方向に移動
/// - 一定距離（loopLength）流れたら +loopLength して位置を戻し、無限に見せる
/// </summary>
public class WorldLoopScroller : MonoBehaviour
{
    [Header("Speed")]
    [Tooltip("ベース速度（重さ減速などは次のステップで入れる）")]
    [SerializeField] private float baseSpeed = 8f;

    [Header("Loop")]
    [Tooltip("ループ距離（例：Groundの奥行き）")]
    [SerializeField] private float loopLength = 200f;

    [Tooltip("ループ判定のしきい値（少し余裕を持たせる）")]
    [SerializeField] private float loopTriggerZ = -200f;

    [Header("Debug")]
    [SerializeField] private bool logDistance = false;

    private float _distance;

    private void Reset()
    {
        // 初期値の目安：
        // UnityのPlaneは 10 x 10 ユニット
        // GroundのScale.z が 20 なら奥行きは 10 * 20 = 200
        baseSpeed = 8f;
        loopLength = 200f;
        loopTriggerZ = -200f;
        logDistance = false;
    }

    private void Update()
    {
        float speed = baseSpeed;

        // 世界を後ろへ流す（= プレイヤーは前進しているように見える）
        Vector3 pos = transform.position;
        pos.z -= speed * Time.deltaTime;
        transform.position = pos;

        // 距離を積算（GDD式: Distance += CurrentSpeed * deltaTime）
        _distance += speed * Time.deltaTime;

        // ループ：一定以上後ろに行ったら、前へ戻す
        if (transform.position.z <= loopTriggerZ)
        {
            Vector3 p = transform.position;
            p.z += loopLength;
            transform.position = p;
        }

        if (logDistance)
        {
            // 毎フレームだとうるさいので、適度に出したいなら後で間引く
            Debug.Log($"Distance: {_distance:0.00}");
        }
    }

    public float Distance => _distance;
    public float BaseSpeed
    {
        get => baseSpeed;
        set => baseSpeed = Mathf.Max(0f, value);
    }
}
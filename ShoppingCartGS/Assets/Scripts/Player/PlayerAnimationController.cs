using UnityEngine;

/// <summary>
/// PlayerVisual の走行アニメーション再生状態を制御する。
/// ゲーム進行や移動処理には関与せず、見た目の Animator 制御のみを担当する。
/// </summary>
public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator targetAnimator;
    [SerializeField] private float runningAnimationSpeed = 1f;

    private void Awake()
    {
        if (targetAnimator == null)
        {
            targetAnimator = GetComponent<Animator>();
        }
    }

    private void OnEnable()
    {
        PlayRunAnimation();
    }

    /// <summary>
    /// 走行アニメーションを再生する。
    /// </summary>
    public void PlayRunAnimation()
    {
        if (targetAnimator == null)
        {
            return;
        }

        targetAnimator.speed = runningAnimationSpeed;
    }

    /// <summary>
    /// 現在の表示フレームのまま走行アニメーションを停止する。
    /// </summary>
    public void StopRunAnimation()
    {
        if (targetAnimator == null)
        {
            return;
        }

        targetAnimator.speed = 0f;
    }

    /// <summary>
    /// 走行アニメーションを初期状態に戻して再生する。
    /// Retry時の再開用。
    /// </summary>
    public void RestartRunAnimation()
    {
        if (targetAnimator == null)
        {
            return;
        }

        targetAnimator.Rebind();
        targetAnimator.Update(0f);
        targetAnimator.speed = runningAnimationSpeed;
    }
}

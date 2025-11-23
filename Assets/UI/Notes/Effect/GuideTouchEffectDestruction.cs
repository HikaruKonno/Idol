/*
 * ファイル
 * GuideTouchEffectDestruction C#
 * 
 * システム
 * ガイドのタッチエフェクトの再生が終わったらオブジェクトを破棄するシステム
 * 
 * 作成
 * 2025/09/24 寺門 冴羽
 */

using UnityEngine;

public class GuideTouchEffectDestruction : MonoBehaviour
{
    // --------------------------------------------------------------------------------
    // 変数宣言
    // --------------------------------------------------------------------------------

    private Animator m_animator;        // Animatorを保存する変数


    public delegate void OnDestroyDelegate();
    private event OnDestroyDelegate _onDestroyDelegate;

    // --------------------------------------------------------------------------------
    // メイン関数
    // --------------------------------------------------------------------------------

    // 最初のフレームの処理
    void Start()
    {
        // Animatorコンポーネントを保存する
        m_animator = gameObject.GetComponent<Animator>();
    }

    // 毎フレームの処理
    void Update()
    {
        // アニメーションの再生が終わったら処理
        if (m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
        {
            // このオブジェクトを破棄
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        _onDestroyDelegate?.Invoke();
    }

    public void BindOnDestroyCallback(OnDestroyDelegate callback)
    {
        if (callback == null)
        {
            return;
        }

        if (_onDestroyDelegate != null)
        {
            _onDestroyDelegate += callback;
        }
        else
        {
            _onDestroyDelegate = new OnDestroyDelegate(callback);
        }

    }
}

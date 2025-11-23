/*
 * ファイル
 * UIExpandAndFadeSimultaneous C#
 * 
 * システム
 * エフェクトを拡大しながら同時にフェードアウトさせる
 * 
 * 作成
 * 2025/09/01 坂上　壱希
 * 
 * 最終変更
 * 2025/09/29　坂上　壱希
 */
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))] // Image, RawImage など
sealed public class EffectExpandAndFadeSimultaneous : MonoBehaviour
{
    [SerializeField]
    private float m_duration = 1.0f;              // 拡大＆フェードの時間
    [SerializeField]
    private Vector3 m_targetScale = Vector3.one; // 最終スケール

    private Graphic m_uiGraphic;
    private Vector3 m_initialScale = Vector3.zero;
    private float m_timer = 0f;

    // プール用参照（自作SimpleObjectPoolなど）
    private SimpleObjectPool m_pool;

    public void SetPool(SimpleObjectPool _pool)
    {
        m_pool = _pool;
    }

    void OnEnable()
    {
        m_uiGraphic = GetComponent<Graphic>();
        transform.localScale = m_initialScale;

        // アルファ初期化
        Color color = m_uiGraphic.color;
        color.a = 1f;
        m_uiGraphic.color = color;

        m_timer = 0f;
    }

    void Update()
    {
        m_timer += Time.deltaTime;
        float timer = Mathf.Clamp01(m_timer / m_duration);

        // スケールの補間
        transform.localScale = Vector3.Lerp(m_initialScale, m_targetScale, timer);

        // アルファの補間
        Color color = m_uiGraphic.color;
        color.a = Mathf.Lerp(1f, 0f, timer);
        m_uiGraphic.color = color;

        // 完了したらプールに返す
        if (timer >= 1f)
        {
            ReleaseToPool();
        }
    }

    /// <summary>
    /// オブジェクトをプールに返却するか、プールがなければ破棄する処理を行う。
    /// </summary>
    /// <remarks>
    /// - m_pool が存在する場合は、プール管理クラスの Release メソッドを呼び出してゲームオブジェクトを再利用可能にする。
    /// - m_pool が null の場合は、ゲームオブジェクトを破棄してメモリを解放する。
    /// </remarks>
    private void ReleaseToPool()
    {
        if (m_pool != null)
        {
            // プールに返却
            m_pool.Release(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

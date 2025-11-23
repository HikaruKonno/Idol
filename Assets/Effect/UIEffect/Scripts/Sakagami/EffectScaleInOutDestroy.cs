/*
 * ファイル
 * UIScaleInOutDestroy C#
 * 
 * システム
 * エフェクトを拡大、縮小しながらフェードアウトする
 * 
 * 作成
 * 2025/09/01 坂上　壱希
 * 
 * 最終変更
 * 2025/09/29　坂上　壱希
 */
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class EffectScaleInOutDestroy : MonoBehaviour
{
    [Header("基本設定")]
    [SerializeField]
    private float m_expandDuration = 0.3f;
    [SerializeField]
    private float m_shrinkDuration = 0.3f;
    [SerializeField]
    private Vector3 m_maxScale = Vector3.one;
    [SerializeField]
    private float m_rotationSpeed = 180f;

    [Header("フェードアウト設定")]
    [SerializeField]
    private bool m_enableFadeOut = true;
    [SerializeField]
    private float m_fadeOutSpeed = 1.0f;

    [Header("色変化設定")]
    [SerializeField]
    private bool m_enableColorChange = true;
    [SerializeField]
    private Color m_startColor = Color.white;
    [SerializeField]
    private Color m_endColor = Color.red;

    [Header("ランダム化モード")]
    public bool m_useRandom = false;
    [SerializeField]
    private Vector2 m_expandDurationRange = new Vector2(0.2f, 0.5f);
    [SerializeField]
    private Vector2 m_shrinkDurationRange = new Vector2(0.2f, 0.5f);
    [SerializeField]
    private Vector2 m_scaleRange = new Vector2(0.8f, 1.5f);
    [SerializeField]
    private Vector2 m_rotationSpeedRange = new Vector2(-360f, 360f);
    [SerializeField]
    private Vector2 m_fadeOutSpeedRange = new Vector2(0.5f, 2f);

    private Graphic m_graphic;
    private float m_timer = 0f;
    private float m_alpha = 1f;

    private enum State { Expanding, Shrinking, Done }
    private State m_state = State.Expanding;

    void OnEnable()
    {
        m_graphic = GetComponent<Graphic>();
        transform.localScale = Vector3.zero;
        transform.rotation = Quaternion.identity;
        m_timer = 0f;
        m_alpha = 1f;
        m_state = State.Expanding;

        // ランダム適用
        if (m_useRandom)
        {
            m_expandDuration = Random.Range(m_expandDurationRange.x, m_expandDurationRange.y);
            m_shrinkDuration = Random.Range(m_shrinkDurationRange.x, m_shrinkDurationRange.y);
            m_rotationSpeed = Random.Range(m_rotationSpeedRange.x, m_rotationSpeedRange.y);
            m_fadeOutSpeed = Random.Range(m_fadeOutSpeedRange.x, m_fadeOutSpeedRange.y);
            float scaleValue = Random.Range(m_scaleRange.x, m_scaleRange.y);
            m_maxScale = new Vector3(scaleValue, scaleValue, 1f);
        }

        // 初期カラーとアルファ設定
        if (m_graphic != null)
        {
            m_graphic.color = m_startColor;
        }

        SetAlpha(1f);
    }

    void Update()
    {
        m_timer += Time.deltaTime;

        // 回転
        transform.Rotate(Vector3.forward, m_rotationSpeed * Time.deltaTime);

        switch (m_state)
        {
            case State.Expanding:

                float tExpand = Mathf.Clamp01(m_timer / m_expandDuration);

                transform.localScale = Vector3.Lerp(Vector3.zero, m_maxScale, tExpand);

                if (m_enableColorChange)
                {
                    SetColor(Color.Lerp(m_startColor, m_endColor, tExpand));
                }

                if (tExpand >= 1f)
                {
                    m_timer = 0f;
                    m_state = State.Shrinking;
                }
                break;

            case State.Shrinking:

                float tShrink = Mathf.Clamp01(m_timer / m_shrinkDuration);

                transform.localScale = Vector3.Lerp(m_maxScale, Vector3.zero, tShrink);

                if (m_enableColorChange)
                {
                    SetColor(Color.Lerp(m_endColor, m_startColor, tShrink));
                }

                if (m_enableFadeOut)
                {
                    m_alpha = Mathf.MoveTowards(m_alpha, 0f, m_fadeOutSpeed * Time.deltaTime);
                    SetAlpha(m_alpha);
                }

                if (tShrink >= 1f)
                {
                    m_state = State.Done;
                    ReleaseToPool();
                }
                break;
        }
    }

    private void SetAlpha(float _alpha)
    {
        if (m_graphic != null)
        {
            Color color = m_graphic.color;
            color.a = _alpha;
            m_graphic.color = color;
        }
    }

    private void SetColor(Color _color)
    {
        if (m_graphic != null)
        {
            _color.a = m_graphic.color.a; // アルファはフェードと連動
            m_graphic.color = _color;
        }
    }

    public void SetPool(SimpleObjectPool _pool)
    {
        m_pool = _pool;
    }

    private SimpleObjectPool m_pool;

    private void ReleaseToPool()
    {
        if (m_pool != null)
        {
            m_pool.Release(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

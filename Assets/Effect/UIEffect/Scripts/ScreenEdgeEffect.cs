using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class ScreenEdgeEffect : MonoBehaviour
{
    [Header("共通設定")]
    [SerializeField] private float m_lifeTime = 2f;
    [SerializeField] private bool m_useRandom = false;
    [SerializeField] private Vector3 m_maxScale = Vector3.one;

    [Header("スケール設定")]
    [SerializeField] private bool m_enableScale = true;
    [SerializeField] private float m_expandDuration = 0.3f;
    [SerializeField] private float m_shrinkDuration = 0.3f;
    [SerializeField] private Vector2 m_expandDurationRange = new Vector2(0.2f, 0.5f);
    [SerializeField] private Vector2 m_shrinkDurationRange = new Vector2(0.2f, 0.5f);
    [SerializeField] private Vector2 m_scaleRange = new Vector2(0.8f, 1.5f);

    [Header("回転設定")]
    [SerializeField] private bool m_enableRotation = true;
    [SerializeField] private float m_rotationSpeed = 180f;
    [SerializeField] private Vector2 m_rotationSpeedRange = new Vector2(-360f, 360f);

    [Header("フェードアウト設定")]
    [SerializeField] private bool m_enableFadeOut = true;
    [SerializeField] private float m_fadeOutSpeed = 1.0f;
    [SerializeField] private Vector2 m_fadeOutSpeedRange = new Vector2(0.5f, 2f);

    [Header("色変化設定")]
    [SerializeField] private bool m_enableColorChange = true;
    [SerializeField] private Color m_startColor = Color.white;
    [SerializeField] private Color m_endColor = Color.red;

    private Graphic m_graphic;
    private float m_timer;
    private float m_alpha = 1f;

    private enum State { Expanding, Shrinking, Done }
    private State m_state = State.Expanding;

    private SimpleObjectPool pool;

    void OnEnable()
    {
        m_graphic = GetComponent<Graphic>();
        m_timer = 0f;
        m_alpha = 1f;
        m_state = State.Expanding;

        if (m_enableScale)
        {
            // スケールアニメーションを使う場合はゼロから開始
            transform.localScale = Vector3.zero;
        }
        else
        {
            // アニメーションを使わない場合はInspectorの値をそのまま使う
            // m_maxScale もInspectorで設定可能
            transform.localScale = m_maxScale;
        }

        transform.rotation = Quaternion.identity;

        if (m_useRandom)
        {
            if (m_enableScale)
            {
                m_expandDuration = Random.Range(m_expandDurationRange.x, m_expandDurationRange.y);
                m_shrinkDuration = Random.Range(m_shrinkDurationRange.x, m_shrinkDurationRange.y);
                float scaleValue = Random.Range(m_scaleRange.x, m_scaleRange.y);
                m_maxScale = new Vector3(scaleValue, scaleValue, 1f);
            }

            if (m_enableRotation)
            {
                m_rotationSpeed = Random.Range(m_rotationSpeedRange.x, m_rotationSpeedRange.y);
            }

            if (m_enableFadeOut)
            {
                m_fadeOutSpeed = Random.Range(m_fadeOutSpeedRange.x, m_fadeOutSpeedRange.y);
            }
        }

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
        if (m_enableRotation)
            transform.Rotate(Vector3.forward, m_rotationSpeed * Time.deltaTime);

        if (m_enableScale)
        {
            switch (m_state)
            {
                case State.Expanding:
                    float expand = Mathf.Clamp01(m_timer / m_expandDuration);
                    transform.localScale = Vector3.Lerp(Vector3.zero, m_maxScale, expand);

                    if (m_enableColorChange)
                        SetColor(Color.Lerp(m_startColor, m_endColor, expand));

                    if (expand >= 1f)
                    {
                        m_timer = 0f;
                        m_state = State.Shrinking;
                    }
                    break;

                case State.Shrinking:
                    float shrink = Mathf.Clamp01(m_timer / m_shrinkDuration);
                    transform.localScale = Vector3.Lerp(m_maxScale, Vector3.zero, shrink);

                    if (m_enableColorChange)
                        SetColor(Color.Lerp(m_endColor, m_startColor, shrink));

                    if (m_enableFadeOut)
                    {
                        m_alpha = Mathf.MoveTowards(m_alpha, 0f, m_fadeOutSpeed * Time.deltaTime);
                        SetAlpha(m_alpha);
                    }

                    if (shrink >= 1f)
                    {
                        m_state = State.Done;
                        ReleaseToPool();
                    }
                    break;
            }
        }
        else
        {
            // スケールなしの場合はライフタイムで終了
            if (m_enableFadeOut)
            {
                m_alpha = Mathf.MoveTowards(m_alpha, 0f, m_fadeOutSpeed * Time.deltaTime);
                SetAlpha(m_alpha);
            }

            if (m_timer >= m_lifeTime)
                ReleaseToPool();
        }
    }

    private void SetAlpha(float a)
    {
        if (m_graphic != null)
        {
            Color c = m_graphic.color;
            c.a = a;
            m_graphic.color = c;
        }
    }

    private void SetColor(Color color)
    {
        if (m_graphic != null)
        {
            color.a = m_graphic.color.a;
            m_graphic.color = color;
        }
    }

    public void SetPool(SimpleObjectPool pool) => this.pool = pool;

    private void ReleaseToPool()
    {
        if (pool != null) pool.Release(gameObject);
        else Destroy(gameObject);
    }
}

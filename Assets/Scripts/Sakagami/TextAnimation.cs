/*
 * ファイル
 * TextAnimation C#
 * 
 * システム
 * テキストをフェードイン、アウトさせる
 * 
 * 作成
 * 2025/09/23 坂上　壱希
 * 
 * 最終変更
 * 2025/09/29　坂上　壱希
 */
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class TextAnimation : MonoBehaviour
{
    [SerializeField]
    private float[] m_zoomInScales = { 80f, 75f, 70f };  // 各回のズームイン倍率
    [SerializeField]
    private float[] m_zoomOutScales = { 70f, 65f, 60f }; // 各回のズームアウト倍率
    [SerializeField]
    private float m_animationSpeed = 3f;           // アニメーション速度
    [SerializeField]
    private int m_repeatCount = 2;        // 繰り返し回数（0以下は無限ループ）

    private RectTransform m_rectTransform;
    private float m_timer = 0f;
    private int m_currentRepeat = 0;    //繰り返し回数
    private bool m_isAnimating = true;

    void Awake()
    {
        m_rectTransform = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        // 表示された瞬間にアニメーション初期化
        m_timer = 0f;
        m_currentRepeat = 0;
        m_isAnimating = true;
    }

    void Update()
    {
        if (!m_isAnimating)
            return;

        m_timer += Time.deltaTime * m_animationSpeed;

        float zoomIn = GetValueForCurrentRepeat(m_zoomInScales, m_currentRepeat);
        float zoomOut = GetValueForCurrentRepeat(m_zoomOutScales, m_currentRepeat);

        if (m_timer >= 2f)
        {
            m_timer -= 2f;
            m_currentRepeat++;

            if (m_repeatCount > 0 && m_currentRepeat >= m_repeatCount)
            {
                m_isAnimating = false;
                m_rectTransform.localScale = new Vector3(zoomOut, zoomOut, 1f);
                return;
            }

            // 繰り返し開始時にスケールを次の縮小値に合わせる（繋ぎ目のガクつき防止）
            float nextZoomOut = GetValueForCurrentRepeat(m_zoomOutScales, m_currentRepeat);
            m_rectTransform.localScale = new Vector3(nextZoomOut, nextZoomOut, 1f);
        }

        float scale;
        if (m_timer < 1f)
        {
            float t = Mathf.SmoothStep(0f, 1f, m_timer);
            scale = Mathf.Lerp(zoomOut, zoomIn, t);
        }
        else
        {
            float t = Mathf.SmoothStep(0f, 1f, m_timer - 1f);
            scale = Mathf.Lerp(zoomIn, zoomOut, t);
        }

        m_rectTransform.localScale = new Vector3(scale, scale, 1f);
    }


    private float GetValueForCurrentRepeat(float[] array, int index)
    {
        if (array == null || array.Length == 0)
            return 1f; // デフォルト倍率

        if (index < array.Length)
            return array[index];
        else
            return array[array.Length - 1]; // 配列最後の値を使い回す
    }
}

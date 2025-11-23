/*
 * ファイル
 * TransitionManager C#
 * 
 * システム
 * シーン遷移時のフェードイン・フェードアウト部分を管理するシングルトン
 * 
 * 変更履歴
 * 2025/09/22　奥山　凜　作成
 * 2025/10/30　奥山　凜　フェード時間を指定できるように変更
 */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// シーン遷移時のフェードイン・フェードアウト部分を管理するシングルトン
/// </summary>
public class TransitionManager : Singleton<TransitionManager>

{

    [SerializeField]
    private const float DEFAULT_FADE_DURATION = 0.5f;
    private float m_fadeDuration = DEFAULT_FADE_DURATION;        // フェードにかかる時間
    private CanvasGroup m_canvasGroup;                           // このシングルトンでAdd
    private GameObject m_canvasObject;                           // このシングルトンで生成



    

    [SerializeField]
    private AnimationCurve m_fadeCurve = new AnimationCurve(        // フェードの緩急を制御するためのアニメーションカーブ

        new Keyframe(0, 0, 0, 0),       // 開始点: 時間0, 値0, 接線(傾き)を調整

        new Keyframe(1, 1, 2, 0)        // 終了点: 時間1, 値1, 接線(傾き)を調整

    );



    protected override void Awake()
    {

        base.Awake();


        InitializeFadePanel();      // フェード用のUIを生成

    }

    /// <summary>
    /// 画面を白くフェードインさせる<br/>
    /// 引数1： _fadeDuration フェード時間<br/>
    ///         ない場合デフォルトの時間を使用<br/>
    /// </summary>
    /// <param name="_fadeDuration">フェード時間</param>
    /// <returns>なし</returns>
    public IEnumerator FadeIn(float? _fadeDuration = null)
    {
        if (_fadeDuration.HasValue)
        {
            m_fadeDuration = _fadeDuration.Value;
        }
        else
        {
            m_fadeDuration = DEFAULT_FADE_DURATION;
        }
        yield return FadeRoutine(1.0f);
    }

    /// <summary>
    /// 白い画面からフェードアウトさせる<br/>
    /// 引数1： _fadeDuration フェード時間<br/>
    ///         ない場合デフォルトの時間を使用<br/>
    /// </summary>
    /// <param name="_fadeDuration">フェード時間</param>
    /// <returns>なし</returns>

    public IEnumerator FadeOut(float? _fadeDuration = null)
    {
        if(_fadeDuration.HasValue)
        {
            m_fadeDuration = _fadeDuration.Value;
        }
        else
        {
            m_fadeDuration = DEFAULT_FADE_DURATION;
        }
         yield return FadeRoutine(0.0f);
    }

    /// <summary>
    /// 白い画面からフェードアウトさせる<br/>
    /// </summary>
    /// <returns>なし</returns>
    private void InitializeFadePanel()
    {
        m_canvasObject = new GameObject("TransitionCanvas");        // Canvas用のオブジェクトを生成
        m_canvasObject.transform.SetParent(this.transform);         // 生成したCanvasをTransitionManagerの子オブジェクトにして、シーン遷移で破棄されないようにする

        Canvas canvas = m_canvasObject.AddComponent<Canvas>();      // キャンバスとしてm_canvasObjectにコンポーネントを追加
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // 他のUIより必ず手前に表示

        CanvasScaler canvasScaler = m_canvasObject.AddComponent<CanvasScaler>();        // キャンバスのサイズを設定
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);


        GameObject panelObject = new GameObject("FadePanel");                           // イメージ用のオブジェクトを生成
        panelObject.transform.SetParent(m_canvasObject.transform, false);

        Image image = panelObject.AddComponent<Image>();        // イメージとしてpanelObjectにコンポーネントを追加
        image.color = Color.white;
        image.raycastTarget = false;

        RectTransform rectTransform = panelObject.GetComponent<RectTransform>();        // UIなのでRectTransformを追加
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;


        m_canvasGroup = panelObject.AddComponent<CanvasGroup>();        // 透過率をまとめて変更するためCanvasGroupを追加
        m_canvasGroup.alpha = 0f;
    }

    /// <summary>
    /// 画面を白くフェードイン、またはフェードアウトさせる処理の実装<br/>
    /// 透明度を目標値に変化させる<br/>
    /// 引数1： _targetAlpha 目標の透明度
    /// </summary>
    /// <param name="_targetAlpha">目標の透明度</param>
    /// <returns>なし</returns>
    private IEnumerator FadeRoutine(float _targetAlpha)
    {
        float startAlpha = m_canvasGroup.alpha;
        float time = 0;

        while (time < m_fadeDuration)
        {
            time += Time.deltaTime;

            float progress = Mathf.Clamp01(time / m_fadeDuration);      // 進行度(0-1)を計算

            float curveValue = m_fadeCurve.Evaluate(progress);      // 進行度をカーブに渡し、緩急が適用された値を取得

            m_canvasGroup.alpha = Mathf.LerpUnclamped(startAlpha, _targetAlpha, curveValue);        // 緩急がついた値を使って、現在の透明度から目標の透明度へ変化させる

            yield return null;
        }

        m_canvasGroup.alpha = _targetAlpha; // 確実に目標値に設定
    }

    // TransitionManagerが破棄される際に、自身が生成したUIも一緒に破棄する
    protected override void OnDestroy()

    {
        base.OnDestroy(); // 親クラス(Singleton)のOnDestroyを必ず呼び出す

        // エディッタでエラーが出るため対策
        // エディタでのみ即時破棄、ビルド後は通常の破棄
        if (m_canvasObject != null)
        {
#if UNITY_EDITOR
            DestroyImmediate(m_canvasObject);
#else
            Destroy(m_canvasObject);
#endif
        }
    }
}




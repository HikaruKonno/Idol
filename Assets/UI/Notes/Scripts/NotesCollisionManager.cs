/*
 * ファイル
 * NotesCollisionMaganger C#
 * 
 * システム
 * ノーツの当たり判定を管理するシステム
 * 
 * 作成
 * 2025/09/11 寺門 冴羽
 * 
 * 最終変更
 * 2025/09/25 今野　光
 * 
 * TODO
 * いまスピード重視でisHandフラグを追加して無理やりやってるけど、やり方きもいから
 * 手用のスクリプトに別ける。
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotesCollisionMaganger : MonoBehaviour
{
    enum State
    {
        Idle,
        Shrinking,
        PulseFade
    }

    enum LeftOrRight
    {
        Left,
        Right
    }

    // --------------------------------------------------------------------------------
    // 変数宣言
    // --------------------------------------------------------------------------------

    #region Field
    // 判定の合格割合
    [SerializeField, Range(0f, 1f)]
    private float m_passRate;
    // 当たり判定のノーツオブジェクト(０番目から順番)
    [SerializeField]
    List<NotesCollisionDetection> m_noteLists;
    // このノーツが手かどうか
    [SerializeField]
    private bool m_isHand = false;
    // このノーツがバンかどうか
    [SerializeField]
    private bool m_isBan = false;
    // 手の判定タイミングの画像（手以外は使わない）
    [SerializeField]
    private Image m_handOutline;
    // 判定が現れる瞬間の一番大きいサイズ
    [SerializeField]
    private float m_maxSize;
    // 判定が重なる時のサイズ
    [SerializeField]
    private float m_standardSize;
    // 外側の当たり判定のタイミングを知らせるUI接近のスピード
    [SerializeField]
    private float ADD_ORANGE = 0.1f;
    // ふわっと大きくなる倍率
    [SerializeField] float pulseScale = 1.2f;
    // ふわっと＋フェードの時間
    [SerializeField] float pulseDuration = 0.4f;
    [SerializeField] private LeftOrRight leftOrRight = LeftOrRight.Right;

    // ノーツに触った個数
    private int m_detectionCount;
    // 画像
    private Image m_image;
    // 透明度を足していく
    private const float ADD_ALPHA = 0.1f;

    private float m_handOutlineNormalized;

    // ノーツの最大数
    private int m_maxNotes;
    private int m_currentNotes;

    private State state = State.Idle;

    // フィールド追加（Awake で保存する）
    private bool m_initialIsHand;
    private float flipSign = 1f;
    #endregion

    #region デバッグ用
    public Action<int, int> DisableCallBack;
    #endregion

    // --------------------------------------------------------------------------------
    // メイン関数
    // --------------------------------------------------------------------------------

    #region LifeCycle
    // Awake 内で保存
    private void Awake()
    {
        m_initialIsHand = m_isHand; // ← 追加

        if (m_isHand)
        {
            m_image = transform.GetChild(0).GetComponent<Image>();
            m_handOutline = transform.GetChild(0).transform.GetChild(0).GetComponent<Image>();
        }
        else if (m_isBan)
        {
            m_image = transform.GetChild(0).GetComponent<Image>();
        }
        else
        {
            m_image = GetComponent<Image>();
        }
    }
    private void Update()
    {
        // alpha 増加（既存ロジック）
        if (m_image.color.a < 1f)
        {
            Color tmp = m_image.color;
            tmp.a = Mathf.Min(tmp.a + ADD_ALPHA, 1f);
            m_image.color = tmp;
        }

        // 正規化値を増やす（既存ロジック）
        if (m_handOutlineNormalized < 1f && m_isHand && state != State.PulseFade)
        {

            m_handOutlineNormalized = Mathf.Min(m_handOutlineNormalized + ADD_ORANGE, 1f);

            var maxScale = new Vector3(flipSign * m_maxSize, m_maxSize, m_maxSize);
            var standardScale = new Vector3(flipSign * m_standardSize, m_standardSize, m_standardSize);

            m_handOutline.rectTransform.localScale
                = Vector3.Lerp(maxScale, standardScale, m_handOutlineNormalized);
            // 元のサイズに戻った瞬間を検出
            if (Mathf.Approximately(m_handOutlineNormalized, 1f) ||
            m_handOutline.rectTransform.localScale.magnitude <= Vector3.one.magnitude * (m_standardSize + 0.001f))
            {
                // Shrinking 完了を示す状態へ
                state = State.Shrinking;
                StartPulseFadeIfNeeded();
            }

#if UNITY_EDITOR && false
            Debug.LogWarning(
                $"正規化したアウトラインの値: {m_handOutlineNormalized}"
            );
#endif
        }
    }

    // OnEnable を確実に初期化するように修正
    private void OnEnable()
    {
        // Inspector のフラグを復元（Timeline で上書きされていても元に戻す）
        m_isHand = m_initialIsHand;

        // 初期化
        Init();

        flipSign = (leftOrRight == LeftOrRight.Left) ? -1f : 1f;

        // 子オブジェクトを格納して有効化（既存）
        var children = new Transform[transform.childCount];
        for (var i = 0; i < children.Length; ++i)
        {
            children[i] = transform.GetChild(i);
            GameObject obj = children[i].gameObject;
            obj.SetActive(true);
        }

        // m_handOutline は grandchild の場合もあるので明示的に有効化・初期化
        if (m_isHand && m_handOutline != null)
        {
            m_handOutline.gameObject.SetActive(true);     // ← 追加
            m_handOutline.rectTransform.localScale = new Vector3(flipSign * m_maxSize, m_maxSize, m_maxSize);
        }
    }

    // 非アクティブ状態になった時に呼び出される。
    private void OnDisable()
    {

        //DisableCallBack?.Invoke(m_detectionCount,m_maxNotes);
    }

    #endregion

    // --------------------------------------------------------------------------------
    // サブ関数
    // --------------------------------------------------------------------------------

    /// <summary>
    /// 接触カウントを足す
    /// </summary>
    public void AddDetectionCount()
    {
        // 接触回数のカウント
        m_detectionCount++;
        // ノーツの順番の更新
        m_currentNotes++;

        // 接触回数が指定以上なら演出を流す
        if (m_detectionCount >= m_noteLists.Count * m_passRate)
        {
#if UNITY_EDITOR
            // 完璧の演出を流す
            Debug.Log("完璧！");
#endif
            // ノーツの判定をユーティリティクラスに渡す。
            NotesJudgmentUtility.SetJudge(true);
            NotesJudgmentUtility.Invoke();
            // 音の再生
            AudioManager.Instance.PlaySE(AudioName.Clap);
        }

        if (m_detectionCount >= m_maxNotes)
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// ノーツの当たった順番の確認
    /// </summary>
    /// <param name="notes">現在当たったノーツ</param>
    public bool IsNotesSequence(NotesCollisionDetection notes)
    {
        if (m_currentNotes == m_noteLists.FindIndex(item => item == notes))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 初期化
    /// </summary>
    private void Init()
    {
        m_detectionCount = 0;
        m_currentNotes = 0;
        m_maxNotes = m_noteLists.Count;
        Color tmp = m_image.color;
        tmp.a = 0f;
        m_image.color = tmp;
        state = State.Idle;
        NotesJudgmentUtility.SetJudge(false);
        // 手のノーツだけ
        if (m_isHand)
        {
            m_handOutlineNormalized = 0f;
            m_handOutline.rectTransform.localScale = Vector3.one * m_maxSize;
        }
    }

    /// <summary>
    /// 条件（state == Shrinking）を満たしたらパルス＋フェードを開始する。
    /// </summary>
    private void StartPulseFadeIfNeeded()
    {
        if (state == State.Shrinking)
        {
            state = State.PulseFade;
            StopAllCoroutines(); // 必要なら既存のコルーチンを止める
            StartCoroutine(PulseAndFadeCoroutine());
        }
    }

    /// <summary>
    /// 一定時間でスケールを変化させながら画像を透明化し、終了時にコールバックを呼ぶ。
    /// </summary>
    /// <returns></returns>
    private IEnumerator PulseAndFadeCoroutine()
    {
        float elapsed = 0f;
        float startScale = m_standardSize;
        float targetScale = m_standardSize * pulseScale;
        var rt = m_handOutline.rectTransform;

        while (elapsed < pulseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / pulseDuration));
            float scale = Mathf.Lerp(startScale, targetScale, t);

            // flipSign を反映
            rt.localScale
                = new Vector3(flipSign * scale, scale, scale);

            // 透明度フェード（既存）
            Color img = m_image.color;
            img.a = Mathf.Lerp(m_image.color.a, 0f, t);
            m_image.color = img;

            yield return null;
        }

        // 最終値も反映
        rt.localScale = new Vector3(flipSign * targetScale, targetScale, targetScale);
        Color final = m_image.color;
        final.a = 0f;
        m_image.color = final;

        OnPulseFadeComplete();
    }

    /// <summary>
    /// オブジェクトを非表示にして進行状態をリセットする
    /// </summary>
    private void OnPulseFadeComplete()
    {
        // 終了処理: 非表示、リセット、プールに返す、などプロジェクトに合わせて処理を入れる
        m_handOutline.gameObject.SetActive(false);
        // リセット例
        m_handOutlineNormalized = 0f;
        state = State.Idle;
    }
}

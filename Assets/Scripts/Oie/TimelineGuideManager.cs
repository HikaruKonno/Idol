using System;
using System.Collections;
using UnityEngine;

public class TimelineGuideManager : MonoBehaviour
{
    [SerializeField, Header("タイムラインで管理するオブジェクト")]
    private CanvasGroup[] m_guideGroups;
    [SerializeField, Header("フェードするまでの間隔")]
    private float m_duration = 0.3f;

    private void Awake()
    {
        // 配列に子オブジェクトを入れる
        m_guideGroups = new CanvasGroup[transform.childCount];
        for (int i =0; i < transform.childCount; ++i)
        {
            m_guideGroups[i] = transform.GetChild(i).GetComponent<CanvasGroup>();

            if (m_guideGroups[i] == null)
            {
                Debug.LogError($"子オブジェクト '{transform.GetChild(i).name}' に CanvasGroup コンポーネントがありません！", transform.GetChild(i).gameObject);
            }
            else
            {
                // 初期化状態は透明かつ非アクティブにしておく
                m_guideGroups[i].alpha = 0f;
                m_guideGroups[i].gameObject.SetActive(false);
            }
        }  
    }

    /// <summary>
    /// ガイドをアクティブにする関数
    /// ActiveEventで指定できる引数は1つまで
    /// </summary>
    public void ActiveGuide(int index)
    {
        if (index >= 0 && index < m_guideGroups.Length)
        {
            // 対象のCanvasGroupを取得
            CanvasGroup group = m_guideGroups[index];

            // オブジェクトをアクティブにする
            group.gameObject.SetActive(true);

            // コルーチンを開始してフェードインさせる
            StartCoroutine(FadeCoroutine(group, 1f));
        }
    }

    /// <summary>
    /// ガイドを非アクティブにする関数
    /// ActiveEventで指定できる引数は1つまで
    /// </summary>
    public void InActiveGuide(int index)
    {
        if (index >= 0 && index < m_guideGroups.Length)
        {
            // 対象のCanvasGroupを取得
            CanvasGroup group = m_guideGroups[index];

            // コルーチンを開始してフェードアウトさせる
            StartCoroutine(FadeCoroutine(group, 0f));
        }
    }

    /// <summary>
    /// CanvasGroupのα値を変更し、完了後に非アクティブにするコルーチン
    /// </summary>
    /// <param name="group">対象のCanvasGroup</param>
    /// <param name="targetAlpha">αの目標値</param>
    /// <param name="duration">フェードするまでの間隔</param>
    /// <returns></returns>
    private IEnumerator FadeCoroutine(CanvasGroup group, float targetAlpha)
    {
        // 処理の対象が見つからない場合は何もしない
        if(group == null) yield break;

        float startAlpha = group.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < m_duration)
        {
            // Lerpでα値を計算し、更新する
            group.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / m_duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 最終的なα値を設定
        group.alpha = targetAlpha;

        // フェードアウトが完了した場合、オブジェクトを非アクティブにする
        if(targetAlpha <= 0)
        {
            group.gameObject.SetActive(false);
        }
    }
}

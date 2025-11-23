using System;
using UnityEngine;

/// <summary>
/// ノーツの判定のユーティリティクラス
/// </summary>
public static class NotesJudgmentUtility
{
    static bool m_isSucces;
    static Action m_succesCallBack;
    static Action m_failureCallBack;

    /// <summary>
    /// ノーツの成否判定
    /// </summary>
    /// <returns>成否判定</returns>
    public static bool IsSucces()
    {
        return m_isSucces;
    }

    /// <summary>
    /// 成否判定をセットする
    /// </summary>
    /// <param name="judge">成功ならtrue</param>
    public static void SetJudge(bool judge)
    {
        m_isSucces = judge;
    }

    /// <summary>
    /// 成功時のコールバック処理を登録する
    /// </summary>
    /// <param name="action">登録するコールバック処理</param>
    public static void RegistryFailureCallBack(Action action)
    {
        m_failureCallBack += action;
    }

    /// <summary>
    /// 失敗時のコールバック処理を登録する
    /// </summary>
    /// <param name="action">登録するコールバック処理</param>
    public static void RegistrySuccesCallBack(Action action)
    {
        m_succesCallBack += action;
    }

    /// <summary>
    /// コールバックの実行
    /// </summary>
    public static void Invoke()
    {
        // 成功 or 失敗のコールバックを実行する
        if(m_isSucces)
        {
            m_succesCallBack?.Invoke();
        }
        else
        {
            m_failureCallBack?.Invoke();
        }
    }
}

/*
 * ファイル
 * Singleton C#
 * 
 * システム
 * シングルトンのベース
 * 
 * 変更履歴
 * 2025/07/01　奥山　凜　作成
 */

using UnityEngine;

/// <summary>
/// シングルトンのベース<br/>
/// T = 継承先のクラス
/// </summary>
public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    public static T Instance        // 外部から触れるこのオブジェクトのインスタンス
    {
        get
        {
            if (m_applicationIsQuitting)
            {
                // アプリケーション終了時は新しいインスタンスを返さない
                return null;
            }
            else if (m_instance == null)
            {
                SetupInstance();
            }
            return m_instance;
        }
    }

    private static T m_instance;                             // このシングルトンのインスタンス
    private static bool m_applicationIsQuitting = false;     // アプリケーション終了時、シングルトンが削除されるタイミングに新しくこのシングルトンを生成しないようにするフラグ

    protected virtual void Awake()
    {
        // シングルトン
        if (m_instance == null)
        {
            m_instance = this as T;
            DontDestroyOnLoad(this.gameObject);     // このオブジェクトをシーン切り替えで破棄しないようにする
        }
        else
        {
            Destroy(gameObject);        // 既に存在するなら新しく作ったものを破棄
            return;
        }
    }

    /// <summary>
    /// シングルトンがアプリケーション終了時に新しく生成されないようシングルトン破棄時にフラグをオンに
    /// </summary>
    protected virtual void OnDestroy()
    {
        m_applicationIsQuitting = true;
    }

    /// <summary>
    /// このスクリプトを持ったオブジェクトを検索、無ければ作成し、インスタンスとして登録する<br/>
    /// </summary>
    /// <returns>なし</returns>
    private static void SetupInstance()
    {
        m_instance = FindFirstObjectByType<T>();

        if (m_instance == null)
        {
            GameObject gameObj = new GameObject();
            gameObj.name = typeof(T).Name;
            m_instance = gameObj.AddComponent<T>();

            DontDestroyOnLoad(gameObj);
        }
    }
}

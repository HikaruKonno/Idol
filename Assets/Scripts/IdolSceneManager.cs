/*
 * ファイル
 * IdolSceneManager C#
 * 
 * システム
 * シーン遷移を管理するシングルトン
 * 
 * 変更履歴
 * 2025/09/21　奥山　凜　作成
 * 2025/09/22　奥山　凜　TransitionManager適用
 * 2025/10/30　奥山　凜　フェード時間を指定できるように変更
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// シーン遷移を管理するシングルトン
/// </summary>
public class IdolSceneManager : Singleton<IdolSceneManager>
{
    public static event System.Action OnLoadStarted;
    public static event System.Action OnLoadFinished;

    /// <summary>
    /// シーンのビルドインデックスと対応したenum
    /// </summary>
    public enum SceneBuildIndex : int
    {
        Title = 0,
        Tutorial,
        InGame,
        Ending,
        Mediapipe
    }

    public bool IsLoading { get; private set; } = false;

    /// <summary>
    /// メディアパイプのシーン以外を全てアンロード。指定したシーンをロードする（enum版）<br/>
    /// 引数1：_sceneBuildIndex ロードしたいシーンのビルドインデックス<br/>
    /// 引数2：_fadeTime フェードにかかる時間<br/>
    /// </summary>
    /// <param name="_sceneBuildIndex">ロードしたいシーンのビルドインデックス</param>
    /// <param name="_fadeTime">フェードにかかる時間</param>
    /// <returns>なし</returns>
    public void UnloadAndLoadSceneKeepingMediapipe(SceneBuildIndex _sceneBuildIndex, float? _fadeTime = null)
    {
        int sceneBuildIndex = (int)_sceneBuildIndex;

        if (!Application.CanStreamedLevelBeLoaded(sceneBuildIndex))
        {
#if UNITY_EDITOR
            Debug.Log("そのシーン番号はビルドの対象シーンに存在しません");
#endif
            return;
        }

        if (IsLoading)
        {
#if UNITY_EDITOR
            Debug.Log("シーンロード中のためロード出来ません");
#endif
            return;
        }
        StartCoroutine(UnloadAndLoadExceptMediapipeRoutine(sceneBuildIndex, _fadeTime));
    }

    /// <summary>
    /// メディアパイプのシーン以外を全てアンロード。指定したシーンをロードする（int版）<br/>
    /// 引数1：_sceneBuildIndex ロードしたいシーンのビルドインデックス<br/>
    /// 引数2：_fadeTime フェードにかかる時間<br/>
    /// </summary>
    /// <param name="_sceneBuildIndex">ロードしたいシーンのビルドインデックス</param>
    /// <param name="_fadeTime">フェードにかかる時間</param>
    /// <returns>なし</returns>
    public void UnloadAndLoadSceneKeepingMediapipe(int _sceneBuildIndex, float? _fadeTime = null)
    {
        if (!Application.CanStreamedLevelBeLoaded(_sceneBuildIndex))
        {
#if UNITY_EDITOR
            Debug.Log("そのシーン番号はビルドの対象シーンに存在しません");
#endif
            return;
        }

        if (IsLoading)
        {
#if UNITY_EDITOR
            Debug.Log("シーンロード中のためロード出来ません");
#endif
            return;
        }
        StartCoroutine(UnloadAndLoadExceptMediapipeRoutine(_sceneBuildIndex, _fadeTime));
    }

    /// <summary>
    /// メディアパイプのシーン以外を全てアンロードし、ロードし直す<br/>
    /// 引数1：_fadeTime フェードにかかる時間<br/>
    /// </summary>
    /// <param name="_fadeTime">フェードにかかる時間</param>
    /// <returns>なし</returns>
    public void ReloadAllScenesExceptMediapipe(float? _fadeTime = null)
    {
        if (IsLoading)
        {
#if UNITY_EDITOR
            Debug.Log("シーンロード中のためロード出来ません");
#endif
            return;
        }

        StartCoroutine(ReloadScenesExceptCoroutine(_fadeTime));
    }

    /// <summary>
    /// Mediapipeのシーンを残しその他のシーンをアンロード、その後新たなシーンを追加でロードする<br/>
    /// 引数1：_sceneBuildIndex ロードするシーンのビルドインデックス<br/>
    /// 引数2：_fadeTime フェードにかかる時間<br/>
    ///        マイナスの値の場合デフォルトの値を使用<br/>
    /// </summary>
    /// <param name="_sceneBuildIndex">ロードするシーンのビルドインデックス</param>
    /// <param name="_fadeTime">フェードにかかる時間</param>
    /// <returns>なし</returns>
    private IEnumerator UnloadAndLoadExceptMediapipeRoutine(int _sceneBuildIndex, float? _fadeTime = null)
    {
        IsLoading = true;
        OnLoadStarted?.Invoke(); // ロード開始イベントを発行

        yield return TransitionManager.Instance.FadeIn(_fadeTime);

        List<AsyncOperation> unloadOperations = new List<AsyncOperation>();

        // 現在のシーンをすべて確認
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            if (scene.buildIndex != (int)SceneBuildIndex.Mediapipe)
            {
                AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(scene);
                if (asyncOperation != null)
                {
                    unloadOperations.Add(asyncOperation);
                }
            }
        }

        // 全てのアンロード完了を待つ
        foreach (AsyncOperation op in unloadOperations)
        {
            yield return new WaitUntil(() => op.isDone);
        }

        // 新しいシーンを追加でロード（Mediapipeは残す）
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(_sceneBuildIndex, LoadSceneMode.Additive);
        yield return new WaitUntil(() => loadOperation.isDone);
        // ロードしたシーンをアクティブにする
        Scene loadedScene = SceneManager.GetSceneByBuildIndex(_sceneBuildIndex);
        if (loadedScene.IsValid())
        {
            SceneManager.SetActiveScene(loadedScene);
        }

        OnLoadFinished?.Invoke(); // ロード完了イベントを発行


        yield return TransitionManager.Instance.FadeOut(_fadeTime);

        IsLoading = false;
    }

    /// <summary>
    /// 全てのシーンをリロードする<br/>
    /// 引数1：_fadeTime フェードにかかる時間<br/>
    /// </summary>
    /// <param name="_fadeTime">フェードにかかる時間</param>
    /// <returns>なし</returns>
    private IEnumerator ReloadScenesExceptCoroutine(float? _fadeTime = null)
    {
        IsLoading = true;
        OnLoadStarted?.Invoke(); // ロード開始イベントを発行

        yield return TransitionManager.Instance.FadeIn(_fadeTime);

        List<int> scenesToReload = new List<int>();
        int? sceneToSetActive = null;

        // 現在のシーンを確認
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.buildIndex != (int)SceneBuildIndex.Mediapipe)
            {
                scenesToReload.Add(scene.buildIndex);

                // アクティブシーンだったら記憶しておく
                if (SceneManager.GetActiveScene() == scene)
                {
                    sceneToSetActive = scene.buildIndex;
                }
            }
        }

        // アンロード
        List<AsyncOperation> operations = new List<AsyncOperation>();
        foreach (int sceneBuildIndex in scenesToReload)
        {
            operations.Add(SceneManager.UnloadSceneAsync(sceneBuildIndex));
        }
        // 全てのアンロード完了を待つ
        foreach (AsyncOperation op in operations)
        {
            yield return new WaitUntil(() => op.isDone);
        }


        // ロード
        List<AsyncOperation> loadOperations = new List<AsyncOperation>();
        foreach (int sceneBuildIndex in scenesToReload)
        {
            loadOperations.Add(SceneManager.LoadSceneAsync(sceneBuildIndex, LoadSceneMode.Additive));
        }
        foreach (AsyncOperation op in loadOperations)
        {
            yield return new WaitUntil(() => op.isDone);
        }

        // 再アクティブ化
        if (sceneToSetActive.HasValue)
        {
            Scene reloadedScene = SceneManager.GetSceneByBuildIndex(sceneToSetActive.Value);
            if (reloadedScene.IsValid())
            {
                SceneManager.SetActiveScene(reloadedScene);
            }
        }

        OnLoadFinished?.Invoke(); // ロード完了イベントを発行

        yield return TransitionManager.Instance.FadeOut(_fadeTime);

        IsLoading = false;
    }
}

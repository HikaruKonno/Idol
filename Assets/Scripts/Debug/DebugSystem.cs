/*
 * ファイル
 * DebugSystem C#
 * 
 * システム
 * シーン切り替えのデバッグキー
 * 
 * 変更履歴
 * 2025/09/17　奥山　凜　作成
 * 2025/09/22　奥山　凜　IdolSceneManagerを使用したものに変更
 * 2025/10/31  奥山　凜　Escapeでゲーム終了を追加
 */

using UnityEngine;

/// <summary>
/// シーン切り替えのデバッグキーのシングルトン
/// </summary>
public class DebugSystem : Singleton<DebugSystem>
{
    void Update()
    {
        if (IdolSceneManager.Instance.IsLoading)
        {
            return;
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        else if (Input.GetKeyDown(KeyCode.F1))
        {
            IdolSceneManager.Instance.UnloadAndLoadSceneKeepingMediapipe(IdolSceneManager.SceneBuildIndex.Title);
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            IdolSceneManager.Instance.UnloadAndLoadSceneKeepingMediapipe(IdolSceneManager.SceneBuildIndex.Tutorial);
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            IdolSceneManager.Instance.UnloadAndLoadSceneKeepingMediapipe(IdolSceneManager.SceneBuildIndex.InGame);
        }
        else if (Input.GetKeyDown(KeyCode.F4))
        {
            IdolSceneManager.Instance.UnloadAndLoadSceneKeepingMediapipe(IdolSceneManager.SceneBuildIndex.Ending);
        }
        else if (Input.GetKeyDown(KeyCode.F5))
        {
            IdolSceneManager.Instance.ReloadAllScenesExceptMediapipe();
        }
    }
}

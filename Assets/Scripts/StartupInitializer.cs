/*
 * ファイル
 * StartupInitializer C#
 * 
 * システム
 * ゲーム開始(起動)時に一度だけ初期化処理するクラス
 * エディタ上でも使用可
 * 何処かに宣言、アタッチする必要なし
 * 
 * 変更履歴
 * 2025/07/03　奥山　凜　作成
 * 2025/09/22　奥山　凜　メディアパイプのシーンを追加でロードするように変更
 */

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲーム開始(起動)時に一度だけ初期化処理するクラス<br/>
/// エディタ上でも使用可<br/>
/// 何処かに宣言、アタッチする必要なし
/// </summary>
public class StartupInitializer : MonoBehaviour
{
    public static int FPS { get; private set; } = 60;       // 指定するFPS

    /// <summary>
    /// 起動時（ゲーム開始時）、シーン読み込みやAwake前に一度だけ実行される関数<br/>
    /// </summary>
    /// <returns>なし</returns>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void StartupInitialize()
    {
        // デバッグキーのシングルトンの生成
        DebugSystem debugSystem = DebugSystem.Instance;

        // sceneUnLoaded時にBGMを停止するように関数を登録
        SceneManager.sceneUnloaded += AudioManager.Instance.StopsBGMWhenSwitchingScenes;
        SceneManager.sceneUnloaded += AudioManager.Instance.StopsSEWhenSwitchingScenes;

        // フレームレートをディスプレイと同期せず固定
        QualitySettings.vSyncCount = 0;
        // 60fpsに指定
        Application.targetFrameRate = FPS;

        Cursor.visible = false;

        // シーンAがロードされている状態で、シーンBを追加ロードする
        SceneManager.LoadScene((int)IdolSceneManager.SceneBuildIndex.Mediapipe, LoadSceneMode.Additive);
    }
    }
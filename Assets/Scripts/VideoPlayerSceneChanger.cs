/*
 * ファイル
 * VideoPlayerSceneChanger C#
 * 
 * システム
 * ビデオマネージャーの再生終了のイベントにVideoEndHandlerを登録し、
 * 指定したビルドインデックスのシーンへの切り替え処理を呼び出す
 * 
 * 変更履歴
 * 2025/09/18　奥山　凜　作成
 */
using UnityEngine;
using UnityEngine.Video;


/// <summary>
/// ビデオマネージャーの再生終了のイベントにVideoEndHandlerを登録し、<br/>
/// 指定したビルドインデックスのシーンへの切り替え処理を呼び出すクラス
/// </summary>
public class VideoPlayerSceneChanger : MonoBehaviour
{
    [SerializeField]
    private VideoPlayer m_videoPlayer;                              // 再生終了を待つビデオプレイヤー
    [SerializeField]
    private IdolSceneManager.SceneBuildIndex m_sceneBuildIndex;     // 再生終了時に切り替える先のビルドインデックス

    void Start()
    {
        // 再生終了イベントにメソッドを登録
        m_videoPlayer.loopPointReached += VideoEndHandler;
    }

    /// <summary>
    /// シーンを切り替える<br/>
    /// 引数1：_videoPlayer VideoPlayerのloopPointReachedに登録する為に引数を合わせる
    /// </summary>
    /// <param name="_videoPlayer">VideoPlayerのloopPointReachedに登録する為に引数を合わせる</param>
    /// <returns>なし</returns>
    private void VideoEndHandler(VideoPlayer _videoPlayer)
    {
        IdolSceneManager.Instance.UnloadAndLoadSceneKeepingMediapipe((int)m_sceneBuildIndex);
    }
}
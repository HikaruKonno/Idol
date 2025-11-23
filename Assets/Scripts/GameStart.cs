/*
 * ファイル
 * GameStart C#
 * 
 * システム
 * タイトル部分のPVやスタートについての処理が書かれたクラス
 * 
 * 変更履歴
 * 2025/09/18　奥山　凜　作成
 * 2025/09/24　奥山　凜　PVが流れるように変更
 * 2025/09/25　奥山　凜　タイトルを動画に変更
 * 2025/10/30　奥山　凜　スタート時の音声がなるように変更
 */

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// タイトル部分のPVやスタートについての処理が書かれたクラス
/// </summary>
public class GameStart : MonoBehaviour
{
    // メディアパイプのランドマーク取得する為の変数
    [SerializeField]
    private MediapipeResultDataContainer m_mediapipeResultDataContainer;


    [SerializeField]
    private ProgressTimer m_dwellSelectTimer = new ProgressTimer { Duration = 5f, ElapsedTime = 0f };     // 手をかざして選択を開始するためのタイマー

    [SerializeField]
    private ProgressTimer m_attractModeTimer = new ProgressTimer { Duration = 5f, ElapsedTime = 0f };     // タイトルからPVに戻るためのタイマー

    [SerializeField]
    private float m_fadeTime = 1.5f;             // 次のシーンに行くフェードイン、アウトそれぞれにかかる時間

    [SerializeField]
    private RawImage m_pvRawImage;              // PVのイメージ
    [SerializeField]
    private VideoPlayer m_pvVideoPlayer;        // PVのビデオプレイヤー

    [SerializeField]
    private RawImage m_titleRawImage;           // titleのイメージ
    [SerializeField]
    private VideoPlayer m_titleVideoPlayer;     // titleのビデオプレイヤー

    [SerializeField]
    bool m_isPlayingPv = false;                 // PVを流しているか


    [SerializeField]
    private Camera m_uiCamera;                  // UIを映しているカメラ。ScreenSpace-Overlayの時はnullで良い
    [SerializeField]
    private RectTransform m_leftHandRectTransform;      // 左手を合わせる先のUIのRectTransform
    [SerializeField]
    private RectTransform m_rightHandRectTransform;     // 右手を合わせる先のUIのRectTransform
    [SerializeField]
    private AudioSource m_TitleAudioSource;             // タイトル画面のAudioSource
    private float m_TitleAudioVolume;                   // 始めの音量を保存しておく変数


    /// <summary>
    /// 目標の時間と経過時間を持った構造体
    /// </summary>
    [System.Serializable]
    public struct ProgressTimer
    {
        // 目標となる時間
        [SerializeField]
        public float Duration;

        // 経過時間
        [SerializeField]
        public float ElapsedTime;
    }

    private Vector2 m_screenSize;       // 画面サイズ

    private void Awake()
    {
        // 画面サイズ
        m_screenSize = new Vector2(Screen.width, Screen.height);


        // 再生終了イベントにメソッドを登録
        m_pvVideoPlayer.loopPointReached += VideoEndHandler;

        m_TitleAudioVolume = m_TitleAudioSource.volume;
    }

        private void Start()
    {
        if (m_mediapipeResultDataContainer == null)
        {
            m_mediapipeResultDataContainer = Resources.Load("Datas/Scene/MediapipeResultDataContainer") as MediapipeResultDataContainer;
        }
        StopVideo(m_pvRawImage, m_pvVideoPlayer);
        PlayVideo(m_titleRawImage, m_titleVideoPlayer);
    }
    private void Update()
    {
        if (m_mediapipeResultDataContainer == null)
        {
            return;
        }

        UpdateDwellSelection();
        if (!m_isPlayingPv)
        {
            PlayVideoAfterIdle();
        }
    }

    /// <summary>
    /// 指定秒数UI位置に手を検出し続けていたらゲームを開始する<br/>
    /// </summary>
    /// <returns>なし</returns>
    private void UpdateDwellSelection()
    {
        if ((m_mediapipeResultDataContainer.LeftHandNormalizedLandmarkList != null) && (m_mediapipeResultDataContainer.RightHandNormalizedLandmarkList != null))
        {
            Vector2 leftHandViewportPos = MediaPipeUtils.ConvertToViewportPos(m_mediapipeResultDataContainer.LeftHandNormalizedLandmarkList[(int)EHandLandmarks.IndexMCP]);
            Vector2 leftScreenPos = leftHandViewportPos * m_screenSize;

            Vector2 rightHandViewportPos = MediaPipeUtils.ConvertToViewportPos(m_mediapipeResultDataContainer.RightHandNormalizedLandmarkList[(int)EHandLandmarks.IndexMCP]);
            Vector2 rightScreenPos = rightHandViewportPos * m_screenSize;

            bool isLeftHandOverImage = IsPositionOverImageWithRect(leftScreenPos, m_leftHandRectTransform, m_uiCamera);
            bool isRightHandOverImage = IsPositionOverImageWithRect(rightScreenPos, m_rightHandRectTransform, m_uiCamera);



            // 両手がUI位置にあるなら時間を加算
            if (isLeftHandOverImage && isRightHandOverImage)
            {
                m_dwellSelectTimer.ElapsedTime += Time.deltaTime;

                if (IsTimerFinished(m_dwellSelectTimer))
                {
                    AudioManager.Instance.PlaySE(AudioName.StartingGameSound, true);

                    IdolSceneManager.Instance.UnloadAndLoadSceneKeepingMediapipe(IdolSceneManager.SceneBuildIndex.Tutorial, m_fadeTime);
                    enabled = false;
                }
                return;
            }
            else if(isLeftHandOverImage || isRightHandOverImage)      // どちらかがUI位置にあるならリセットはしない
            {
                return;
            }
        }

        m_dwellSelectTimer.ElapsedTime = 0f;
    }

    /// <summary>
    /// PVを消して背後のタイトルに戻る<br/>
    /// 引数1： _videoPlayer PVのビデオプレイヤー<br/>
    /// </summary>
    /// <param name="_videoPlayer">PVのビデオプレイヤー</param>
    /// <returns>なし</returns>
    private void VideoEndHandler(VideoPlayer _videoPlayer)
    {
        m_TitleAudioSource.volume = m_TitleAudioVolume;
        m_TitleAudioSource.Play();
        StopVideo(m_pvRawImage, m_pvVideoPlayer);
        m_isPlayingPv = false;

        // 背景のタイトルの動画を最初から再生し直す
        StopVideo(m_titleRawImage, m_titleVideoPlayer);
        PlayVideo(m_titleRawImage, m_titleVideoPlayer);
    }

    /// <summary>
    /// タイトルを一定時間表示後PVを再生<br/>
    /// </summary>
    /// <returns>なし</returns>
    private void PlayVideoAfterIdle()
    {
        m_attractModeTimer.ElapsedTime += Time.deltaTime;

        // 切り替え一秒前からボリュームを絞り始める
        if (m_attractModeTimer.ElapsedTime + 1 >= m_attractModeTimer.Duration)
        {
            m_TitleAudioSource.volume = m_TitleAudioVolume * Mathf.Max(m_TitleAudioVolume * (m_attractModeTimer.Duration - m_attractModeTimer.ElapsedTime), 0f);
        }
        if (IsTimerFinished(m_attractModeTimer))
        {
            
            m_TitleAudioSource.Stop();
            PlayVideo(m_pvRawImage, m_pvVideoPlayer);
            m_attractModeTimer.ElapsedTime = 0f;
            m_isPlayingPv = true;

        }
    }

    /// <summary>
    /// タイマーが目標時間を経過したか調べる<br/>
    /// 引数1： _progressTimer 調べる対象のタイマー<br/>
    /// </summary>
    /// <param name="_progressTimer">調べる対象のタイマー</param>
    /// <returns>タイマーが目標時間を経過したか</returns>
    private bool IsTimerFinished(ProgressTimer _progressTimer)
    {
        return (_progressTimer.ElapsedTime >= _progressTimer.Duration);
    }

    /// <summary>
    /// ビデオを再生する<br/>
    /// 引数1： _rawImage ビデオを映しているイメージ<br/>
    /// 引数2： _videoPlayer 対象のビデオプレイヤー<br/>
    /// </summary>
    /// <param name="_rawImage">ビデオを映しているイメージ</param>
    /// <param name="_videoPlayer">対象のビデオプレイヤー</param>
    /// <returns>なし</returns>
    public void PlayVideo(RawImage _rawImage, VideoPlayer _videoPlayer)
    {
        if (_rawImage != null)
        {
            _rawImage.enabled = true; // 表示する
            _videoPlayer.Play();                // 再生する
        }
    }

    /// <summary>
    /// ビデオを停止する<br/>
    /// 引数1： _rawImage ビデオを映しているイメージ<br/>
    /// 引数2： _videoPlayer 対象のビデオプレイヤー<br/>
    /// </summary>
    /// <param name="_rawImage">ビデオを映しているイメージ</param>
    /// <param name="_videoPlayer">対象のビデオプレイヤー</param>
    /// <returns>なし</returns>
    public void StopVideo(RawImage _rawImage, VideoPlayer _videoPlayer)
    {
        if (_rawImage != null)
        {
            _videoPlayer.Stop();                // 停止する
            _rawImage.enabled = false; // 非表示にする
        }
    }

    /// <summary>
    /// _screenPositionがターゲットのレクトの中にあるか調べる<br/>
    /// 引数1： _screenPosition ターゲットの内にあるか調べたいスクリーン座標<br/>
    /// 引数2： _targetRect ターゲットとなるレクト<br/>
    /// 引数3： _camera ターゲットが配置されているキャンバスを映すカメラ<br/>
    ///         ScreenSpace-Overlayの時はnullで良い<br/>
    /// </summary>
    /// <param name="_screenPosition">ターゲットの内にあるか調べたいスクリーン座標</param>
    /// <param name="_targetRect">ターゲットとなるレクト</param>
    /// <param name="_camera">ターゲットが配置されているキャンバスを映すカメラ</param>
    /// <returns>対象のレクト内にあるかどうか</returns>
    public bool IsPositionOverImageWithRect(Vector2 _screenPosition, RectTransform _targetRect, Camera _camera)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(_targetRect, _screenPosition, _camera);
    }
}
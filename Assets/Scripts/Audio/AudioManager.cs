/*
 * ファイル
 * AudioManager C#
 * 
 * システム
 * BGMとSEを再生するマネージャー。（シングルトン）
 * 
 * 変更履歴
 * 2025/07/16　奥山　凜　作成
 * 2025/09/08　坂上
 */

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// BGMとSEを再生するマネージャー。（シングルトン）
/// </summary>
public class AudioManager : Singleton<AudioManager>
{
    [SerializeField]
    private AudioDataList m_audioDataList;        // BGMやSEの入ったDatalist。スクリプトにインスペクターからセットする。
    private AudioSource m_bgmAudioSource;         // BGMを再生するAudioSource
    private AudioSource m_seAudioSource;          // SEを再生するAudioSource
    private bool m_isPlaybackBGMAcrossScenes;     // 再生しているBGMがシーンを跨いで再生されるか
    private bool m_isPlaybackSEAcrossScenes;      // 再生しているSEがシーンを跨いで再生されるか

    protected override void Awake()
    {
        base.Awake();

        // BGM用、SE用のAudioSourceを用意
        {
            m_bgmAudioSource = gameObject.AddComponent<AudioSource>();
            m_seAudioSource = gameObject.AddComponent<AudioSource>();

            m_bgmAudioSource.playOnAwake = false;
            m_bgmAudioSource.volume = 1.0f;
            m_bgmAudioSource.loop = true;

            m_seAudioSource.playOnAwake = false;
            m_seAudioSource.volume = 1.0f;
        }


        m_isPlaybackBGMAcrossScenes = false;
        m_isPlaybackSEAcrossScenes = false;


        if (m_audioDataList.IsUnityNull())
        {
            // BGMとSEの入ったデータリストの読み込み
            m_audioDataList = Resources.Load("Datas/Audio/AudioDataList") as AudioDataList;
        }
    }

    /// <summary>
    /// Enumで指定したSEを再生するく<br/>
    /// 引数1：_seDataName 再生したいSE
    /// 引数2：_isPlaybackAcrossScenes シーンを跨いで再生を続けるか
    /// </summary>
    /// <param name="_seDataName">再生したいSE（Enum）</param>
    /// <param name="_isPlaybackAcrossScenes">シーンを跨いで再生を続けるか</param>
    /// <returns>なし</returns>
    public void PlaySE(AudioName _seDataName, bool _isPlaybackAcrossScenes = false)
    {
        if (m_audioDataList == null)
        {
            return;
        }

        // 坂上　追加---------------------------------------------------------
        AudioData audioData = m_audioDataList.GetAudioData(_seDataName);
        if (audioData == null || audioData.AudioClip == null)
        {
            return;
        }

        m_isPlaybackSEAcrossScenes = _isPlaybackAcrossScenes;
        m_seAudioSource.clip = audioData.AudioClip;
        m_seAudioSource.volume = audioData.Volume;
        m_seAudioSource.Play();
        //-------------------------------------------------------------------
    }

    /// <summary>
    /// Enumで指定したBGMを再生する<br/>
    /// 引数でループするか、シーンを跨いで再生を続けるかを決める<br/>
    /// 引数1：_bgmDataName 再生したいBGM（Enum）<br/>
    /// 引数2：_isLoop ループ再生するか<br/>
    /// 引数3：_isPlaybackAcrossScenes シーンを跨いで再生を続けるか
    /// </summary>
    /// <param name="_bgmDataName">再生したいBGM（Enum）</param>
    /// <param name="_isLoop">ループ再生するか</param>
    /// <param name="_isPlaybackAcrossScenes">シーンを跨いで再生を続けるか</param>
    /// <returns>なし</returns>
    public void PlayBGM(AudioName _bgmDataName, bool _isLoop = true, bool _isPlaybackAcrossScenes = false)
    {
        if (m_audioDataList == null)
        {
            return;
        }

        AudioClip audioClip = m_audioDataList.GetAudioClip(_bgmDataName);
        if (audioClip != null)
        {
            m_bgmAudioSource.loop = _isLoop;
            m_isPlaybackBGMAcrossScenes = _isPlaybackAcrossScenes;
            m_bgmAudioSource.clip = audioClip;
            m_bgmAudioSource.Play();
        }
    }


    /// <summary>
    /// セットしているAudioClipが指定したAudioClipと同じかつ再生中ならtrueを返す<br/>
    /// 引数1：_bgmDataName 再生中か確認するBGM
    /// </summary>
    /// <returns>引数で指定したBGMを再生中か否か</returns>
    /// <param name="_bgmDataName">再生中か確認するBGM</param>
    public bool IsPlayingTheNamesBGM(AudioName _bgmDataName)
    {
        bool ret = false;
        // データリストの取得に失敗している場合リターン
        if (m_audioDataList == null)
        {
            return ret;
        }


        AudioClip audioClip = m_audioDataList.GetAudioClip(_bgmDataName);
        if (audioClip != null)
        {
            if ((m_bgmAudioSource.isPlaying) && (m_bgmAudioSource.clip == audioClip))
            {
                ret = true;
            }
            
        }

        return ret;
    }

    /// <summary>
    /// BGMの再生を止める
    /// </summary>
    /// <returns>なし</returns>
    public void StopBGM()
    {
        m_bgmAudioSource.Stop();
        m_isPlaybackBGMAcrossScenes = false;
    }

    /// <summary>
    /// SEの再生を止める
    /// </summary>
    /// <returns>なし</returns>
    public void StopSE()
    {
        m_seAudioSource.Stop();
        m_isPlaybackSEAcrossScenes = false;
    }

    /// <summary>
    /// シーンの破棄時、シーンを跨いで再生する設定で無ければBGMを止める<br/>
    /// StartupInitializerでシーン破棄時のデリゲートに登録する<br/>
    /// 引数はデリゲートにバインドするために合わせて用意る必要がある<br/>
    /// 引数1：_scene デリゲートから渡されるシーンの情報が入る
    /// </summary>
    /// <param name="_scene">デリゲートから渡されるシーンの情報が入る</param>>
    /// <returns>なし</returns>
    public void StopsBGMWhenSwitchingScenes(Scene _scene)
    {
        if (m_isPlaybackBGMAcrossScenes == false)
        {
            m_bgmAudioSource.Stop();
        }
    }
    /// <summary>
    /// シーンの破棄時、シーンを跨いで再生する設定で無ければSEを止める<br/>
    /// StartupInitializerでシーン破棄時のデリゲートに登録する<br/>
    /// 引数はデリゲートにバインドするために合わせて用意る必要がある<br/>
    /// 引数1：_scene デリゲートから渡されるシーンの情報が入る
    /// </summary>
    /// <param name="_scene">デリゲートから渡されるシーンの情報が入る</param>>
    /// <returns>なし</returns>
    public void StopsSEWhenSwitchingScenes(Scene _scene)
    {
        if (m_isPlaybackSEAcrossScenes == false)
        {
            m_seAudioSource.Stop();
        }
    }
}

/*
 * ファイル
 * AudioDataList C#
 * 
 * システム
 * 音声のクリップを持つDataのDataList
 * 
 * 変更履歴
 * 2025/07/16　奥山　凜　作成
 * 2025/09/08　坂上
 */

using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 音声のクリップを持つDataのDataList
/// </summary>
[CreateAssetMenu(fileName = "AudioDataList", menuName = "ScriptableObjects/CreateAudioDataList")]
public class AudioDataList : DataListBase<AudioName, AudioData>
{
    /// <summary>
    /// データリストからデータを取得し、データからオーディオクリップを取得する<br/>
    /// 引数1：_eAudioName 探すクリップの種類
    /// </summary>
    /// <param name="_eAudioName">探すクリップの種類</param>
    /// <returns>検索したクリップ（ない場合null）</returns>
    public AudioClip GetAudioClip(AudioName _eAudioName)
    {
        AudioData audioData = GetData(_eAudioName);

        if (audioData.IsUnityNull())
        {
            return null;
        }

        return audioData.AudioClip;
    }

    //　坂上 追加: AudioData を直接返すメソッド
    /// <summary>
    /// データリストからデータを取得し、データを取得する<br/>
    /// 引数1：_name 探すクリップの種類
    /// </summary>
    /// <param name="_name">探すクリップの種類</param>
    /// <returns>検索したData</returns>
    public AudioData GetAudioData(AudioName _name)
    {
        return GetData(_name);
    }
}
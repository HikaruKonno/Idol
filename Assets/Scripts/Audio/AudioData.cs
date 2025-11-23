/*
 * ファイル
 * AudioData C#
 * 
 * システム
 * 音声のクリップを持つData
 * 
 * 変更履歴
 * 2025/07/16　奥山　凜　作成
 * 2025/09/08　坂上
 */
using UnityEngine;

/// <summary>
/// 音声のクリップを持つData
/// </summary>
[CreateAssetMenu(fileName = "AudioData", menuName = "ScriptableObjects/CreateAudioData")]
public class AudioData : DataBase<AudioName>
{
    public AudioClip AudioClip => m_audioClip;

    [SerializeField]
    private AudioClip m_audioClip;

    // 坂上　追加------------------------------
    public float Volume => m_volume;
    [SerializeField, Range(0f, 1f)]
    private float m_volume = 1f; // Default100%
    //----------------------------------------
}

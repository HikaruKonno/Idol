/*
 * ファイル
 * HolisticResultReceiver C#
 * 
 * システム
 * Mediapipeの検出結果を受け取るイベントに、スクリプタブルオブジェクトの関数を登録する
 * 
 * 変更履歴
 * 2025/09/09　奥山　凜　作成
 */

using Mediapipe.Unity.Sample.Holistic;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Mediapipeの検出結果を受け取るイベントに、スクリプタブルオブジェクトの関数を登録するクラス<br/>
/// </summary>
public class HolisticResultReceiver : MonoBehaviour
{
    [SerializeField]
    private MediapipeResultDataContainer m_mediapipeResultDataContainer;        // 登録する関数のあるスクリプタブルオブジェクト
    [SerializeField]
    private HolisticTrackingSolution m_holisticTrackingSolution;                // Mediapipeの結果を受け取るイベントのあるクラス
    
    // WebカメラがセットされMediapipeに画像が流れてくるのを待つためIEnumerator
    private IEnumerator Start()
    {
        // インスペクターでセットしていなかった場合の保険
        if (m_mediapipeResultDataContainer == null)
        {
            m_mediapipeResultDataContainer = Resources.Load("Datas/Scene/MediapipeResultDataContainer") as MediapipeResultDataContainer;
        }

        // カメラ画像が設定させるのを待ち
        yield return StartCoroutine(MediaPipeUtils.WaitImageSourceReady());

        // HandLandmarker のイベント登録
        if (!m_holisticTrackingSolution.IsUnityNull())
        {
            m_holisticTrackingSolution.OnPoseLandmarksReceived += m_mediapipeResultDataContainer.HandleOnPoseLandmarksReceived;
            m_holisticTrackingSolution.OnLeftHandLandmarksReceived += m_mediapipeResultDataContainer.HandleOnLeftHandLandmarksReceived;
            m_holisticTrackingSolution.OnRightHandLandmarksReceived += m_mediapipeResultDataContainer.HandleOnRightHandLandmarksReceived;
        }
        Destroy(this);      // このコンポーネントを削除
    }
}

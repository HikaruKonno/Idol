/*
 * ファイル
 * MediapipeResultDataContainer C#
 * 
 * システム
 * Mediapipeから受け取るPoseやハンドジェスチャーの結果を格納する
 * 
 * 変更履歴
 * 2025/09/09　奥山　凜　作成
 */

using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Mediapipeから受け取るPoseやハンドジェスチャーの結果を格納するスクリプタブルオブジェクト<br/>
/// </summary>
[CreateAssetMenu(fileName = "MediapipeResultDataContainer", menuName = "ScriptableObjects/CreateMediapipeResultDataContainer")]
public class MediapipeResultDataContainer : ScriptableObject
{
    public IReadOnlyList<Mediapipe.NormalizedLandmark> CopyPoseNormalizedLandmarks => m_copyPoseNormalizedLandmarks;
    private List<Mediapipe.NormalizedLandmark> m_copyPoseNormalizedLandmarks;           // 検出されたPoseのランドマーク

    public IReadOnlyList<Mediapipe.NormalizedLandmark> LeftHandNormalizedLandmarkList => m_leftHandNormalizedLandmarkList;
    private List<Mediapipe.NormalizedLandmark> m_leftHandNormalizedLandmarkList;        // 検出された左手のランドマーク
    public IReadOnlyList<Mediapipe.NormalizedLandmark> RightHandNormalizedLandmarkList => m_rightHandNormalizedLandmarkList;
    private List<Mediapipe.NormalizedLandmark> m_rightHandNormalizedLandmarkList;       // 検出された右手のランドマーク

    public bool IsPoseTracked => m_isPoseTracked;
    private bool m_isPoseTracked = false;            // ポーズが検出されているか
    public bool IsLeftHandTracked => m_isLeftHandTracked;
    private bool m_isLeftHandTracked = false;        // 左手が検出されているか
    public bool IsRightHandTracked => m_isRightHandTracked;
    private bool m_isRightHandTracked = false;       // 右手が検出されているか

    public HandShapeDetector.HandShape LeftHandShape => m_leftHandShape;
    private HandShapeDetector.HandShape m_leftHandShape = HandShapeDetector.HandShape.Unknown;        // 検出された左手のハンドジェスチャー
    public HandShapeDetector.HandShape RightHandShape => m_rightHandShape;
    private HandShapeDetector.HandShape m_rightHandShape = HandShapeDetector.HandShape.Unknown;       // 検出された右手のハンドジェスチャー


    /// <summary>
    /// 左手の手の形をセットする関数<br/>
    /// 引数1：_rightHandShape スクリプタブルオブジェクトに設定する現在の右手の形<br/>
    /// </summary>
    /// <param name="_rightHandShape">スクリプタブルオブジェクトに設定する現在の右手の形</param>
    /// <returns>なし</returns>
    public void SetLeftHandShape(HandShapeDetector.HandShape _leftHandShape)
    {
        m_leftHandShape = _leftHandShape;
    }

    /// <summary>
    /// 右手の手の形をセットする関数<br/>
    /// 引数1：_rightHandShape スクリプタブルオブジェクトに設定する現在の右手の形<br/>
    /// </summary>
    /// <param name="_rightHandShape">スクリプタブルオブジェクトに設定する現在の右手の形</param>
    /// <returns>なし</returns>
    public void SetRightHandShape(HandShapeDetector.HandShape _rightHandShape)
    {
        m_rightHandShape = _rightHandShape;
    }

    /// <summary>
    /// MediapipeからLandmarkの結果を受け取る為、イベントに登録する関数<br/>
    /// 引数1：_poselandmarkList Mediapipeから受け取る、検出されたポーズのランドマークのリスト<br/>
    /// </summary>
    /// <param name="_poselandmarkList">Mediapipeから受け取る、検出されたポーズのランドマークのリスト</param>
    /// <returns>なし</returns>
    public void HandleOnPoseLandmarksReceived(Mediapipe.NormalizedLandmarkList _poselandmarkList)
    {
        if (_poselandmarkList.IsUnityNull())
        {
            m_isPoseTracked = false;
            return;
        }
        m_isPoseTracked = true;

        // オリジナルの内容のコピーを作成
        List<Mediapipe.NormalizedLandmark> deepCopyLandmarks = _poselandmarkList.Landmark.Select(lm => lm.Clone()).ToList();

        // 本来のリストに無い、左右の肩の中点等の中点を追加
        deepCopyLandmarks.Add(Mediapipe.NormalizedLandmark.Midpoint(deepCopyLandmarks[(int)PoseLandmarksIndex.LeftHip], deepCopyLandmarks[(int)PoseLandmarksIndex.RightHip]));
        deepCopyLandmarks.Add(Mediapipe.NormalizedLandmark.Midpoint(deepCopyLandmarks[(int)PoseLandmarksIndex.LeftShoulder], deepCopyLandmarks[(int)PoseLandmarksIndex.RightShoulder]));
        deepCopyLandmarks.Add(Mediapipe.NormalizedLandmark.Midpoint(deepCopyLandmarks[(int)PoseLandmarksIndex.LeftEar], deepCopyLandmarks[(int)PoseLandmarksIndex.RightEar]));
        deepCopyLandmarks.Add(Mediapipe.NormalizedLandmark.Midpoint(deepCopyLandmarks[(int)PoseLandmarksIndex.LeftEye], deepCopyLandmarks[(int)PoseLandmarksIndex.RightEye]));
        deepCopyLandmarks.Add(Mediapipe.NormalizedLandmark.Midpoint(deepCopyLandmarks[(int)PoseLandmarksIndex.LeftMouth], deepCopyLandmarks[(int)PoseLandmarksIndex.RightMouth]));
        deepCopyLandmarks.Add(Mediapipe.NormalizedLandmark.Midpoint(deepCopyLandmarks[(int)PoseLandmarksIndex.LeftHeel], deepCopyLandmarks[(int)PoseLandmarksIndex.LeftFootIndex]));
        deepCopyLandmarks.Add(Mediapipe.NormalizedLandmark.Midpoint(deepCopyLandmarks[(int)PoseLandmarksIndex.RightHeel], deepCopyLandmarks[(int)PoseLandmarksIndex.RightFootIndex]));
        deepCopyLandmarks.Add(Mediapipe.NormalizedLandmark.Midpoint(deepCopyLandmarks[(int)PoseLandmarksIndex.LeftPinky], deepCopyLandmarks[(int)PoseLandmarksIndex.LeftIndex]));
        deepCopyLandmarks.Add(Mediapipe.NormalizedLandmark.Midpoint(deepCopyLandmarks[(int)PoseLandmarksIndex.RightPinky], deepCopyLandmarks[(int)PoseLandmarksIndex.RightIndex]));


        m_copyPoseNormalizedLandmarks = deepCopyLandmarks;
    }

    /// <summary>
    /// MediapipeからLandmarkの結果を受け取る為、イベントに登録する関数<br/>
    /// 引数1：_leftHandlandmarkList Mediapipeから受け取る、検出された左手のランドマークのリスト<br/>
    /// </summary>
    /// <param name="_leftHandlandmarkList">Mediapipeから受け取る、検出された左手のランドマークのリスト</param>
    /// <returns>なし</returns>
    public void HandleOnLeftHandLandmarksReceived(Mediapipe.NormalizedLandmarkList _leftHandlandmarkList)
    {
        if (_leftHandlandmarkList.IsUnityNull())
        {
            m_isLeftHandTracked = false;
            return;
        }
        m_isLeftHandTracked = true;

        // オリジナルの内容のコピーを作成、一部既存のランドマークから中間点のランドマークを計算してAdd
        List<Mediapipe.NormalizedLandmark> deepCopyLandmarks = _leftHandlandmarkList.Landmark.Select(lm => lm.Clone()).ToList();

        m_leftHandNormalizedLandmarkList = deepCopyLandmarks;
    }

    /// <summary>
    /// MediapipeからLandmarkの結果を受け取る為、イベントに登録する関数<br/>
    /// 引数1：_rightHandLandmarkList Mediapipeから受け取る、検出された右手のランドマークのリスト<br/>
    /// </summary>
    /// <param name="_rightHandLandmarkList">Mediapipeから受け取る、検出された右手のランドマークのリスト</param>
    /// <returns>なし</returns>
    public void HandleOnRightHandLandmarksReceived(Mediapipe.NormalizedLandmarkList _rightHandLandmarkList)
    {
        if (_rightHandLandmarkList.IsUnityNull())
        {
            m_isRightHandTracked = false;
            return;
        }
        m_isRightHandTracked = true;

        // オリジナルの内容のコピーを作成、一部既存のランドマークから中間点のランドマークを計算してAdd
        List<Mediapipe.NormalizedLandmark> deepCopyLandmarks = _rightHandLandmarkList.Landmark.Select(lm => lm.Clone()).ToList();

        m_rightHandNormalizedLandmarkList = deepCopyLandmarks;
    }
}
/*
 * ファイル
 * MediaPipeLandmarks C#
 * 
 * システム
 * Mediapipeのランドマークの要素数に使用する列挙型をまとめたファイル
 * 番号はMediaPipeの公式に書いてあります
 * 
 * 変更履歴
 * 2025/07/02　奥山　凜　作成
 * 2025/07/04　奥山　凜　PoseのMidを追加
 */

/// <summary>
/// MediaPipeのPoseLandmarkの配列の要素番号として使うenum<br/>
/// 後半のMid～と書いてあるものは自分で追加したもの<br/>
/// MidはMediaPipeから配列を受け取った後、自身で計算して追加する<br/>
/// </summary>
public enum PoseLandmarksIndex
{
    Nose = 0,
    LeftEyeInner,
    LeftEye,
    LeftEyeOuter,
    RightEyeInner,
    RightEye,
    RightEyeOuter,
    LeftEar,
    RightEar,
    LeftMouth,
    RightMouth,
    LeftShoulder,
    RightShoulder,
    LeftElbow,
    RightElbow,
    LeftWrist,
    RightWrist,
    LeftPinky,
    RightPinky,
    LeftIndex,
    RightIndex,
    LeftThumb,
    RightThumb,
    LeftHip,
    RightHip,
    LeftKnee,
    RightKnee,
    LeftAnkle,
    RightAnkle,
    LeftHeel,
    RightHeel,
    LeftFootIndex,
    RightFootIndex,

    // 計算して既存のランドマークのリストにaddする（addの順番注意）
    MidHip,
    MidShoulder,
    MidEar,
    MidEye,
    MidMouth,
    LeftMidHeelToFootIndex,
    RightMidHeelToFootIndex,
    LeftMidPinkyToIndex,
    RightMidPinkyToIndex,

}

/// <summary>
/// MediaPipeのHandLandmarkの配列の要素番号として使うenum<br/>
/// 左右とも同じくこの列挙型を使う<br/>
/// </summary>
public enum EHandLandmarks
{
    Wrist = 0,
    ThumbCMC,
    ThumbMCP,
    ThumbIP,
    ThumbTip,
    IndexMCP,
    IndexPIP,
    IndexDIP,
    IndexTip,
    MiddleMCP,
    MiddlePIP,
    MiddleDIP,
    MiddleTip,
    RingMCP,
    RingPIP,
    RingDIP,
    RingTip,
    PinkyMCP,
    PinkyPIP,
    PinkyDIP,
    PinkyTip
}
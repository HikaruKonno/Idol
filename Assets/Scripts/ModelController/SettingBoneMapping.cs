/*
 * ファイル
 * SettingBoneMapping C#
 * 
 * システム
 * Mediapipeから受け取るランドマークをモデルに対応させるため、
 * 各ボーンと対応するランドマークをまとめ、配列に入れる関数が用意されたクラス
 * 
 * 変更履歴
 * 2025/07/02　奥山　凜　作成
 * 2025/07/05　奥山　凜　三軸の回転をとるため外積のベクトルを追加
 * 2025/07/24　奥山　凜　フィルターの追加
 * 2025/09/03　奥山　凜　アニメーションに変更したため使用しなくなったHandのマッピングと、同じく使用しない一部Poseのマッピングを削除
 * 2025/09/11　奥山　凜　外積が潰れた時用の、別の予備の外積を用意
 */

using UnityEngine;
using Unity.VisualScripting;

/// <summary>
/// Mediapipeから受け取るランドマークをモデルに対応させるため、<br/>
/// 各ボーンと対応するランドマークをまとめ、配列に入れる関数が用意されたクラス<br/>
/// 非MonoBehaviour
/// </summary>
public static class SettingBoneMapping
{
    /// <summary>
    /// 後述のBoneLandmarkMapping内のベクトルが、トラッキングによりUnityのモデルがTポーズをとる時、どの方向を向いているかを指定するのに使うenum
    /// ベクトルをキャラクターのローカルのどの方向として適用するかを決定する
    /// </summary>
    public enum DirectionCheckResult
    {
        Forward = 0,
        Backward,
        Left,
        Right,
        Up,
        Down,
    }

    /// <summary>
    /// ボーンとランドマークを対応させるための情報を纏めたクラス<br/>
    /// 非MonoBehaviour
    /// </summary>
    [System.Serializable]
    public class BoneLandmarkMapping
    {
        public HumanBodyBones Bone { get; private set; }             // モデルのどのボーンに使用する情報か

        public int StartLandmarkIndex { get; private set; }          // ボーンの方向のベクトルの始点（対象のランドマークを取得するためにListの要素数として使用）
        public int EndLandmarkIndex { get; private set; }            // ボーンの方向のベクトルの終点（対象のランドマークを取得するためにListの要素数として使用）
        public DirectionCheckResult VectorDirection { get; private set; }     // startLandmarkIndexとendLandmarkIndexの二点間のベクトルを、モデルのローカルのどの方向として使用するか
        
        // 三軸分の回転を得るのに二ベクトル必要
        public int StartOtherLandmarkIndex { get; private set; }     // 外積に使う2ベクトル目の始点
        public int EndOtherLandmarkIndex { get; private set; }       // 外積に使う2ベクトル目の終点
        public DirectionCheckResult CrossProductDirection { get; private set; }     // 二ベクトルから求める外積のベクトルをローカルのどの方向として使用するか

        // 外積が潰れた時の保険
        public int FallbackStartOtherLandmarkIndex { get; private set; }     // 外積に使う二ベクトルが重なり、外積が潰れた時に使用する、代用の二ベクトル目の始点
        public int FallbackEndOtherLandmarkIndex { get; private set; }       // 外積が潰れた時に使用する、代用の二ベクトル目の終点
        public DirectionCheckResult FallbackCrossProductDirection { get; private set; }     // 外積が潰れた時に使用する、二ベクトルから求める外積のベクトルをローカルのどの方向として使用するか


        public RotationFilter RotationFilter { get; private set; }       // ランドマークの細かな揺れを抑えるためのフィルター

        /// <summary>
        /// コンストラクタでランドマークのenumをintにキャストし、この変数で各変数に代入する<br/>
        /// 
        /// 引数1： _bone                            対応させるのボーンの種類<br/>
        /// 引数2： _startLandmarkIndex              ボーンの方向のベクトルの始点のランドマーク<br/>
        /// 引数3： _endLandmarkIndex                ボーンの方向のベクトルの終点のランドマーク<br/>
        /// 引数4： _vectorDirection                 _startLandmarkIndexと_endLandmarkIndexの二点間のベクトルを、モデルのローカルのどの方向として使用するか<br/>
        /// 引数5： _startOtherLandmarkIndex         外積に使う2ベクトル目の始点のランドマーク<br/>
        /// 引数6： _endOtherLandmarkIndex           外積に使う2ベクトル目の終点のランドマーク<br/>
        /// 引数7： _crossProductDirection           二ベクトルから求める外積のベクトルを、ローカルのどの方向として使用するか<br/>
        /// 引数8： _fallbackStartOtherLandmarkIndex 外積に使う二ベクトルが重なり、外積が潰れた時に使用する、代用の二ベクトル目の始点のランドマーク<br/>
        /// 引数9： _fallbackEndOtherLandmarkIndex   外積が潰れた時に使用する、代用の二ベクトル目の終点のランドマーク<br/>
        /// 引数10： _fallbackCrossProductDirection  外積が潰れた時に使用する、二ベクトルから求める外積のベクトルをローカルのどの方向として使用するか<br/>
        /// 引数11： _minCutoff                      フィルターの最低限の滑らかさ<br/>
        /// 引数12： _beta                           フィルターの、動きの速さに対する応答性<br/>
        /// 引数13： _dCutoff                        速度（微分）のフィルタ強度<br/>
        /// 引数14： _rotationLimit                  フィルターの回転の制限<br/>
        /// </summary>
        /// <param name="_bone">対応させるのボーンの種類</param>
        /// <param name="_startLandmarkIndex">ボーンの方向のベクトルの始点のランドマーク</param>
        /// <param name="_endLandmarkIndex">ボーンの方向のベクトルの終点のランドマーク</param>
        /// <param name="_vectorDirection">_startLandmarkIndexと_endLandmarkIndexの二点間のベクトルを、モデルのローカルのどの方向として使用するか</param>
        /// <param name="_startOtherLandmarkIndex">外積に使う2ベクトル目の始点のランドマーク</param>
        /// <param name="_endOtherLandmarkIndex">外積に使う2ベクトル目の終点のランドマーク</param>
        /// <param name="_crossProductDirection">二ベクトルから求める外積のベクトルを、ローカルのどの方向として使用するか</param>
        /// <param name="_fallbackStartOtherLandmarkIndex">外積に使う二ベクトルが重なり、外積が潰れた時に使用する、代用の二ベクトル目の始点のランドマーク</param>
        /// <param name="_fallbackEndOtherLandmarkIndex">外積が潰れた時に使用する、代用の二ベクトル目の終点のランドマーク</param>
        /// <param name="_fallbackCrossProductDirection">外積が潰れた時に使用する、二ベクトルから求める外積のベクトルをローカルのどの方向として使用するか</param>
        /// <param name="_minCutoff">フィルターの最低限の滑らかさ</param>
        /// <param name="_beta">フィルターの、動きの速さに対する応答性</param>
        /// <param name="_dCutoff">速度（微分）のフィルタ強度</param>
        /// <param name="_rotationLimit">フィルターの回転の制限</param>
        /// <returns>なし（このクラスのインスタンス）</returns>
        public BoneLandmarkMapping(HumanBodyBones _bone,
            PoseLandmarksIndex _startLandmarkIndex, PoseLandmarksIndex _endLandmarkIndex, DirectionCheckResult _vectorDirection,
            PoseLandmarksIndex _startOtherLandmarkIndex, PoseLandmarksIndex _endOtherLandmarkIndex, DirectionCheckResult _crossProductDirection,
            PoseLandmarksIndex _fallbackStartOtherLandmarkIndex, PoseLandmarksIndex _fallbackEndOtherLandmarkIndex, DirectionCheckResult _fallbackCrossProductDirection,
            float _minCutoff, float _beta, float _dCutoff, RotationLimit _rotationLimit = null)
        {
            Bone = _bone;

            StartLandmarkIndex = (int)_startLandmarkIndex;
            EndLandmarkIndex = (int)_endLandmarkIndex;
            VectorDirection = _vectorDirection;

            StartOtherLandmarkIndex = (int)_startOtherLandmarkIndex;
            EndOtherLandmarkIndex = (int)_endOtherLandmarkIndex;
            CrossProductDirection = _crossProductDirection;

            FallbackStartOtherLandmarkIndex = (int)_fallbackStartOtherLandmarkIndex;
            FallbackEndOtherLandmarkIndex = (int)_fallbackEndOtherLandmarkIndex;
            FallbackCrossProductDirection = _fallbackCrossProductDirection;


            RotationFilter = new RotationFilter(StartupInitializer.FPS, _minCutoff, _beta, _dCutoff, _rotationLimit);
        }

        ~BoneLandmarkMapping()
        {
            RotationFilter = null;
        }
    }

    /// <summary>
    /// 各ボーンと対応するランドマークをまとめ配列にその情報を格納する（Pose用）<br/>
    /// 引数1： _boneMappings マッピングを格納する配列<br/>
    /// 引数2： _minCutoff    フィルターの最低限の滑らかさ<br/>
    /// 引数3： _beta         フィルターの、動きの速さに対する応答性<br/>
    /// 引数4： _dCutoff      速度（微分）のフィルタ強度
    /// </summary>
    /// <param name="_boneMappings">マッピングを格納する配列</param>
    /// <param name="_minCutoff">フィルターの最低限の滑らかさ</param>
    /// <param name="_beta">フィルターの、動きの速さに対する応答性</param>
    /// <param name="_dCutoff">速度（微分）のフィルタ強度</param>
    /// <returns>なし</returns>
    public static void SettingPoseBoneMappings(ref BoneLandmarkMapping[] _boneMappings, float _minCutoff = 0.6f, float _beta = 0.0001f, float _dCutoff = 1.0f)
    {
        if (!_boneMappings.IsUnityNull())
        {
            return;
        }

        HumanBodyBones bone = HumanBodyBones.LastBone;

        RotationLimit hipRotationLimit = new RotationLimit(Vector2.zero, new Vector2(-179f, 180f), Vector2.zero);

        _boneMappings = new BoneLandmarkMapping[]
        {
            new BoneLandmarkMapping (bone = HumanBodyBones.Hips,
                            PoseLandmarksIndex.MidHip, PoseLandmarksIndex.MidShoulder,DirectionCheckResult.Up,
                            PoseLandmarksIndex.LeftHip,PoseLandmarksIndex.RightHip,DirectionCheckResult.Forward,
                            PoseLandmarksIndex.LeftHip,PoseLandmarksIndex.RightHip,DirectionCheckResult.Forward,
                            _minCutoff, _beta, _dCutoff, hipRotationLimit),
            new BoneLandmarkMapping (bone = HumanBodyBones.Spine,
                            PoseLandmarksIndex.MidHip, PoseLandmarksIndex.MidShoulder,DirectionCheckResult.Up,
                            PoseLandmarksIndex.LeftHip,PoseLandmarksIndex.RightHip,DirectionCheckResult.Forward,
                            PoseLandmarksIndex.LeftHip,PoseLandmarksIndex.RightHip,DirectionCheckResult.Forward,
                            _minCutoff, _beta, _dCutoff),
            new BoneLandmarkMapping (bone = HumanBodyBones.UpperChest,
                            PoseLandmarksIndex.MidHip, PoseLandmarksIndex.MidShoulder,DirectionCheckResult.Up,
                            PoseLandmarksIndex.LeftShoulder, PoseLandmarksIndex.RightShoulder,DirectionCheckResult.Forward,
                            PoseLandmarksIndex.LeftShoulder, PoseLandmarksIndex.RightShoulder,DirectionCheckResult.Forward,
                            _minCutoff, _beta, _dCutoff),

            new BoneLandmarkMapping (bone = HumanBodyBones.Neck,
                            PoseLandmarksIndex.MidShoulder, PoseLandmarksIndex.MidEar,DirectionCheckResult.Up,
                            PoseLandmarksIndex.LeftEar, PoseLandmarksIndex.RightEar,DirectionCheckResult.Forward,
                            PoseLandmarksIndex.LeftEar, PoseLandmarksIndex.RightEar,DirectionCheckResult.Forward,
                            _minCutoff, _beta, _dCutoff),
            new BoneLandmarkMapping (bone = HumanBodyBones.Head,
                            PoseLandmarksIndex.MidMouth, PoseLandmarksIndex.MidEye,DirectionCheckResult.Up,
                            PoseLandmarksIndex.LeftMouth, PoseLandmarksIndex.RightMouth,DirectionCheckResult.Forward,
                            PoseLandmarksIndex.LeftMouth, PoseLandmarksIndex.RightMouth,DirectionCheckResult.Forward,
                            _minCutoff, _beta, _dCutoff),


    
            new BoneLandmarkMapping (HumanBodyBones.RightUpperArm,
                            PoseLandmarksIndex.RightShoulder, PoseLandmarksIndex.RightElbow,DirectionCheckResult.Left,
                            PoseLandmarksIndex.LeftShoulder, PoseLandmarksIndex.RightShoulder,DirectionCheckResult.Up,
                            PoseLandmarksIndex.MidHip, PoseLandmarksIndex.MidShoulder,DirectionCheckResult.Forward,
                                    _minCutoff, _beta, _dCutoff),
            new BoneLandmarkMapping (HumanBodyBones.RightLowerArm,
                            PoseLandmarksIndex.RightElbow, PoseLandmarksIndex.RightWrist,DirectionCheckResult.Left,
                            PoseLandmarksIndex.LeftShoulder, PoseLandmarksIndex.RightShoulder,DirectionCheckResult.Up,
                            PoseLandmarksIndex.MidHip, PoseLandmarksIndex.MidShoulder,DirectionCheckResult.Forward,
                                    _minCutoff, _beta, _dCutoff),
            new BoneLandmarkMapping (HumanBodyBones.RightHand,
                            PoseLandmarksIndex.RightWrist, PoseLandmarksIndex.RightMidPinkyToIndex,DirectionCheckResult.Left,
                            PoseLandmarksIndex.RightIndex, PoseLandmarksIndex.RightPinky,DirectionCheckResult.Up,
                            PoseLandmarksIndex.MidHip, PoseLandmarksIndex.MidShoulder,DirectionCheckResult.Forward,
                                    _minCutoff, _beta, _dCutoff),

            new BoneLandmarkMapping (HumanBodyBones.LeftUpperArm,
                            PoseLandmarksIndex.LeftShoulder, PoseLandmarksIndex.LeftElbow,DirectionCheckResult.Right,
                            PoseLandmarksIndex.LeftShoulder, PoseLandmarksIndex.RightShoulder,DirectionCheckResult.Up,
                            PoseLandmarksIndex.MidHip, PoseLandmarksIndex.MidShoulder,DirectionCheckResult.Backward,
                                    _minCutoff,_beta, _dCutoff),
            new BoneLandmarkMapping (HumanBodyBones.LeftLowerArm,
                            PoseLandmarksIndex.LeftElbow, PoseLandmarksIndex.LeftWrist,DirectionCheckResult.Right,
                            PoseLandmarksIndex.LeftShoulder, PoseLandmarksIndex.RightShoulder,DirectionCheckResult.Up,
                            PoseLandmarksIndex.MidHip, PoseLandmarksIndex.MidShoulder,DirectionCheckResult.Backward,
                                    _minCutoff, _beta, _dCutoff),
            new BoneLandmarkMapping (HumanBodyBones.LeftHand,   
                            PoseLandmarksIndex.LeftWrist, PoseLandmarksIndex.LeftMidPinkyToIndex,DirectionCheckResult.Right,
                            PoseLandmarksIndex.LeftPinky, PoseLandmarksIndex.LeftIndex,DirectionCheckResult.Up,
                            PoseLandmarksIndex.MidHip, PoseLandmarksIndex.MidShoulder,DirectionCheckResult.Backward,
                                    _minCutoff, _beta, _dCutoff),
        };
    }
}
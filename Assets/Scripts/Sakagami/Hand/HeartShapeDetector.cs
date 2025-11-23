/*
 * ファイル
 * HeartShapeDetector C#
 * 
 * システム
 * ハートの形かを判断するscript
 * 
 * 作成
 * 2025/09/08 坂上　壱希
 * 
 * 最終変更
 * 2025/09/29 坂上　壱希
 */
using System.Collections.Generic;
using UnityEngine;

public class HeartShapeDetector : MonoBehaviour
{
    [SerializeField]
    MediapipeResultDataContainer _mediapipeResultDataContainer;

    public enum Heart : byte
    {
        Heart,
        None
    }

    [SerializeField, ReadOnly]
    private Heart _heart = Heart.None;
    public Heart HeartShape => _heart;

    [Tooltip("親指同士が「接触している」とみなす最大距離（単位：ワールド座標距離）。この値以下なら「親指が近接している」と判定する。")]
    [SerializeField]
    private float m_thumbsDistanceThreshold = 0.03f;

    [Tooltip("親指の先端が、手のひら（基準点）より少し低くても許容する高さの誤差（単位：Y軸距離）。この許容範囲を超えて下にある場合は「接触していない」とみなす。")]
    [SerializeField]
    private float m_thumbElevationTolerance = 0.025f;


    private void Update()
    {
        if (_mediapipeResultDataContainer == null)
        {
            return;
        }

        _heart = Heart.None;

        if (_mediapipeResultDataContainer.LeftHandNormalizedLandmarkList == null ||
            _mediapipeResultDataContainer.RightHandNormalizedLandmarkList == null)
        {
            return;
        }


        if (IsHeartShape(_mediapipeResultDataContainer.LeftHandNormalizedLandmarkList, _mediapipeResultDataContainer.RightHandNormalizedLandmarkList))
        {
            _heart = Heart.Heart;
        }
    }


    bool IsHeartShape(IReadOnlyList<Mediapipe.NormalizedLandmark> _hand1, IReadOnlyList<Mediapipe.NormalizedLandmark> _hand2)
    {
        // 指を交互に重ねた場合でもハートになってしまう

        //親指の先同士が近くに無ければ
        float thumbDistance = Vector3.Distance(ToVector3(_hand1[4]), ToVector3(_hand2[4]));
        if (thumbDistance >= m_thumbsDistanceThreshold)
        {
            return false;
        }

        // 親指の先が手のひらより少し上でも許容
        if (!(_hand1[4].Y >= _hand1[2].Y - m_thumbElevationTolerance && _hand2[4].Y >= _hand2[2].Y - m_thumbElevationTolerance))
        {
            return false;
        }

        // 指が丸められているかチェック
        bool fingersBent = AreFingersBent(_hand1) && AreFingersBent(_hand2);
        if (!fingersBent)
        {
            return false;
        }

        // 小指と親指の距離をチェック
        float thumbToPinkyDist1 = Vector2.Distance(
            new Vector2(_hand1[4].X, _hand1[4].Y),
            new Vector2(_hand1[20].X, _hand1[20].Y)
        );
        float thumbToPinkyDist2 = Vector2.Distance(
            new Vector2(_hand2[4].X, _hand2[4].Y),
            new Vector2(_hand2[20].X, _hand2[20].Y)
        );

        if (thumbToPinkyDist1 < 0.03f || thumbToPinkyDist2 < 0.03f)
        {
            return false;
        }

        // 条件を満たす場合、ハートのポーズと判定
        return true;
    }

    // 補助関数：MediaPipeの NormalizedLandmark を Unity の Vector3 に変換
    // MediaPipeの正規化座標 (0?1) をそのまま Vector3 に変換する（座標スケーリングは行わない）
    Vector3 ToVector3(Mediapipe.NormalizedLandmark lm)
    {
        return new Vector3(lm.X, lm.Y, lm.Z);
    }

    /// <summary>
    /// 指先が中間関節より上にあるかどうかを判定して、指が「曲がっている」とみなすかを返す。
    /// 親指を除いた4本指（人差し指?小指）を対象とする。
    /// </summary>
    /// <param name="hand">MediaPipeから取得した正規化済みの手のランドマークリスト</param>
    /// <returns>すべての指が曲がっていれば true、それ以外は false</returns>
    bool AreFingersBent(IReadOnlyList<Mediapipe.NormalizedLandmark> hand)
    {
        float tolerance = 0.02f;  // 指先が少し下にあっても許容する範囲（Y軸方向）

        // 各指の [先端, 中間関節] のインデックス（親指を除く）
        int[][] fingerJoints = new int[][]
        {
        new int[] {8, 6},   // 人差し指: TIP, PIP
        new int[] {12, 10}, // 中指
        new int[] {16, 14}, // 薬指
        new int[] {20, 18}  // 小指
        };

        foreach (var joint in fingerJoints)
        {
            float tipY = hand[joint[0]].Y;
            float jointY = hand[joint[1]].Y;

            // 指先が中間関節より大きく下（＝曲がってない）にある場合は false
            if (tipY < jointY - tolerance)
            {
                return false;
            }
        }

        // すべての指先が中間関節より上（またはほぼ同じ）なら「曲がっている」と判断
        return true;
    }
}

/*
 * ファイル
 * HandShapeDetector C#
 * 
 * システム
 * 手の形を判断するscript
 * 
 * 作成
 * 2025/09/02 坂上　壱希
 * 
 * 最終変更
 * 2025/09/29 坂上　壱希
 */
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HeartShapeDetector))]
[RequireComponent(typeof(BangShapeDetector))]
sealed public class HandShapeDetector : MonoBehaviour
{
    // 手のlandmarkの取得と手の形のセット用
    [SerializeField]
    private MediapipeResultDataContainer m_mediapipeResultDataContainer;

    //private HeartShapeDetector m_heartShapeDetector;    // ハート検出
    private BangShapeDetector m_bangShapeDetector;      // バン検出

    // キャラクターのアニメーションのステート名と合わせるので
    // 変更・追加をした場合はお知らせください（奥山）
    public enum HandShape : byte
    {
        Unknown,
        Rock,
        Scissors,
        Paper,
        Good,
        OK,
        Fuck,
        Heart,
        Bang,
        DoubleBang
    }

    private enum HandOrientation : byte
    {
        Up,
        Down,
        Left,
        Right
    }

    [SerializeField, ReadOnly]
    private HandShape m_rightHandShape; // 右手の形

    [SerializeField, ReadOnly]
    private HandShape m_leftHandShape;  // 左手の形

    private void Awake()
    {
        // コンポーネントの取得
        //m_heartShapeDetector = GetComponent<HeartShapeDetector>();
        m_bangShapeDetector = GetComponent<BangShapeDetector>();
    }

    private void Update()
    {
        if (m_mediapipeResultDataContainer.RightHandNormalizedLandmarkList != null)
        {
            // 右手の形を検出
            m_rightHandShape = GetHandShape(
                m_mediapipeResultDataContainer.RightHandNormalizedLandmarkList,
                "Right"
            );

            // コンテナに情報を伝える
            m_mediapipeResultDataContainer.SetRightHandShape(m_rightHandShape);
        }

        if (m_mediapipeResultDataContainer.LeftHandNormalizedLandmarkList != null)
        {
            // 左手の形を検出
            m_leftHandShape = GetHandShape(
                m_mediapipeResultDataContainer.LeftHandNormalizedLandmarkList,
                "Left"
            );

            // コンテナに情報を伝える
            m_mediapipeResultDataContainer.SetLeftHandShape(m_leftHandShape);
        }
    }

    /// <summary>
    /// 手のランドマークと利き手情報から手の形を推定します。
    /// </summary>
    /// <param name="_hand">手のランドマーク</param>
    /// <param name="_handedness">"Left" または "Right"</param>
    /// <returns>検出された手の形状</returns>
    private HandShape GetHandShape(
        IReadOnlyList<Mediapipe.NormalizedLandmark> _hand,
        string _handedness
    )
    {
        // 第二引数が第一引数よりY座標が大きかったらtrue
        bool IsExtendedY(int _tipIndex, int _pipIndex) =>
            _hand[_tipIndex].Y < _hand[_pipIndex].Y;

        // 第二引数が第一引数よりX座標が大きかったらtrue
        bool IsExtendedX(int _tipIndex, int _pipIndex) =>
            _hand[_tipIndex].X < _hand[_pipIndex].X;

        Vector3 wrist = new Vector3(_hand[0].X, _hand[0].Y, _hand[0].Z);
        Vector3 middlePip = new Vector3(_hand[9].X, _hand[9].Y, _hand[9].Z);

        float deltaX = middlePip.x - wrist.x;
        float deltaY = middlePip.y - wrist.y;

        HandOrientation orientation;

        if (Mathf.Abs(deltaY) > Mathf.Abs(deltaX))
        {
            orientation = deltaY < 0 ? HandOrientation.Up : HandOrientation.Down;
        }
        else
        {
            orientation = deltaX < 0 ? HandOrientation.Left : HandOrientation.Right;
        }

        // 指が開いているかのフラグ
        bool thumb = false;
        bool index = false;
        bool middle = false;
        bool ring = false;
        bool pinky = false;

        // MediaPipeは左右が反転するので名前が逆
        bool isLeftHand = _handedness == "Right";

        //Debug.Log("手の向き：" + orientation);

        // 上下左右で判定
        switch (orientation)
        {
            case HandOrientation.Up:
                // 右手ならX判定を反転させる（MediaPipeの仕様）
                if (isLeftHand)
                {
                    // 表
                    if (
                        IsExtendedX((int)EHandLandmarks.Wrist, (int)EHandLandmarks.ThumbTip)
                        || IsExtendedY((int)EHandLandmarks.Wrist, (int)EHandLandmarks.ThumbTip)
                    )
                    {
                        //Debug.Log(orientation + "表");
                        // ↓ここの条件がおかしい
                        // thumb = isLeftHand ? IsExtendedX(3, 4) || IsExtendedY(3, 4) : IsExtendedX(4, 3);
                        thumb = IsExtendedY((int)EHandLandmarks.ThumbTip, (int)EHandLandmarks.ThumbIP);
                    }
                    else
                    {
                        // 裏
                        //Debug.Log(orientation + "裏");
                        thumb = isLeftHand
                            ? IsExtendedX((int)EHandLandmarks.ThumbTip, (int)EHandLandmarks.ThumbIP)
                            : IsExtendedX((int)EHandLandmarks.ThumbIP, (int)EHandLandmarks.ThumbTip);
                    }
                }

                index = IsExtendedY(8, 6);
                middle = IsExtendedY(12, 10);
                ring = IsExtendedY(16, 14);
                pinky = IsExtendedY(20, 18);
                break;

            case HandOrientation.Down:
                // 表裏の判定
                if (isLeftHand && IsExtendedX(4, 0))
                {
                    // 表
                    //Debug.Log(orientation + "表");
                    thumb = isLeftHand ? IsExtendedX(4, 3) : IsExtendedX(3, 4);
                }
                else
                {
                    // 裏
                    //Debug.Log(orientation + "裏");
                    thumb = isLeftHand ? IsExtendedX(3, 4) : IsExtendedX(4, 3);
                }

                index = IsExtendedY(6, 8);
                middle = IsExtendedY(10, 12);
                ring = IsExtendedY(14, 16);
                pinky = IsExtendedY(18, 20);
                break;

            case HandOrientation.Left:
                // 表裏の判定
                if (isLeftHand && IsExtendedY(4, 0))
                {
                    // 表
                    //Debug.Log(orientation + "表");
                    thumb = isLeftHand ? IsExtendedX(4, 3) : IsExtendedX(3, 4);
                }
                else
                {
                    // 裏
                    //Debug.Log(orientation + "裏");
                    thumb = isLeftHand ? IsExtendedX(3, 4) : IsExtendedX(4, 3);
                }

                index = IsExtendedX(8, 6);
                middle = IsExtendedX(12, 10);
                ring = IsExtendedX(16, 14);
                pinky = IsExtendedX(20, 18);
                break;

            case HandOrientation.Right:
                // 表裏の判定
                if (isLeftHand && IsExtendedY(0, 4))
                {
                    // 表
                    //Debug.Log(orientation + "表");
                    thumb = isLeftHand ? IsExtendedX(3, 4) : IsExtendedX(4, 3);
                }
                else
                {
                    // 裏
                    //Debug.Log(orientation + "裏");
                    thumb = isLeftHand ? IsExtendedX(3, 4) : IsExtendedX(4, 3);
                }

                index = IsExtendedX(6, 8);
                middle = IsExtendedX(10, 12);
                ring = IsExtendedX(14, 16);
                pinky = IsExtendedX(18, 20);
                break;

            default:
                thumb = index = middle = ring = pinky = false;
                break;
        }

        // ---- ここから手の形の判定 ----

        int count = (thumb ? 1 : 0) + (index ? 1 : 0) + (middle ? 1 : 0) + (ring ? 1 : 0) + (pinky ? 1 : 0);

        // 親指が正しく取れないため一旦親指の判定を無視した変数
        int countWithoutThumb = (index ? 1 : 0) + (middle ? 1 : 0) + (ring ? 1 : 0) + (pinky ? 1 : 0);

        // ↓ここから必要に応じてtrueにして使う

#if false
        // Good
        if (thumb && !index && !middle && !ring && !pinky)
        {
            return HandShape.Good;
        }
#endif

#if false
        // OK
        float thumbTipDist = Vector3.Distance(
            new Vector3(_hand[4].X, _hand[4].Y, _hand[4].Z),
            new Vector3(_hand[8].X, _hand[8].Y, _hand[8].Z)
        );

        bool fingersExtended = middle && ring && pinky;

        if (thumbTipDist < 0.05f && fingersExtended)
        {
            return HandShape.OK;
        }
#endif

#if false
        if (m_heartShapeDetector.HeartShape == HeartShapeDetector.Heart.Heart)
        {
            return HandShape.Heart;
        }
#endif

#if false
        if (m_bangShapeDetector.RightBangShape == BangShapeDetector.Bang.DoubleBang)
        {
            return HandShape.DoubleBang;
        }
#endif

        // グーチョキパー判定
#if true
        // グー（本来なら親指で判定したい）
        if (countWithoutThumb <= 0)
        {
            return HandShape.Rock;
        }
#endif

#if true
        // チョキ
        if (index && middle && !ring && !pinky)
        {
            return HandShape.Scissors;
        }
#endif

#if true
        // パー
        if (countWithoutThumb >= 4)
        {
            return HandShape.Paper;
        }
#endif

        // バンポーズ
        if (m_bangShapeDetector.RightBangShape == BangShapeDetector.Bang.Bang)
        {
            return HandShape.Bang;
        }

        // ネタ枠（現在無効化中）
        // if (!index && middle && !ring && !pinky)
        // {
        //     return HandShape.Fuck;
        // }

        // それ以外は未判定扱い
        return HandShape.Unknown;
    }
}

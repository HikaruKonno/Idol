/*
 * ファイル
 * BangShapeDetector C#
 * 
 * システム
 * バンの形かを判断するscript
 * 
 * 作成
 * 2025/09/12 坂上　壱希
 * 
 * 最終変更
 * 2025/09/29 坂上　壱希
 */
using System.Collections.Generic;
using UnityEngine;

sealed public class BangShapeDetector : MonoBehaviour
{
    [SerializeField]
    private MediapipeResultDataContainer m_mediapipeResultDataContainer;
    [SerializeField]
    private HandShapeDetector _handShapeDetector;

    public enum Bang : byte
    {
        DoubleBang,
        Bang,
        None
    }

    public enum HandSide : byte
    {
        Left,
        Right
    }

    [Header("Bang 判定結果")]
    public Bang RightBangShape => m_rightBangShape;
    [SerializeField, Tooltip("左手の形"), ReadOnly, BigField]
    private Bang m_rightBangShape = Bang.None;

    public Bang LeftBangShape => m_leftBangShape;
    [Space]
    [SerializeField, Tooltip("右手の形"), ReadOnly, BigField]
    private Bang m_leftBangShape = Bang.None;

    [Header("判定パラメータ")]
    [SerializeField, Tooltip("両手の人差し指の先端が近接していると判定する最大距離")]
    private float m_doubleBangDistanceThreshold = 0.05f;

    private void Update()
    {
        m_rightBangShape = Bang.None;
        m_leftBangShape = Bang.None;

        if (m_mediapipeResultDataContainer == null)
        {
            return;
        }

        var rightHand = m_mediapipeResultDataContainer.RightHandNormalizedLandmarkList;
        var leftHand = m_mediapipeResultDataContainer.LeftHandNormalizedLandmarkList;

        if (rightHand != null && IsGunSign(rightHand))
        {
            m_rightBangShape = Bang.Bang;
        }

        if (leftHand != null && IsGunSign(leftHand))
        {
            m_leftBangShape = Bang.Bang;
        }

        if (m_rightBangShape == Bang.Bang && m_leftBangShape == Bang.Bang)
        {
            if (DoubleGunSign(rightHand, leftHand))
            {
                m_rightBangShape = Bang.DoubleBang;
                m_leftBangShape = Bang.DoubleBang;
            }
        }
    }

    private bool DoubleGunSign(IReadOnlyList<Mediapipe.NormalizedLandmark> _hand1, IReadOnlyList<Mediapipe.NormalizedLandmark> _hand2)
    {
        if (_hand1 == null || _hand2 == null)
        {
            //Debug.Log("DoubleGunSign: 片方の手が null");
            return false;
        }

        //指先の座標が近いかどうか
        float indexTipDistance = Vector3.Distance(
            LandmarkConverter.ToVector3(_hand1[(int)EHandLandmarks.IndexTip]),
            LandmarkConverter.ToVector3(_hand2[(int)EHandLandmarks.IndexTip])
            );

        if (indexTipDistance > m_doubleBangDistanceThreshold)
        {
            //Debug.Log($"DoubleGunSign: 指先距離が遠い ({indexTipDistance})");
            return false;
        }

        //両手がBangかどうか
        if (m_leftBangShape != Bang.Bang && m_rightBangShape != Bang.Bang)
        {
            return false;
        }

        return true;
    }

    private bool IsGunSign(IReadOnlyList<Mediapipe.NormalizedLandmark> _hand)
    {
        if (_hand == null || _hand.Count < 21)
        {
            //Debug.Log("IsGunSign: Hand data is null or insufficient.");
            return false;
        }


        if (_hand[(int)EHandLandmarks.ThumbTip].Y > _hand[(int)EHandLandmarks.ThumbMCP].Y)
        {
            //Debug.Log($"{handSide}: 親指が立っていない (Y座標: {hand[4].Y} > {hand[2].Y})");
            return false;
        }

        //指が奥に向かっているか
        if (_hand[(int)EHandLandmarks.IndexMCP].Z < _hand[(int)EHandLandmarks.IndexTip].Z)
        {
            return false;
        }

        //Debug.Log($"{handSide}: Bang ポーズ検出成功！");
        return true;
    }
}

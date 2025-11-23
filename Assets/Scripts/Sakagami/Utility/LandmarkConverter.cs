/*
 * ファイル
 * LandmarkConverter C#
 * 
 * システム
 * Landmark系を別の型にコンバートする
 * 
 * 作成
 * 2025/09/10 坂上　壱希
 * 
 * 最終変更
 * 2025/09/29 坂上　壱希
 */
using Mediapipe.Tasks.Components.Containers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class LandmarkConverter
{
    // -------------------------------
    // Tasks → Proto 変換
    // -------------------------------

    public static Mediapipe.NormalizedLandmark ConvertToProto(NormalizedLandmark _lm)
    {
        var proto = new Mediapipe.NormalizedLandmark
        {
            X = _lm.x,
            Y = _lm.y,
            Z = _lm.z,
        };

        if (_lm.visibility.HasValue)
        {
            proto.Visibility = _lm.visibility.Value;
        }

        if (_lm.presence.HasValue)
        {
            proto.Presence = _lm.presence.Value;
        }

        return proto;
    }

    public static List<Mediapipe.NormalizedLandmark> ConvertToProtoList(in List<NormalizedLandmark> _list)
    {
        return _list.Select(ConvertToProto).ToList();
    }

    // -------------------------------
    // Proto → Tasks 変換
    // -------------------------------

    public static NormalizedLandmark ConvertToTask(Mediapipe.NormalizedLandmark _proto)
    {
        return NormalizedLandmark.CreateFrom(_proto);
    }

    public static List<NormalizedLandmark> ConvertToTaskList(in List<Mediapipe.NormalizedLandmark> _list)
    {
        return _list.Select(ConvertToTask).ToList();
    }

    /// <summary>
    /// Mediapipe.NormalizedLandmarkをVector3に変換
    /// </summary>
    /// <param name="_lm">Mediapipe.NormalizedLandmark</param>
    /// <returns>UnityEngine.Vector3</returns>
    public static Vector3 ToVector3(in Mediapipe.NormalizedLandmark _lm)
    {
        return new Vector3(_lm.X, _lm.Y, _lm.Z);
    }
}

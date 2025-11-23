/*
 * ファイル
 * MediaPipeUtils C#
 * 
 * システム
 * ランドマークのUnity上への座標変換など、Mediapipeを使用する上で頻繁に使用する関数をまとめたもの
 * 
 * 変更履歴
 * 2025/07/23　奥山　凜　作成
 * 2025/07/23　奥山　凜　WaitImageSourceReady追加
 * 2025/07/23　奥山　凜　ConvertToViewportPos追加
 */

using Mediapipe.Unity;
using Mediapipe.Unity.Sample;
using System.Collections;
using UnityEngine;

/// <summary>
/// ランドマークのUnity上への座標変換など、Mediapipeを使用する上で頻繁に使用する関数をまとめたクラス
/// </summary>
public static class MediaPipeUtils
{

    /// <summary>
    /// LandmarkをUnityの座標系に変換し、Victor3として返す<br/>
    /// 引数1：_landmark    変換するランドマーク<br/>
    /// 引数2：_imageHeight カメラの画像の縦側の比率<br/>
    /// 引数3：_imageWidth  カメラの画像の横側の比率<br/>
    /// 引数4：_isMirrored  左右反転をするか<br/>
    /// 引数5：_xScale　　  ｘを補正する値（カメラ画像を変換する影響でｘ軸方向は曖昧なため）<br/>
    /// </summary>
    /// <param name="_landmark">変換するランドマーク</param>
    /// <param name="_imageHeight">カメラの画像の縦側の比率</param>
    /// <param name="_imageWidth">カメラの画像の横側の比率</param>
    /// <param name="_isMirrored">左右反転をするか</param>
    /// <param name="_xScale">ｘを補正する値（カメラ画像を変換する影響でｘ軸方向は曖昧なため）</param>
    /// <returns>Unityの座標系に変換したランドマーク</returns>
    public static Vector3 ConvertToUnityPos(Mediapipe.NormalizedLandmark _landmark, int _imageHeight, int _imageWidth, bool _isMirrored = false, float _xScale = 1.0f)
    {
        Vector3 vector3 = Vector3.zero;
        // 0.5fを基準にすることで、画面中央が原点(0,0,0)になる
        vector3.x = (0.5f - _landmark.X) * (float)_imageWidth;
        vector3.y = (0.5f - _landmark.Y) * (float)_imageHeight; // UnityのY軸は上向きなので反転
        vector3.z = (_landmark.Z - 0.5f) * (float)_imageWidth * _xScale;

        if (_isMirrored == true)
        {
            vector3.x = -vector3.x;
        }

        return vector3;
    }

    /// <summary>
    /// LandmarkをUnityのビュー座標系に変換し、Victor2として返す<br/>
    /// 引数1：_landmark 変換するランドマーク<br/>
    /// </summary>
    /// <param name="_landmark">変換するランドマーク</param>
    /// <returns>Landmarkのビュー座標</returns>
    public static Vector2 ConvertToViewportPos(Mediapipe.NormalizedLandmark _landmark)
    {
        Vector2 vector2;
        // メディアパイプのランドマークは画像外も推測してランドマークを出すため画面外のランドマークが1以上や0以下になる
        vector2.x = 1 - Mathf.Clamp(_landmark.X, 0.0f, 1.0f);        
        vector2.y = 1 - Mathf.Clamp(_landmark.Y, 0.0f, 1.0f);


        return vector2;
    }

    /// <summary>
    /// ランドマーク内の信頼性に関する値を確認し、信頼性が両方の閾値を超えるかどうかをboolで返す<br/>
    /// 引数1：_landmark 信頼性を確認するランドマーク<br/>
    /// 引数2：_presenceThreshold 存在確率の閾値<br/>
    /// （存在確率 ＝ ランドマークがシーン内に存在しているかどうかの推定スコア（信頼度が低い0.0f～1.0f信頼度が高い））<br/>
    /// 引数3：_visibilityThreshold 可視性の閾値　<br/>
    /// （可視性　 ＝ ランドマークが他の物体に隠されているかどうかの確率スコア（信頼度が低い0.0f～1.0f信頼度が高い））<br/>
    /// </summary>
    /// <param name="_landmark">信頼性を確認するランドマーク</param>
    /// <param name="_presenceThreshold">存在確率の閾値<br/>（存在確率 ＝ ランドマークがシーン内に存在しているかどうかの推定スコア（信頼度が低い0.0f～1.0f信頼度が高い））</param>
    /// <param name="_visibilityThreshold">可視性の閾値<br/>（可視性　 ＝ ランドマークが他の物体に隠されているかどうかの確率スコア（信頼度が低い0.0f～1.0f信頼度が高い））</param>
    /// <returns>信頼性が閾値を超えたか</returns>
    public static bool IsLandmarkReliable(Mediapipe.NormalizedLandmark _landmark, float _presenceThreshold = 0.001f, float _visibilityThreshold = 0.001f)
    {
        return (_landmark.Presence >= _presenceThreshold) && (_landmark.Visibility >= _visibilityThreshold);
    }

    /// <summary>
    /// カメラから受け取る画像サイズの横を返す
    /// </summary>
    /// <returns>画像サイズ（横）<br/>画像がない場合0が返る</returns>
    public static int GetImageWidth()
    {
        if ((ImageSourceProvider.ImageSource is ImageSource imageSource) && !(imageSource == null))
        {
            return ImageSourceProvider.ImageSource.textureWidth;
        }
        return 0;
    }

    /// <summary>
    /// カメラから受け取る画像サイズの縦を返す
    /// </summary>
    /// <returns>画像サイズ（縦）<br/>画像がない場合0が返る</returns>
    public static int GetImageHeight()
    {
        if ((ImageSourceProvider.ImageSource is ImageSource imageSource) && !(imageSource == null))
        {
            return ImageSourceProvider.ImageSource.textureHeight;
        }
        return 0;
    }


    /// <summary>
    /// MediaPipeのImageSourceが設定されるのを待つ
    /// </summary>
    /// <returns>なし</returns>
    public static IEnumerator WaitImageSourceReady()
    {
        // MediaPipeの初期化が終わるまで待つ（幅が 0 のままの状態を回避）
        yield return new WaitUntil(() =>
            ImageSourceProvider.ImageSource != null &&
            ImageSourceProvider.ImageSource.textureWidth > 0 &&
            ImageSourceProvider.ImageSource.textureHeight > 0
        );
    }
}
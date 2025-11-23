/*
 * ファイル
 * CreateAnimToSpline C#
 * 
 * システム
 * アイドルのアニメーションから動的にノーツを生成するシステム
 * アイドルの手の座標をスクリーン座標に変換してその座標からスプラインを生成する
 * アニメーションのキーフレームのフレーム時間を参考にスプラインを生成する
 * スプラインからノーツ用のメッシュと判定を生成してアセットとして保存する
 * 
 * 作成
 * 2025/09/02 寺門 冴羽
 * 
 * 最終変更
 * 2025/09/04 寺門 冴羽
 */

//using System.Collections.Generic;
//using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
//using UnityEngine.Serialization;
//using UnityEngine.Splines;

public class CreateAnimToSpline : MonoBehaviour
{
    /* ツールにしたい人生だった
    [MenuItem("Tools/CreateAnimToSpline")]
    static public void AttachCreateAnimToSplineSystem()
    {
        // 選択したオブジェクト
        var obj = Selection.activeObject;

        // 選択したオブジェクトがカメラだったとき
        if (obj as Camera)
        {
            // AnimationClipオブジェクト
            var animationClip = obj as AnimationClip;
            Debug.Log("==========CreateAnimToSpline=====");
            Debug.Log(animationClip.name);
            
            // スクリプトをアタッチ
            
        }
        else
        {
            // 選択したオブジェクトが指定のカメラではなかったとき
            Debug.Log("UICameraじゃないよ");
        }
    }
    */

    /*
   [SerializeField] AnimationClip m_fromAnimationClip;

    float[] GetAnimationFlame(AnimationClip _fromAnimationClip)
    {
        // CreateAnimToSplineの開始ログ
        Debug.Log("==========CreateAnimToSpline==========");
        Debug.Log(_fromAnimationClip.name);

        float[] result;

        // アニメーションカーブの配列
        EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(_fromAnimationClip as AnimationClip);

        foreach (EditorCurveBinding binding in curveBindings)
        {
            // アニメーションカーブのログ
            //Debug.Log("----------------------");
            //Debug.Log(binding.propertyName);

            // アニメーションカーブ
            AnimationCurve curve = AnimationUtility.GetEditorCurve(_fromAnimationClip, binding);
            result = new float[curve.keys.Length];

            // 各キーフレームの情報
            for (var i = 0; i < curve.keys.Length; i++)
            {
                Keyframe key = curve.keys[i];
                //Debug.Log(key.time + ", " + key.value);

                // キーフレームの時間を取得

                result[i] = key.time;
            }
            return result;
        }
    }
    */
}
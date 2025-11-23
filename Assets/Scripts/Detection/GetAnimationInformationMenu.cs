/*
 * ファイル
 * GetAnimationInformationMenu C#
 * 
 * システム
 * 選択したアニメーションの情報を取得してデバッグログで表示するツール
 * 
 * 作成
 * 2025/09/02 寺門 冴羽
 * 
 * 最終変更
 * 2025/09/04 寺門 冴羽
 */

using UnityEditor;
using UnityEngine;
# if UNITY_EDITOR
public class GetAnimationInformationMenu
{
    [MenuItem("Tools/GetAnimationInformation")]
    static public void GetAnimationInformation()
    {
        // 選択したオブジェクト
        var obj = Selection.activeObject;

        // 選択したオブジェクトがAnimationClipだったとき
        if (obj as AnimationClip)
        {
            // AnimationClipオブジェクト
            var animationClip = obj as AnimationClip;
            Debug.Log("==========GetAnimationClip==========");
            Debug.Log("アニメーションの名前：" + animationClip.name);

            // アニメーションカーブの配列
            EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(animationClip as AnimationClip);

            foreach (EditorCurveBinding binding in curveBindings)
            {
                Debug.Log("--------------------");
                Debug.Log("プロパティの名前：" + binding.propertyName);

                // アニメーションカーブ
                AnimationCurve curve = AnimationUtility.GetEditorCurve(animationClip, binding);

                // 各キーフレームの情報
                for (var i = 0; i < curve.keys.Length; i++)
                {
                    Keyframe key = curve.keys[i];
                    Debug.Log("キーフレームの時間：" + key.time + " , " + "キーフレームの値：" + key.value);
                }
            }
        }
        else
        {
            // 選択したオブジェクトがAnimationClipではなかったとき
            Debug.Log("AnimationClipじゃないよ");
        }
    }
}
#endif
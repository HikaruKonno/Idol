using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
//using static Mediapipe.VideoPreStreamCalculatorOptions.Types;
# if UNITY_EDITOR
public class KeyReductionProcessor : EditorWindow
{
    public float eps = 0.0001f;

    public AnimationClip anim_clip;

    [MenuItem("Tools/KeyReductionTool")]
    public static void ShowWindow()
    {
        GetWindow<KeyReductionProcessor>("KeyReductionTool");
    }

    private void OnGUI()
    {
        eps = EditorGUILayout.DelayedFloatField("しきい値の設定", eps);
        EditorGUILayout.LabelField("0.01以下がよさそう");
        EditorGUILayout.LabelField(" ");
        anim_clip = EditorGUILayout.ObjectField("AnimationClip", anim_clip, typeof(AnimationClip), true) as AnimationClip;
        
        if (GUILayout.Button("KeyReduction"))
        {
            if(anim_clip != null)
            {
                KeyReduction(anim_clip, eps);
            }
            else
            {
                Debug.LogError("KeyReductionTool：アニメーションクリップを設定してください");
            }
        }
    }

    static void KeyReduction(AnimationClip _anim_clip, float _eps)
    {
        Debug.Log("Key Reduction...");
        var path = AssetDatabase.GetAssetPath(_anim_clip);
        foreach (var binding in AnimationUtility.GetCurveBindings(_anim_clip).ToArray())
        {
            // AnimationClipよりAnimationCurveを取得
            AnimationCurve curve = AnimationUtility.GetEditorCurve(_anim_clip, binding);
            // キーリダクションを行う
            AnimationCurveKeyReduction(curve,_eps);
            // AnimationClipにキーリダクションを行ったAnimationCurveを設定
            AnimationUtility.SetEditorCurve(_anim_clip, binding, curve);
        }
        // AnimationClip名の作成
        string anim_clip_name = Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path);
        // AnimationClipファイルの書き出し
        WriteAnimationCurve(_anim_clip, anim_clip_name);
        Debug.Log("KeyReduction Sucess");
    }

    // AnimationClipファイルの書き出し
    static private void WriteAnimationCurve(AnimationClip _anim_clip, string anim_clip_name)
    {
        string tmp_name = anim_clip_name + "_tmp.anim"; // テンポラリファイル名
        // AnimationClipのコピーを作成
        var copyClip = Object.Instantiate(_anim_clip);
        // テンポラリAnimationClipファイルの作成
        AssetDatabase.CreateAsset(copyClip, tmp_name);
        // テンポラリファイルから移し替え
        FileUtil.ReplaceFile(tmp_name, anim_clip_name + ".anim"); // コピー先ファイルがなければ自動で生成される。
        // テンポラリAnimationClipファイルの削除
        AssetDatabase.DeleteAsset(tmp_name);
        // データベースの更新
        AssetDatabase.Refresh();
    }

    // ２つのキーから、指定した時間の値を取得する
    static private float GetValueFromTime(Keyframe key1, Keyframe key2, float time)
    {
        float t;
        float a, b, c;
        float kd, vd;

        if (key1.outTangent == Mathf.Infinity) return key1.value; // コンスタント値

        kd = key2.time - key1.time;
        vd = key2.value - key1.value;
        t = (time - key1.time) / kd;

        a = -2 * vd + kd * (key1.outTangent + key2.inTangent);
        b = 3 * vd - kd * (2 * key1.outTangent + key2.inTangent);
        c = kd * key1.outTangent;

        return key1.value + t * (t * (a * t + b) + c);
    }

    // 指定したキーの値はKey1とKey2から得られた補間値と同じ値であるかを調べる
    static private bool IsInterpolationValue(Keyframe key1, Keyframe key2, Keyframe comp, float _eps)
    {
        // 調査するキーのある位置
        var val1 = GetValueFromTime(key1, key2, comp.time);

        // 元の値からの差分の絶対値がしきい値以下であるか？
        if (_eps < System.Math.Abs(comp.value - val1)) return false;

        // key1からcompの間
        var time = key1.time + (comp.time - key1.time) * 0.5f;
        val1 = GetValueFromTime(key1, comp, time);
        var val2 = GetValueFromTime(key1, key2, time);

        // 差分の絶対値がしきい値以下であるか？
        return (System.Math.Abs(val2 - val1) <= _eps) ? true : false;
    }

    // 削除するインデックスリストの取得する。keysは３つ以上の配列
    static public IEnumerable<int> GetDeleteKeyIndex(Keyframe[] keys, float _eps)
    {
        for (int s_idx = 0, i = 1; i < keys.Length - 1; i++)
        {
            // 前後のキーから補間した値と、カレントのキーの値を比較
            if (IsInterpolationValue(keys[s_idx], keys[i + 1], keys[i], _eps))
            {
                yield return i; // 削除するインデックスを追加
            }
            else
            {
                s_idx = i; // 次の先頭インデックスに設定
            }
        }
    }

    // 入力されたAnimationCurveのキーリダクションを行う
    static public void AnimationCurveKeyReduction(AnimationCurve in_curve, float _eps)
    {
        if (in_curve.keys.Length <= 2) return; // Reductionの必要なし

        // 削除インデックスリストの取得
        var del_indexes = GetDeleteKeyIndex(in_curve.keys, _eps).ToArray();

        // 不要なキーを削除する
        foreach (var del_idx in del_indexes.Reverse()) in_curve.RemoveKey(del_idx);
    }
}
#endif
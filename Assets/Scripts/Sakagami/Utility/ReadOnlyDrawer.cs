#if UNITY_EDITOR
/*
 * ファイル
 * ReadOnlyDrawer C#
 * 
 * システム
 * インスペクター上で読み取り専用(編集不可)にする
 * 
 * 作成
 * 2025/09/11 坂上　壱希
 * 
 * 最終変更
 * 2025/09/29　坂上　壱希
 */
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
sealed public class ReadOnlyDrawer : PropertyDrawer
{
    /// <summary>
    /// フィールドの描画処理。GUI を一時的に無効にして編集不可にする。
    /// </summary>
    /// <param name="_position">描画位置</param>
    /// <param name="_property">対象のプロパティ</param>
    /// <param name="_label">フィールドのラベル</param>
    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
    {
        // フィールドの編集を無効化
        GUI.enabled = false;

        // 通常のフィールド描画
        EditorGUI.PropertyField(_position, _property, _label, true);

        // GUIの状態を元に戻す
        GUI.enabled = true;
    }
}
#endif
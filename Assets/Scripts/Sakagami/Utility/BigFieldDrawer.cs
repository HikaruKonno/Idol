#if UNITY_EDITOR
/*
 * ファイル
 * BigFieldDrawer C#
 * 
 * システム
 * 
 * 
 * 作成
 * 2025/09/12 坂上　壱希
 * 
 * 最終変更
 * 2025/09/29 坂上　壱希
 */
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(BigFieldAttribute))]
sealed public class BigFieldDrawer : PropertyDrawer
{
    /// <summary>
    /// フィールドの描画処理
    /// </summary>
    /// <param name="_position">描画位置</param>
    /// <param name="_property">対象のプロパティ</param>
    /// <param name="_label">ラベル</param>
    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
    {
        var big = (BigFieldAttribute)attribute;

        var labelStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = big.FontSize,
            fontStyle = FontStyle.Bold,
        };

        var fieldStyle = new GUIStyle(EditorStyles.textField)
        {
            fontSize = big.FontSize,
        };

        EditorGUI.BeginProperty(_position, _label, _property);

        // ラベル幅を取得
        float labelWidth = EditorGUIUtility.labelWidth;

        // ラベルの表示
        Rect labelRect = new Rect(_position.x, _position.y, labelWidth, _position.height);
        EditorGUI.LabelField(labelRect, _label, labelStyle);

        // フィールドの表示位置を計算
        Rect fieldRect = new Rect(_position.x + labelWidth, _position.y, _position.width - labelWidth, _position.height);

        // 値の種類によって描画方法を分ける
        switch (_property.propertyType)
        {
            case SerializedPropertyType.Integer:
                _property.intValue = EditorGUI.IntField(fieldRect, _property.intValue, fieldStyle);
                break;

            case SerializedPropertyType.Float:
                _property.floatValue = EditorGUI.FloatField(fieldRect, _property.floatValue, fieldStyle);
                break;

            case SerializedPropertyType.String:
                _property.stringValue = EditorGUI.TextField(fieldRect, _property.stringValue, fieldStyle);
                break;

            case SerializedPropertyType.Boolean:
                _property.boolValue = EditorGUI.Toggle(fieldRect, _property.boolValue);
                break;

            default:
                EditorGUI.PropertyField(fieldRect, _property, GUIContent.none);
                break;
        }

        EditorGUI.EndProperty();
    }

    /// <summary>
    /// プロパティの高さを取得する
    /// </summary>
    /// <param name="_property">対象のプロパティ</param>
    /// <param name="_label">ラベル</param>
    /// <returns>描画高さ</returns>
    public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label)
    {
        BigFieldAttribute big = (BigFieldAttribute)attribute;
        // 高さ調整：デフォルト高さ＋フォントサイズの差分
        return EditorGUIUtility.singleLineHeight + (big.FontSize - 12);
    }
}
#endif

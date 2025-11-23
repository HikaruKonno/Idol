/*
 * ファイル
 * AudienceLightSystemEditor　C#
 * 
 * システム
 * AudienceLightSystemクラスのインスペクターでの表示を行う
 * 通常だとAudienceInfoに使用しているfloat2やint2が、インスペクターで配列の要素数を追加した際に初期値を設定できないため作成
 * 
 * AudienceLightSystemのインスペクターに表示される変数の名称変更や変数の追加に応じて追加対応させる必要あり
 * 
 * 変更履歴
 * 2025/10/10　奥山　凜　作成
 */

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AudienceLightSystem))]
/// <summary>
/// 観客の個体毎のTransform（ペンライトのアニメーション含む）とペンライトの色計算をする
/// </summary>
public class AudienceLightSystemEditor : Editor
{
    // インスペクターに表示したい各プロパティを保持する変数
    private SerializedProperty m_penlightMeshProp;
    private SerializedProperty m_penlightMaterialProp;
    
    private SerializedProperty m_humanMeshProp;
    private SerializedProperty m_humanMaterialProp;
    private SerializedProperty m_shoulderOffsetProp;

    private SerializedProperty m_blocksProp;

    /// <summary>
    /// スクリプト内の変数名（文字列）を使って、対応するプロパティを探して取得
    /// </summary>
    private void OnEnable()
    {
        m_penlightMeshProp = serializedObject.FindProperty("m_penlightMesh");
        m_penlightMaterialProp = serializedObject.FindProperty("m_penlightMaterial");
        m_humanMeshProp = serializedObject.FindProperty("m_humanMesh");
        m_humanMaterialProp = serializedObject.FindProperty("m_humanMaterial");
        m_shoulderOffsetProp = serializedObject.FindProperty("m_shoulderOffset"); 
        m_blocksProp = serializedObject.FindProperty("m_audienceBlocks");
    }

    /// <summary>
    /// インスペクターを毎フレーム描画するメインの関数 
    /// </summary>
    /// <returns>なし</returns>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // メッシュとマテリアルのフィールドを描画
        EditorGUILayout.PropertyField(m_penlightMeshProp);
        EditorGUILayout.PropertyField(m_penlightMaterialProp);
        EditorGUILayout.PropertyField(m_humanMeshProp);
        EditorGUILayout.PropertyField(m_humanMaterialProp);
        EditorGUILayout.PropertyField(m_shoulderOffsetProp);

        // 配列のカスタム処理
        int previousSize = m_blocksProp.arraySize;
        EditorGUILayout.PropertyField(m_blocksProp, true); // 配列を描画

        if (m_blocksProp.arraySize > previousSize) // 配列のサイズが増えたら
        {
            // デフォルト値を作成して新しい要素に設定
            var defaultBlock = AudienceLightSystem.AudienceBlock.CreateDefaultAudienceBlock();
            string json = JsonUtility.ToJson(defaultBlock);
            SerializedProperty newElement = m_blocksProp.GetArrayElementAtIndex(m_blocksProp.arraySize - 1);
            EditorJsonUtility.FromJsonOverwrite(json, newElement);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
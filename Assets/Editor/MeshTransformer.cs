#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class MeshTransformer : EditorWindow
{
    private Vector3 positionOffset = Vector3.zero;
    private Vector3 eulerRotation = Vector3.zero;
    private Vector3 scale = Vector3.one; // デフォルトは等倍
    private bool pivotOnBoundsCenter = true; // デフォルトで中心を基点にする

    private Mesh selectedMesh;

    [MenuItem("Assets/Transform Mesh...", false, 20)]
    public static void ShowWindow()
    {
        GetWindow<MeshTransformer>("Mesh Transformer");
    }

    void OnGUI()
    {
        GUILayout.Label("1. Projectウィンドウでメッシュを選択", EditorStyles.boldLabel);

        selectedMesh = Selection.activeObject as Mesh;

        if (selectedMesh != null)
        {
            EditorGUILayout.LabelField("選択中のメッシュ:", selectedMesh.name);
            EditorGUILayout.LabelField("Bounds Center:", selectedMesh.bounds.center.ToString());
        }
        else
        {
            EditorGUILayout.HelpBox("メッシュアセットが選択されていません。", MessageType.Warning);
        }

        GUILayout.Space(10);

        // --- トランスフォーム設定 ---
        GUILayout.Label("2. 適用するトランスフォームを入力", EditorStyles.boldLabel);
        positionOffset = EditorGUILayout.Vector3Field("Position Offset", positionOffset);
        eulerRotation = EditorGUILayout.Vector3Field("Rotation", eulerRotation);
        scale = EditorGUILayout.Vector3Field("Scale", scale);

        GUILayout.Space(10);

        // --- 基点の設定 ---
        GUILayout.Label("3. 基点（Pivot）の設定", EditorStyles.boldLabel);
        pivotOnBoundsCenter = EditorGUILayout.Toggle("Pivot around Bounds Center", pivotOnBoundsCenter);

        GUILayout.Space(20);

        GUI.enabled = selectedMesh != null;

        if (GUILayout.Button("トランスフォームを適用して保存"))
        {
            ApplyAndSave();
        }

        GUI.enabled = true;
    }

    private void ApplyAndSave()
    {
        // --- 1. 設定値の準備 ---
        Quaternion rotation = Quaternion.Euler(eulerRotation);
        // Toggleの状態に応じて基点を設定
        Vector3 pivot = pivotOnBoundsCenter ? selectedMesh.bounds.center : Vector3.zero;

        // --- 2. 新しいメッシュの準備 ---
        Mesh newMesh = new Mesh();
        newMesh.name = $"{selectedMesh.name}_transformed";

        Vector3[] vertices = selectedMesh.vertices;
        Vector3[] normals = selectedMesh.normals;
        Vector4[] tangents = selectedMesh.tangents;

        // --- 3. 全頂点・法線・接線に処理を適用 ---
        for (int i = 0; i < vertices.Length; i++)
        {
            // --- 頂点 (Vertex) の処理 ---
            // 3-1. 基点が原点に来るように一度移動
            Vector3 vert = vertices[i] - pivot;
            // 3-2. スケールと回転を適用
            vert = Vector3.Scale(vert, scale); // Scaleを先に適用
            vert = rotation * vert;
            // 3-3. 基点の位置を元に戻し、さらにオフセットを加える
            vertices[i] = vert + pivot + positionOffset;

            // --- 法線 (Normal) の処理 (回転のみ適用) ---
            if (i < normals.Length)
            {
                normals[i] = rotation * normals[i];
            }

            // --- 接線 (Tangent) の処理 (回転のみ適用) ---
            if (i < tangents.Length)
            {
                Vector3 tangentDir = new Vector3(tangents[i].x, tangents[i].y, tangents[i].z);
                tangentDir = rotation * tangentDir;
                tangents[i] = new Vector4(tangentDir.x, tangentDir.y, tangentDir.z, tangents[i].w);
            }
        }

        // --- 4. 処理後のデータを新しいメッシュに設定 ---
        newMesh.vertices = vertices;
        newMesh.normals = normals;
        newMesh.tangents = tangents;

        // その他のデータはコピー
        newMesh.triangles = selectedMesh.triangles;
        newMesh.uv = selectedMesh.uv;
        newMesh.colors = selectedMesh.colors;

        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals(); // 必要に応じて法線を再計算
        newMesh.RecalculateTangents(); // 必要に応じて接線を再計算

        // --- 5. 新しいアセットとして保存 ---
        string originalPath = AssetDatabase.GetAssetPath(selectedMesh);
        string newPath = AssetDatabase.GenerateUniqueAssetPath(originalPath.Replace(".asset", "_transformed.asset"));

        AssetDatabase.CreateAsset(newMesh, newPath);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("成功", $"メッシュのトランスフォームを変更し、保存しました:\n{newPath}", "OK");
        Selection.activeObject = newMesh;
    }
}
#endif
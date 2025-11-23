#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AudienceSpawner))]
public class SpawnEditorCustom : Editor
{
    /// <summary>
    /// Inspectorウィンドウのカスタムを作成する関数
    /// </summary>
    public override void OnInspectorGUI()
    {
        // もともとのInspectorを表示して、ボタンを追加するため
        DrawDefaultInspector();

        // targetには、Inspectorで選択されているオブジェクトが入っている
        // AudienceSpawnerの中の関数や変数にアクセスするためにObject型のtargetをキャスト
        AudienceSpawner spawner = (AudienceSpawner)target;

        // Inspectorのボタンの前にスペースを空ける
        GUILayout.Space(10);

        // ボタンが押されたときの処理
        if (GUILayout.Button("スポナーが観客を生成"))
        {
            // 一回のUndo操作で全観客を消すためにグループ化する
            Undo.IncrementCurrentGroup();

            // Editor上でのUndo操作の名前を設定
            Undo.SetCurrentGroupName("Created Audience");

            // 観客の生成
            spawner.SpwanAudienceGrid();

            // グループ化終了
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        }

        // ボタンが押されたときの処理
        if (GUILayout.Button("スポナーが観客を消去"))
        {
            // 一回のUndo操作で全観客を消すためにグループ化する
            Undo.IncrementCurrentGroup();

            // Editor上でのUndo操作の名前を設定
            Undo.SetCurrentGroupName("deleted Audience");

            // 観客の生成
            spawner.DeleteAudience();

            // グループ化終了
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        }
    }
}
#endif
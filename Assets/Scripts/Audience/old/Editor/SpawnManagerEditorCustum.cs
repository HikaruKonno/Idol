#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AudienceSpawnerManager))]
public class SpawnManagerEditorCustom : Editor
{
    /// <summary>
    /// Inspectorウィンドウのカスタムを作成する関数
    /// </summary>
    public override void OnInspectorGUI()
    {
        // もともとのInspectorを表示して、ボタンを追加するため
        DrawDefaultInspector();

        // targetには、Inspectorで選択されているオブジェクトが入っている
        // AudienceSpawnerManagerの中の関数や変数にアクセスするためにObject型のtargetをキャスト
        AudienceSpawnerManager spawnerManager = (AudienceSpawnerManager)target;

        // ボタンが押されたときの処理
        if (GUILayout.Button("子オブジェクトのSpawnerをListに格納"))
        {
            // 一回のUndo操作で全観客を消すためにグループ化する
            Undo.IncrementCurrentGroup();

            // Editor上でのUndo操作の名前を設定
            Undo.SetCurrentGroupName("Add SpawnerList");

            // 子オブジェクトのスポナーの取得
            spawnerManager.AddAllSpawnerForChild();

            // グループ化終了
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        }

        // ボタンが押されたときの処理
        if (GUILayout.Button("Listのリセット"))
        {
            // 一回のUndo操作で全観客を消すためにグループ化する
            Undo.IncrementCurrentGroup();

            // Editor上でのUndo操作の名前を設定
            Undo.SetCurrentGroupName("Reset SpawnerList");

            // スポナーリストの要素数を0にしてリセットする
            spawnerManager.ResetSpawnerList();

            // グループ化終了
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        }

        // Inspectorのボタンの前にスペースを空ける
        GUILayout.Space(10);

        // ボタンが押されたときの処理
        if (GUILayout.Button("Listに入っているSpawnerから観客を生成"))
        {
            // 一回のUndo操作で全観客を消すためにグループ化する
            Undo.IncrementCurrentGroup();

            // Editor上でのUndo操作の名前を設定
            Undo.SetCurrentGroupName("Created Audience For SpawnManager");

            // 観客の生成
            spawnerManager.SpawnAudienceForList();

            // グループ化終了
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        }

        // ボタンが押されたときの処理
        if (GUILayout.Button("Listに入っているSpawnerから観客を消去"))
        {
            // 一回のUndo操作で全観客を消すためにグループ化する
            Undo.IncrementCurrentGroup();

            // Editor上でのUndo操作の名前を設定
            Undo.SetCurrentGroupName("deleted Audience For SpawnManager");

            // 観客の生成
            spawnerManager.DeleteAudienceForList();

            // グループ化終了
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        }
    }
}
#endif
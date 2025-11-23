using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.IO;    // ファイルパスの操作に必要

// EditorWindowを継承して、独自のウィンドウを作成する
public class TimelineGeneratorWindow : EditorWindow
{
    // ウィンドウに表示する変数
    private PlayableDirector targetDirector;    // 操作したいPlayable Director
    private TextAsset csvFile;                  // 読み込むCSVファイル

    // Unityのメニューバーに「Tools/Timeline Generator」という項目を追加する
    [MenuItem("Tools/Timeline Generator")]
    public static void ShowWindow()
    {
        // ウィンドウを表示する
        GetWindow<TimelineGeneratorWindow>("Timeline Generator");
    }

    // ウィンドウのUIを描画するメソッド
    private void OnGUI()
    {
        GUILayout.Label("Timeline Generator Settings", EditorStyles.boldLabel);

        // 操作対象のPlayable Directorをアタッチする欄
        targetDirector = (PlayableDirector)EditorGUILayout.ObjectField("Target Director", targetDirector, typeof(PlayableDirector), true);

        // 読み込むCSVファイルをアタッチする欄
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);

        // 「Generate Timeline」ボタン。押されたら処理を実行する
        if(GUILayout.Button("Generate Timeline"))
        {
            if(targetDirector != null && csvFile != null)
            {
                // メインの生成処理を呼び出す
                GenerateTimelineFromCSV();
            }
            else
            {
                // エラーメッセージを表示
                Debug.LogError("Target DirectorまたはCSV Fileが設定されていません。");
            }
        }
    }

    // CSVからタイムラインを生成するメイン処理
    private void GenerateTimelineFromCSV()
    {
        // 既存のトラックを全て削除して、まっさらな状態にする
        TimelineAsset timelineAsset = targetDirector.playableAsset as TimelineAsset;
        // トラックは逆順で削除しないとエラーになることがある
        for (int i = timelineAsset.outputTrackCount - 1; i>= 0; i--)
        {
            timelineAsset.DeleteTrack(timelineAsset.GetOutputTrack(i));
        }

        // CSVファイルの内容を文字列として読み込み、改行で分割して各行を取得
        string[] lines = csvFile.text.Split('\n');

        // CSVの各行をループで処理（1行目はヘッダーなのでスキップ）
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();  // 前後の余白を削除
            if (string.IsNullOrEmpty(line)) continue;   // 空の行は無視

            // 行をカンマで分割して、各セルのデータを取得
            string[] values = line.Split(',');

            // CSVの列に対応
            string actionType = values[0];
            string targetObjectName = values[1];
            float startTime = float.Parse(values[2]);
            float duration  = float.Parse(values[3]);
            string option1 = values.Length > 4 ? values[4] : "";    // 5列目がない場合もあるのでチェック

            // シーン内から操作対象のGameObjectを探す
            GameObject targetObject = GameObject.Find(targetObjectName);
            if (targetObjectName == null) 
            {
                Debug.LogWarning($"オブジェクトが見つかりません： { targetObjectName }");
                continue;   // 次の行へ
            }

            // ActionTypeに応じて処理を分岐
            switch (actionType)
            {
                case "ACTIVE":
                    AddActivationClip(timelineAsset, targetObject, startTime, duration);
                    break;

                case "ANIMATE":
                    // AnimationClipをプロジェクト内から名前で探す
                    AnimationClip clip = FindAnimationClipByName(option1);
                    if (clip != null)
                    {
                        AddAnimationClip(timelineAsset, targetObject, clip, startTime);
                    }
                    else
                    {
                        Debug.LogWarning($"アニメーションクリップが見つかりません： {option1}");
                    }
                    break;
            }
        }

        Debug.Log("タイムラインの生成が完了しました");
    }

    // Activationクリップを追加する処理
    private void AddActivationClip(TimelineAsset timeline, GameObject obj, float start, float dur)
    {
        ActivationTrack track = timeline.CreateTrack<ActivationTrack>(null, obj.name);
        targetDirector.SetGenericBinding(track, obj);
        TimelineClip clip = track.CreateDefaultClip();
        clip.start = start;
        clip.duration = dur;
    }

    // Animationクリップを追加する処理
    private void AddAnimationClip(TimelineAsset timeline, GameObject obj, AnimationClip animClip, float start)
    {
        // Animatorがなければ追加
        Animator animator = obj.GetComponent<Animator>();
        if (animator == null) animator = obj.AddComponent<Animator>();

        AnimationTrack track = timeline.CreateTrack<AnimationTrack>(null, obj.name);
        targetDirector.SetGenericBinding(track, animator);
        TimelineClip clip = track.CreateClip(animClip);
        clip.start = start;
        // AnimationClipの長さをクリップの長さに自動で設定
        clip.duration = animClip.length;
    }

    // プロジェクト内のAnimationClipを名前で探すヘルパー関数
    private AnimationClip FindAnimationClipByName(string name)
    {
        // AssetDatabase.FindAssetsは"t:AnimationClip"でクリップだけを検索できる
        string[] guids = AssetDatabase.FindAssets(name + "t:AnimationClip");
        if (guids.Length == 0) return null;

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

    }
}

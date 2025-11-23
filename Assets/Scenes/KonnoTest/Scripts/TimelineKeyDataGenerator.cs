// Assets/Editor/TimelineKeyDataGenerator.cs
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelineKeyDataGenerator
{
    [MenuItem("Tools/Timeline/Generate RecordedKeyData")]
    public static void Generate()
    {
        // 選択中の GameObject から PlayableDirector を取得
        var go = Selection.activeGameObject;
        if (go == null)
        {
            Debug.LogError("PlayableDirector を持つ GameObject を選択してください");
            return;
        }

        var director = go.GetComponent<PlayableDirector>();
        if (director == null)
        {
            Debug.LogError("PlayableDirector コンポーネントがありません");
            return;
        }

        var timeline = director.playableAsset as TimelineAsset;
        if (timeline == null)
        {
            Debug.LogError("Director に TimelineAsset が設定されていません");
            return;
        }

        // 最初の AnimationTrack の infiniteClip を取得
        var animTrack = timeline.GetOutputTracks()
            .OfType<AnimationTrack>()
            .FirstOrDefault();
        if (animTrack == null || animTrack.infiniteClip == null)
        {
            Debug.LogError("AnimationTrack または infiniteClip が見つかりません");
            return;
        }

        var clip = animTrack.infiniteClip;
        var bindings = AnimationUtility.GetCurveBindings(clip);

        // RecordedKeyData を作成
        var data = ScriptableObject.CreateInstance<RecordedKeyData>();
        var list = bindings.Select(bind =>
        {
            var curve = AnimationUtility.GetEditorCurve(clip, bind);
            return new RecordedCurve
            {
                path = bind.path,
                propertyName = bind.propertyName,
                times = curve.keys.Select(k => k.time).ToArray(),
                values = curve.keys.Select(k => k.value).ToArray()
            };
        }).ToArray();

        data.curves = list;

        // 保存先パスを決定（プロジェクト直下 Assets/RecordedKeyData.asset）
        var path = "Assets/Scenes/KonnoTest/Data/RecordedKeyData.asset";
        AssetDatabase.CreateAsset(data, path);
        AssetDatabase.SaveAssets();
        Debug.Log($"RecordedKeyData を生成しました：{path}");
    }
}
#endif
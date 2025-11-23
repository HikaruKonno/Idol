#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Linq;

public class TimelineKeyExtractor : MonoBehaviour
{
    [MenuItem("Tools/Extract Timeline Animation Keys")]
    public static void ExtractKeysFromSelectedDirector()
    {
        // 選択中の GameObject が PlayableDirector を持っているか
        var go = Selection.activeGameObject;
        if (go == null)
        {
            Debug.LogError("Hierarchy で PlayableDirector を持つ GameObject を選択してください");
            return;
        }

        var director = go.GetComponent<PlayableDirector>();
        if (director == null)
        {
            Debug.LogError("選択中の GameObject に PlayableDirector がありません");
            return;
        }

        // TimelineAsset を取得
        var timeline = director.playableAsset as TimelineAsset;
        if (timeline == null)
        {
            Debug.LogError("PlayableDirector に TimelineAsset が設定されていません");
            return;
        }

        // 最初の AnimationTrack の infiniteClip（キー打ち込み用クリップ）を取得
        var animTrack = timeline.GetOutputTracks()
            .OfType<AnimationTrack>()
            .FirstOrDefault();
        if (animTrack == null || animTrack.infiniteClip == null)
        {
            Debug.LogError("AnimationTrack または infiniteClip が見つかりません");
            return;
        }

        AnimationClip clip = animTrack.infiniteClip;
        Debug.Log($"=== Extracting from clip: {clip.name} ===");

        // カーブバインディング（Float カーブ）を取得
        var curveBindings = AnimationUtility.GetCurveBindings(clip);
        foreach (var bind in curveBindings)
        {
            var curve = AnimationUtility.GetEditorCurve(clip, bind);
            Debug.Log($"{bind.path} / {bind.propertyName} → Keys: {curve.keys.Length}");
            foreach (var key in curve.keys)
            {
                Debug.Log($"  time: {key.time:F3}s, value: {key.value:F3}");
            }
        }

        // オブジェクト参照カーブ（Sprite 切り替えやマテリアルなど）も必要なら
        var objRefBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
        foreach (var bind in objRefBindings)
        {
            var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, bind);
            Debug.Log($"{bind.path} / {bind.propertyName} → ObjKeys: {keyframes.Length}");
            foreach (var kf in keyframes)
            {
                string valName = kf.value != null ? kf.value.name : "null";
                Debug.Log($"  time: {kf.time:F3}s, value: {valName}");
            }
        }
    }
}
#endif
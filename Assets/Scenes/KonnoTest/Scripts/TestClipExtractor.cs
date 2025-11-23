using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TestClipExtractor : MonoBehaviour
{
    // Inspector でセットするか PlayableDirector から取得
    public TimelineAsset timelineAsset;

    void Start()
    {
        Debug.Log("▶ ClipExtractor.Start");

        var track = timelineAsset.GetOutputTracks()
            .OfType<AnimationTrack>()
            .FirstOrDefault();
        if (track != null && track.infiniteClip != null)
        {
            AnimationClip recorded = track.infiniteClip;
            Debug.Log($"Recorded Clip: {recorded.name}");
        }
    }
}

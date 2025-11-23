using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Linq;

public class FindComponent : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DumpPlayingAudioSources();
    }

    [ContextMenu("Dump Playing AudioSources")]
    void DumpPlayingAudioSources()
    {
        // 非アクティブ含む全AudioSourceを取得
        var sources = Object.FindObjectsOfType<AudioSource>(true);
        foreach (var src in sources)
        {
            if (src.isPlaying && src.clip != null)
            {
                Debug.LogError($"{GetHierarchyPath(src.transform)} → {src.clip.name}");
            }
        }
    }

    // Transformの階層パスを再帰的に取得
    string GetHierarchyPath(Transform t)
    {
        return t.parent == null
            ? t.name
            : GetHierarchyPath(t.parent) + "/" + t.name;
    }

    [ContextMenu("Dump Timeline AudioBindings")]
    void DumpBindings()
    {
        foreach (var dir in FindObjectsOfType<PlayableDirector>())
        {
            var timeline = dir.playableAsset as TimelineAsset;
            if (timeline == null) continue;

            Debug.Log($"[Timeline] {dir.gameObject.name} ({dir.name})");
            foreach (var track in timeline.GetOutputTracks()
                                         .Where(t => t is AudioTrack))
            {
                var audioTrack = (AudioTrack)track;
                var binding = dir.GetGenericBinding(audioTrack);
                Debug.Log($"  ▶ Track: {audioTrack.name} → Binding: {binding}");
                foreach (var clip in audioTrack.GetClips())
                {
                    var asset = clip.asset as AudioPlayableAsset;
                    Debug.Log($"    • Clip: {asset.clip.name} @ {clip.start:F2}s, Loop={asset.loop}");
                }
            }
        }
    }
}

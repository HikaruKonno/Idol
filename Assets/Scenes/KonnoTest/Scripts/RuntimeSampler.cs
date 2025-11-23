using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Linq;

public class RuntimeSampler : MonoBehaviour
{
    public PlayableDirector director;
    public GameObject target;           // アニメーションを適用したいオブジェクト
    AnimationClip clip;

    void Start()
    {
        // TimelineAsset → AnimationTrack → infiniteClip を取得
        var timeline = director.playableAsset as TimelineAsset;
        var animTrack = timeline.GetOutputTracks()
            .OfType<AnimationTrack>()
            .FirstOrDefault();
        clip = animTrack.infiniteClip;
    }

    void Update()
    {
        if (clip == null) return;

        // PlayableDirector.time に合わせてサンプリング
        float t = (float)director.time;
        clip.SampleAnimation(target, t);
    }
}

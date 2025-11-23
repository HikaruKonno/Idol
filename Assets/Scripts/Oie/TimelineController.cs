using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelineController : MonoBehaviour
{
    [SerializeField]
    private PlayableDirector playableDirector;

    [SerializeField]
    private AnimationClip clipToAdd;

    void Generate()
    {
        if (playableDirector == null || clipToAdd == null) return;

        // タイムラインアセットを取得
        TimelineAsset timelineAsset = playableDirector.playableAsset as TimelineAsset;
        if (timelineAsset == null) return;

        // 最初に見つかったAnimationTrackを取得
        AnimationTrack targetTrack = null;
        foreach (var track in timelineAsset.GetRootTracks())
        {
            if (track is AnimationTrack)
            {
                targetTrack = track as AnimationTrack;
                break;
            }
        }
        if (targetTrack == null)
        {
            targetTrack = timelineAsset.CreateTrack<AnimationTrack>(null, "New Animation Track");
        }

        // 新しいクリップを作成
        TimelineClip newClip = targetTrack.CreateDefaultClip();
        newClip.asset = clipToAdd;              // AnimationClipを割り当て
        newClip.start = 2.0;                    // 2秒の位置から開始
        newClip.duration = clipToAdd.length;    // クリップの長さを設定
    }


    /// <summary>
    ///  指定したオブジェクトを特定の時間アクティブにするトラックとクリップを追加する
    /// </summary>
    /// <param name="director">操作対象のPlayableDirector</param>
    /// <param name="objectToActivate">アクティブにするGameObject</param>
    /// <param name="startTime">開始時間（秒）</param>
    /// <param name="duration">有効化する期間（秒）</param>
    public void AddActivationClip(PlayableDirector director, GameObject objectToActivate, float startTime, float duration)
    {
        TimelineAsset timelineAsset = director.playableAsset as TimelineAsset;

        // ActivationTrackを作成
        ActivationTrack activationTrack = timelineAsset.CreateTrack<ActivationTrack>(null, objectToActivate.name + "Activation");

        // トラックにGameObjectをバインドする
        director.SetGenericBinding(activationTrack, objectToActivate);

        // クリップを作成
        TimelineClip newClip = activationTrack.CreateDefaultClip();
        newClip.start = startTime;
        newClip.duration = duration;
    }


    /// <summary>
    /// 指定したオブジェクトにアニメーションクリップを配置する
    /// </summary>
    /// <param name="director">操作対象のPlayableDirector</param>
    /// <param name="objectToAnimate">動かすGameObject（Animatorが必要）</param>
    /// <param name="animationClip">再生するAnimationClip</param>
    /// <param name="startTime">開始時間（秒）</param>
    public void AddAnimationClip(PlayableDirector director, GameObject objectToAnimate, AnimationClip animationClip, float startTime)
    {
        // Animatorコンポーネントがなければ追加
        Animator animator = objectToAnimate.GetComponent<Animator>();
        if(animator == null)
        {
            animator = objectToAnimate.AddComponent<Animator>();
        }

        TimelineAsset timelineAsset = director.playableAsset as TimelineAsset;

        // AnimationTrakを作成
        AnimationTrack animTrack = timelineAsset.CreateTrack<AnimationTrack>(null, objectToAnimate.name + "Animation");

        // トラックにAnimatorをバインド
        director.SetGenericBinding(animTrack, animator);

        // クリップを作成してAnimationClipを割り当て
        TimelineClip newClip = animTrack.CreateClip(animationClip);
        newClip.start = startTime;
    }
}

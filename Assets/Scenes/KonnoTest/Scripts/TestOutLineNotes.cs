using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TestOutLineNotes : MonoBehaviour
{
    [SerializeField] private string targetTrackName;
    [Tooltip("バインド先 GameObject で絞り込む場合")]
    [SerializeField]
    private GameObject targetBoundObject;

    private PlayableDirector _director;
    private AnimationTrack _track;

    void Awake()
    {
        _director = GetComponent<PlayableDirector>();
        LogAllAnimationTracks();
        FindTargetTrack();

        if (_track != null)
            Debug.Log($"[FindTargetTrack] Found track => {_track.name} (clips: {_track.GetClips().Count()})");
        else
            Debug.LogError("[FindTargetTrack] targetTrack が見つかりませんでした");

        Debug.Log("Director.playableAsset → "
    + (_director.playableAsset != null ? _director.playableAsset.name : "null"));

        Debug.Log("Director.playableAsset → "
    + (_director.playableAsset != null
       ? _director.playableAsset.name
       : "null"));
    }

    // Inspector の targetTrackName か targetBoundObject から AnimationTrack を検索して _track にセット
    private void FindTargetTrack()
    {
        var timeline = _director.playableAsset as TimelineAsset;
        if (timeline == null)
        {
            Debug.LogError("PlayableDirector に TimelineAsset が設定されていません。");
            return;
        }

        // 1) 名前で絞り込み
        if (!string.IsNullOrEmpty(targetTrackName))
        {
            _track = timeline.GetOutputTracks()
                             .OfType<AnimationTrack>()
                             .FirstOrDefault(t => t.name == targetTrackName);
            if (_track == null)
            {
                Debug.LogWarning($"AnimationTrack '{targetTrackName}' が見つかりませんでした。");
            }
        }

        // 2) バインド先 GameObject で絞り込み
        if (targetBoundObject != null)
        {
            _track = timeline.GetOutputTracks()
                             .OfType<AnimationTrack>()
                             .FirstOrDefault(t =>
                                 _director.GetGenericBinding(t) as GameObject
                                 == targetBoundObject
                             );
            if (_track == null)
            {
                Debug.LogWarning($"'{targetBoundObject.name}' にバインドされた AnimationTrack が見つかりませんでした。");
            }
        }

        // 3) どちらも指定がなければ、最初の AnimationTrack を採用
        _track = timeline.GetOutputTracks()
                         .OfType<AnimationTrack>()
                         .FirstOrDefault();
        if (_track == null)
        {
            Debug.LogError("TimelineAsset に AnimationTrack がひとつもありません。");
        }

        if (_track != null)
        {
            var clips = _track.GetClips().OrderBy(c => c.start).ToList();
            Debug.Log($"=== {_track.name} のクリップ数: {clips.Count} ===");
            foreach (var c in clips)
                Debug.Log($"clip '{c.displayName}' start={c.start:F2}, duration={c.duration:F2}");
        }
    }

    /// <summary>
    /// 各クリップの継続時間（B–A、D–C…）をそのまま取得
    /// </summary>
    public List<double> GetClipDurations()
    {
        var list = new List<double>();
        if (_track == null) return list;

        // start → end を列挙
        foreach (var clip in _track.GetClips().OrderBy(c => c.start))
            list.Add(clip.duration);  // duration == clip.end – clip.start

        return list;
    }

    /// <summary>
    /// クリップ同士が連続再生（間にギャップがない）している区間ごとの合計継続時間を取得
    /// 例: [A〜B][gap][C〜D][E〜F] → { B−A, (D−C)+(F−E) }
    /// </summary>
    public List<double> GetContinuousSegments()
    {
        var segments = new List<double>();
        if (_track == null)
        {
            return segments;
        }

        // (start, end) のリストを作成 & 時系列ソート
        var ranges = _track.GetClips()
                           .Select(c => (start: c.start, end: c.start + c.duration))
                           .OrderBy(r => r.start)
                           .ToList();

        if (ranges.Count == 0)
        {
            return segments;
        }

        // 連続区間をマージしながら継続時間を計算
        double segStart = ranges[0].start;
        double segEnd = ranges[0].end;

        for (int i = 1; i < ranges.Count; i++)
        {
            var (s, e) = ranges[i];

            if (Mathf.Approximately((float)s, (float)segEnd) || s < segEnd)
            {
                // 次のクリップが前の終了時刻と重なっている or ピッタリ隣接 → マージ
                segEnd = Math.Max(segEnd, e);
            }
            else
            {
                // ギャップあり → セグメント終了
                segments.Add(segEnd - segStart);

                // 新しいセグメント開始
                segStart = s;
                segEnd = e;
            }
        }

        // 最後のセグメントも追加
        segments.Add(segEnd - segStart);
        return segments;
    }

    // 追加ヘルパー
    private void LogAllAnimationTracks()
    {
        var ta = _director.playableAsset as TimelineAsset;
        if (ta == null) { Debug.LogError("PlayableDirector に TimelineAsset が設定されていません"); return; }

        var trackNames = ta.GetOutputTracks()
                           .OfType<AnimationTrack>()
                           .Select(t => t.name)
                           .ToArray();
        Debug.Log("Available AnimationTracks: " + string.Join(", ", trackNames));
    }
}

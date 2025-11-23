using UnityEngine;

public class TestTimeLine : MonoBehaviour
{
    [SerializeField] private TestOutLineNotes outlineNotes;

    void Start()
    {
        // 1) クリップごとの継続時間
        var clipDurations = outlineNotes.GetClipDurations();
        Debug.Log("Clip Durations: " + string.Join(", ", clipDurations));

        // 2) ギャップなし連続再生区間の継続時間
        var segments = outlineNotes.GetContinuousSegments();
        Debug.Log("Continuous Segments: " + string.Join(", ", segments));
    }
}

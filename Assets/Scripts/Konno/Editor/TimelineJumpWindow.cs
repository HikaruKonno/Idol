#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineJumpWindow : EditorWindow
{
    PlayableDirector director;
    double jumpTime = 0.0;
    bool autoJumpOnPlay = false;

    [MenuItem("Tools/Timeline Jump")]
    public static void ShowWindow()
    {
        GetWindow<TimelineJumpWindow>("Timeline Jump");
    }

    void OnEnable()
    {
        // Playモード遷移時にコールバックを登録
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
    }

    void OnGUI()
    {
        GUILayout.Label("Timeline Jump Tool", EditorStyles.boldLabel);

        // Director フィールド
        director = (PlayableDirector)EditorGUILayout.ObjectField(
            "Director", director, typeof(PlayableDirector), true);

        // ジャンプ秒数フィールド
        jumpTime = EditorGUILayout.DoubleField("Jump Time (s)", jumpTime);

        // 手動ジャンプボタン
        if (GUILayout.Button("ジャンプ実行"))
        {
            Jump();
        }

        GUILayout.Space(10);

        // Play時自動ジャンプトグル
        autoJumpOnPlay = EditorGUILayout.Toggle(
            "Auto Jump On Play", autoJumpOnPlay);
    }

    void OnPlayModeChanged(PlayModeStateChange state)
    {
        // 「再生開始直後」にだけ呼び出す
        if (autoJumpOnPlay && state == PlayModeStateChange.EnteredPlayMode)
        {
            // EditorApplication.delayCallで遅延実行すると
            // PlayModeに完全に切り替わった後にEvaluateできる
            EditorApplication.delayCall += () =>
            {
                Jump();
            };
        }
    }

    void Jump()
    {
        if (director == null)
        {
            Debug.LogWarning("PlayableDirector が未設定です。");
            return;
        }

        director.time = jumpTime;
        director.Play();
        Debug.Log($"TimelineJump: time set to {jumpTime} sec");
    }
}
#endif
/*
 * ファイル
 * FPSDisplay C#
 * 
 * システム
 * ゲーム内のFPSを表示する
 * 
 * 作成
 * 2025/10/02 坂上　壱希
 * 
 * 最終変更
 * 2025/10/8 坂上　壱希
 */
#if UNITY_EDITOR
using UnityEngine;

sealed public class FPSDisplay : MonoBehaviour
{
    // フレーム間の経過時間をスムージングして計算するための変数
    private float m_deltaTime = 0.0f;

    void Update()
    {
        // フレーム間の経過時間をスムージングして計算
        m_deltaTime += (Time.unscaledDeltaTime - m_deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        int width = Screen.width;
        int height = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(10, 10, width, height * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = height * 2 / 50;
        style.normal.textColor = Color.white;

        float fps = 1.0f / m_deltaTime;
        string text = string.Format("{0:0.} FPS", fps);

        GUI.Label(rect, text, style);
    }
}
#endif

#define TOP_DRAW
#define ALL_DRAW
#undef ALL_DRAW

using System;
using System.Collections.Generic;
using UnityEngine;
using static NotesCollisionDetection;

public class QuadViewportCollision : MonoBehaviour
{
    /// <summary>
    /// ノーツの種類
    /// </summary>
    public enum DetectionTag
    {
        /// <summary> 左手</summary>
        LeftHand,
        /// <summary> 右手</summary>
        RightHand,
    }

    /// <summary>
    /// 画面を4分割した時の位置
    /// </summary>
    enum Position : int
    {
        LeftUp,
        RightUp,
        LeftDown,
        RightDown
    }

    //-----------------------------------------------
    // メンバ変数
    //-----------------------------------------------
    #region Field
    // ノーツの配置を決めるリスト
    [SerializeField]
    private List<Position> m_notesPosList;
    // メディアパイプのランドマーク取得する為の変数
    [SerializeField]
    private MediapipeResultDataContainer m_mediapipeContainer;
    // ノーツの種類タグの設定
    [SerializeField]
    private DetectionTag m_detectionTag;

    // ビューポート座標のランドマーク群の保持
    private List<Vector2> m_points;
    private RectTransform m_rt;
    private Camera m_mainCamera;
    private Rect[] m_viewPortCollisionRects;

    private GUIStyle _labelStyle;
    private Texture2D _whiteTex;
    #endregion

    //----------------------------------------------
    // Unityライフサイクル
    //----------------------------------------------
    #region LifeCycle
    private void Awake()
    {
        // OnEnableの前にコンポーネントを取得したいから
        m_rt = GetComponent<RectTransform>();
        m_mainCamera = Camera.main;
        m_viewPortCollisionRects = new Rect[4];
        m_viewPortCollisionRects[(int)Position.LeftUp] = Rect.MinMaxRect(0f, 0.5f, 0.5f, 1f);
        m_viewPortCollisionRects[(int)Position.RightUp] = Rect.MinMaxRect(0.5f, 0.5f, 1f, 1f);
        m_viewPortCollisionRects[(int)Position.LeftDown] = Rect.MinMaxRect(0f, 0f, 0.5f, 0.5f);
        m_viewPortCollisionRects[(int)Position.RightDown] = Rect.MinMaxRect(0.5f, 0f, 1f, 0.5f);
    }

    // 最初のフレームの処理
    void Start()
    {

    }

    private void Update()
    {
        // 右手のノーツだったら
        if (m_detectionTag == DetectionTag.RightHand)
        {

            // 右手のランドマークがあるかチェック
            if (m_mediapipeContainer.RightHandNormalizedLandmarkList == null)
            {
                return;
            }

            // ランドマークの矩形範囲のリスト
            List<Vector2> landMarkersRectList = new List<Vector2>
        {
            MediaPipeUtils.ConvertToViewportPos(m_mediapipeContainer.RightHandNormalizedLandmarkList[(int)EHandLandmarks.Wrist]),
            MediaPipeUtils.ConvertToViewportPos(m_mediapipeContainer.RightHandNormalizedLandmarkList[(int)EHandLandmarks.ThumbIP]),
            MediaPipeUtils.ConvertToViewportPos(m_mediapipeContainer.RightHandNormalizedLandmarkList[(int)EHandLandmarks.PinkyMCP])
        };

            m_points = landMarkersRectList;

            Rect rect = GetViewportBoundsFromPoints(landMarkersRectList);

            if (IsLandMarkHit(rect))
            {
                Debug.LogWarning($"名前:{m_notesPosList}:判定取れてるよ!!!!");
                if (m_notesPosList.Count > 0)
                {
                    m_notesPosList.RemoveAt(0);
                }
            }

        }
        // 左手のノーツ
        else
        {
            // 左手のランドマークがあるかチェック
            if (m_mediapipeContainer.LeftHandNormalizedLandmarkList == null)
            {
                return;
            }

            // ランドマークの矩形範囲のリスト
            List<Vector2> landMarkersRectList = new List<Vector2>
        {
            MediaPipeUtils.ConvertToViewportPos(m_mediapipeContainer.LeftHandNormalizedLandmarkList[(int)EHandLandmarks.Wrist]),
            MediaPipeUtils.ConvertToViewportPos(m_mediapipeContainer.LeftHandNormalizedLandmarkList[(int)EHandLandmarks.ThumbIP]),
            MediaPipeUtils.ConvertToViewportPos(m_mediapipeContainer.LeftHandNormalizedLandmarkList[(int)EHandLandmarks.PinkyMCP])
        };

            m_points = landMarkersRectList;

            Rect rect = GetViewportBoundsFromPoints(landMarkersRectList);

            if (IsLandMarkHit(rect))
            {
                Debug.LogWarning($"名前:{m_notesPosList}:判定取れてるよ!!!!");
                if (m_notesPosList.Count > 0)
                {
                    m_notesPosList.RemoveAt(0);
                }
            }
        }

    }
    #endregion

    //-----------------------------------------------
    // メンバ関数
    //-----------------------------------------------

    /// <summary>
    /// 点群（ランドマーク）から矩形を取得
    /// </summary>
    /// <param name="points">矩形生成に使うランドマーク</param>
    /// <returns>矩形の範囲</returns>
    private Rect GetViewportBoundsFromPoints(List<Vector2> points)
    {
        if (points == null || points.Count == 0)
        {
            return new Rect();
        }

        float minX = points[0].x;
        float maxX = points[0].x;
        float minY = points[0].y;
        float maxY = points[0].y;

        foreach (var pt in points)
        {
            minX = Mathf.Min(minX, pt.x);
            maxX = Mathf.Max(maxX, pt.x);
            minY = Mathf.Min(minY, pt.y);
            maxY = Mathf.Max(maxY, pt.y);
        }

        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    private bool IsLandMarkHit(Rect rect)
    {
        if (m_notesPosList.Count > 0)
        {
            // Rect.Overlaps はデフォルトで境界を含まない重なり判定
            return rect.Overlaps(m_viewPortCollisionRects[(int)m_notesPosList[0]]);
        }
        else
        {
            return false;
        }
    }

    // ビューポートRect -> OnGUIで使えるスクリーンRectに変換
    private Rect ViewportRectToScreenRect(Rect viewport)
    {
        if (m_mainCamera == null) m_mainCamera = Camera.main;

        // ビューポートの左下と右上をスクリーン座標に変換
        Vector3 bl = m_mainCamera.ViewportToScreenPoint(new Vector3(viewport.xMin, viewport.yMin, 0f));
        Vector3 tr = m_mainCamera.ViewportToScreenPoint(new Vector3(viewport.xMax, viewport.yMax, 0f));

        // OnGUI は Y 軸が反転しているため補正
        bl.y = Screen.height - bl.y;
        tr.y = Screen.height - tr.y;

        // bl は左下, tr は右上 なので OnGUI の左上基準 Rect を作る
        float x = bl.x;
        float y = tr.y; // top
        float w = tr.x - bl.x;
        float h = bl.y - tr.y;
        return new Rect(x, y, w, h);
    }

    private void EnsureGuiResources()
    {
        if (_whiteTex == null)
        {
            _whiteTex = Texture2D.whiteTexture; // 既存の白テクスチャを使う
        }
        if (_labelStyle == null)
        {
            _labelStyle = new GUIStyle(GUI.skin.label);
            _labelStyle.normal.textColor = Color.white;
            _labelStyle.fontSize = 12;
        }
    }

    // 共通で使う描画関数（既存の ViewportRectToScreenRect, EnsureGuiResources を前提）
    private void DrawViewportRectDebug(Rect vpRect, int debugIndex)
    {
        Rect sr = ViewportRectToScreenRect(vpRect);

        // 色はインデックスや用途に応じて決める
        Color fill = new Color(1f, 1f, 1f, 0.15f);
        Color outline = new Color(1f, 1f, 1f, 0.9f);

        // 例えば各ポジションで色を分けたい場合
        switch (debugIndex)
        {
            case (int)Position.LeftUp: fill = new Color(1f, 0f, 0f, 0.25f); break;
            case (int)Position.RightUp: fill = new Color(0f, 1f, 0f, 0.25f); break;
            case (int)Position.LeftDown: fill = new Color(0f, 0f, 1f, 0.25f); break;
            case (int)Position.RightDown: fill = new Color(1f, 1f, 0f, 0.25f); break;
        }

        // 塗り
        GUI.color = fill;
        GUI.DrawTexture(sr, _whiteTex);

        // 枠線
        GUI.color = outline;
        GUI.DrawTexture(new Rect(sr.x, sr.y, sr.width, 2), _whiteTex); // top
        GUI.DrawTexture(new Rect(sr.x, sr.yMax - 2, sr.width, 2), _whiteTex); // bottom
        GUI.DrawTexture(new Rect(sr.x, sr.y, 2, sr.height), _whiteTex); // left
        GUI.DrawTexture(new Rect(sr.xMax - 2, sr.y, 2, sr.height), _whiteTex); // right

        // ラベル
        string label = Enum.GetName(typeof(Position), debugIndex) ?? $"[{debugIndex}]";
        Vector2 labelSize = _labelStyle.CalcSize(new GUIContent(label));
        Rect labelRect = new Rect(sr.x + 4, sr.y + 4, labelSize.x + 6, labelSize.y + 4);
        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(labelRect, _whiteTex);
        GUI.color = Color.white;
        GUI.Label(new Rect(sr.x + 6, sr.y + 6, labelSize.x, labelSize.y), label, _labelStyle);
    }

    private void OnGUI()
    {
#if TOP_DRAW
        if (m_viewPortCollisionRects == null || m_viewPortCollisionRects.Length == 0) return;
        if (m_mainCamera == null) m_mainCamera = Camera.main;
        if (m_mainCamera == null) return;

        EnsureGuiResources();

        // 先頭だけ描画する場合
        if (m_notesPosList != null && m_notesPosList.Count > 0)
        {
            int posIndex = (int)m_notesPosList[0];
            if (posIndex >= 0 && posIndex < m_viewPortCollisionRects.Length)
            {
                DrawViewportRectDebug(m_viewPortCollisionRects[posIndex], posIndex);
            }
        }
#elif ALL_DRAW
// すべて描画
 if (m_viewPortCollisionRects == null || m_viewPortCollisionRects.Length == 0) return;
    if (m_mainCamera == null) m_mainCamera = Camera.main;
    if (m_mainCamera == null) return;

    EnsureGuiResources();

    if (m_notesPosList != null && m_notesPosList.Count > 0)
    {
        for (int i = 0; i < m_notesPosList.Count; i++)
        {
            int posIndex = (int)m_notesPosList[i];
            if (posIndex >= 0 && posIndex < m_viewPortCollisionRects.Length)
            {
                DrawViewportRectDebug(m_viewPortCollisionRects[posIndex], posIndex);
            }
        }
    }
#endif
    }
}

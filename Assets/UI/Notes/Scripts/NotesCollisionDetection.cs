/*
 * ファイル
 * NotesCollisionDetection C#
 * 
 * システム
 * ノーツの当たり判定をとる
 * 
 * 作成
 * 2025/09/11 寺門 冴羽
 * 2025/09/18 今野　光
 * 2025/09/24 寺門 冴羽：ノーツの消える演出の追加
 * 2025/09/24 寺門 冴羽：ノーツの消える演出が多重に生成されてるのを修正landMarkersRectList
 * 2025/10/13 奥山 凜：landMarkersRectListの配列をUpdateで毎回確保していたのをAwakeで一度確保するように変更
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NotesCollisionDetection : MonoBehaviour
{
    // --------------------------------------------------------------------------------
    // 変数宣言
    // --------------------------------------------------------------------------------

    #region Field
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

    // ノーツの種類タグの設定
    [SerializeField]
    private DetectionTag m_detectionTag;
    // メインカメラ
    [SerializeField]
    private Camera m_mainCamera;
    // メディアパイプのランドマーク取得する為の変数
    [SerializeField]
    private MediapipeResultDataContainer m_mediapipeContainer;
    // ビューポート上での半径（0〜1）
    [SerializeField]
    private float m_radius = 0.2f;
    // ノーツが存在する時間
    [SerializeField]
    private float m_ExistenceTime;

    //
    // ノーツの消滅演出用
    //

    // ノーツの消滅アニメーションの再生フラグ 初めて触ったときのみ
    private bool m_destroyDirectionFlag;

    // ノーツの消滅時間
    [SerializeField]
    private float m_destroyDirectionTime;
    private float m_destroyDirectionTimer;

    // 判定エフェクトのプレハブ
    [SerializeField]
    private GameObject GuideEffectPrefab;


    // ビューポート座標のランドマーク群の保持
    private List<Vector2> m_points;
    // ノーツの当たり判定を管理しているクラス
    private NotesCollisionMaganger m_notesColMng;
    // 円の当たり判定を可視化するテクスチャ
    private Texture2D m_circleTex;
    private Vector3 m_viewportPos;
    private RectTransform m_rt;
    #endregion

    private List<Vector2> m_landMarkersRectList;

    // --------------------------------------------------------------------------------
    // メイン関数
    // --------------------------------------------------------------------------------

    private void Awake()
    {
        // OnEnableの前にコンポーネントを取得したいから
        m_rt = GetComponent<RectTransform>();
        m_mainCamera = Camera.main;

        m_landMarkersRectList= new List<Vector2>(3);
    }

    // 最初のフレームの処理
    void Start()
    {
        // 親オブジェクトのNotesCollisionManagerを取得
        m_notesColMng = gameObject.transform.parent.GetComponent<NotesCollisionMaganger>();

        // 消滅アニメーションの再生フラグの初期化
        m_destroyDirectionFlag = true;

        // 親オブジェクトのNotesCollisionManagerを取得出来なかった場合のエラーログ
        if (m_notesColMng == null)
        {
            Debug.LogError("NotesCollisionManagerが親オブジェクトに設定してください。");
            Debug.LogError($"名前:{gameObject.name}");
        }
    }

    private void Update()
    {
        m_landMarkersRectList.Clear(); // 最初に中身を空にする

        // 右手のノーツだったら
        if (m_detectionTag == DetectionTag.RightHand)
        {

            // 右手のランドマークがあるかチェック
            if (m_mediapipeContainer.RightHandNormalizedLandmarkList == null)
            {
                return;
            }

            // ランドマークの矩形範囲のリスト
            m_landMarkersRectList.Add(MediaPipeUtils.ConvertToViewportPos(m_mediapipeContainer.RightHandNormalizedLandmarkList[(int)EHandLandmarks.Wrist]));
            m_landMarkersRectList.Add(MediaPipeUtils.ConvertToViewportPos(m_mediapipeContainer.RightHandNormalizedLandmarkList[(int)EHandLandmarks.ThumbIP]));
            m_landMarkersRectList.Add(MediaPipeUtils.ConvertToViewportPos(m_mediapipeContainer.RightHandNormalizedLandmarkList[(int)EHandLandmarks.PinkyMCP]));

            m_points = m_landMarkersRectList;

            Rect rect = GetViewportBoundsFromPoints(m_landMarkersRectList);

            // 手の当たり判定（矩形）にノーツ（円）が当たっていたら処理
            if (IsCircleIntersectingRect(m_viewportPos, m_radius, rect))
            {
                if (m_notesColMng.IsNotesSequence(this))
                {
                    // ガイドの判定関数
                    GuideDetection();
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
            m_landMarkersRectList.Add(MediaPipeUtils.ConvertToViewportPos(m_mediapipeContainer.LeftHandNormalizedLandmarkList[(int)EHandLandmarks.Wrist]));
            m_landMarkersRectList.Add(MediaPipeUtils.ConvertToViewportPos(m_mediapipeContainer.LeftHandNormalizedLandmarkList[(int)EHandLandmarks.ThumbIP]));
            m_landMarkersRectList.Add(MediaPipeUtils.ConvertToViewportPos(m_mediapipeContainer.LeftHandNormalizedLandmarkList[(int)EHandLandmarks.PinkyMCP]));


            m_points = m_landMarkersRectList;

            Rect rect = GetViewportBoundsFromPoints(m_landMarkersRectList);

            // 手の当たり判定（矩形）にノーツ（円）が当たっていたら処理
            if (IsCircleIntersectingRect(m_viewportPos, m_radius, rect))
            {
                if (m_notesColMng.IsNotesSequence(this))
                {
                    // ガイドの判定関数
                    GuideDetection();
                }
            }
        }

        // 消滅アニメーション処理
        //TimeDestroy();
    }

    private void OnEnable()
    {
        // スクリーン座標を取得
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(
            null,
            m_rt.position
        );
        // ノーツの座標をビューポート座標に変換
        m_viewportPos = m_mainCamera.ScreenToViewportPoint(screenPos);

#if UNITY_EDITOR
        Debug.LogWarning("有効化：" + gameObject.name);
#endif
    }

    //---------------------------------------------------------
    // サブ関数
    //---------------------------------------------------------

    /// <summary>
    /// ランドマークの点とノーツが当たっているか判断する
    /// </summary>
    /// <param name="point">ランドマークの点の座標(ビューポート座標)</param>
    /// <param name="center">ノーツの中心点(ビューポート座標)</param>
    /// <param name="radius">ノーツの半径(半径)</param>
    /// <returns>当たっていたらtrueを返す</returns>
    private bool IsPointInsideViewportCircle(Vector2 point, Vector2 center, float radius)
    {
        // ビューポート座標（0〜1）での点・円
        return Vector2.Distance(point, center) < radius;
    }

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

    /// <summary>
    /// ノーツと手が当たっているか判断する
    /// </summary>
    /// <param name="circleCenter">ノーツの中心点</param>
    /// <param name="radius">半径（大きさ）</param>
    /// <param name="rect">矩形</param>
    /// <returns>当たっていたらtrue</returns>
    bool IsCircleIntersectingRect(Vector2 circleCenter, float radius, Rect rect)
    {
        // 円の中心を矩形にクランプ（最近傍点を取得）
        float closestX = Mathf.Clamp(circleCenter.x, rect.xMin, rect.xMax);
        float closestY = Mathf.Clamp(circleCenter.y, rect.yMin, rect.yMax);

        // 最近傍点と円の中心との距離
        float dx = circleCenter.x - closestX;
        float dy = circleCenter.y - closestY;

        // 距離の二乗と半径の二乗を比較（平方根回避）
        return (dx * dx + dy * dy) < (radius * radius);
    }

    // 消滅演出
    void TimeDestroy()
    {
        // 消滅するかどうか
        if (m_destroyDirectionFlag)
        {
            return;
        }

        // 消滅演出の時間計測
        m_destroyDirectionTimer += Time.deltaTime;

        // 消滅演出の時間が超えたら
        if (m_destroyDirectionTimer >= m_destroyDirectionTime)
        {
            // 消滅タイマーリセット
            m_destroyDirectionTimer = 0;

            // 非アクティブにする
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// システム：ガイドの判定が成功した時の処理
    /// 引数　　：なし
    /// 戻り値　：なし
    /// </summary>
    void GuideDetection()
    {
        // フラグが降りてるなら処理をしない
        if (!m_destroyDirectionFlag)
        {
            return;
        }

        // 一番上の親のCanvasを取得
        Transform canvasObj = transform.root.gameObject.transform;

        // ガイドの成功エフェクトをこのオブジェクトの位置に生成
        GameObject effect = Instantiate<GameObject>(GuideEffectPrefab, m_rt.transform.position, Quaternion.identity, canvasObj);

        // 生成したエフェクトのデストロイのコールバックにフラグのリセットの関数をバインド
        if(effect.TryGetComponent(out GuideTouchEffectDestruction guideTouchEffectDestruction))
        {
            guideTouchEffectDestruction.BindOnDestroyCallback(ResetParticleFlag);
        }

#if UNITY_EDITOR
        Debug.LogWarning("ぱーてぃくる生成！");
#endif
        // 消滅フラグを下げる
        m_destroyDirectionFlag = false;

        // 坂上　追加
        // SEを鳴らす
        AudioManager.Instance.PlaySE(AudioName.Notes1);
        
        // 接触カウントを足す
        m_notesColMng.AddDetectionCount();

        // 非アクティブにする
        gameObject.SetActive(false);
    }

    //---------------------------------------------------------
    // イベント関数
    //---------------------------------------------------------
    /*
    // 非アクティブになった時の処理
    void OnDisable()
    {
        // 消滅フラグを下げる
        m_destroyDirectionFlag = true;
        Debug.LogWarning("offぱーてぃくる生成許可！off");

        // 消滅タイマーリセット
        m_destroyDirectionTimer = 0;
    }
    */
     

    public void ResetParticleFlag()
    {
        // 消滅フラグを上げる
        m_destroyDirectionFlag = true;
#if UNITY_EDITOR
        Debug.LogWarning("offぱーてぃくる生成許可！off");
#endif
    }


    //-----------------------------------------
    // デバッグ用
    // 当たり判定の表示
    //-----------------------------------------
#if UNITY_EDITOR && false
    void OnGUI()
    {
        if (m_mainCamera == null)
        {
            m_mainCamera = Camera.main;
            if (m_mainCamera == null) return;
        }


        if (m_points == null || m_points.Count == 0)
        {
            return;
        }

        // このオブジェクトのワールド位置 → スクリーン座標　→ ビューポート座標
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(null, m_rt.position);
        Vector3 viewportPos = m_mainCamera.ScreenToViewportPoint(screenPos);

        // １．点群から矩形を作る（ビューポート座標）
        Rect viewportRect = GetViewportBoundsFromPoints(m_points);

        // ２．矩形のビューポート座標をスクリーン座標に変換
        Vector3 topLeft = m_mainCamera.ViewportToScreenPoint(new Vector3(viewportRect.xMin, viewportRect.yMax, 0));
        Vector3 bottomRight = m_mainCamera.ViewportToScreenPoint(new Vector3(viewportRect.xMax, viewportRect.yMin, 0));

        // GUIはY軸反転なので調整
        topLeft.y = Screen.height - topLeft.y;
        bottomRight.y = Screen.height - bottomRight.y;

        Rect screenRect = new Rect(
            topLeft.x,
            topLeft.y,
            bottomRight.x - topLeft.x,
            bottomRight.y - topLeft.y
        );

        // ３．矩形を描画（半透明緑）
        GUI.color = new Color(0, 1, 0, 0.3f);
        GUI.DrawTexture(screenRect, Texture2D.whiteTexture);

        GUI.color = new Color(0, 1, 0, 0.8f);
        GUI.DrawTexture(new Rect(screenRect.x, screenRect.y, screenRect.width, 2), Texture2D.whiteTexture); // 上線
        GUI.DrawTexture(new Rect(screenRect.x, screenRect.yMax - 2, screenRect.width, 2), Texture2D.whiteTexture); // 下線
        GUI.DrawTexture(new Rect(screenRect.x, screenRect.y, 2, screenRect.height), Texture2D.whiteTexture); // 左線
        GUI.DrawTexture(new Rect(screenRect.xMax - 2, screenRect.y, 2, screenRect.height), Texture2D.whiteTexture); // 右線

        // カメラの前にある（z > 0）のみ表示
        if (viewportPos.z < 0)
        {
            return;
        }

        // スクリーン上での半径（Y基準）
        float screenRadius = m_radius * Screen.height;

        // OnGUIはY軸反転
        screenPos.y = Screen.height - screenPos.y;

        Rect rect = new Rect(
            screenPos.x - screenRadius,
            screenPos.y - screenRadius,
            screenRadius * 2,
            screenRadius * 2
        );

        // テクスチャがなければ作成
        if (m_circleTex == null)
        {
            m_circleTex = MakeCircleTexture(64);
        }

        GUI.color = new Color(1, 0, 0, 0.5f);
        GUI.DrawTexture(rect, m_circleTex);
    }

    /// <summary>
    /// 簡易的な円形テクスチャ生成
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    private Texture2D MakeCircleTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
        Color transparent = new Color(0, 0, 0, 0);
        Color fill = Color.white;

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float r = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                tex.SetPixel(x, y, dist <= r ? fill : transparent);
            }
        }

        tex.Apply();
        return tex;
    }
#endif
}
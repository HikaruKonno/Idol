#define DEBUG
#undef DEBUG
#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections.Generic;
using UnityEngine;


public class AudienceSpawner : MonoBehaviour
{
    [Header("共通設定")]
    // 共通設定を使用するかのフラグ
    [SerializeField]
    private bool m_isShareSettings = true;
    // 共通設定のスクリプタブルオブジェクトを格納する変数
    [SerializeField]
    private AudienceSpawnerShareData m_shareData;

    [Header("配置する範囲")]
    [SerializeField, Tooltip("横")]
    private float m_spawnLength;
    [SerializeField, Tooltip("縦")]
    private float m_spawnWidth;
    [SerializeField, Tooltip("高さ")]
    private float m_spawnHeight;

    [Header("配置個数")]
    [SerializeField, Tooltip("横方向に生成する数")]
    private int m_spawnLengthCount;
    [SerializeField, Tooltip("縦方向に生成する数")]
    private int m_spawnWidthCount;
    [SerializeField, Tooltip("横の列の生成が終わったら足していく数")]
    private float m_stepLengthPerColumn;
    [SerializeField, Tooltip("縦の列の生成が終わったら足していく数")]
    private float m_stepWidthPerColumn;
    // 生成する方向
    [SerializeField]
    private Quaternion m_spawnRotation;
    // 観客を向かせるターゲットのTransform
    [SerializeField, Tooltip("ターゲット")]
    private Transform m_targetTransform;
    // モデルが前を向くためのY軸補正角（元々回転させている場合は、その値をここに格納する）
    [SerializeField, Tooltip("元々回転させている場合は、その値をここに格納する")]
    private float m_rotationOffsetY = 0f;

    [Header("生成時のノイズ")]
    [SerializeField, Tooltip("横の最大のノイズ")]
    private float m_maxNoiseLength;
    [SerializeField, Tooltip("横の最小のノイズ")]
    private float m_minNoiseLength;
    [SerializeField, Tooltip("縦の最大のノイズ")]
    private float m_maxNoiseWidth;
    [SerializeField, Tooltip("縦の最小のノイズ")]
    private float m_minNoiseWidth;
    [SerializeField, Tooltip("高さの最大のノイズ")]
    private float m_maxNoiseHight;
    [SerializeField, Tooltip("高さの最小のノイズ")]
    private float m_minNoiseHight;

    [Header("生成するオブジェクト")]
    // 生成するオブジェクト
    [SerializeField]
    private GameObject m_audiencePrefab;
    // 観客のサイリウムの色
    [SerializeField]
    private List<Color> m_psylliumColorList;
    [SerializeField, Tooltip("サイリウムのエミッシブの強さ")]
    private float m_psylliumIntensity = 2.0f;

    [Header("ギズモの設定")]
    [SerializeField]
    private Color m_colour;
    // 表示に関する列挙型
    private enum GizmoType : byte
    {
        Never = 0,              // 表示しない
        SelectedOnly = 1,       // 選択したときのみ
        Always = 2              // 常時表示
    }
    [SerializeField]
    private GizmoType m_gizmoType;

    // 生成された観客のリスト
    private List<GameObject> m_spawnedAudienceList = new List<GameObject>();
    // スポナーのインデックス
    private int m_spawnerIndex = 0;

    private void Awake()
    {
        if (m_shareData != null)
        {
            // 共通設定から値を取得
            m_targetTransform = m_shareData.m_targetTransform;
        }
    }

    /// <summary>
    /// 引数のオブジェクトの方向を向かせる関数
    /// </summary>
    /// <param name="_targetTransform">向かせたいターゲットの Transform</param>
    /// <param name="_rotationOffsetY">元々回転させている場合の補正値</param>
    public void RotateChildrenTowardsTargetY(Transform _targetTransform, float _rotationOffsetY = 0f)
    {
        if (_targetTransform == null)
        {
#if DEBUG && UNITY_EDITOR
            Debug.LogError($"{_targetTransform}が設定されていません");
#endif
            return;
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            // 子供の取得
            Transform child = transform.GetChild(i);

            // ターゲットとの距離
            Vector3 direction = _targetTransform.position - child.position;

            // 高さは変更しない
            direction.y = 0f;

            // ターゲットとの距離が非常に近い位置なら、その子オブジェクトの処理はスキップする。
            if (direction.sqrMagnitude < 0.0001f) continue;

            // ターゲットの方向を向く回転を計算
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Y軸角度に補正を加える
            float angleY = targetRotation.eulerAngles.y + _rotationOffsetY;

            // 回転を適用（X/Zは0に保つ）
            child.rotation = Quaternion.Euler(0f, angleY, 0f);
        }
    }

    /// <summary>
    /// m_targetTransform の方向を向かせる関数
    /// </summary>
    public void RotateTowardsTargetY()
    {
        if (m_targetTransform == null)
        {
#if DEBUG && UNITY_EDITOR
            Debug.LogError($"{m_targetTransform}が設定されていません");
#endif
            return;
        }

        // ターゲットの位置の取得
        Vector3 targetpos = m_targetTransform.position;

        for (int i = 0; i < transform.childCount; i++)
        {
            // 子供の取得
            Transform child = transform.GetChild(i);

            // ターゲットとの距離
            Vector3 direction = targetpos - child.position;

            // 高さは変更しない
            direction.y = 0f;

            // ターゲットとの距離が非常に近い位置なら、その子オブジェクトの処理はスキップする。
            if (direction.sqrMagnitude < 0.0001f) continue;

            // ターゲットの方向を向く回転を計算
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Y軸角度に補正を加える
            float angleY = targetRotation.eulerAngles.y + m_rotationOffsetY;

            // 回転を適用（X/Zは0に保つ）
            child.rotation = Quaternion.Euler(0f, angleY, 0f);
        }
    }

    /// <summary>
    /// Grid状に観客の生成する関数
    /// SpawnEditorCustomからも呼べるようにしたいためpublicにする
    /// </summary>
    public void SpwanAudienceGrid()
    {
        if (m_audiencePrefab == null)
        {
#if UNITY_EDITOR
            Debug.LogError("生成するプレハブが設定されていません");
#endif
            return;
        }

        // 生成する間隔
        float widthSpace;       // 縦
        float lengthSpace;      // 横

        // 生成する間隔を計算する
        if (m_spawnWidth > 1)       // 縦
        {
            // 間隔は、個数 - 1 のため
            widthSpace = m_spawnWidth / (m_spawnWidthCount - 1);
        }
        else
        {
            widthSpace = 0;
        }

        if (m_spawnLength > 1)      // 横
        {
            lengthSpace = m_spawnLength / (m_spawnLengthCount - 1);
        }
        else
        {
            lengthSpace = 0;
        }

        // 配置する開始位置を計算
        Vector3 startPos = transform.position - new Vector3(m_spawnLength / 2f,
                                                            transform.position.y - m_spawnHeight,
                                                            m_spawnWidth / 2f);

        if (m_isShareSettings)
        {
            // 共通の設定を使用する場合
            SpawnAudienceForShare();
        }
        else
        {
            // 個別の設定を使用する場合
            SpawnAudienceForIndividual();
        }

#if DEBUG && UNITY_EDITOR
        Debug.Log($"オブジェクト名：{this.gameObject.name}　観客の生成を完了　生成した数：{m_spawnedAudienceList.Count}");
#endif
    }

    /// <summary>
    /// 個別の設定を使用して生成する場合
    /// </summary>
    private void SpawnAudienceForIndividual()
    {
        for (int widthIndex = 0; widthIndex < m_spawnWidthCount; widthIndex++)
        {
            // 横
            for (int lengthIndex = 0; lengthIndex < m_spawnLengthCount; lengthIndex++)
            {
                // ローカル空間での配置位置（中心からのオフセット）
                Vector3 localOffset = new Vector3(
                    -m_spawnLength / 2f + lengthIndex * (m_spawnLength / (m_spawnLengthCount - 1)),　　// X軸：左から配置
                    (m_stepWidthPerColumn * widthIndex) + (m_stepLengthPerColumn * lengthIndex),   　　// Y軸：列が増えるごとに高さを足していく
                    -m_spawnWidth / 2f + widthIndex * (m_spawnWidth / (m_spawnWidthCount - 1))     　　// Z軸：前から配置
                );

                // ノイズをローカル空間で追加
                Vector3 transformNoise = new Vector3(
                    Random.Range(m_minNoiseLength, m_maxNoiseLength),
                    Random.Range(m_minNoiseHight, m_maxNoiseHight),
                    Random.Range(m_minNoiseWidth, m_maxNoiseWidth)
                );

                // 最終的な配置位置
                Vector3 localSpawnPos = localOffset + transformNoise;

                // ローカル → ワールド変換
                Vector3 spawnPos = transform.TransformPoint(localSpawnPos);

                // 観客を生成
                GameObject audience = Instantiate(m_audiencePrefab, spawnPos, m_spawnRotation, this.transform);

                // サイリウムの取得
                // ペンライトと観客の場合
                Transform psylliumTransform = audience.transform.Find("Penlight/PsylliumHandle/Psyllium");

                if (psylliumTransform == null)
                {
                    // ペンライトのみの場合
                    psylliumTransform = audience.transform.Find("PsylliumPrefab/PsylliumHandle/Psyllium");
                }

                if (psylliumTransform != null)
                {
                    Renderer psylliumRenderer = psylliumTransform.GetComponent<Renderer>();

                    if (psylliumRenderer != null)
                    {
                        // リストの中からランダムに色を選ぶ
                        float randomColorIndex = Random.Range(0, m_psylliumColorList.Count);
                        // Random.Range は flaot で返されるので要素番号用に Int に変換
                        int colorIndex = Mathf.FloorToInt(randomColorIndex);

                        // インスタンスを明示的に生成
                        Material newMat = new Material(psylliumRenderer.sharedMaterial);

                        newMat.enableInstancing = true;

                        // HDRPやURPのマテリアルの変更は、newMat.color = Color.red; ではなくSetColorを使う
                        newMat.SetColor("_BaseColor", m_psylliumColorList[colorIndex]);

                        // Emissionを有効化
                        newMat.EnableKeyword("_EMISSION");
                        // エミッシブの色も変更
                        newMat.SetColor("_EmissionColor", m_psylliumColorList[colorIndex] * m_psylliumIntensity);

                        // 適用
                        psylliumRenderer.sharedMaterial = newMat;
                    }
                    else
                    {
#if UNITY_EDITOR
                        Debug.LogWarning("Psyllium の Renderer が見つかりません");
#endif
                    }
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogWarning("Psyllium のオブジェクトが見つかりません");
#endif
                }


                // 生成した観客リストに追加
                m_spawnedAudienceList.Add(audience);

                // 生成した観客に名前を設定
                audience.name = $"Audience_{m_spawnerIndex}_{widthIndex}_{lengthIndex}";

#if UNITY_EDITOR
                // Undo操作（Ctrl＋Z）に対応させる
                Undo.RegisterCreatedObjectUndo(audience, "Created Audience");
#endif
            }
        }
    }

    /// <summary>
    /// 共通の設定を使用して生成する場合
    /// </summary>
    private void SpawnAudienceForShare()
    {
        for (int widthIndex = 0; widthIndex < m_spawnWidthCount; widthIndex++)
        {
            // 横
            for (int lengthIndex = 0; lengthIndex < m_spawnLengthCount; lengthIndex++)
            {
                // ローカル空間での配置位置
                // 全体の長さの半分を引いて、中心を基準にする
                Vector3 localOffset = new Vector3(
                    -m_spawnLength / 2f + lengthIndex * (m_spawnLength / (m_spawnLengthCount - 1)),     // X軸：左から配置
                    (m_stepWidthPerColumn * widthIndex) + (m_stepLengthPerColumn * lengthIndex),        // Y軸：列が増えるごとに高さを足していく
                    -m_spawnWidth / 2f + widthIndex * (m_spawnWidth / (m_spawnWidthCount - 1))          // Z軸：前から配置
                );

                // ノイズをローカル空間で追加
                Vector3 transformNoise = new Vector3(
                    Random.Range(m_shareData.m_minNoiseLength, m_shareData.m_maxNoiseLength),
                    Random.Range(m_shareData.m_minNoiseHight, m_shareData.m_maxNoiseHight),
                    Random.Range(m_shareData.m_minNoiseWidth, m_shareData.m_maxNoiseWidth)
                );

                // 最終的な配置位置
                Vector3 localSpawnPos = localOffset + transformNoise;

                // ローカルからワールド変換
                Vector3 spawnPos = transform.TransformPoint(localSpawnPos);

                // 観客を生成
                GameObject audience = Instantiate(m_audiencePrefab, spawnPos, m_spawnRotation, this.transform);

                // サイリウムの取得
                // ペンライトと観客の場合
                Transform psylliumTransform = audience.transform.Find("Penlight/PsylliumHandle/Psyllium");

                if (psylliumTransform == null)
                {
                    // ペンライトのみの場合
                    psylliumTransform = audience.transform.Find("PsylliumPrefab/PsylliumHandle/Psyllium");
                }

                if (psylliumTransform != null)
                {
                    Renderer psylliumRenderer = psylliumTransform.GetComponent<Renderer>();

                    if (psylliumRenderer != null)
                    {
                        // リストの中からランダムに色を選ぶ
                        float randomColorIndex = Random.Range(0, m_shareData.m_psylliumColorList.Count);

                        // Random.Range は flaot で返されるので要素番号用に Int に変換
                        int colorIndex = Mathf.FloorToInt(randomColorIndex);

                        // インスタンスを明示的に生成
                        Material newMat = new Material(psylliumRenderer.sharedMaterial);

                        // HDRPやURPのマテリアルの変更は、newMat.color = Color.red; ではなくSetColorを使う
                        newMat.SetColor("_BaseColor",
                                        m_shareData.m_psylliumColorList[colorIndex]);

                        // Emissionを有効化
                        newMat.EnableKeyword("_EMISSION");

                        // エミッシブの色も変更
                        // HDRP用
                        newMat.SetColor("_EmissiveColor",
                                      m_shareData.m_psylliumColorList[colorIndex] * m_shareData.m_psylliumIntensity);

                        // 適用
                        psylliumRenderer.material = newMat;
                    }
                    else
                    {
#if UNITY_EDITOR
                        Debug.LogWarning("Psyllium の Renderer が見つかりません");
#endif
                    }
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogWarning("Psyllium のオブジェクトが見つかりません");
#endif
                }

                // 生成した観客リストに追加
                m_spawnedAudienceList.Add(audience);

                // 生成した観客に名前を設定
                audience.name = $"Audience_{m_spawnerIndex}_{widthIndex}_{lengthIndex}";

#if UNITY_EDITOR
                // Undo操作（Ctrl＋Z）に対応させる
                Undo.RegisterCreatedObjectUndo(audience, "Created Audience");
#endif
            }
        }
    }

    /// <summary>
    /// 子オブジェクトの観客を削除する
    /// </summary>
    public void DeleteAudience()
    {
#if UNITY_EDITOR
        Debug.Log($"オブジェクト名：{this.gameObject.name} の観客を削除します");
#endif

        // 子オブジェクトがいなければ早期リターン
        if (transform.childCount == 0)
        {
#if UNITY_EDITOR
            Debug.LogWarning("削除対象の観客が存在しません");
#endif
            return;
        }

        // 子オブジェクトの数を減少させる
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            // 子オブジェクトの取得
            Transform child = transform.GetChild(i);

            // 名前が "Audience_" で始まるオブジェクトを削除
            if (child.name.StartsWith("Audience_"))
            {
#if UNITY_EDITOR
                // Unityエディタで実行している場合はUndo対応させる
                // ゲームオブジェクトを即座に削除
                Undo.DestroyObjectImmediate(child.gameObject);
#else
        // Unityエディタ以外で実行している場合は通常の削除
        Destroy(child.gameObject);
#endif
            }
        }

        // リストも初期化
        m_spawnedAudienceList.Clear();

#if UNITY_EDITOR
        Debug.Log($"オブジェクト名：{this.gameObject.name}　観客をすべて削除しました");
#endif
    }

    // ギズモの処理
    #region GizmosProcess

    /// <summary>
    /// 常にギズモを表示
    /// </summary>
    void OnDrawGizmos()
    {
        if (m_gizmoType == GizmoType.Always)
        {
            DrawGizmos();
        }
    }

    /// <summary>
    /// オブジェクトを選択している場合のみギズモを表示する
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (m_gizmoType == GizmoType.SelectedOnly)
        {
            DrawGizmos();
        }
    }

    /// <summary>
    /// ギズモの表示を行う関数
    /// </summary>
    void DrawGizmos()
    {
        Gizmos.color = new Color(m_colour.r, m_colour.g, m_colour.b, m_colour.a);

        // オブジェクトの位置・回転・スケールを考慮してギズモを描画するために、Gizmos.matrix に localToWorldMatrix を設定
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        // 中心の位置とサイズを計算
        Vector3 center = Vector3.zero + new Vector3(0f, m_spawnHeight / 2f, 0f);
        Vector3 size = new Vector3(m_spawnLength, 0.1f, m_spawnWidth);

        // ギズモの描画
        Gizmos.DrawCube(center, size);

        // 別のオブジェクトも座標変換の影響を受けてしまうのを防止するためにギズモの設定を元に戻す
        Gizmos.matrix = originalMatrix;
    }


    #endregion
}
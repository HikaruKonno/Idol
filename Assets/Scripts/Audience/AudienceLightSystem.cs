/*
 * ファイル
 * AudienceLightSystem　C#
 * 
 * システム
 * 観客をAudienceBlock配列の情報を元に描画する
 * 
 * 1.JobSystemを用いてオブジェクト毎の動き（位置）と色を計算する
 * 2.計算した情報をGPUにコピー
 * 3.観客とペンライトの描画命令をGPUに出す
 * 4.描画
 * 
 * ※通常だとAudienceInfoに使用しているfloat2やint2が、インスペクターで配列の要素数を追加した際に初期値を設定できないため、
 * 　AudienceLightSystemEditor.csにてインスペクターの表示の設定を行っています。
 * 　その為インスペクターに表示する変数の追加、名前変更をした際はAudienceLightSystemEditor.csも対応して設定を行って下さい。
 * 
 * 変更履歴
 * 2025/10/10　奥山　凜　作成
 * 2025/10/31　奥山　凜　ガイド（ノーツ）の成功判定に合わせて観客のリアクションが増加するように変更
 */

using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// 観客をAudienceBlock配列の情報を元に描画する
/// </summary>
public class AudienceLightSystem : MonoBehaviour
{
    /// <summary>
    /// JobSystemでも使用するAudienceInfoには参照型のTransformを持たせられないので用意
    /// NativeArray作成時にAudienceInfo配列を渡す必要があるので実行後はAudienceBlock配列を解放しAudienceInfo配列とTransform配列に移す
    /// </summary>
    [System.Serializable]
    public struct AudienceBlock
    {
        public Transform SpawnPoint;            // 座席の中心
        public Transform TargetPoint;           // 波の中心やペンライトを振る向きとなる座標
        public AudienceInfo AudienceInfo;       // 座席の情報

        /// <summary>
        /// AudienceBlockを初期化する関数
        /// </summary>
        public static AudienceBlock CreateDefaultAudienceBlock()
        {
            return new AudienceBlock
            {
                SpawnPoint = null,
                TargetPoint = null,
                AudienceInfo = AudienceInfo.Default() // デフォルト値を設定
            };
        }
    }

    [SerializeField]
    private Mesh m_penlightMesh = null;             // ペンライトのメッシュ
    [SerializeField]
    private Material m_penlightMaterial = null;     // ペンライトのマテリアル
    [SerializeField]
    private Mesh m_humanMesh = null;                // 観客のメッシュ
    [SerializeField]
    private Material m_humanMaterial = null;        // 観客のマテリアル
    [SerializeField]
    private Vector3 m_shoulderOffset;               // 肩の位置のオフセット
    [SerializeField]
    private AudienceBlock[] m_audienceBlocks;       // ペンライトの集団の情報。実行時にAudienceInfoのみ、Transformのみの配列にコピー後破棄


    private Transform[] m_spawnPoints;                  // m_audienceBlocksからTransformをコピーしておく配列（参照型はJobシステムに渡せない為）
    private Transform[] m_targetPoints;                 // m_audienceBlocksからTransformをコピーしておく配列（参照型はJobシステムに渡せない為）
    private AudienceInfo[] m_audienceInfos;             // m_audienceBlocksからAudienceInfoをコピーしておく配列
    private int m_totalSeatCount = 0;                   // 全ブロックの合計席数

    private MaterialPropertyBlock m_matProps;       // 描画命令毎にシェーダーに追加データを送るのに使用。このコードではIDオフセットに使用
    private RenderParams m_penlightRenderParams;            // Graphics.RenderMeshInstancedでペンライトを描画する際に使用するマテリアルを指定するRenderParams
    private RenderParams m_humanRenderParams;               // Graphics.RenderMeshInstancedで人間を描画する際に使用するマテリアルを指定するRenderParams

    private AudienceAnimationJob m_job;                     // 観客毎のペンライトの振りのTransformと色を計算するJobシステム

    private NativeArray<AudienceInfo> m_config;             // Job Systemに渡すAudienceInfo
    private NativeArray<int> m_seatCountOffsets;            // Job Systemに渡す観客毎の番号
    private NativeArray<Matrix4x4> m_penlightMatrices;      // Job Systemに渡すペンライトのインスタンス毎のトランスフォーム
    private NativeArray<Color> m_penlightColors;            // 全ペンライトの色情報
    private NativeArray<Matrix4x4> m_humanMatrices;         // 観客（人間）のインスタンス毎のトランスフォーム
    private GraphicsBuffer m_penlightColorBuffer;           // 計算した色情報をCPUからGPUに転送するためのバッファ。このバッファをシェーダーが読み取ってペンライト毎の色をつける


    private static readonly float2 MAX_SWING_ANGLE_VARIANCE = math.float2(1.6f, 1.6f);      // ペンライトの振りの大きさの下限
    private static readonly float2 MIN_SWING_ANGLE_VARIANCE = math.float2(0.1f, 0.3f);      // ペンライトの振りの大きさの上限
    private const float EXCITEMENT_INCREMENT = 0.005f;                                    // 一回の判定成功での盛り上がりの上昇幅
    /// <summary>
    /// コンポーネント追加時やリセット時に、自動でm_audienceBlocksの一つ目の要素を生成
    /// </summary>
    private void Reset()
    {
        if (m_audienceBlocks == null || m_audienceBlocks.Length == 0)
        {
            m_audienceBlocks = new AudienceBlock[1];
            m_audienceBlocks[0] = AudienceBlock.CreateDefaultAudienceBlock();
        }
    }

    /// <summary>
    /// NativeArray、GraphicsBufferの解放
    /// </summary>
    void OnDestroy()
    {
        // メモリリークしないようNativeArrayとGraphicsBufferのメモリを解放
        m_config.Dispose();
        m_seatCountOffsets.Dispose();
        m_penlightMatrices.Dispose();
        m_penlightColors.Dispose();
        m_penlightColorBuffer.Dispose();
        m_humanMatrices.Dispose();
    }

    void Start()
    {
        if ((m_penlightMesh == null) || (m_penlightMaterial == null) || (m_humanMesh == null) || (m_humanMaterial == null))
        {
#if UNITY_EDITOR
            Debug.Log("必要なメッシュまたはマテリアルが設定されていません");
#endif
            enabled = false;
            return;
        }

        // m_audienceBlocks内の情報をAudienceInfoのみ、Transformのみの配列に移す
        {
            int index = m_audienceBlocks.Length;

            if (index == 0)
            {
#if UNITY_EDITOR
                Debug.Log("観客についての配列情報がありません");
#endif
                return;
            }

            m_spawnPoints = new Transform[index];
            m_targetPoints = new Transform[index];
            m_audienceInfos = new AudienceInfo[index];
            for (int i = 0; i < index; i++)
            {
                m_audienceInfos[i] = m_audienceBlocks[i].AudienceInfo;              // ペンライトの集団の情報をコピー
                m_spawnPoints[i] = m_audienceBlocks[i].SpawnPoint;                  // 座席の基準位置の情報をコピー
                m_targetPoints[i] = m_audienceBlocks[i].TargetPoint;        // ペンライトのwaveの中心点の情報をコピー

                m_audienceInfos[i].SpawnPoint = (float4x4)m_spawnPoints[i].localToWorldMatrix;
                m_audienceInfos[i].TargetPoint = m_targetPoints[i].position;
                m_audienceInfos[i].shoulderOffset = m_shoulderOffset;
                // 席数の総合数を加算
                m_totalSeatCount += m_audienceInfos[i].TotalSeatCount;
            }

            m_audienceBlocks = null;
        }

        // ペンライト、人間の色情報やTransformなどのメモリの確保
        {
            m_matProps = new MaterialPropertyBlock();

            m_penlightMatrices = new NativeArray<Matrix4x4>(m_totalSeatCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            m_penlightColors = new NativeArray<Color>(m_totalSeatCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            m_penlightColorBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, m_totalSeatCount, sizeof(float) * 4);      // GPUにデータを送るための_colorBufferを初期化
            m_penlightRenderParams = new RenderParams(m_penlightMaterial) { matProps = m_matProps };

            m_humanMatrices = new NativeArray<Matrix4x4>(m_totalSeatCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            m_humanRenderParams = new RenderParams(m_humanMaterial) { matProps = m_matProps };       // 人間には現在個体毎のマテリアルの違いは無いためpenlightの物を使いまわす
        }

        m_config = new NativeArray<AudienceInfo>(m_audienceInfos, Allocator.Persistent);                // AudienceInfoをJob Systemに渡せるようにする
        m_seatCountOffsets = new NativeArray<int>(m_audienceInfos.Length, Allocator.Persistent);        // Job Systemに渡す観客毎の番号
        int accumulatedSeats = 0;
        for (int i = 0; i < m_audienceInfos.Length; i++)
        {
            accumulatedSeats += m_audienceInfos[i].TotalSeatCount;
            m_seatCountOffsets[i] = accumulatedSeats;
        }


        // AudienceAnimationJobというJobを生成
        // 同時に必要な情報を渡す
        m_job = new AudienceAnimationJob()
        {
            Configs = m_config,
            SeatCountOffsets = m_seatCountOffsets,
            Time = Time.time,
            PenlightMatrices = m_penlightMatrices,
            PenlightColors = m_penlightColors,
            HumanMatrices = m_humanMatrices,
        };

        {
            NotesJudgmentUtility.RegistrySuccesCallBack(ExciteAudience);
        }
    }

    void Update()
    {
        // Job Systemによる位置と色の計算
        {
            // AudienceInfo内の位置情報（float4x4）をTransformを元に更新
            for (int i = 0; i < m_config.Length; i++)
            {
                m_audienceInfos[i].SpawnPoint = (float4x4)m_spawnPoints[i].localToWorldMatrix;
                m_audienceInfos[i].TargetPoint = m_targetPoints[i].position;
                m_config[i] = m_audienceInfos[i];
            }

            m_job.Time = Time.time;


            // Jobを実行し完了を待つ
            // Job内部では各ペンライトのマトリクスとカラーが計算され、m_matrices、m_colorsに書き込む
            // シート数（ペンライト数）の回数分計算する
            // 64はバッチサイズ。一つのコアがまとめて処理するペンライトの計算の数
            m_job.Schedule(m_totalSeatCount, 64).Complete();
        }

        // Jobで計算された色データをGPUが読み取れる_colorBufferにコピー
        m_penlightColorBuffer.SetData(m_penlightColors);
        // 対象のマテリアルにStringの名前で_colorBufferを設定。
        m_penlightMaterial.SetBuffer("_InstanceColorBuffer", m_penlightColorBuffer);

        // 描画命令（一度の描画命令で扱えるインスタンス数に上限があるため全体を分割して描画）
        int startseatCount = 0;
        // ペンライトの集団毎に描画
        // i = 開始位置
        for (int i = 0; i < m_audienceInfos.Length; i++)
        {
            int seatCount = m_audienceInfos[i].TotalSeatCount;

            m_matProps.SetInteger("_InstanceIDOffset", startseatCount);
            // 一ブロック描画
            Graphics.RenderMeshInstanced(m_penlightRenderParams, m_penlightMesh, 0, m_penlightMatrices, seatCount, startseatCount);
            Graphics.RenderMeshInstanced(m_humanRenderParams, m_humanMesh, 0, m_humanMatrices, seatCount, startseatCount);
            startseatCount += seatCount;
        }
    }


    /// <summary>
    /// ノーツの判定成功時のコールバックに紐づける<br/>
    /// ペンライトの振り幅など、観客のリアクションを大きくする<br/>
    /// </summary>
    /// <returns>なし</returns>
    void ExciteAudience()
    {
        for (int i = 0; i < m_audienceInfos.Length; i++)
        {
            m_audienceInfos[i].SwingAngleVariance = math.clamp(m_audienceInfos[i].SwingAngleVariance + math.float2(EXCITEMENT_INCREMENT, EXCITEMENT_INCREMENT), MIN_SWING_ANGLE_VARIANCE, MAX_SWING_ANGLE_VARIANCE);
        }
    }


    /// <summary>
    /// ノーツの判定失敗時のコールバックに紐づける<br/>
    /// ペンライトの振り幅など、観客のリアクションを小さくする<br/>
    /// </summary>
    /// <returns>なし</returns>
    void SootheAudience()
    {
        for (int i = 0; i < m_audienceInfos.Length; i++)
        {
            m_audienceInfos[i].SwingAngleVariance = math.clamp(m_audienceInfos[i].SwingAngleVariance - math.float2(EXCITEMENT_INCREMENT, EXCITEMENT_INCREMENT), MIN_SWING_ANGLE_VARIANCE, MAX_SWING_ANGLE_VARIANCE);
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// 観客の範囲を描画する
    /// </summary>
    private void OnDrawGizmos()
    {
        // インスペクターで設定されたm_audienceBlocksが無ければ何もしない
        if (m_audienceBlocks == null || m_audienceBlocks.Length == 0)
        {
            return;
        }

        // ギズモの色を設定（シアン、半透明）
        Gizmos.color = new Color(0.0f, 1.0f, 1.0f, 0.5f);

        // 設定された各AudienceBlockに対してループ処理
        foreach (var block in m_audienceBlocks)
        {
            // SpawnPointが設定されていない場合はスキップ
            if (block.SpawnPoint == null)
            {
                continue;
            }

            AudienceInfo info = block.AudienceInfo;

            // ギズモの座標系をSpawnPointのTransformに合わせる
            Gizmos.matrix = block.SpawnPoint.localToWorldMatrix;

            // 1. 座席ブロック1つあたりのサイズを計算
            float boxSizeX = info.SeatPerBlock.x * info.SeatPitch.x;
            float boxSizeZ = info.SeatPerBlock.y * info.SeatPitch.y;
            Vector3 singleBlockBoxSize = new Vector3(boxSizeX, 0.2f, boxSizeZ);

            // 2. 全ブロックと全通路を含めた「グリッド全体の合計サイズ」を計算
            float totalGridWidth = (info.BlockCount.x * boxSizeX) + Mathf.Max(0, info.BlockCount.x - 1) * info.AisleWidth.x;
            float totalGridDepth = (info.BlockCount.y * boxSizeZ) + Mathf.Max(0, info.BlockCount.y - 1) * info.AisleWidth.y;

            // 3. グリッドの描画開始位置（左奥の角）を計算
            //    (SpawnPointが中心なので、合計サイズの半分だけマイナス方向へ移動)
            float startX = -totalGridWidth / 2.0f;
            float startZ = totalGridDepth / 2.0f; // Zはプラスからマイナス方向へ描画

            // 4. 各ブロックを正しい位置に描画
            for (int y = 0; y < info.BlockCount.y; y++)
            {
                for (int x = 0; x < info.BlockCount.x; x++)
                {
                    // 現在のブロックの中心座標を計算
                    float centerX = startX + (boxSizeX / 2.0f) + x * (boxSizeX + info.AisleWidth.x);
                    float centerZ = startZ - (boxSizeZ / 2.0f) - y * (boxSizeZ + info.AisleWidth.y);

                    Vector3 center = new Vector3(centerX, 0.1f, centerZ);

                    // ワイヤーフレームの箱を描画
                    Gizmos.DrawWireCube(center, singleBlockBoxSize);
                }
            }
        }
    }
#endif
}






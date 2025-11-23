/*
 * ファイル
 * AudienceInfo　C#
 * 
 * システム
 * Job Systemに渡す観客の集団の情報
 * 
 * 変更履歴
 * 2025/10/10　奥山　凜　作成
 */

using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

/// <summary>
/// ペンライトの振る向き
/// </summary>
public enum SwingDirection { Horizontal, Vertical }

/// <summary>
/// Job Systemに渡す観客の集団の情報
/// </summary>
[System.Serializable]
public struct AudienceInfo
{
    public float4x4 SpawnPoint;     // 集団の位置の基準となる中心地。Job Systemでは参照型は扱えないのでTransformではなくfloat4x4
    public float3 shoulderOffset;   // 肩の位置のオフセット
    [Space]

    public int2 SeatPerBlock;       // 1ブロックに何席配置するか
    public float2 SeatPitch;        // 席と席の間隔

    [Space]     // インスペクター上の見た目に空白を開ける属性

    public int2 BlockCount;         // ブロックを全部でいくつ並べるか
    public float2 AisleWidth;       // ブロック間の通路の幅
    [Space]
    public float SwingFrequency;    // ペンライトを振るアニメーションの速さ
    public float SwingOffset;       // 腕の長さに相当するオフセット値

    [Header("Color Parameters (色の調整)")]
    public float BaseHue;           // ペンライトのベースの色
    public float2 HueVariance;      // 色相のばらつき幅

    [Header("Animation Variance (個体差の調整)")]
    public SwingDirection SwingMode;        // 振りの向き
    public float2 PositionVarianceXZ;       // 位置の水平方向のばらつき幅
    public float2 SwingAngleVariance;       // ペンライトを振る角度のばらつき幅
    public float2 ArmLengthVariance;        // 腕の長さ(オフセット)のばらつき幅


    [Header("Noise Parameters (ゆらぎの調整)")]
    public  float PhaseNoiseTimeScale;      // タイミングのゆらぎの時間変化スケール
    public float AxisNoiseTimeScale;        // 軸のゆらぎの時間変化スケール
    public float2 NoiseSeedRange;           // ゆらぎ生成用の乱数シードの範囲
    [Range(0f, 1f)]                         // 0から1のスライダーで調整できるようにする属性
    public float SwingAxisTiltFactor;       // 軸の傾き具合 (0=真横, 1=大きく傾く)

    [Header("Wave & LookAt Animation")]
    public bool EnableWaveAnimation;        // 観客のペンライトがターゲットを中心に波のように点滅するか
    public bool EnableLookAtTarget;         // 観客がターゲットを向くか
    public float3 TargetPoint;              // 波の中心やペンライトを振る向きとなる座標
    public float WaveSpeed;                 // ウェーブ（点滅）のスピード
    public float WaveFrequency;             // ウェーブ（点滅）の周波数
    public float WaveIntensity;             // ウェーブ（点滅）の色の強さ

    /// <summary>
    /// パラメータに適切な設定をするための初期値
    /// </summary>
    /// <returns>AudienceInfo</returns>
    public static AudienceInfo Default()
      => new AudienceInfo()
      {
          SpawnPoint     = 0f,
          shoulderOffset = Vector3.zero,

          SeatPerBlock = math.int2(12, 8),
          SeatPitch    = math.float2(0.4f, 0.8f),
          BlockCount   = math.int2(1, 1),
          AisleWidth   = math.float2(0.7f, 1.2f),
          SwingFrequency = 0.5f,
          SwingOffset    = 0.3f,

          BaseHue     = 300f / 360f,
          HueVariance = math.float2(-0.15f, 0.15f),

          SwingMode   = SwingDirection.Vertical,
          PositionVarianceXZ = math.float2(-0.01f, 0.01f),
          SwingAngleVariance = math.float2(0.3f, 0.5f),
          ArmLengthVariance  = math.float2(0.9f, 1.1f),

          PhaseNoiseTimeScale = 0f,
          AxisNoiseTimeScale  = 0f,
          NoiseSeedRange      = math.float2(0f, 0.4f),
          SwingAxisTiltFactor = 0.2f,

          EnableWaveAnimation = false,
          EnableLookAtTarget  = true,
          TargetPoint   = new float3(0, 0, 16),
          WaveSpeed     = 2.8f,
          WaveFrequency = 0.53f,
          WaveIntensity = 50f
      };

    /// <summary>
    /// 1ブロックあたりの合計隻数
    /// </summary>
    public int BlockSeatCount
      => SeatPerBlock.x * SeatPerBlock.y;

    /// <summary>
    /// 配置される座席の総数
    /// </summary>
    public int TotalSeatCount
      => SeatPerBlock.x * SeatPerBlock.y * BlockCount.x * BlockCount.y;

    /// <summary>
    /// 何ブロックの何番の席かという座標に変換する<br/>
    /// 通し番号から二次元的な位置を割り出す<br/>
    /// 引数1：_index 全体の通し番号<br/>
    /// </summary>
    /// <param name="_index">全体の通し番号</param>>
    /// <returns>何ブロックの何席か</returns>
    public (int2 block, int2 seat) GetCoordinatesFromIndex(int _index)
    {
        int si = _index / BlockSeatCount;
        int pi = _index - BlockSeatCount * si;

        int sy = si / BlockCount.x;
        int sx = si - BlockCount.x * sy;

        int py = pi / SeatPerBlock.x;
        int px = pi - SeatPerBlock.x * py;
        return (math.int2(sx, sy), math.int2(px, py));
    }

    /// <summary>
    /// GetCoordinatesFromIndexで求めたブロックと席の座標からローカルの2D平面座標を計算<br/>
    /// 席の間隔や通路の幅を考慮して全体の中心が原点に来るように配置<br/>
    /// 引数1：_block どのブロックか<br/>
    /// 引数2：_seat 何番目の席か<br/>
    /// </summary>
    /// <param name="_block">どのブロックか</param>
    /// <param name="_seat">何番目の席か</param>
    /// <returns>その席の平面上の位置</returns>
    public float2 GetPositionOnPlane(int2 _block, int2 _seat)
      => SeatPitch * (_seat - (float2)(SeatPerBlock - 1) * 0.5f)
         + (SeatPitch * (float2)(SeatPerBlock - 1) + AisleWidth) * (_block - (float2)(BlockCount - 1) * 0.5f);

    /// <summary>
    /// 人間のTransform（float4x4）とペンライトのTransform（float4x4）を求める
    /// 引数1：_positionOnPlane 席の平面上の位置
    /// 引数2：_time 時間
    /// 引数3：_seed 乱数の種
    /// </summary>
    /// <param name="_positionOnPlane">席の平面上の位置</param>
    /// <param name="_time">時間</param>
    /// <param name="_seed">乱数の種</param>
    /// <returns>人間のTransform（float4x4）、ペンライトのTransform（float4x4）</returns>
    public (float4x4 humanMatrix, float4x4 penlightMatrix)  GetHumanAndPenlightMatrix(float2 _positionOnPlane, float _time, uint _seed)
    {
        float4x4 humanMatrix = GetHumanMatrix(_positionOnPlane, _seed);
        float4x4 penlightMatrix = GetPenlightMatrix(humanMatrix, _time, _seed);

        return (humanMatrix, penlightMatrix);
    }

    /// <summary>
    /// 観客の人間の最終的なTransform（float4x4）を取得する<br/>
    /// 引数1：_positionOnPlane その座席の人間のワールド上の位置<br/>
    /// 引数2：_seed 時間<br/>
    /// </summary>
    /// <param name="_positionOnPlane"> その座席の人間のワールド上の位置</param>
    /// <param name="_seed"> 時間</param>
    /// <returns>観客の人間のTransform（float4x4）</returns>
    public float4x4 GetHumanMatrix(float2 _positionOnPlane, uint _seed)
    {
        // 各観客に固有の乱数ジェネレータを初期化
        Random rand = new Random(_seed);
        rand.NextUInt4(); // 最初の数値をいくつか進めて、後の乱数とパターンが重ならないようにする

        // 座席内のローカル位置を計算
        float3 origin = float3.zero;
        origin.xz = _positionOnPlane + rand.NextFloat2(PositionVarianceXZ) * SeatPitch;
        
        // ローカル位置をワールド座標に変換
        float3 humanWorldOrigin = math.transform(SpawnPoint, origin);

        // ワールド空間での体の向きを計算（ターゲットの方向を向く）
        quaternion humanWorldRotation = quaternion.identity; // デフォルトは無回転
        if (EnableLookAtTarget)
        {
            // 体の位置からターゲットへの方向ベクトルを計算
            float3 lookDirection = math.normalize(TargetPoint - humanWorldOrigin);
            // その方向を向くための回転を生成
            humanWorldRotation = quaternion.LookRotation(lookDirection, new float3(0, 1, 0));
        }

        // 観客用の最終的な行列（位置、回転、スケール）を返す
        return float4x4.TRS(humanWorldOrigin, humanWorldRotation, new float3(1, 1, 1));
    }

    /// <summary>
    /// 観客の位置からペンライトの振りや個体差を含めた、<br/>
    /// ペンライトの最終的なTransform（float4x4）を取得する<br/>
    /// 引数1：_humanMatrix その座席、人間のワールド上の位置<br/>
    /// 引数2：_time 時間<br/>
    /// 引数3：_seed 乱数の種<br/>
    /// </summary>
    /// <param name="_humanMatrix"> その座席、ペンライトのワールド上の位置</param>
    /// <param name="_time"> 時間</param>
    /// <param name="_seed"> 乱数の種</param>
    /// <returns>ペンライトのTransform（float4x4）</returns>
    private float4x4 GetPenlightMatrix(float4x4 _humanMatrix, float _time, uint _seed)
    {
        Random rand = new Random(_seed);
        rand.NextUInt4();

        // 振りのアニメーション計算
        float phase = 2 * math.PI * SwingFrequency * _time;
        float nr1 = rand.NextFloat(NoiseSeedRange.x, NoiseSeedRange.y);
        phase += noise.snoise(math.float2(nr1, _time * PhaseNoiseTimeScale));

        float angle = math.cos(phase);
        float angle_unsmooth = math.smoothstep(-1, 1, angle) * 2 - 1;
        angle = math.lerp(angle, angle_unsmooth, rand.NextFloat());
        angle *= rand.NextFloat(SwingAngleVariance.x, SwingAngleVariance.y);

        float nr2 = rand.NextFloat(NoiseSeedRange.x, NoiseSeedRange.y);
        float dx = noise.snoise(math.float2(nr2, _time * AxisNoiseTimeScale + 100));

        float3 axis;
        if (SwingMode == SwingDirection.Horizontal)
        {
            axis = math.normalize(math.float3(dx * SwingAxisTiltFactor, 0, 1));
        }
        else
        {
            axis = math.normalize(math.float3(1, 0, dx * SwingAxisTiltFactor));
        }

        float offset = SwingOffset * rand.NextFloat(ArmLengthVariance.x, ArmLengthVariance.y);

        // 人間のモデルの原点（足元）から肩までのローカルな移動
        float4x4 shoulderOffsetMatrix = float4x4.Translate(shoulderOffset);

        // 肩を基点とした振りの回転
        quaternion localSwingRotation = quaternion.AxisAngle(axis, angle);
        float4x4 swingRotationMatrix = new float4x4(localSwingRotation, float3.zero);

        // 腕の長さ分のローカルな移動
        float4x4 armOffsetMatrix = float4x4.Translate(new float3(0, offset, 0));

        // まずローカルでの変換をすべて合成します
        float4x4 combinedLocalMatrix = math.mul(math.mul(shoulderOffsetMatrix, swingRotationMatrix), armOffsetMatrix);

        // 最後に人間のワールド行列を掛け合わせます
        return math.mul(_humanMatrix, combinedLocalMatrix);
    }

    /// <summary>
    /// 一本のペンライトの色を計算<br/>
    /// 引数1：_penlightMatrix その座席のペンライトのワールド上の位置<br/>
    /// 引数2：_time 時間<br/>
    /// 引数3：_seed 乱数の種<br/>
    /// </summary>
    /// <param name="_penlightMatrix"> その座席のペンライトのワールド上の位置</param>
    /// <param name="_time"> 時間</param>
    /// <param name="_seed"> 乱数の種</param>
    /// <returns>そのペンの色</returns>
    public Color GetPenlightColor(float4x4 _penlightMatrix, float _time, uint _seed)
    {

        Random rand = new Random(_seed);
        rand.NextUInt4();

        float value = 1;
        
        if (EnableWaveAnimation)
        {
            // _finalMatrixからペンライトのワールド座標を抽出
            float3 worldPos = _penlightMatrix.c3.xyz;

            // ワールド座標で指定された波の中心(WaveOrigin)との距離を計算
            float wave = math.distance(worldPos.xz, TargetPoint.xz);
            wave = math.sin(wave * WaveFrequency - _time * WaveSpeed) * 0.5f + 0.5f;
            value = wave * wave * WaveIntensity + 0.1f;
        }
        
        return Color.HSVToRGB(Mathf.Clamp(BaseHue + rand.NextFloat(HueVariance.x, HueVariance.y), 0f, 1f), 1f, value);      // 鮮やかな色を生成
    }

}

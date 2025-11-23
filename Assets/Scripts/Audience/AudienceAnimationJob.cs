/*
 * ファイル
 * AudienceAnimationJob　C#
 * 
 * システム
 * 観客の個体毎のTransform（ペンライトのアニメーション含む）とペンライトの色計算をする
 * 
 * 変更履歴
 * 2025/10/10　奥山　凜　作成
 */

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

// UnityのBurstコンパイラがこのコードをCPUが直接実行できる超高速なネイティブコードに変換するための属性
[BurstCompile]
/// <summary>
/// 観客の個体毎のTransform（ペンライトのアニメーション含む）とペンライトの色計算をするAnimationJob
/// </summary>
struct AudienceAnimationJob : IJobParallelFor
{
    [NativeDisableParallelForRestriction]
    public NativeArray<AudienceInfo> Configs;       // 観客の情報
    
    [NativeDisableParallelForRestriction]
    public NativeArray<int> SeatCountOffsets;       // 各観客ブロックの座席数の累積和を格納した配列
                                                    // 例：ブロック0が100席、ブロック1が150席なら(100, 250)

    public float Time;                              // 時間

    // 出力データ
    [WriteOnly] public NativeArray<Matrix4x4> PenlightMatrices;       // ペンライトのTransform（Matrix4x4）情報を入れる場所
    [WriteOnly] public NativeArray<Matrix4x4> HumanMatrices;          // 観客の人間のTransform（Matrix4x4）情報を入れる場所
    [WriteOnly] public NativeArray<Color> PenlightColors;             // ペンライトの色情報を入れる場所

    /// <summary>
    /// 観客の個体毎のTransform（ペンライトのアニメーション含む）とペンライトの色計算をする
    /// </summary>
    /// <param name="i">処理対象のペンライトの、全体でのインデックス</param>
    /// <returns>なし</returns>
    public void Execute(int i)
    {
        // グローバルインデックスから、Configとローカルインデックスを特定
        int configIndex = 0;
        // 累積和の配列をチェックし、'i' がどの範囲に含まれるかで、どのConfigかを判断
        for (int j = 0; j < SeatCountOffsets.Length; j++)
        {
            if (i < SeatCountOffsets[j])
            {
                configIndex = j;
                break;
            }
        }

        // 該当するConfigを取得
        var config = Configs[configIndex];

        // そのConfig内でのローカルインデックスを計算
        // (全体インデックス - 前のブロックまでのインデックスの合計)
        int localIndex = i - (configIndex > 0 ? SeatCountOffsets[configIndex - 1] : 0);

        // ローカルインデックスを使って、座標と色を計算
        (int2 block, int2 seat) = config.GetCoordinatesFromIndex(localIndex);
        float2 pos = config.GetPositionOnPlane(block, seat);

        // 乱数のシード値は、全体でユニークになるようにグローバルインデックス 'i' を基に生成
        uint seed = (uint)i * 2u + 123u;

        // 計算結果を全体の出力配列に書き込む
        (float4x4 humanMatrix, float4x4 penlightMatrix) = config.GetHumanAndPenlightMatrix(pos, Time, seed++);

        PenlightMatrices[i] = penlightMatrix;
        HumanMatrices[i] = humanMatrix;
        PenlightColors[i] = config.GetPenlightColor(penlightMatrix, Time, seed++);
    }
}
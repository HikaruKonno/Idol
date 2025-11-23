/*
 * ファイル
 * Helper.hlsl
 * 
 * システム
 * 観客のペンライトのシェーダーグラフで使用
 * PUインスタンシングが有効な場合、バッファーから観客毎の色情報を取得する
 * PUインスタンシングが有効でない場合、固定色（赤）を返す
 * 
 * 変更履歴
 * 2025/10/10　奥山　凜　作成
 */

// GPUインスタンシングが有効な場合
#if UNITY_ANY_INSTANCING_ENABLED

// C#スクリプトから送られてくるインスタンス毎の色データ
StructuredBuffer<float4> _InstanceColorBuffer;
// C#スクリプトでバッチ処理（分割処理）をしている場合のインスタンスIDのオフセット値
uint _InstanceIDOffset;

// バッファーから対応するインデックスの色情報を取り出す
void GetInstanceColor_float(out float4 color)
{
    // バッファーの中から対応するインデックスの色情報を取り出す（現在描画しているインスタンスのID+オフセット値）
    color = _InstanceColorBuffer[unity_InstanceID + _InstanceIDOffset];
}
// GPUインスタンシングが有効でない場合
#else

// 固定色（赤）を返す
void GetInstanceColor_float(out float4 color)
{
    color = float4(1, 0, 0, 1);
}

#endif

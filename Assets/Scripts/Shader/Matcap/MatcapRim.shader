/*
 * ファイル
 * MatcapRIm.shader
 * 
 * 説明
 * Matcapとリムライトを組み合わせたシェーダー
 * Matcapはライトの影響を受けないため、リムライトで立体感を強調する
 * 現在使用しているオブジェクトはない
 */
Shader "Universal Render Pipeline/MatcapRim"
{
    Properties
    {
        [Header(Texture)]
        _MatcapTex ("Matcap Texture", 2D) = "white" {}

        [Header(Color)]
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)

        [Header(Rim)]
        _RimPower ("Rim Power", Range(0.1, 10.0)) = 2.0
        _RimIntensity ("Rim Intensity", Range(0, 5.0)) = 1.0
    }

    SubShader
    {
        // タグ設定
        Tags { "RenderPipeline" = "HDRenderPipeline" "RenderType" = "Opaque" }

        // ------------------------------------------------------------------
        // メイン描画パス
        // ------------------------------------------------------------------
        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "ForwardOnly" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // コアライブラリをインクルード
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Propertiesで定義した変数をCBUFFERにまとめる
            CBUFFER_START(UnityPerMaterial)
            float4 _MatcapTex_ST;
            float4 _BaseColor;
            float _RimPower;
            float _RimIntensity;
            CBUFFER_END

            TEXTURE2D(_MatcapTex);
            SAMPLER(sampler_MatcapTex);

            // 頂点シェーダーへの入力
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;      // 法線
            };

            // 頂点シェーダーからフラグメントシェーダーへの出力
            struct Varyings
            {
                float4 positionHCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;    // UV
                float3 normalWS     : TEXCOORD1;    // 法線
                float3 positionWS   : TEXCOORD2;    // ワールド座標
            };

            // 頂点シェーダー
            Varyings vert(Attributes IN)
            {
                Varyings OUT = (Varyings)0;
                // ワールド座標に変換
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);    
                // オブジェクト空間からクリップ空間へ変換
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                // 法線をオブジェクト空間からビュー空間へ変換
                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);
                float3 normalVS = mul((float3x3)UNITY_MATRIX_V, normalWS);  // UNITY_MATRIX_V（View Matrix）
                OUT.normalWS = normalWS;
                // ビュー空間法線のxy成分からMatcap用のUVを計算
                // 法線ベクトルの成分は-1～1の範囲なので、0～1の範囲に変換する
                OUT.uv = normalVS.xy * 0.5 + 0.5;

                return OUT;
            }

            // フラグメントシェーダー
            half4 frag(Varyings IN) : SV_Target
            {
                float3 normal = normalize(IN.normalWS);
                float3 viewDir = normalize(_WorldSpaceCameraPos - IN.positionWS);

                // リムライトの計算（フレネル）
                float rim = pow(1.0 - saturate(dot(normal, viewDir)), _RimPower);

                half4 matcapColor = SAMPLE_TEXTURE2D(_MatcapTex, sampler_MatcapTex, IN.uv);
                half3 finalColor = _BaseColor.rgb + matcapColor.rgb * rim * _RimIntensity;

                return half4(finalColor, _BaseColor.a);
            }
            ENDHLSL
        }
    }
}
/*
 * ファイル
 * Glass.shader
 * 
 * 説明
 * ガラスシェーダー
 * 屈折、反射、フレネル、色収差、模様を実装
 * 現在使用しているオブジェクトはない
 */
Shader "Custom/Glass"
{
    Properties
    {
        [Header(Color)]
        _Color ("Color", Color) = (1, 1, 1, 1)

        [Header(Refraection)]
        _RefractionPower ("Refraction Power", Range(0.1, 1.0)) = 0.3    // 反射の強さ
        _ChromaticAberration ("Chromatic Aberration", Range(0.0, 0.1)) = 0.01 // 色収差の強さ

        [Header(Fresnel)]
        _FresnelPower ("Fresnel Power", Range(0.1, 10.0)) = 3.0

        [Header(Enviroment)]
        _Cube ("Enviroment Cubemap", Cube) = "_Skybox" {}

        _MablePattern ("Marble Pattern", 2D) = "white" {}
        _PatternColor ("Pattern Color", Color) = (0.7, 0.7, 0.7, 1)
        _PatternStrength ("Pattern Strength", Range(0, 1)) = 0.5
    }

    SubShader
    {
        // タグ設定
        Tags { "RenderPipeline" = "HDRenderPipeline" "RenderType" = "Transparent" "Queue" = "Transparent" }

        // ------------------------------------------------------------------
        // メイン描画パス
        // ------------------------------------------------------------------
        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "ForwardOnly" }
            Blend SrcAlpha OneMinusSrcAlpha // アルファブレンド
            ZWrite Off      // 深度書き込みオフ
            Cull Front        // 前面カリング

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // コアライブラリをインクルード
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Propertiesで定義した変数をCBUFFERにまとめる
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            half4 _Color;
            float _RefractionPower;  // 反射の強さ
            float _ChromaticAberration; // 色収差の強さ
            float _FresnelPower;
            float4 _MablePattern_ST; // 模様用のUV座標
            half4 _PatternColor;      // 模様の色
            float _PatternStrength;   // 模様の強さ
            CBUFFER_END

            TEXTURECUBE(_Cube);
            SAMPLER(sampler_Cube);
            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);
            TEXTURE2D(_MablePattern);
            SAMPLER(sampler_MablePattern);

            // 頂点シェーダーへの入力
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
            };

            // 頂点シェーダーからフラグメントシェーダーへの出力
            struct Varyings
            {
                float4 positionHCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;    // UV
                float3 normalWS     : TEXCOORD1;    // 法線
                float3 positionWS   : TEXCOORD2;    // ワールド座標
                float4 screenPos    : TEXCOORD3;    // スクリーン座標
                float2 patternUV    : TEXCOORD4;    // 模様用のUV
            };

            // 頂点シェーダー
            Varyings vert(Attributes IN)
            {
                Varyings OUT = (Varyings)0;

                // 座標変換
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);    // ワールド座標に変換
                OUT.positionHCS = TransformWorldToHClip(OUT.positionWS);         // クリップ座標に変換
                // ワールド空間の法線を計算
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                // ワールド座標と視線方向の計算
                float3 worldViewDir = GetWorldSpaceViewDir(OUT.positionWS);
                // スクリーン座標を計算
                OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                OUT.patternUV = IN.uv * _MainTex_ST.xy + _MainTex_ST.zw; // テクスチャのUV座標を計算
                OUT.uv = IN.uv;

                return OUT;
            }

            // フラグメントシェーダー
            half4 frag(Varyings IN) : SV_Target
            {
                // 必要なベクトルを取得
                float3 normal = normalize(IN.normalWS);
                float3 viewDir = normalize(_WorldSpaceCameraPos - IN.positionWS);
                float3 reflectionVec = reflect(-viewDir, normal);

                // リムライトの計算（フレネル）
                float fresnel = pow(1.0 - saturate(dot(normal, -viewDir)), _FresnelPower);

                // 屈折色の計算
                // 法線ベクトルでUVをずらす
                float2 offset = normal.xy * _RefractionPower;
                // screenPos.xy / IN.screenPos.wでパースペクティブ補正したUV座標を求める
                float2 refractionUV = IN.screenPos.xy / IN.screenPos.w;
                // ずらしたUVで背景テクスチャをサンプリング
                half4 refractionColor = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, refractionUV + offset);

                // 反射色の計算
                float3 reflectionColor = SAMPLE_TEXTURE2D(_Cube, sampler_Cube, reflectionVec);

                // 色収差の計算
                float2 offsetR = normal.xy * (_RefractionPower + _ChromaticAberration);
                float2 offsetG = normal.xy * _RefractionPower;
                float2 offsetB = normal.xy * (_RefractionPower - _ChromaticAberration);

                // 色収差の適用
                half R = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, refractionUV + offsetR).r;
                half G = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, refractionUV + offsetG).g;
                half B = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, refractionUV + offsetB).b;
                refractionColor.rgb = half4(R, G, B, 1.0);

                // 模様の計算
                half4 patternColor = SAMPLE_TEXTURE2D(_MablePattern, sampler_MablePattern, IN.patternUV);
                half3 patternFinalColor = patternColor.rgb * _PatternColor.rgb; // 模様の色を乗算

                // 最終的な色を計算
                half3 finalColor = lerp(refractionColor.rgb, reflectionColor, fresnel);

                // 最終的な色に模様をブレンド
                finalColor = lerp(finalColor, patternFinalColor, patternColor.a * _PatternStrength); // パターンのアルファも考慮
                half finalAlpha = lerp(_Color.a, 1.0, fresnel);

                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
}
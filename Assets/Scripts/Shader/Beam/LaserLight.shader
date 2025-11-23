/*
 * ファイル
 * LaserLight.shader
 * 
 * 説明
 * レーザーライトのシェーダー
 * 基本的にはビームライトの処理と同じであるが、分割する処理を削減
 * 頂点シェーダーで頂点を押し出して円錐形に変形し、フラグメントシェーダーでビームの見た目を計算する
 * フレネルを利用して輪郭を光らす
 * ノイズテクスチャを利用してビームに揺らぎを加える
 */

Shader "HDRP/LaserLight"
{
    Properties
    {
        [Header(Texture)]
        _NoiseTex ("Noise Texture", 2D) = "white" {}

        [Header(Color)]    // 見た目
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Intensity ("Light Intensity", Range(0, 10)) = 1

        [Header(Size)]
        _BeamWidth ("Beam Width", Range(0.01, 0.5)) = 0.01
        _BeamLength ("Beam Length", Range(0.01, 3.0)) = 1.0

        [Header(Noise)]
        _NoiseSpeed ("Noise Speed", Range(-5.0, 5.0)) = 1.0
        _NoisePower ("Noise Power", Range(0.0, 1.0)) = 1.0

        [Header(Rim)]
        _RimPower ("Rim Power", Range(0.1, 10.0)) = 3.0
    }

    SubShader
    {
        // URP用のタグ設定
        Tags { "RenderPipeline" = "HDRenderPipeline" "RenderType" = "Transparent" "Queue" = "Transparent" }

        // ------------------------------------------------------------------
        // メイン描画パス (UniversalForward)
        // ------------------------------------------------------------------
        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "ForwardOnly" }
            Blend One One   // 加算合成
            ZWrite Off      // 深度書き込みオフ
            Cull Off        // 両面描画（光の内側と外側で表現を分ける為）

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // URPのコアライブラリをインクルード
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // 深度テクスチャをサンプリングするためのライブラリ
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            // Propertiesで定義した変数をCBUFFERにまとめる
            CBUFFER_START(UnityPerMaterial)
            float4 _NoiseTex_ST;
            half4 _Color;
            float _Intensity;
            float _BeamWidth;
            float _BeamLength;
            float _NoiseSpeed;
            float _NoisePower;
            float _RimPower;
            CBUFFER_END

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

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
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;    // UV
                float3 normalWS     : TEXCOORD1;    // 法線
                float3 positionWS   : TEXCOORD2;    // ワールド座標
            };

            // 頂点シェーダー
            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;

                // 頂点変形の計算
                // UVのY座標（高さ）に応じて、広がる量を計算
                // (1.0 - v.uv.y)で、モデルの根本が1、先端が0になるようにする
                float expansionAmount = (1.0 - v.uv.y) * _BeamWidth;

                // 計算した量だけ、法線方向に頂点を押し出して円錐形にする
                v.positionOS.xyz += v.normalOS.xyz * _BeamWidth;

                // Y軸（縦方向）にスケールを掛けて長さを調整する
                v.positionOS.y *= _BeamLength;

                // 座標変換
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);    // ワールド座標に変換
                o.positionCS = TransformWorldToHClip(o.positionWS);         // クリップ座標に変換

                // ワールド空間の法線を計算
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.uv = v.uv;
                return o;
            }

            // フラグメントシェーダー
            half4 frag(Varyings i) : SV_Target
            {
                float3 normal = normalize(i.normalWS);
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.positionWS);

                // リムライトの計算（フレネル）
                float rim = (pow(max(0, dot(normal, -viewDir)), _RimPower));

                // ノイズテクスチャをスクロールさせる
                float2 scrolledUV = i.uv;
                scrolledUV.y = frac(i.uv.y + _Time.y * _NoiseSpeed);
                // ノイズテクスチャをサンプリング
                half noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, scrolledUV).r;
                noise = lerp(1.0, noise, _NoisePower);

                // 最終的な色を計算
                half3 baseColor = _Color.rgb * _Intensity * rim * noise * pow(1.5, -_BeamWidth * 2);
                half4 col = half4(baseColor, 1.0);

                // ブルームが爆発しないように色をクランプし、最終的な色を渡す
                return clamp(col, 0, 3);
            }
            ENDHLSL
        }
    }
}
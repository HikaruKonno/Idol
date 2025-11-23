/*
 * ファイル
 * LEDLight.shader
 * 
 * 説明
 * LEDライトパネルのような表現をするシェーダー
 * テクスチャを使用してドット絵風にする
 */
Shader "HDRP/LEDLight"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PixShape ("Pixel Shape Texture", 2D) = "white" {}
        _UV_X ("Pixel num x", Range(10, 4000)) = 960
        _UV_Y ("Pixel num y", Range(10, 4000)) = 360
        _Intensity ("intensity", float) = 1
    }
    SubShader
    {
        // タグ設定
        Tags { "RenderPipeline" = "HDRenderPipeline" "RenderType"="Queue" }

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "ForwardOnly" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // コアライブラリをインクルード
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _PixShape_ST;
                float _UV_X;
                float _UV_Y;
                float _Intensity;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_PixShape);
            SAMPLER(sampler_PixShape);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 positionWS   : TEXCOORD1;
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                // 縦横何個並べるか（解像度）
                float2 uv_res = float2(_UV_X, _UV_Y);

                // テクスチャのピクセルの中心をサンプリング
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, (floor(i.uv * uv_res) / uv_res + (1 / (uv_res * 2))));
                float2 uv = i.uv * uv_res;
                
                // テクスチャサンプリング
                half4 pix = SAMPLE_TEXTURE2D(_PixShape, sampler_PixShape, uv);

                // カラーの計算
                half3 final_rgb = col.rgb * pix.rgb * _Intensity;
                half final_alpha = pix.a;

                return half4(final_rgb, final_alpha);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }

            ZWrite On
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // 必要なインクルードファイル
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitDepthNormalsPass.hlsl"
            ENDHLSL
        }
    }
}

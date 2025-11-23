/*
 * ファイル
 * PlanarReflection.shader
 * 
 * 説明
 * 平面反射を利用して、水面や鏡のような反射を表現するシェーダー
 * URP用に書き直したもの
 * URPからHDRPに変更したことによって使えなくなってしまった為、現在使用していない
 */
Shader "Custom/ReflectionOverlayURP"
{
    Properties
    {
        _Color("Base Color", Color) = (1, 1, 1, 0.5)
        _ReflectionTex("Reflection Texture", 2D) = "white" {}
        _reflectionFactor("Reflection Factor", Range(0, 1)) = 1.0
        _Roughness("Roughness (Blur)", Range(0, 1)) = 0.0
        _BlurRadius("Blur Radius", Range(0, 10)) = 5.0
    }
    SubShader
    {
        // 半透明オブジェクトとして扱うためのタグ
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            // アルファブレンディングを有効にする
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off // 奥にあるものが透けて見えるようにする

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _reflectionFactor;
                float _Roughness;
                float _BlurRadius;
            CBUFFER_END

            TEXTURE2D(_ReflectionTex);
            SAMPLER(sampler_ReflectionTex);

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float4 screenPos    : TEXCOORD1;
            };

            float gaussianWeight(float x, float sigma)
            {
                return exp(-(x * x) / (2.0 * sigma * sigma));
            }

            // ガウスぼかし関数
            half4 gaussianBlur(TEXTURE2D(tex), SAMPLER(sampler_tex), float2 uv, float blurAmount)
            {
                if (blurAmount <= 0.001)
                {
                    return SAMPLE_TEXTURE2D(tex, sampler_tex, uv);
                }
                half4 color = half4(0, 0, 0, 0);
                float totalWeight = 0.0;
                float2 texelSize = 1.0 / _ScreenParams.xy;
                int sampleCount = (int)lerp(3, 9, _Roughness);
                float stepSize = blurAmount * _BlurRadius;
                float sigma = stepSize * 0.5;
                for (int x = -sampleCount; x <= sampleCount; x++)
                {
                    for (int y = -sampleCount; y <= sampleCount; y++)
                    {
                        float2 offset = float2(x, y) * texelSize * stepSize;
                        float2 sampleUV = uv + offset;
                        if (saturate(sampleUV.x) == sampleUV.x && saturate(sampleUV.y) == sampleUV.y)
                        {
                            float distance = length(float2(x, y));
                            float weight = gaussianWeight(distance, sigma);
                            color += SAMPLE_TEXTURE2D(tex, sampler_tex, sampleUV) * weight;
                            totalWeight += weight;
                        }
                    }
                }
                if (totalWeight > 0.0) return color / totalWeight;
                return half4(0, 0, 0, 0);
            }

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.screenPos = ComputeScreenPos(o.positionCS);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float2 reflectionUV = float2(1.0 - screenUV.x, screenUV.y);
                
                half4 reflectionColor = gaussianBlur(_ReflectionTex, sampler_ReflectionTex, reflectionUV, _Roughness);

                // 反射とベースカラーをブレンドする
                // lerpの第3引数で反射の強さを制御
                half4 finalColor = lerp(_Color, reflectionColor, _reflectionFactor);
                
                // 最終的なアルファ値はBase Colorのアルファで制御
                finalColor.a = _Color.a;

                return finalColor;
            }
            ENDHLSL
        }
    }
}
/*
 * ファイル
 * Scroll.shader
 * 
 * 説明
 * スクロールするテクスチャを利用して、表面が動いているように見えるシェーダー
 * 現在使用しているオブジェクトはない
 */
Shader "Universal Render Pipeline/Scroll"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.1, 0.1, 0.1, 1)
        _NoiseTex ("Noise Texture (Grayscale)", 2D) = "gray" {}
        _ScrollSpeed ("Scroll Speed", Float) = 0.1
        [HDR] _EmissiveColor ("Emissive Color", Color) = (0, 1, 1, 1)
        _EmissiveIntensity ("Emissive Intensity", Range(1, 20)) = 5.0
        _Smoothness ("Smoothness", Range(0, 1)) = 0.9
        _Metallic ("Metallic", Range(0, 1)) = 0.9
    }

    SubShader
    {
        // タグ設定
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType"="Opaque" }

        // ------------------------------------------------------------------
        // メイン描画パス
        // ------------------------------------------------------------------
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // GIを正しく扱うためのプラグマ
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog

            // コアライブラリをインクルード
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Propertiesで定義した変数をCBUFFERにまとめる
            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            float4 _NoiseTex_ST;
            float _ScrollSpeed;
            half4 _EmissiveColor;
            half _EmissiveIntensity;
            half _Smoothness;
            half _Metallic;
            CBUFFER_END

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            // 頂点シェーダーへの入力
            struct Attributes
            {
                float4 positionOS   : POSITION;     // オブジェクト空間の位置
                float3 normalOS     : NORMAL;       // オブジェクト空間の法線
                float2 uv           : TEXCOORD0;    // UV座標
                // ライトマップ用のUVを受け取る
                float2 lightmapUV   : TEXCOORD1;
            };

            // 頂点シェーダーからフラグメントシェーダーへの出力
            struct Varyings
            {
                float4 positionCS   : SV_POSITION;  // クリップ空間の位置
                float2 uv           : TEXCOORD0;    // UV座標
                float3 positionWS   : TEXCOORD1;    // ワールド空間の位置
                float3 normalWS     : TEXCOORD2;    // ワールド空間の法線
                float4 shadowCoord  : TEXCOORD3; // シャドウ座標
            #ifdef LIGHTMAP_ON
                float2 lightmapUV   : TEXCOORD4;
            #endif
            };

            // 頂点シェーダー
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                // ヘルパー関数を使って位置情報を取得
                VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.positionOS.xyz);

                // 頂点位置と法線の両方の情報を取得
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionCS = TransformWorldToHClip(OUT.positionWS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _NoiseTex);

                // ヘルパー関数を使ってシャドウ座標を取得
                OUT.shadowCoord = GetShadowCoord(vertexInput);
            #ifdef LIGHTMAP_ON
                OUT.lightmapUV = IN.lightmapUV * unity_LightmapST.xy + unity_LightmapST.zw;
            #endif

                return OUT;
            }

            // フラグメントシェーダー
            half4 frag(Varyings IN) : SV_Target
            {
                // UVスクロール
                float2 scrolledUV1 = IN.uv + float2(_Time.y * _ScrollSpeed, 0);

                // ノイズテクスチャを読み出し、合成する
                half noiseValue1 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, scrolledUV1).r;
                half finalNoise = noiseValue1;

                half3 baseColor = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, scrolledUV1).rgb;

                // 発行色を計算する
                half3 emission = baseColor * _EmissiveIntensity;

                // ライティング計算に必要な情報を準備
                InputData inputData;
                inputData.positionWS = IN.positionWS;
                inputData.normalWS = normalize(IN.normalWS);
                inputData.viewDirectionWS = normalize(GetWorldSpaceNormalizeViewDir(IN.positionWS));
                
                // GIとシャドウの情報をヘルパー関数を使って取得
                #if defined(LIGHTMAP_ON)
                    inputData.bakedGI = SAMPLE_GI(IN.lightmapUV, IN.positionWS, inputData.normalWS);
                #else
                    inputData.bakedGI = SampleSH(inputData.normalWS);
                #endif
                
                // SHADOWS_SHADOWMASKが有効な時だけGetShadowMaskを呼び出す
                #if defined(SHADOWS_SHADOWMASK)
                    inputData.shadowMask = GetShadowMask(IN.shadowCoord);
                #else
                    inputData.shadowMask = float4(1.0, 1.0, 1.0, 1.0); // デフォルト値
                #endif

                SurfaceData surfaceData;
                surfaceData.albedo = _BaseColor.rgb;
                surfaceData.metallic = _Metallic;
                surfaceData.specular = half3(0.5, 0.5, 0.5);
                surfaceData.smoothness = _Smoothness;
                surfaceData.normalTS = float3(0, 0, 1);
                surfaceData.occlusion = 1.0;
                surfaceData.emission = emission;
                surfaceData.alpha = 1.0;
                surfaceData.clearCoatMask = 0.0h;
                surfaceData.clearCoatSmoothness = 0.0h;
                
                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                color.rgb = MixFog(color.rgb, inputData.positionWS); // フォグを適用
                return color;
            }
            ENDHLSL
        }
    }
}
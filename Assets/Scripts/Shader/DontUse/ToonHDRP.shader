/*
 * ファイル
 * ToonHDRP.shader
 * 
 * 説明
 * HDRP用のToonShader
 * 直前でURPからHDRPに変更した為、まだ未完成
 * そのため、Unity標準のToonShaderを使用中
 */
Shader "HDRP/ToonHDRP"
{
    Properties
    {
        [Header(Base)]
        _MainTex ("Texture", 2D) = "white" {}
        _LightColor ("Light Color", Color) = (1, 1, 1, 1)

        [Header(Shadow)]
        _ShadeMid ("Middle Shadow Color", Color) = (0.8, 0.8, 0.8, 1)
        _ShadeDark ("Dark Shadow Color", Color) = (0.5, 0.5, 0.5, 1)
        _Threshold1 ("Mid Threshold", Range(0, 1)) = 0.5
        _Threshold2 ("Dark Threshold", Range(0, 1)) = 0.2
        _ShadowBlur ("Shadow Blur", Range(0, 0.1)) = 0.05

        [Header(Skin Settings)]
        _SubsurfaceColor ("Subsurface Color", Color) = (1, 0.4, 0.2, 1)
        _SubsurfaceIntensity ("Subsurface Intensity", Range(0, 1)) = 0.1

        [Header(Highlight)]
        _HighlightColor ("Highlight Color", Color) = (1, 1, 1, 1)
        _HighlighThreshold ("Highlight Threshold", Range(0, 1)) = 0.7
        _HighlighSmoothness ("Highlight Smoothness", Range(0.01, 0.5)) = 0.1

        _SpecularGloss ("Specular Gloss", Range(1, 256)) = 20

        [Header(Hair Matcap)]
        _MatCapTex ("MatCap Texture", 2D) = "gray" {}
        _MatCapColor ("MatCap Color", Color) = (1, 1, 1, 1)
        _MatCapIntensity ("MatCap Intensity", Range(0, 1)) = 0.1

        [Header(Additional Light Settings)]
        _AddLightIntensity ("Additional Light Intensity", Range(0, 5)) = 1

        [Header(Face Light)]
        _FaceLightIntensity ("Face Light Intensity", Range(0, 1)) = 0.5

        [Header(Rimlight)]
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower ("Rim Power", Range(1, 10)) = 4
        _RimIntensity ("Rim Intensity", Range(0, 2)) = 1

        [Header(Outline)]
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline Width", Range(0.0, 0.001)) = 0

        [Toggle(_USE_FACE_MODE)] _UseSkin ("Face Mode", Float) = 0
        [Toggle(_USE_EYE_MODE)] _UseEye ("Eye Mode", Float) = 0
        [Toggle(_USE_CLOTH_MODE)] _UseCloth ("Cloth Mode", Float) = 0
        [Toggle(_USE_HAIR_MODE)] _UseHair ("Hair Mode", Float) = 0
        [Toggle(_USE_HIGHLIGHT_MODE)] _UseHighLight ("HighLight Mode", Float) = 0
    }
    SubShader
    {
        Tags 
        {
            "RenderType"="Opaque"
            "RenderPipeline"="HDRenderPipeline"
        }
        LOD 100

        Pass
        {
            Tags {"LightMode" = "Forward"}
            Cull Off // 両面描画
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

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
                float2 uv           : TEXCOORD0;
                float3 positionWS   : TEXCOORD1;
                float3 normalWS     : TEXCOORD2;
            };

            // Propertiesで定義した変数をCBUFFERにまとめる
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _LightColor;
                half4 _ShadeMid;
                half4 _ShadeDark;
                half _Threshold1;
                half _Threshold2;
                half _ShadowBlur;
                half4 _SubsurfaceColor;
                half _SubsurfaceIntensity;
                half4 _HighlightColor;
                half _HighlighThreshold;
                half _HighlighSmoothness;
                half _AddLightIntensity;
                half _SpecularGloss;
                half4 _MatCapTex_ST;
                half4 _MatCapColor;
                half _MatCapIntensity;
                half _FaceLightIntensity;
                half4 _RimColor;
                half _RimPower;
                half _RimIntensity;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            sampler2D _MatCapTex;


            // 頂点シェーダー
            Varyings vert(Attributes v)
            {
                Varyings o;
                // 頂点位置と法線の両方の情報を取得
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                o.uv = v.uv;
                return o;
            }

            half4 frag(Varyings i, bool isFrontFace : SV_IsFrontFace) : SV_Target
            {
                // テクスチャの色を取得
                half3 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).rgb;

                // 法線と視線の方向を計算
                // isFrontFaceがtrueなら正の法線、falseなら負の法線
                float3 normalDir = normalize(i.normalWS) * (isFrontFace ? 1.0 : -1.0);
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.positionWS);

                // メインライトを取得
                DirectionalLightData mainLight = _DirectionalLightDatas[0];
                float3 mainLightDir = -mainLight.forward;
                float3 mainLightColor = mainLight.color;
                
                // ライトと法線の内積を計算（saturate()は0～1にクランプする関数）
                float nl = saturate(dot(normalDir, mainLightDir));
                float lit = 0;
                half3 accumulatedLightColor = mainLightColor;
                half3 highlight = 0;

                // --- シェーダーモード別処理 ---
                half3 skinShade = 0;

                #if _USE_FACE_MODE
                    // メインライトによる陰影
                    float mainLightStrength = nl;
                    // 顔用の補助光（カメラからのライト）
                    float faceLightStrength = saturate(dot(normalDir, viewDir) * _FaceLightIntensity);
                    // メインライトと補助光を合成し、シャドウを適用
                    // mainLight.shadowAttenuationはセルフシャドウと落影の両方を考慮した値
                    
                    //lit = (mainLightStrength * mainLight.shadowAttenuation) + faceLightStrength;

                    // 顔のサブサーフェイスシェーディング
                    float subsurfaceFactor = saturate(nl);
                    //skinShade = _SubsurfaceColor.rgb * subsurfaceFactor * _SubsurfaceIntensity * mainLight.shadowAttenuation;

                    // ハイライトを表示しない
                    highlight = 0;
                #elif _USE_EYE_MODE
                    lit = 1.0; // 影なし
                    
                    // ハイライトを表示しない
                    highlight = 0;
                #elif _USE_CLOTH_MODE
                    //lit = nl * mainLight.shadowAttenuation;
                    lit = max(lit, 0.3); // 最小の明るさを保証
                    
                    highlight = 0; // ハイライトなし
                #else
                    //lit = nl * mainLight.shadowAttenuation;
                    
                    // サブサーフェイスシェーディングの計算
                    float subsurfaceFactor = saturate(nl);
                    //skinShade = _SubsurfaceColor.rgb * subsurfaceFactor * _SubsurfaceIntensity * mainLight.shadowAttenuation;

                    // --- ハイライト ---
                    // ライト方向と視線方向のハーフベクトルを計算
                    // ハーフベクトルは、光源方向と視線方向の平均ベクトル
                    // normalize()で正規化して単位ベクトルにする
                    // ハーフベクトルは、光源と視線の中間方向を表す
                    // これを使うことで、光沢のある表面のハイライトを計算できる
                    float3 halfDir = normalize(mainLightDir + viewDir);
                    // 法線とハーフベクトルの内積を取り、光沢度の鋭さを調整
                    float specFactor = pow(saturate(dot(normalDir, halfDir)), _SpecularGloss);
                    // smoothstepでハイライトをぼかす
                    float highlightFactor = smoothstep(_HighlighThreshold - _HighlighSmoothness, _HighlighThreshold + _HighlighSmoothness, specFactor);
                    // ハイライトの色を決定
                    highlight = highlightFactor * _HighlightColor.rgb * mainLightColor;
                    // 影の中ではハイライトが出ないようにする
                    //highlight *= mainLight.shadowAttenuation;
                #endif
                
                // --- 追加ライトの計算 ---
                // int additionalLightsCount = GetAdditionalLightsCount();
                // for (int j = 0; j < additionalLightsCount; ++j)
                // {
                //     // 追加ライトを取得
                //     Light addLight = GetAdditionalLight(j, i.positionWS);
                //     // 追加ライトの方向を取得
                //     float add_nl = saturate(dot(normalDir, addLight.direction));
                //     // 追加ライトの距離減衰を考慮
                //     float attenuation = addLight.distanceAttenuation * addLight.shadowAttenuation;
                //     float lightIntensity = add_nl * attenuation  * _AddLightIntensity;

                //     // 光の強度に加算する
                //     lit += lightIntensity;
                //     accumulatedLightColor += addLight.color * lightIntensity;
                // }

                // for (int j = 0; j < _PunctualLightCount; ++j) // HDRP ではこれ
                // {
                //     PunctualLightData addLight = _PunctualLightDatas[j];

                //     // 方向ベクトル（ワールド座標基準）
                //     float3 L = normalize(addLight.positionWS - i.positionWS);

                //     // Lambert の NdotL
                //     float add_nl = saturate(dot(normalDir, L));

                //     // 距離減衰（HDRP の場合 inverseSqrAttenuation を使う）
                //     float distanceAtten = rcp(1.0 + addLight.invSqrAttenuation * 
                //                              dot(addLight.positionWS - i.positionWS,
                //                                  addLight.positionWS - i.positionWS));
                //     accumulatedLightColor     += addLight.color * lightIntensity;
                // }

                // --- トゥーン陰影計算 ---
                /* litの値に基づいて、1影と2影の割合を滑らかに計算
                   _Threshold1：光と1影の境界
                   _Threshold2：1影と2影の境界
                   _ShadowBlur：ぼかし幅
                   smoothstep(min, max, value)
                   smoothstepは線形補間を滑らかにする関数
                   valueがminとmaxの間にあるとき、0.0から1.0へ滑らかに変化する値を返す*/
                float shadeFactor1 = smoothstep(_Threshold2 - _ShadowBlur, _Threshold2 + _ShadowBlur, lit);
                float shadeFactor2 = smoothstep(_Threshold1 - _ShadowBlur, _Threshold1 + _ShadowBlur, lit);

                // 1影と2影の色を補間
                half3 shade = lerp(_ShadeDark.rgb, _ShadeMid.rgb, shadeFactor1);
                // メインライトの色を考慮して最終的な陰影色を計算
                shade = lerp(shade, _LightColor.rgb, shadeFactor2);

                mainLightColor = mainLightColor * shade;
                
                // --- リムライト ---
                /*サーフェイスの法線と光の入射方向に依存するリムの強さを求める
                  リムライトの強さは内積の結果が-1.0に近づくほど強くなる
                  正規化された2つのベクトルの内積はなす角が0°なら1.0、90°なら-1.0、180°なら-1.0になる*/
                
                // リムの強さを計算（saturate()を使用する事で裏側の計算を無視）
                float rimDot = 1.0 - saturate(dot(viewDir, normalDir));
                // pow()を使用して、リムの幅の変化を指数関数的にする
                float rimFactor = pow(rimDot, _RimPower);
                // 最終的なリムの色と光を決定
                half3 rimLight = rimFactor * _RimColor.rgb * _RimIntensity;

                // --- 環境光 ---
                // half3 ambient = SampleSH(normalDir);
                // // 影の部分に近いほど環境光を弱くする
                // ambient *= lerp(_ShadeDark.rgb, mainLightColor1, shadeFactor2);

                // --- 最終的な色を合成 ---
                half3 finalColor = baseColor * (shade * accumulatedLightColor) + highlight + rimLight + skinShade;

                #if _USE_HAIR_MODE
                    // 髪の毛のマテリアルでは、マットキャップを使用
                    // 法線をワールド空間からビュー空間（カメラから見た空間）に変換
                    float3 viewNormal = mul((float3x3)UNITY_MATRIX_V, normalDir);
                    // ビュー空間法線のXY成分を、MatCapテクスチャをサンプリングするためのUV座標に変換
                    // .xyは -1～1の範囲なので、0～1の範囲にマッピングする
                    float2 matcapUV = viewNormal.xy * 0.5 + 0.5;

                    // 計算したUVでMatCapテクスチャをサンプリング
                    half3 matcapHighlight = tex2D(_MatCapTex, matcapUV).rgb;

                    // MatCapは全てのライティングを置き換える
                    // ベース色とMatCapの色、調整用の色を乗算して最終的な色とする
                    finalColor += matcapHighlight * _MatCapColor.rgb * _MatCapIntensity;
                #endif

                return half4(finalColor, 1.0);
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

        // ------------------------------------------------------------------
        // アウトラインパス
        // ------------------------------------------------------------------
        Pass
        {
            Name "Outline"
            ZTest On
            ZWrite On // Zバッファ書き込みオフ
            Cull Front // 前面をカリングして裏面だけ描画

            HLSLPROGRAM
            #pragma vertex vertOutline
            #pragma fragment fragOutline

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _OutlineColor;
                float _OutlineWidth;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            // 頂点シェーダーへの入力
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
            };

            // 頂点シェーダーからフラグメントシェーダーへの出力
            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
            };

            // アウトライン用の頂点シェーダー
            Varyings vertOutline(Attributes v)
            {
                Varyings o;
                // 法線方向に頂点を押し出す
                float3 positionOS = v.positionOS.xyz + v.normalOS * _OutlineWidth;

                // 押し出した頂点をクリップ空間に変換
                o.positionCS = TransformObjectToHClip(positionOS);
                return o;
            }

            // アウトライン用のフラグメントシェーダー
            half4 fragOutline(Varyings i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
        
        // ------------------------------------------------------------------
        // 影を落とすためのパス
        // ------------------------------------------------------------------
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On // Zバッファ書き込みをオンにする
            ZTest LEqual // ZテストをLessEqualに設定

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            #pragma multi_compile_instancing
    
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"


            ENDHLSL
        }
    }
}
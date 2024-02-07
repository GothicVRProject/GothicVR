Shader "Lit/AlphaToCoverage"
{
    Properties
    {
        _MainTex("Texture", 2DArray) = "" {}
        _Cutoff ("Cutoff", Range(0,1)) = 0.7
        _MipScale ("Mip scale", Range(0,1)) = 0.25
        _DistanceFade("Distance to wide coverage", Float) = 10
        [HideInInspector]_StationaryLightCount("Stationary light count", Int) = 0
    }
    SubShader
    {
        Tags {  "RenderType" = "TransparentCutout" "RenderPipeline" = "UniversalPipeline" "RenderQueue" = "AlphaTest" }
         AlphaToMask On

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 uv : TEXCOORD0;
                half3 color : COLOR;
                
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                float4 uv : TEXCOORD0;
                float distance : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                half3 diffuse : COLOR;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D_ARRAY(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_TexelSize;
                half _Cutoff;
                half _MipScale;
                float _DistanceFade;
                int _StationaryLightCount;
                float4x4 _StationaryLightIndices;
            CBUFFER_END

            #include "GothicIncludes.hlsl"
            #include "StationaryLighting.hlsl"

            half3 DiffuseLighting(half3 normal, float3 worldPos, half3 color)
            {
                half3 diffuse = SunAndAmbientDiffuse(normal, color);

                //for (int j = 0; j < min(MAX_VISIBLE_LIGHTS, unity_LightData.y); j++)
                //{
                //    int lightIndex = GetPerObjectLightIndex(j);
                //    Light light = CustomGetAdditionalPerObjectLight(lightIndex, i.worldPos);
                //    diffuse += AdditionalUnityLightDiffuse(light, i.normal);
                //}

                for (int k = 0; k < min(_StationaryLightCount, MAX_AFFECTING_STATIONARY_LIGHTS); k++)
                {
                    diffuse += AdditionalStationaryDiffuse(_StationaryLightIndices[k / 4][k % 4], worldPos, normal);
                }

                return diffuse;
            }

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float3 toCamera = TransformObjectToWorld(v.vertex) - _WorldSpaceCameraPos;
                o.distance = length(toCamera);
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = float4(v.uv.xy * REFERENCE_TEX_ARRAY_SIZE * _MainTex_TexelSize.xy, v.uv.zw);
                o.worldPos = TransformObjectToWorld(v.vertex);
                o.diffuse = DiffuseLighting(TransformObjectToWorldNormal(v.normal), o.worldPos, v.color);

                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float mipLevel = CalcMipLevel(i.uv.xy * _MainTex_TexelSize.zw);
                half4 albedo = SAMPLE_TEXTURE2D_ARRAY_LOD(_MainTex, sampler_MainTex, i.uv.xy, i.uv.z, clamp(mipLevel, 0, i.uv.w));
                // Rescale alpha by mip level since preserved coverage mip maps can't be generated at runtime.
                albedo.a *= 1 + max(0, CalcMipLevel(i.uv * _MainTex_TexelSize.zw)) * _MipScale;
                // Rescale alpha by partial derivative, faded by distance. This way, at a distance, the wide coverage is kept to reduce aliasing further.
                albedo.a = lerp((albedo.a - _Cutoff) / max(fwidth(albedo.a), 0.0001) + 0.5, albedo.a, saturate(max(i.distance, 0.0001) / _DistanceFade));

                half3 diffuse = albedo.rgb * i.diffuse;
                diffuse = ApplyFog(diffuse, i.worldPos);
                return half4(diffuse, albedo.a);
            }
            ENDHLSL
        }
    }
}

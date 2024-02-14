Shader "Lit/World"
{
    Properties
    {
        _MainTex("Texture", 2DArray) = "" {}
        [HideInInspector]_StationaryLightCount("Stationary light count", Int) = 0
    }
    SubShader
    {
        Tags {  "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "RenderQueue" = "Geometry" }

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
                half3 color : COLOR;
                half3 normal : NORMAL;
                float4 uv : TEXCOORD0; // uv, array slice, max mip level
                
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                half3 diffuse : COLOR;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D_ARRAY(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_TexelSize;
                int _StationaryLightCount;
                float4x4 _StationaryLightIndices;
                float4x4 _StationaryLightIndices2;
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
                if (_StationaryLightCount >= MAX_AFFECTING_STATIONARY_LIGHTS)
                {
                    for (int l = 0; l < min(_StationaryLightCount - MAX_AFFECTING_STATIONARY_LIGHTS, MAX_AFFECTING_STATIONARY_LIGHTS); l++)
                    {
                        diffuse += AdditionalStationaryDiffuse(_StationaryLightIndices2[l / 4][l % 4], worldPos, normal);
                    }
                }

                return diffuse;
            }

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.worldPos = TransformObjectToWorld(v.vertex);
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = float4(v.uv.xy * REFERENCE_TEX_ARRAY_SIZE * _MainTex_TexelSize.xy, v.uv.zw);
                o.diffuse = DiffuseLighting(TransformObjectToWorldNormal(v.normal), o.worldPos, v.color);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float mipLevel = CalcMipLevel(i.uv.xy * _MainTex_TexelSize.zw);
                half4 albedo = SAMPLE_TEXTURE2D_ARRAY_LOD(_MainTex, sampler_MainTex, i.uv.xy, i.uv.z, clamp(mipLevel, 0, i.uv.w));
                half3 diffuse = albedo * i.diffuse;
                
                diffuse = ApplyFog(diffuse, i.worldPos);
                return half4(diffuse, 1);
            }
            ENDHLSL
        }
    }
}

Shader "Lit/World"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        [HideInInspector]_StationaryLightCount("Stationary light count", Int) = 0
    }
    SubShader
    {
        Tags {  "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "RenderQueue" = "Geometry" }

        Pass
        {
            HLSLPROGRAM
            #define MAX_TOTAL_STATIONARY_LIGHTS 512
            #define MAX_AFFECTING_STATIONARY_LIGHTS 16
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 color : COLOR;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                float3 vertexLighting : COLOR;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                real3 diffuse : TEXCOORD2;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            float3 _SunDirection;
            real3 _SunColor;
            real3 _AmbientColor;
            real _PointLightIntensity;
            float4 _GlobalStationaryLightPositionsAndAttenuation[MAX_TOTAL_STATIONARY_LIGHTS];
            real3 _GlobalStationaryLightColors[MAX_TOTAL_STATIONARY_LIGHTS];

            CBUFFER_START(UnityPerMaterial)
                TEXTURE2D(_MainTex);
                SAMPLER(sampler_MainTex);
                float4 _MainTex_ST;
                float4 _MainTex_TexelSize;
                int _StationaryLightCount;
                float4x4 _StationaryLightIndices;
                float4x4 _StationaryLightIndices2;
            CBUFFER_END

            float CustomDistanceAttenuation(float distanceSqr, float rcpSqrRange)
            {
                return 1 - saturate(distanceSqr * rcpSqrRange);
            }

            Light CustomGetAdditionalPerObjectLight(int perObjectLightIndex, float3 positionWS)
            {
                // Abstraction over Light input constants
#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
                float4 lightPositionWS = _AdditionalLightsBuffer[perObjectLightIndex].position;
                half3 color = _AdditionalLightsBuffer[perObjectLightIndex].color.rgb;
                half4 distanceAndSpotAttenuation = _AdditionalLightsBuffer[perObjectLightIndex].attenuation;
                half4 spotDirection = _AdditionalLightsBuffer[perObjectLightIndex].spotDirection;
                uint lightLayerMask = _AdditionalLightsBuffer[perObjectLightIndex].layerMask;
#else
                float4 lightPositionWS = _AdditionalLightsPosition[perObjectLightIndex];
                half3 color = _AdditionalLightsColor[perObjectLightIndex].rgb;
                half4 distanceAndSpotAttenuation = _AdditionalLightsAttenuation[perObjectLightIndex];
                half4 spotDirection = _AdditionalLightsSpotDir[perObjectLightIndex];
                uint lightLayerMask = asuint(_AdditionalLightsLayerMasks[perObjectLightIndex]);
#endif

                // Directional lights store direction in lightPosition.xyz and have .w set to 0.0.
                // This way the following code will work for both directional and punctual lights.
                float3 lightVector = lightPositionWS.xyz - positionWS * lightPositionWS.w;
                float distanceSqr = max(dot(lightVector, lightVector), HALF_MIN);

                half3 lightDirection = half3(lightVector * rsqrt(distanceSqr));
                // full-float precision required on some platforms
                float attenuation = CustomDistanceAttenuation(distanceSqr, distanceAndSpotAttenuation.x) * AngleAttenuation(spotDirection.xyz, lightDirection, distanceAndSpotAttenuation.zw);

                Light light;
                light.direction = lightDirection;
                light.distanceAttenuation = attenuation;
                light.shadowAttenuation = 1.0; // This value can later be overridden in GetAdditionalLight(uint i, float3 positionWS, half4 shadowMask)
                light.color = color;
                light.layerMask = lightLayerMask;

                return light;
            }

            half3 AdditionalUnityLightDiffuse(Light light, real3 normal)
            {
                real diffuseDot = saturate(dot(light.direction, normal));
                return light.color * light.distanceAttenuation * diffuseDot * _PointLightIntensity;
            }

            half3 AdditionalStationaryDiffuse(uint lightIndex, real3 worldPos, real3 normal)
            {
                float4 lightPosAndAttenuation = _GlobalStationaryLightPositionsAndAttenuation[lightIndex];
                float3 lightVector = lightPosAndAttenuation.xyz - worldPos;
                float distanceSqr = max(dot(lightVector, lightVector), HALF_MIN);
                half3 lightDirection = half3(lightVector * rsqrt(distanceSqr));
                float diffuseDot = saturate(dot(lightDirection, normal));
                
                return _GlobalStationaryLightColors[lightIndex] * CustomDistanceAttenuation(distanceSqr, lightPosAndAttenuation.w) * diffuseDot * _PointLightIntensity;
            }

            half3 DiffuseLighting(v2f i)
            {
                half diffuseDot = saturate(dot(i.normal, -_SunDirection));
                half3  diffuse = saturate(diffuseDot * _SunColor * i.vertexLighting + _AmbientColor);

                //for (int j = 0; j < min(MAX_VISIBLE_LIGHTS, unity_LightData.y); j++)
                //{
                //    int lightIndex = GetPerObjectLightIndex(j);
                //    Light light = CustomGetAdditionalPerObjectLight(lightIndex, i.worldPos);
                //    diffuse += AdditionalUnityLightDiffuse(light, i.normal);
                //}

                for (int k = 0; k < min(_StationaryLightCount, MAX_AFFECTING_STATIONARY_LIGHTS); k++)
                {
                    diffuse += AdditionalStationaryDiffuse(_StationaryLightIndices[k / 4][k % 4], i.worldPos, i.normal);
                }
                if (_StationaryLightCount >= MAX_AFFECTING_STATIONARY_LIGHTS)
                {
                    for (int l = 0; l < min(_StationaryLightCount - MAX_AFFECTING_STATIONARY_LIGHTS, MAX_AFFECTING_STATIONARY_LIGHTS); l++)
                    {
                        diffuse += AdditionalStationaryDiffuse(_StationaryLightIndices2[l / 4][l % 4], i.worldPos, i.normal);
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
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = TransformObjectToWorldNormal(v.normal);
                o.vertexLighting = v.color;
                o.diffuse = DiffuseLighting(o);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half3 diffuse = i.diffuse;// DiffuseLighting(i);

                return half4(albedo.rgb * diffuse, 1);
            }
            ENDHLSL
        }
    }
}

Shader "Unlit/SkyboxWithHorizonFog"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Color2 ("Color for transition", Color) = (1,1,1,1)

        [Header(Sky Layer Settings)]
        _Sky1 ("1st Sky Layer", 2D) = "black" {}
        _Vector1 ("1st Layer Movement", Vector) = (0,0,0,0)
        _Alpha1 ("1st Layer Alpha", Range(0, 1)) = 1

        _Sky2 ("2nd Sky Layer", 2D) = "black" {}
        _Vector2 ("2nd Layer Movement", Vector) = (0,0,0,0)
        _Alpha2 ("2nd Layer Alpha", Range(0, 1)) = 1

        [Header(Sky Layer 2 Settings)]
        _Sky3 ("1st Sky Layer", 2D) = "black" {}
        _Vector3 ("1st Layer Movement", Vector) = (0,0,0,0)
        _Alpha3 ("1st Layer Alpha", Range(0, 1)) = 1

        _Sky4 ("2nd Sky Layer", 2D) = "black" {}
        _Vector4 ("2nd Layer Movement", Vector) = (0,0,0,0)
        _Alpha4 ("2nd Layer Alpha", Range(0, 1)) = 1

        [Header(Fog Settings)]
        _FogColor ("Fog Color", Color) = (1,1,1,1)
        _FogColor2 ("Fog Color for transition", Color) = (1,1,1,1)
        _FogCutoff ("Fog Cutoff", Range(0.0, 1.0)) = 0.25

        _Blend ("Blend value", Range(0,1)) = 0

    }
    SubShader
    {
        Tags
        {
            "QUEUE"="Background" "RenderType"="Background" "PreviewType"="Skybox"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        AlphaToMask Off
        ColorMask RGBA
        ZTest LEqual
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma shader_feature FUZZY
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "UnityShaderVariables.cginc"
            #define ASE_NEEDS_VERT_POSITION
            #pragma shader_feature_local _ENABLEFOG_ON
            #pragma shader_feature_local _ENABLEROTATION_ON

            struct appdata
            {
                float4 vertex : POSITION;
                float3 uv : TEXCOORD0;
            };

            struct v2f
            {
                float3 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD2;
                float4 screenPosition : TEXCOORD4;
                float4 screenSpaceLightPos0 : TEXCOORD5;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _Sky1, _Sky3;
            fixed4 _Color, _Color2;
            fixed4 _FogColor, _FogColor2;
            float _Alpha1, _Alpha3;
            float _FogCutoff, _Blend;
            float2 _Vector1, _Vector3;

            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // uv for the sky
                float2 skyUV = i.worldPos.xz / i.worldPos.y;

                float2 skyUV1 = (skyUV + _Vector1.xy * _Time.x);
                float2 skyUV3 = (skyUV + _Vector3.xy * _Time.x);

                //stars 1st layer
                float4 stars1 = lerp(tex2D(_Sky1, skyUV1) * _Alpha1, tex2D(_Sky3, skyUV3) * _Alpha3, _Blend);
                //stars 2nd layer

                if (i.worldPos.y < 0)
                {
                    stars1 = lerp(_Color, _Color2, _Blend);
                }

                float4 combined = stars1;

                // Calculate the fog color based on the horizon factor
                float horizonFactor = (i.uv.y > 0) ? smoothstep(_FogCutoff, _FogCutoff + 0.1f, i.uv.y) : 0.0f;
                float3 fogCol = lerp(lerp(_FogColor, _FogColor2, _Blend), combined, horizonFactor);

                // Apply Unity's built-in linear fog based on the fogCoords
                UNITY_APPLY_FOG_COLOR(i.fogCoord, combined, fogCol);
                return combined;
            }
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma shader_feature FUZZY
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "UnityShaderVariables.cginc"
            #define ASE_NEEDS_VERT_POSITION
            #pragma shader_feature_local _ENABLEFOG_ON
            #pragma shader_feature_local _ENABLEROTATION_ON

            struct appdata
            {
                float4 vertex : POSITION;
                float3 uv : TEXCOORD0;
            };

            struct v2f
            {
                float3 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD2;
                float4 screenPosition : TEXCOORD4;
                float4 screenSpaceLightPos0 : TEXCOORD5;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _Sky2, _Sky4;
            fixed4 _Color, _Color2;
            fixed4 _FogColor, _FogColor2;
            float _Alpha2, _Alpha4;
            float _FogCutoff, _Blend;
            float2 _Vector2, _Vector4;

            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // uv for the sky
                float2 skyUV = i.worldPos.xz / i.worldPos.y;

                float2 skyUV2 = (skyUV + _Vector2.xy * _Time.x);
                float2 skyUV4 = (skyUV + _Vector4.xy * _Time.x);

                float4 stars2 = lerp(tex2D(_Sky2, skyUV2) * _Alpha2, tex2D(_Sky4, skyUV4) * _Alpha4, _Blend);

                if (i.worldPos.y < 0)
                {
                    stars2 = lerp(_Color, _Color2, _Blend);
                }

                float4 combined = stars2;

                // Calculate the fog color based on the horizon factor
                float horizonFactor = (i.uv.y > 0) ? smoothstep(_FogCutoff, _FogCutoff + 0.1f, i.uv.y) : 0.0f;
                float3 fogCol = lerp(lerp(_FogColor, _FogColor2, _Blend), combined, horizonFactor);

                // Apply Unity's built-in linear fog based on the fogCoords
                UNITY_APPLY_FOG_COLOR(i.fogCoord, combined, fogCol);
                return combined;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
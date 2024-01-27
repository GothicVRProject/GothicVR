Shader "Unlit/SkyboxWithHorizonFog"
{
    Properties
    {
        [Header(Sky Layer Settings)]
        _Sky1("1st Sky Layer", 2D) = "black" {}
        _Vector1("1st Layer Movement", Vector) = (0,0,0,0)
        _Sky1Opacity("Sky 1 opacity", Range(0,1)) = 1
        _Sky2("2nd Sky Layer", 2D) = "black" {}
        _Vector2("2nd Layer Movement", Vector) = (0,0,0,0)
        _Sky2Opacity("Sky 2 opacity", Range(0, 1)) = 1
        _DomeColor1("Layer 1 dome color", Color) = (0,0,0,0)

        [Header(Sky Layer 2 Settings)]
        _Sky3 ("1st Sky Layer", 2D) = "black" {}
        _Sky3Opacity("Sky 3 opacity", Range(0,1)) = 1
        _Vector3 ("1st Layer Movement", Vector) = (0,0,0,0)
        _Sky4 ("2nd Sky Layer", 2D) = "black" {}
        _Vector4("2nd Layer Movement", Vector) = (0,0,0,0)
        _Sky4Opacity("Sky 4 opacity", Range(0,1)) = 1
        _DomeColor2("Layer 2 dome color", Color) = (0,0,0,0)

        _LayerBlend ("Layers blend value", Range(0,1)) = 0

        [Header(Fog Settings)]
        _FogColor ("Fog Color", Color) = (1,1,1,1)
        _FogColor2 ("Fog Color for transition", Color) = (1,1,1,1)
        _FogCutoff ("Fog Cutoff", Range(0.0, 1.0)) = 0.25

    }
    SubShader
    {
        Tags
        {
            "QUEUE"="Background" "RenderType"="Background" "PreviewType"="Skybox"
        }
        ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float3 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _Sky1, _Sky2, _Sky3, _Sky4;
            fixed4 _FogColor, _FogColor2, _DomeColor1, _DomeColor2;
            float _FogCutoff, _Sky1Opacity, _Sky2Opacity, _Sky3Opacity, _Sky4Opacity, _LayerBlend;
            float2 _Vector1, _Vector2, _Vector3, _Vector4;

            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 skyUV = i.worldPos.xz / i.worldPos.y;
                float2 skyUV1 = (skyUV + _Vector1.xy * _Time.x);
                float2 skyUV2 = (skyUV + _Vector2.xy * _Time.x);
                float2 skyUV3 = (skyUV + _Vector3.xy * _Time.x);
                float2 skyUV4 = (skyUV + _Vector4.xy * _Time.x);
                float4 sky1 = tex2D(_Sky1, skyUV1);
                float4 sky2 = tex2D(_Sky2, skyUV2);
                float4 sky3 = tex2D(_Sky3, skyUV3);
                float4 sky4 = tex2D(_Sky4, skyUV4);

                float3 blendedSkies1 = lerp(lerp(_DomeColor1.rgb, sky1.rgb, _Sky1Opacity * sky1.a), sky2.rgb,
                                        _Sky2Opacity * sky2.a);
                float3 blendedSkies2 = lerp(lerp(_DomeColor2.rgb, sky3.rgb, _Sky3Opacity * sky3.a), sky4.rgb,
                                                                _Sky4Opacity * sky4.a);
                float3 blendedSkies = lerp(blendedSkies1, blendedSkies2, _LayerBlend);

                // Calculate the fog color based on the horizon factor
                float horizonFactor = smoothstep(_FogCutoff, _FogCutoff + 0.1f, i.uv.y);
                half3 fogCol = lerp(_FogColor, _FogColor2, _LayerBlend);
                return half4(lerp(fogCol, blendedSkies, horizonFactor), 1);
            }
            ENDCG
        }
    }
}
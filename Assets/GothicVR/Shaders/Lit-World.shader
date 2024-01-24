Shader "Lit/World"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
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

                UNITY_VERTEX_OUTPUT_STEREO
            };

            float3 _SunDirection;
            float3 _SunColor;
            float3 _AmbientColor;

            CBUFFER_START(UnityPerMaterial)
                TEXTURE2D(_MainTex);
                SAMPLER(sampler_MainTex);
                float4 _MainTex_ST;
                float4 _MainTex_TexelSize;
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = TransformObjectToWorldNormal(v.normal);
                o.vertexLighting = v.color;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                
                half diffuseDot = saturate(dot(i.normal, -_SunDirection));
                half3  diffuse = saturate(diffuseDot * _SunColor * i.vertexLighting + _AmbientColor);

                return half4(col.rgb * diffuse, col.a);
            }
            ENDHLSL
        }
    }
}

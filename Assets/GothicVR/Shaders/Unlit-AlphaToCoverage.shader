Shader "Unlit/Unlit-AlphaToCoverage"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Cutoff ("Cutoff", Range(0,1)) = 0.7
        _MipScale ("Mip scale", Range(0,1)) = 0.25
        _DistanceFade("Distance to wide coverage", Float) = 10
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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float distance : TEXCOORD1;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                TEXTURE2D(_MainTex);
                SAMPLER(sampler_MainTex);
                float4 _MainTex_ST;
                float4 _MainTex_TexelSize;
                half _Cutoff;
                half _MipScale;
                float _DistanceFade;
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float3 toCamera = TransformObjectToWorld(v.vertex) - _WorldSpaceCameraPos;
                o.distance = length(toCamera);
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // https://bgolus.medium.com/anti-aliased-alpha-test-the-esoteric-alpha-to-coverage-8b177335ae4f
            float CalcMipLevel(float2 texture_coord)
            {
                float2 dx = ddx(texture_coord);
                float2 dy = ddy(texture_coord);
                float delta_max_sqr = max(dot(dx, dx), dot(dy, dy));

                return max(0.0, 0.5 * log2(delta_max_sqr));
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                // rescale alpha by mip level (if not using preserved coverage mip maps)
                col.a *= 1 + max(0, CalcMipLevel(i.uv * _MainTex_TexelSize.zw)) * _MipScale;
                // Rescale alpha by partial derivative, faded by distance. This way, at a distance, the wide coverage is kept to reduce aliasing further.
                col.a = lerp((col.a - _Cutoff) / max(fwidth(col.a), 0.0001) + 0.5, col.a, saturate(i.distance / _DistanceFade));
                return col;
            }
            ENDHLSL
        }
    }
}

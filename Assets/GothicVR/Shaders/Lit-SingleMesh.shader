Shader "Lit/SingleMesh"
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                half3 color : COLOR;
                half3 normal : NORMAL;
                half2 uv : TEXCOORD0;
                
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                half2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                half3 diffuse : COLOR;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                sampler2D _MainTex;
            CBUFFER_END

            #include "GothicIncludes.hlsl"

            half3 DiffuseLighting(half3 normal, float3 worldPos, half3 color)
            {
                half3 diffuse = SunAndAmbientDiffuse(normal, color);

                for (int j = 0; j < min(MAX_VISIBLE_LIGHTS, unity_LightData.y); j++)
                {
                    int lightIndex = GetPerObjectLightIndex(j);
                    Light light = CustomGetAdditionalPerObjectLight(lightIndex, worldPos);
                    diffuse += AdditionalUnityLightDiffuse(light, normal);
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
                o.uv = v.uv;
                o.diffuse = DiffuseLighting(TransformObjectToWorldNormal(v.normal), o.worldPos, v.color);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 albedo = tex2D(_MainTex, i.uv);
                half3 diffuse = albedo * i.diffuse;
                
                diffuse = ApplyFog(diffuse, i.worldPos);
                return half4(diffuse, 1);
            }
            ENDHLSL
        }
    }
}

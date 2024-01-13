Shader "Unlit/Barrier"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WaveIntensity ("Wave Intensity", Float) = 0.0
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True"
        }
        LOD 100

        Blend SrcAlpha One
        Cull Off Lighting Off ZWrite Off Fog
        {
            Color (0,0,0,0)
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _WaveIntensity;

            v2f vert(appdata v)
            {
                v2f o;
                float time = _Time.y;
                float waveStrength = (_WaveIntensity < 1) ? 1.0f : 1.5f;
                float3 waveParams = float3(0.15f, 0.6f, 1.4f) * waveStrength * 0.7f;
                float timeScaled = time * 0.015f;
                float timeFloor = floor(timeScaled);

                float waveX = sin(v.vertex.z + v.vertex.x * waveParams.z * 0.1f + waveParams.y + time);
                float waveY = sin(v.vertex.y * waveParams.z * 0.1f + waveParams.y + time);

                v.uv.x += waveX * waveParams.x + (_MainTex_ST.x * (timeScaled - timeFloor)) + waveStrength;
                v.uv.y += waveY * waveParams.x + (_MainTex_ST.y * (timeScaled - timeFloor)) + waveStrength;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Sample the texture
                fixed4 texColor = tex2D(_MainTex, i.uv);
                return texColor;
            }
            ENDCG
        }
    }
}
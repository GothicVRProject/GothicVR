Shader "Unlit/ThunderShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ShowThreshold ("Show Threshold", Range(0, 1)) = 0
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float visibility : TEXCOORD1; // Interpolated visibility value
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _ShowThreshold;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // Pass through a visibility value, which could be based on a vertex attribute or calculated.
                // For instance, if you have a custom vertex attribute that holds this value, pass it here.
                o.visibility = v.vertex.z; // This is an example; use the actual data you have.
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {

                if (i.uv.y < 1-_ShowThreshold)
                {
                    discard; // Discard the fragment if it is beyond the threshold.
                }
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
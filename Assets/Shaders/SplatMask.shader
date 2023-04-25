Shader "Unlit/SplatMask"
{
    Properties
    {
        _MainTex("Base (RGB)", 2D) = "black" {}
        _InkColor("Painter Color", Color) = (1,0,0,1)
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;


            float2 _SplatPos;
            float _Radius;
            float _Hardness;
            float _Strength;
            float4 _InkColor;
            float4 _TColor;
            float4 _PainterColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv0 : TEXCOORD0;
                float3 normal : NORMAL;
            };


            // Vert to Frag. Data passed from vertx to fragment shader.
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = v.normal; // Just passing data from the vert to frag shader.
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.uv = v.uv0;
                return o;
            }

            float mask(float2 pos, float2 center, float radius, float hardness) {
                float m = distance(center, pos);
                return 1 - smoothstep(radius * hardness, radius, m);
            }

            float4 frag(v2f i) : COLOR
            {
                _Radius = .1;
                _Hardness = .3;
                _Strength = .3;

                float4 col = tex2D(_MainTex, i.uv);
                float f = mask(i.uv, _SplatPos, _Radius, _Hardness);
                float edge = f * _Strength;
                return lerp(col, _InkColor, edge);
            }
            ENDCG
        }
    }
}

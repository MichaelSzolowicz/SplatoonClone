Shader "Custom/InkableSurfaceShader"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", 2D) = "black" {}
        _Metallic("Metallic", 2D) = "black" {}
        _Normal("Normal", 2D) = "black" {}
        _HeightMap("HeightMap", 2D) = "black" {}

        _Splatmap("Splatmap", 2D) = "black" {}

        _InkColor1 ("InkColor1", Color) = (1,0,0,1)
        _InkColor2 ("InkColor2", Color) = (0,1,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_Splatmap;
            float2 uv_Metallic;
            float2 uv_Glossiness;
            float2 uv_Normal;
            float2 uv_HeightMap;
            float4 _HeightMap_TexelSize;
        };


        sampler2D _Splatmap;
        float4 _InkColor1;
        float4 _InkColor2;

        sampler2D _Metallic;
        sampler2D _Glossiness; 
        sampler2D _Normal;
        sampler2D _HeightMap;
        float4 _HeightMap_TexelSize;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        float4 maskInk(float4 base, float4 target, Input IN)
        {
            float4 splatmap = tex2D(_Splatmap, IN.uv_Splatmap);
            float4 col = lerp(base, _InkColor1, splatmap.x);
            float4 temp = col;

            splatmap = tex2D(_Splatmap, IN.uv_Splatmap);
            col = lerp(temp, _InkColor2, splatmap.y);

            return col;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            float4 c = tex2D (_MainTex, IN.uv_MainTex);
            o.Albedo = maskInk(c, IN);

            float4 metallic = tex2D(_Metallic, IN.uv_Metallic);
            o.Metallic = maskInk(metallic, IN);

            float4 glossy = tex2D(_Glossiness, IN.uv_Glossiness);
            o.Smoothness = maskInk(glossy, IN);

            /*
            float4 normal;
            normal.xy = tex2D(_Normal, IN.uv_Normal.xy) * 2 -1;
            normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));
            o.Normal = normal;
            */

            float3 ts = float3(_HeightMap_TexelSize.xy, 0);

            float2 uv = IN.uv_HeightMap;
            float2 uv0 = uv + ts.xz;
            float2 uv1 = uv + ts.zy;
            float h = tex2D(_HeightMap, uv).r;
            float h0 = tex2D(_HeightMap, uv0).r;
            float h1 = tex2D(_HeightMap, uv1).r;

            float3 p0 = float3 (ts.xz, h0 - h);
            float3 p1 = float3 (ts.zy, h1 - h);

            float3 normal = normalize(cross(p0, p1));
            o.Normal = maskInk(float4(normal, 0), IN);

        }
        ENDCG
    }
    FallBack "Diffuse"
}

Shader "Custom/Gradient"
{
    Properties
    {
        _Color1 ("Color1", Color) = (1,1,1,1)
        _Color2 ("Color2", Color) = (1,1,1,1)
        _Color3 ("Color3", Color) = (1,1,1,1)
        _Color4 ("Color4", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        #include "FourColorGradient.cginc"

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        fixed4 _Color1;
        fixed4 _Color2;
        fixed4 _Color3;
        fixed4 _Color4;

        float3 FourColorGradient(float3 color1, float3 color2, float3 color3, float3 color4, float2 uv)
        {
            return lerp(
                lerp(color1, color2, uv.y),
                lerp(color3, color4, uv.y),
                uv.x
            );
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed3 gradient = FourColorGradient(
                _Color1.rgb, _Color2.rgb, _Color3.rgb, _Color4.rgb, 
                IN.uv_MainTex
            );
            o.Albedo = gradient;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

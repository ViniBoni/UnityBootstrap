Shader "Custom/Toon Shader" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Specular ("Specular", 2D) = "white" {}
        _Gloss ("Smoothness", Range(0,1)) = 0.5
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Scale", Float) = 1
        _LightThreshold("Threshold", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert

        sampler2D _MainTex;
        sampler2D _Specular;
        sampler2D _BumpMap;
        half _Gloss;
        half _BumpScale;
        fixed4 _Color;
        half _LightThreshold;

        struct Input {
            float2 uv_MainTex;
            float2 uv_Specular;
            float2 uv_BumpMap;
            INTERNAL_DATA
        };

        void surf (Input IN, inout SurfaceOutput o) {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Specular = tex2D(_Specular, IN.uv_Specular).rgb;
            o.Gloss = _Gloss;
            o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
            o.Normal = _BumpScale * o.Normal;
            o.Albedo = step(_LightThreshold, dot(o.Normal, _WorldSpaceLightPos0));
        }
        ENDCG
    }
    FallBack "Diffuse"
}


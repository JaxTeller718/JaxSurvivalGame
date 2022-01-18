// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Unlit alpha-blended shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "MapMaker/FogCircle" {
Properties {
    _Color("Main Color", Color) = (1,1,1,1)
    _Sharpness("_Sharpness", Range (0.0, 100.0)) = 1.0
}

SubShader {
    Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
    LOD 100

    ZWrite Off
    Blend DstAlpha Zero

    Pass {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            float4 _Color;
            float _Sharpness;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = _Color;
                float alphaX = i.texcoord.x * 2.0 - 1.0;
                float alphaY = i.texcoord.y * 2.0 - 1.0;
                float alpha = sqrt(alphaX * alphaX + alphaY * alphaY);
                col.a = col.a * alpha * pow(alpha, _Sharpness);
                col.a = clamp(col.a, 0.0, 1.0);
                return col;
            }
        ENDCG
    }
}

}

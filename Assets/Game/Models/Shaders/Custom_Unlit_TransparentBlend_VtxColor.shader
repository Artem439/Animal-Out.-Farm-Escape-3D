Shader "Custom/Unlit_TransparentBlend_VtxColor"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint (RGBA)", Color) = (1,1,1,1)
        [Enum(Alpha,0,Additive,1,Premultiply,2,Multiply,3)] _BlendMode ("Blend Mode", Float) = 0
        _Intensity ("Additive Intensity", Range(0, 5)) = 1
        _Brightness ("Alpha Brightness", Range(0, 5)) = 1
        _ScrollSpeed ("Scroll Speed", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
        }

        Cull Off
        Lighting Off
        ZWrite Off

        // Alpha (режим 0) — стандартная прозрачность
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _Intensity;
            float _Brightness;
            float4 _ScrollSpeed;
            float _BlendMode;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                // Scroll
                float2 scroll = _ScrollSpeed.xy * _Time.y;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex) + scroll;

                o.color = v.color;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 tex = tex2D(_MainTex, i.uv);
                float4 col = tex * _Color * i.color;

                // Alpha
                if (_BlendMode < 0.5)
                {
                    col.rgb *= _Brightness;
                    col.a *= _Brightness;
                }
                // Additive
                else if (_BlendMode < 1.5)
                {
                    col.rgb *= _Intensity;
                    col.a = 0;
                }
                // Premultiply
                else if (_BlendMode < 2.5)
                {
                    col.rgb *= col.a * _Brightness;
                }
                // Multiply
                else
                {
                    col.rgb *= col.rgb;
                    col.a *= _Brightness;
                }

                return col;
            }
            ENDHLSL
        }
    }
}
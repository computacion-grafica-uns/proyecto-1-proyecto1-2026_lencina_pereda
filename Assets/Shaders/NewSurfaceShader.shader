Shader "Custom/ShaderUnversal"
{
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _UseTexture ("Usa Textura", Float) = 0
    }
    SubShader {
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR; 
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            uniform float4x4 _ModelMatrix;
            uniform float4x4 _ViewMatrix; 
            sampler2D _MainTex;
            float _UseTexture;

            v2f vert (appdata v) {
                v2f o;
                float4x4 mvp = mul(mul(UNITY_MATRIX_P, _ViewMatrix), _ModelMatrix);
                o.vertex = mul(mvp, v.vertex); 
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                if (_UseTexture > 0.5) return tex2D(_MainTex, i.uv);
                return i.color;
            }
            ENDCG
        }
    }
}
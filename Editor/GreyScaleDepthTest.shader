Shader "Custom/GreyscaleDepthTest" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _DepthScale ("Depth Scale", Range(0, 1)) = 1
    }

    SubShader {
        Tags {"Queue"="Background" "RenderType"="Opaque"}

        Pass {
            Name "GREYOUT_PASS"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float depth : TEXCOORD1;
            };

            sampler2D _MainTex;
            float _DepthScale;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                UNITY_TRANSFER_DEPTH(o.depth);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float depth = Linear01Depth(i.depth);
                float4 texColor = tex2D(_MainTex, i.uv);
                float4 grey = lerp(texColor, 0.299 * texColor.r + 0.587 * texColor.g + 0.114 * texColor.b, _DepthScale * depth);
                return grey;
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}
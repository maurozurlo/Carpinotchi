Shader "Custom/GraffitiShader" {
    Properties{
        _MainTex("Base (RGBA)", 2D) = "white" {}
        _StencilTex("Stencil (RGB)", 2D) = "white" {}
    }
        SubShader{
            Tags {"Queue" = "Transparent" "RenderType" = "Opaque"}
            Pass {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                sampler2D _MainTex;
                sampler2D _StencilTex;
                float4 _MainTex_ST;

                v2f vert(appdata v) {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target {
                    fixed4 baseColor = tex2D(_MainTex, i.uv);
                    fixed4 stencilColor = tex2D(_StencilTex, i.uv);
                    // Use the alpha channel of the stencil texture as a mask for the base color
                    clip(stencilColor.a - 0.5);
                    return baseColor;
                }
                ENDCG
            }
    }
        FallBack "Diffuse"
}

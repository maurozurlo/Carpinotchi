Shader "Custom/board_shader" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_penX("Pen X", Range(0,1)) = 0.0
		_penY("Pen Y", Range(0,1)) = 0.0
		// This is a single render texture
		_MainTex("Albedo (RGB)", 2D) = "white" {}
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		fixed4 _Color;
		float _penX;
		float _penY;

		void surf(Input IN, inout SurfaceOutputStandard o) {
			float x = IN.uv_MainTex.x;
			float y = IN.uv_MainTex.y;

			float penSize = 0.01f;

			if (x > _penX && x < _penX + penSize && y > _penY && y < _penY + penSize) {
				o.Albedo = float4(1.0f, 1.0f, 1.0f, 1.0f);
			}
		}
		ENDCG
	}
}
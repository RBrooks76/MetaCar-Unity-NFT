Shader "Hidden/Sepia" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Strength ("Sepia", Float) = 0.0
	}
	SubShader {
		Cull Off ZWrite Off ZTest Always

		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#pragma target 2.0
			
			#include "UnityCG.cginc"

			sampler2D _MainTex;

			float _Strength;

			fixed4 frag (v2f_img i) : SV_Target {
				fixed4 tex = tex2D(_MainTex, i.uv);
	
				fixed Y = dot (fixed3(0.299, 0.587, 0.114), tex.rgb);

				fixed4 sepia = float4 (0.191, -0.054, -0.221, 0.0);
				fixed4 final = lerp (tex, (sepia + Y), _Strength);
				
				return final;
			}
			ENDCG
		}
	}
}

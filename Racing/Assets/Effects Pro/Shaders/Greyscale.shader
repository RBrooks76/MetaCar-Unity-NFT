Shader "Hidden/Greyscale" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Strength ("B&W", Float) = 0.0
    }

    SubShader {

        ZTest Always Cull Off ZWrite Off
        
        Pass {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag

            #pragma target 2.0

            #include "UnityCG.cginc"

            sampler2D _MainTex;

            float _Strength;

            fixed4 frag (v2f_img i) : COLOR {

                fixed4 tex = tex2D (_MainTex, i.uv);
				float4 average = (tex.r + tex.g + tex.b) / 3;
				fixed4 final = lerp (tex, average, _Strength);
                return final;
            }
            ENDCG
        }
    }
}
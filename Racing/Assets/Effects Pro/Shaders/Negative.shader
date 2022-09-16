Shader "Hidden/Negative" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
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

            fixed4 frag (v2f_img i) : COLOR {
                fixed4 col = tex2D (_MainTex, i.uv);
                col = 1 - col;
                return col;
            }
            ENDCG
        }
    }
}

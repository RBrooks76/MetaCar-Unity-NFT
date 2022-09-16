Shader "Hidden/Contrast&Brightness" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Cont ("Contrast", Float) = 0.0
        _Bright ("Brightness", Float) = 0.0
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

            float _Bright;
            float _Cont;

            fixed4 frag (v2f_img i) : COLOR {
                fixed4 tex = tex2D (_MainTex, i.uv);

                float f = (259 * (255 + _Cont)) / (255 * (259 - _Cont));

                tex.r = clamp ((f * (tex.r - 0.5) + 0.5), 0.0, 1.0);
                tex.g = clamp ((f * (tex.g - 0.5) + 0.5), 0.0, 1.0);
                tex.b = clamp ((f * (tex.b - 0.5) + 0.5), 0.0, 1.0);

				fixed4 bri = float4 (clamp (tex.r + _Bright, 0.0, 1.0), clamp (tex.g + _Bright, 0.0, 1.0), clamp (tex.b + _Bright, 0.0, 1.0), 1);
                return bri;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
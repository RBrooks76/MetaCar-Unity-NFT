// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "RGSK/Reflection"
{
	Properties
	{
		_MainAlpha("MainAlpha", Range(0, 1)) = 1
		_TintColor("Tint Color (RGB)", Color) = (1,1,1)
		_MainTex("MainTex (RGBA)", 2D) = ""
		_ReflectionTex("ReflectionTex", 2D) = "white" { }
	}

		Subshader
	{
		Tags{ Queue = Transparent }
		ZWrite On
		Colormask RGBA
		Color[_TintColor]
		Blend SrcAlpha OneMinusSrcAlpha

		Pass{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

	struct v2f
	{
		float2 uv : TEXCOORD0;
		float4 refl : TEXCOORD1;
		float4 pos : SV_POSITION;
	};

	float4 _MainTex_ST;

	v2f vert(float4 pos : POSITION, float2 uv : TEXCOORD0)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(pos);
		o.uv = TRANSFORM_TEX(uv, _MainTex);
		o.refl = ComputeScreenPos(o.pos);
		return o;
	}
	sampler2D _MainTex;
	sampler2D _ReflectionTex;
	float4 _TintColor;
	float _MainAlpha;

	fixed4 frag(v2f i) : SV_Target
	{
		fixed4 tex = tex2D(_MainTex, i.uv);
	fixed4 refl = tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(i.refl));
	float4 result = (tex + refl) * _TintColor;
	result.a = _MainAlpha;
	return result;
	}
		ENDCG
	}
	}
}
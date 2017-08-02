// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/TextureCoordinates/Base" {
	SubShader{
		Pass{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0

#include "UnityCG.cginc"

		struct vertexInput {
		float4 vertex : POSITION;
		float4 texcoord0 : TEXCOORD0;
	};

	struct fragmentInput {
		float4 position : SV_POSITION;
		float4 texcoord0 : TEXCOORD0;
	};

	fragmentInput vert(vertexInput i) {
		fragmentInput o;
		o.position = UnityObjectToClipPos(i.vertex);
		o.texcoord0 = i.texcoord0;
		return o;
	}
	float4 frag(fragmentInput i) : COLOR{
		return float4(i.texcoord0.xy,0.0,1.0);
	}
		ENDCG
	}
	}
}
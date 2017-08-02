// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/NewUnlitShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		// _ElectrodeCount("Electrode Count", float) = 64
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				//float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				//float2 uv : TEXCOORD0;
				float4 vertex : POSITION;
				float4 color : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform float cc[4];
			uniform int electrodeCount;
			uniform float4 dataColor[100];
			uniform float4 dataLocation[100];
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				//o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				//UNITY_TRANSFER_FOG(o,o.vertex);
				float4 color = float4(0, 0, 0, 0);
				for (int i = 0; i < electrodeCount; i++)
				{
					float4 dColor = dataColor[i];
					float4 point1 = mul(unity_ObjectToWorld, v.vertex);
					//float4 point2 = mul(unity_ObjectToWorld, dataLocation[i]);
					float4 point2 = dataLocation[i];
					float mag = distance(point1, point2);
					color = color + (1/(mag))*dColor;

				}

				//o.color = color;// v.vertex.xyz;
				//o.color = float4(cc[0], cc[1], cc[2], cc[3]);//v.vertex.xyz;
				o.color = color;
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				// sample the texture
				//fixed4 col = tex2D(_MainTex, i.uv);
				// apply fog
				//UNITY_APPLY_FOG(i.fogCoord, col);
				//float4 myC = float4(i.vertex.x * .01f, i.vertex.y*1.0,0.0,1.0);
				//return myC;
				return float4(i.color.xyz, .25);
			}
			ENDCG
		}
	}
}

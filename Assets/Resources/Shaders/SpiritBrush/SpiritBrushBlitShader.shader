﻿Shader "SpiritBrush/SpiritBrushBlitShader"
{
	Properties
	{
		_MainTex ("_MainTex", 2D) = "white" {}
		_DeltaTex ("_DeltaTex", 2D) = "black" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _DeltaTex;

			uniform float _AddSubtractSign = 1; 
			uniform int _ChannelID = 0;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				float4 deltaCol = tex2D(_DeltaTex, i.uv);
				
				if(_ChannelID == 0) {
					col.x = saturate(col.x + deltaCol.x * _AddSubtractSign);
				}

				if(_ChannelID == 1) {
					col.y = saturate(col.y + deltaCol.x * _AddSubtractSign);
				}

				if(_ChannelID == 2) {
					col.z = saturate(col.z + deltaCol.x * _AddSubtractSign);
				}

				if(_ChannelID == 3) {
					col.w = saturate(col.w + deltaCol.x * _AddSubtractSign);
				}
				// OLD:
				//col.rgb = saturate(col.rgb + deltaCol.rgb * _AddSubtractSign);
				
				return col;
			}
			ENDCG
		}
	}
}

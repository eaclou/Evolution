Shader "UI/NewInspectAgentCurActivityShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Tint ("_Tint", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		ZWrite Off
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
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

			sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform float4 _Tint;
			uniform float _Value;
			uniform int _CurActivityID;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				int frameIndex = _CurActivityID;
				float2 iconSheetTexUV = i.uv;
				iconSheetTexUV.x *= 1.0 / 8.0;
				iconSheetTexUV.x += frameIndex * (1.0 / 8.0);

				float4 finalColor = tex2D(_MainTex, iconSheetTexUV);
				
				return finalColor;
			}
			ENDCG
		}
	}
}

Shader "UI/NewInspectAgentAgeShader"
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

			uniform int _Age;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float ageF = (float)_Age + 1.0;  // avoid divide by 0
				float milestoneFract01 = saturate(100.0 / ageF);
				float milestoneFract02 = saturate(1000.0 / ageF);
				float milestoneFract03 = saturate(10000.0 / ageF);
				float milestoneFract04 = saturate(100000.0 / ageF);

				//float lineThickness = 0.25;
				float pixelAge = ageF * i.uv.x;
				float tickMask = (pixelAge % 500.0) / 500.0;
				float tickMaskB = 1.0 - saturate(abs(tickMask - i.uv.x) * 25);

				float mask01 = 1.0 - saturate(abs(milestoneFract01 - i.uv.x) * 50);
				float mask02 = 1.0 - saturate(abs(milestoneFract02 - i.uv.x) * 25);
				float mask03 = saturate(1.0 - abs(milestoneFract03 - i.uv.x) * 10);
				float mask04 = saturate(1.0 - abs(milestoneFract04 - i.uv.x) * 5);
				//float ageMask01 = saturate(_Value - i.uv.x);
				//float ageF * i.uv.x
				float maskTickMarks = saturate(fmod(100.0 * i.uv.x, 0.1) / ageF);   
				maskTickMarks = 1.0 - saturate(abs(maskTickMarks - i.uv.x) * 100);
				
				float4 finalCol = float4(mask01,mask02,mask03,1);
				finalCol.rgb += tickMaskB * 0.5;
				return finalCol;
			}
			ENDCG
		}
	}
}

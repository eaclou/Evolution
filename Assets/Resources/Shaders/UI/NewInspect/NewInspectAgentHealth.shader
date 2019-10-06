Shader "UI/NewInspectAgentHealthShader"
{
	Properties
	{
		_MainTex ("_MainTex", 2D) = "white" {}
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
			uniform float _HealthHead;
			uniform float _HealthBody;
			uniform float _HealthExternal;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float4 texColor = tex2D(_MainTex, i.uv);
				float distToOrigin = length((i.uv - 0.5) * 2);
				float fillAreaMask = 1.0 - saturate((distToOrigin - _HealthHead) * 25);
				float alpha = 1.0 - saturate((distToOrigin - 0.975) * 25);
				
				fixed4 col = float4(0,0,0,max(max(texColor.x, texColor.y), texColor.z));
				
				float3 bodyHealthHue = lerp(float3(0.5,0,0), float3(0, 1,0), _HealthBody);
				float3 headHealthHue = lerp(float3(0.5,0,0), float3(0, 1,0), _HealthHead);
				float3 externalHealthHue = lerp(float3(0.5,0,0), float3(0, 1,0), _HealthExternal);
				col.rgb = lerp(col.rgb, bodyHealthHue, texColor.b);
				col.rgb = lerp(col.rgb, headHealthHue, texColor.g);
				col.rgb = lerp(col.rgb, externalHealthHue, texColor.r);
				col.rgb *= 1.33;

				return col;
			}
			ENDCG
		}
	}
}

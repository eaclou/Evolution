Shader "Unlit/FluidColliderShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "black" {}
		_VelX ("_VelX", Float) = 0.0
		_VelY ("_VelY", Float) = 0.0
	}
	SubShader
	{
		Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" "IgnoreProjector"="True" }
		LOD 100

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
			uniform float _VelX;
			uniform float _VelY;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float2 coords = float2((i.uv - 0.5) * 2.0);
				float circle = ceil(1.0 - saturate(length(coords)));
				float4 outColor = float4(_VelX, _VelY, 1, circle);	
				float alphaCutoffValue = 0.1;
				clip(outColor.a - alphaCutoffValue);
				return outColor;
			}
			ENDCG
		}
	}
}

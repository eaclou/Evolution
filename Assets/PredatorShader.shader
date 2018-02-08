Shader "Unlit/PredatorShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				//UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				//UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{				
				fixed4 outColor = float4(0.5, 0, 0, 1);
				const float PI = 3.141592;
				
				float2 coords = float2((i.uv - 0.5) * 2.0);
				float circle = ceil(1.0 - saturate(length(coords)));
				float distToOrigin = length(coords);

				float2 pixelDirection = normalize(coords);
				float angleRadians = 0;
				if(pixelDirection.y < 0) {
					angleRadians = -acos(pixelDirection.x);
				}
				if(pixelDirection.y > 0) {
					angleRadians = acos(pixelDirection.x);
				}
				//angleRadians += _Time.y;

				float distortionMask = (sin(angleRadians * 4.3 + _Time.y * 7.0) * 0.5 + 0.5);
				float distortionStrength = 2.3;

				//angleRadians -= distortionStrength * distortionMask;

				float minDist = 0.75;
				float maxDist = 1.1;
				float frequency = 15;
				float rawHeight01 = cos(angleRadians * frequency + distortionStrength * distortionMask) * 0.5 + 0.5;
				float shrinkage = (1 - distortionMask) * 0.3 + 0.8;
				float radius = rawHeight01 * rawHeight01 * shrinkage;
				radius = radius * (maxDist - minDist) + minDist;
				float alpha = saturate(radius - distToOrigin);

				float3 startHue = float3(0.35, 0.0, 0.0);
				float3 endHue = float3(1.2, 0.7, 0.0);

				outColor.rgb = lerp(startHue, endHue, distToOrigin * distToOrigin * distToOrigin);

				outColor.a = alpha;
				float alphaCutoffValue = 0.1;
				clip(outColor.a - alphaCutoffValue);
				return outColor;
			}
			ENDCG
		}
	}
}

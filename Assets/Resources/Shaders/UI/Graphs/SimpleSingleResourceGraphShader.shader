Shader "UI/SimpleSingleResourceGraphShader"
{
	Properties
	{
		_DataTex ("_DataTex", 2D) = "black" {}
		_Tint ("_Tint", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" }
		//ZWrite Off
		//Cull Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				float4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
			};

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
#ifdef UNITY_HALF_TEXEL_OFFSET
				OUT.vertex.xy += (_ScreenParams.zw-1.0)*float2(-1,1);
#endif
				OUT.color = IN.color;
				return OUT;
			}

			sampler2D _DataTex;		
			uniform float4 _Tint;
			uniform float _MaxValue;
			uniform float _MinValue;

			uniform float _SampleCoordMax;
						
			fixed4 frag(v2f IN) : SV_Target
			{
				
				float _ZoomFactorX = 1.0;
				float _ZoomFactorY = 1.0;
								
				float lineWidth = 0.005; // * _ZoomFactorY;
				float lineFadeWidth = lineWidth * 12;
				float gridLineWidthX = 0.001 * _ZoomFactorX;	
				float gridLineWidthY = 0.001 * _ZoomFactorY;	
				float gridDivisions = 10;	
				float gridLineSpacing = 1 / gridDivisions;
				
				half2 finalCoords = IN.texcoord;
				
				half4 bgColor = half4(0, 0, 0, 0.33);
				half4 onColor = half4(0.24, 0.33, 0.26, 1.0);
				half4 centerLineColor = half4(0.5, 0.5, 0.5, 1.0);
				half4 gridColor = half4(0.14, 0.14, 0.14, 1.0);
												
				half4 pixColor = bgColor;
						
					
				//OLD//   float4 fitnessScores = saturate(sqrt(tex2D(_DataTex, float2(finalCoords.x * _SampleCoordMax, 0.5)) / _MaxValue));
				float4 fitnessScores = tex2D(_DataTex, float2(finalCoords.x * _SampleCoordMax, 0.5)); // / _MaxValue;
				float score = (fitnessScores.x - _MinValue) / (_MaxValue - _MinValue);
				
				float distR = abs(finalCoords.y - score);
				
				//  smoothstep( min , max , x )
				//  For values of x between min and max , returns a smoothly varying value that ranges from 0 at x = min to 1 at x = max .
				//  x is clamped to the range [ min , max ] and then the interpolation formula is evaluated:
				
				float isSolidMask = saturate((score - finalCoords.y) * 100);
				pixColor.rgb = lerp(pixColor.rgb, _Tint.rgb, 0.5 * isSolidMask);

				if(distR < (lineWidth + lineFadeWidth)) {
					float smoothDist = smoothstep(0.0, lineFadeWidth, distR - lineWidth);
					pixColor = lerp(_Tint, pixColor, smoothDist) * 1;
				}				
				
				return pixColor;
			}
			ENDCG
		}
	}
}


Shader "Unlit/FitnessDisplayShader"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "black" {}
		_BestScore ("_BestScore", Float) = 1.0		
	}

	SubShader
	{
		Tags
		{ 
			"RenderType"="Opaque"
		}
		
		//Cull Off
		//Lighting Off
		//ZWrite Off
		//ZTest [unity_GUIZTestMode]
		//Blend SrcAlpha OneMinusSrcAlpha
		//ColorMask [_ColorMask]

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
			
			//uniform fixed4 _Color;
			//uniform fixed _ZoomFactorX;
			//uniform fixed _ZoomFactorY;
			//uniform fixed _WarpFactorX;
			//uniform fixed _WarpFactorY;

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

			sampler2D _MainTex;
			fixed _BestScore;
			//sampler2D _FitnessTex;
			//sampler2D _FitnessTexCurrent;

			fixed4 frag(v2f IN) : SV_Target
			{
			    // TEMPORARY!!!!!
				//fixed tempZoomX = 0.5;  // Replace with _ZoomFactorX !!
				//fixed tempZoomY = 0.2;  // Replace with _ZoomFactorY !!!

				float _ZoomFactorX = 1.0;
				float _ZoomFactorY = 1.0;
								
				float lineWidth = 0.002; // * _ZoomFactorY;
				float lineFadeWidth = lineWidth * 12;
				float gridLineWidthX = 0.001 * _ZoomFactorX;	
				float gridLineWidthY = 0.001 * _ZoomFactorY;	
				float gridDivisions = 10;	
				float gridLineSpacing = 1 / gridDivisions;
				
				half2 finalCoords = IN.texcoord;
				//float initialScore = tex2D(_FitnessTex, 0).b;
				//float latestScore = tex2D(_FitnessTex, 1).x;
				float pivotY = 0.5; //(initialScore + latestScore) / 2;
				if((pivotY + (_ZoomFactorY * 0.5)) > 1) {
					pivotY = 1 - (_ZoomFactorY * 0.5);
				}
				if((pivotY - (_ZoomFactorY * 0.5)) < 0) {
					pivotY = (_ZoomFactorY * 0.5);
				}
				finalCoords.x = (finalCoords.x * _ZoomFactorX) + (1 - _ZoomFactorX);
				finalCoords.y = (finalCoords.y * _ZoomFactorY) + (pivotY - (_ZoomFactorY * 0.5));
				
				half4 bgColor = half4(0.21, 0.21, 0.21, 1.0) * 1.1;
				half4 onColor = half4(0.24, 0.33, 0.26, 1.0);
				half4 centerLineColor = half4(0.5, 0.5, 0.5, 1.0);
				half4 gridColor = half4(0.14, 0.14, 0.14, 1.0);
				half4 rChannelColor = half4(1.0, 0.65, 0.65, 1.0);
				half4 gChannelColor = half4(0.65, 1.0, 0.65, 1.0);
				half4 bChannelColor = half4(0.65, 0.65, 1.0, 1.0);
								
				half4 pixColor = bgColor;

				float fitnessScore = tex2D(_MainTex, finalCoords.x).x / _BestScore;

				float distR = abs(finalCoords.y - fitnessScore);
								
				// Create GRIDLINES:
				for(int j = 0; j < gridDivisions; j++) {
					float linePos = j * gridLineSpacing;
					float gridDistX = abs(finalCoords.x - linePos);
					if(gridDistX < (gridLineWidthX + lineFadeWidth)) {
						float smoothDist = smoothstep(0.0, lineFadeWidth, gridDistX - gridLineWidthX);
						pixColor = lerp(pixColor, gridColor, (1.0 - smoothDist) * 0.1);
					}
					float gridDistY = abs(finalCoords.y - linePos);
					if(gridDistY < (gridLineWidthY + lineFadeWidth)) {
						float smoothDist = smoothstep(0.0, lineFadeWidth, gridDistX - gridLineWidthY);
						pixColor = lerp(pixColor, gridColor, (1.0 - smoothDist) * 0.1);
					}
				}

				//  smoothstep( min , max , x )
				//  For values of x between min and max , returns a smoothly varying value that ranges from 0 at x = min to 1 at x = max .
				//  x is clamped to the range [ min , max ] and then the interpolation formula is evaluated:
				
				if(distR < (lineWidth + lineFadeWidth)) {
					float smoothDist = smoothstep(0.0, lineFadeWidth, distR - lineWidth);
					pixColor += lerp(rChannelColor, pixColor, smoothDist) * 1;
				}				
				
				float distToSideScreenEdge = min((1.0 - finalCoords.x), finalCoords.x);
				//if(distToSideScreenEdge < 0.3) {
					//pixColor = lerp(bgColor, pixColor, smoothstep(0.0, 1.0, distToSideScreenEdge / 0.3));
				//}
				return pixColor;
			}
		ENDCG
		}
	}
}

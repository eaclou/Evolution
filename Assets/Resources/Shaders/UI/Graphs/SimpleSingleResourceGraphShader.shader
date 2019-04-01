Shader "UI/SimpleSingleResourceGraphShader"
{
	Properties
	{
		_DataTex ("_DataTex", 2D) = "black" {}
		//_KeyTex ("_KeyTex", 2D) = "black" {}
		//_BrushTex ("_BrushTex", 2D) = "white" {}
		_Tint ("_Tint", Color) = (1,1,1,1)
	}
	SubShader
	{
		//Tags { "RenderType"="Opaque" }
		//LOD 100
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

			//uniform int _SelectedWorldStatsID;
			//uniform float _IsOn;
			//uniform float _GraphCoordStatsStart;
			//uniform float _MouseCoordX;
			//uniform float _MouseCoordY;
			//uniform float _MouseOn;

			
			fixed4 frag(v2f IN) : SV_Target
			{
				//return float4(1, 0.5, 0.2, 1);
				//float4 dataColor = tex2D(_DataTex, IN.texcoord);
				//return dataColor; 
				//float fade = (1.0 - i.uv.x);
				//float4 finalColor = float4(i.color.rgb, fade * fade * 1);	
				//finalColor.a *= brushColor.a;
				//finalColor = float4(0,1,1,1);
				//return finalColor;

				float _ZoomFactorX = 1.0;
				float _ZoomFactorY = 1.0;
								
				float lineWidth = 0.005; // * _ZoomFactorY;
				float lineFadeWidth = lineWidth * 12;
				float gridLineWidthX = 0.001 * _ZoomFactorX;	
				float gridLineWidthY = 0.001 * _ZoomFactorY;	
				float gridDivisions = 10;	
				float gridLineSpacing = 1 / gridDivisions;
				
				half2 finalCoords = IN.texcoord;
				//float initialScore = tex2D(_FitnessTex, 0).b;
				//float latestScore = tex2D(_FitnessTex, 1).x;
				/*float pivotY = 0.5; //(initialScore + latestScore) / 2;
				if((pivotY + (_ZoomFactorY * 0.5)) > 1) {
					pivotY = 1 - (_ZoomFactorY * 0.5);
				}
				if((pivotY - (_ZoomFactorY * 0.5)) < 0) {
					pivotY = (_ZoomFactorY * 0.5);
				}
				finalCoords.x = (finalCoords.x * _ZoomFactorX) + (1 - _ZoomFactorX);
				finalCoords.y = (finalCoords.y * _ZoomFactorY) + (pivotY - (_ZoomFactorY * 0.5));
				*/
				half4 bgColor = half4(0, 0, 0, 0.33);
				half4 onColor = half4(0.24, 0.33, 0.26, 1.0);
				half4 centerLineColor = half4(0.5, 0.5, 0.5, 1.0);
				half4 gridColor = half4(0.14, 0.14, 0.14, 1.0);
												
				half4 pixColor = bgColor;

				float4 fitnessScores = saturate(sqrt(tex2D(_DataTex, float2(finalCoords.x, 0)) / _MaxValue));

				float distR = abs(finalCoords.y - fitnessScores.x);
								
				// Create GRIDLINES:
				/*for(int j = 0; j < gridDivisions; j++) {
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
				}*/

				//  smoothstep( min , max , x )
				//  For values of x between min and max , returns a smoothly varying value that ranges from 0 at x = min to 1 at x = max .
				//  x is clamped to the range [ min , max ] and then the interpolation formula is evaluated:
				
				float isSolidMask = saturate((fitnessScores.x - finalCoords.y) * 100);
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

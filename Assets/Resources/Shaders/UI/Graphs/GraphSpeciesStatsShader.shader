
Shader "DataVis/GraphSpeciesStatsShader"
{
	Properties
	{
		_MainTex ("_MainTex", 2D) = "black" {}
		_ColorKeyTex ("_ColorKeyTex", 2D) = "black" {}
		_MaxValue ("_MaxValue", Float) = 0.00001		
	}

	SubShader
	{
		Tags
		{ 
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}
		
		Cull Off
		Lighting Off
		ZWrite Off
		//ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
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
			sampler2D _ColorKeyTex;

			uniform float _MinValue;
			uniform float _MaxValue;

			uniform int _SelectedSpeciesID;
			uniform int _NumDisplayed;

			fixed4 frag(v2f IN) : SV_Target
			{	
				float4 debugCol = tex2D(_MainTex, IN.texcoord);
				debugCol.a = 1;
				debugCol.rgb /= _MaxValue;
				//debugCol.rg = IN.texcoord;
				debugCol.rgb += tex2D(_ColorKeyTex, IN.texcoord);
				//return debugCol;
				
				float _ZoomFactorX = 1.0;
				float _ZoomFactorY = 1.0;
								
				float lineWidth = 0.0025; // * _ZoomFactorY;
				float lineFadeWidth = lineWidth * 6;
				float gridLineWidthX = 0.001 * _ZoomFactorX;	
				float gridLineWidthY = 0.001 * _ZoomFactorY;	
				float gridDivisions = 10;	
				float gridLineSpacing = 1 / gridDivisions;
				
				half2 finalCoords = IN.texcoord;
				
				float pivotY = 0.5; //(initialScore + latestScore) / 2;
				if((pivotY + (_ZoomFactorY * 0.5)) > 1) {
					pivotY = 1 - (_ZoomFactorY * 0.5);
				}
				if((pivotY - (_ZoomFactorY * 0.5)) < 0) {
					pivotY = (_ZoomFactorY * 0.5);
				}
				finalCoords.x = (finalCoords.x * _ZoomFactorX) + (1 - _ZoomFactorX);
				finalCoords.y = (finalCoords.y * _ZoomFactorY) + (pivotY - (_ZoomFactorY * 0.5));
				
				half4 bgColor = half4(0.1, 0.1, 0.1, 1.0) * 1;
				half4 onColor = half4(0.24, 0.33, 0.26, 1.0);
				half4 centerLineColor = half4(0.5, 0.5, 0.5, 1.0);
				half4 gridColor = half4(0.1, 0.1, 0.1, 1.0);
								
				half4 pixColor = bgColor;

				
								
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
				
				// better as an array/forloop?
				
				for(int i = 0; i < _NumDisplayed; i++) {
					float speciesCoord = (i + 0.5)/_NumDisplayed;
					float isSelectedMask = saturate(1.0 - abs((float)_SelectedSpeciesID - (float)i));
					half4 speciesColor = tex2D(_ColorKeyTex, half2(speciesCoord, 0.5)) * (isSelectedMask + 0.5);
					half4 speciesDataSample = tex2D(_MainTex, float2(finalCoords.x, speciesCoord));
					float scoreValue = saturate((speciesDataSample.x - _MinValue) / (_MaxValue - _MinValue));
					float dist = abs(finalCoords.y - scoreValue);
					if(dist < (lineWidth + lineFadeWidth) * (isSelectedMask + 1.0)) {
						float smoothDist = smoothstep(0.0, lineFadeWidth * (isSelectedMask + 1.0), dist - lineWidth * (isSelectedMask + 1.0));
						pixColor = lerp(saturate(speciesColor + 0.85), pixColor, smoothDist);
					}
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

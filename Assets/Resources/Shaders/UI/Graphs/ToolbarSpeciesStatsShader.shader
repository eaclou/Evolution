
Shader "UI/ToolbarSpeciesStatsShader"
{
	Properties
	{
		_MainTex ("_MainTex", 2D) = "black" {}
		_ColorKeyTex ("_ColorKeyTex", 2D) = "black" {}	
		_MaximumValue ("_MaximumValue", Float) = 1
		_MinimumValue ("_MinimumValue", Float) = 0
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

			uniform float _MinimumValue;
			uniform float _MaximumValue;

			uniform int _SelectedSpeciesID;
			uniform int _NumDisplayed;
			uniform float _NumEntries;

			fixed4 frag(v2f IN) : SV_Target
			{
				
				
				//float2 keyUV = float2((0.5 + 0.0) / 16, 0.25);
				//float4 keyTex = tex2D(_ColorKeyTex, keyUV);
				//float2 uv = ;
				//float4 testCol = tex2D(_MainTex, IN.texcoord);
				//testCol.rgb *= 0.04;
				//float4 testCol = tex2D(_ColorKeyTex, uv);
				//return testCol;

				/*float4 testCol = keyTex;
				if(IN.texcoord.x > IN.texcoord.y) {
					testCol = debugCol;
				}
				return testCol;*/

				//debugCol.a = 1;
				//debugCol.rgb /= _MaxValue;
				//debugCol.rg = IN.texcoord;
				//debugCol.rgb += tex2D(_ColorKeyTex, IN.texcoord);
				//return debugCol;
				
				float _ZoomFactorX = 1.0;
				float _ZoomFactorY = 1.0;
								
				float lineWidth = 0.004; // * _ZoomFactorY;
				float lineFadeWidth = lineWidth * 3;
				float gridLineWidthX = 0.001 * _ZoomFactorX;	
				float gridLineWidthY = 0.001 * _ZoomFactorY;	
				float gridDivisions = 10;	
				float gridLineSpacing = 1 / gridDivisions;
				
				half2 finalCoords = IN.texcoord;

				//_NumEntries 0-1 = num pixels width of statsTex
				float halfPixSize = 0.5 / max(1, _NumEntries);  // avoid div0
				// startCoord = halfPixSize (0 should map to this)
				//end coord = 1.0 - halfPixSize (1 should map to this)
				//u range = halfPixSize * 2
				float uRange = 1.0 - halfPixSize * 2.0;
				//finalCoords.x = finalCoords.x * uRange + halfPixSize;
				
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
				half4 bgColor = half4(0, 0, 0.25, 0.25);
				half4 onColor = half4(0.24, 0.33, 0.26, 1.0);
				half4 centerLineColor = half4(0.5, 0.5, 0.5, 1.0);
				half4 gridColor = half4(0.1, 0.1, 0.1, 1.0);
								
				half4 pixColor = bgColor;
								
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
				
				//_MinValue = 0.2;
				//_MaxValue = 2.5;

				_NumDisplayed = 32;
				
				// better as an array/forloop?
				
				for(int i = 0; i < _NumDisplayed; i++) {
					float speciesCoord = (i + 0.5)/_NumDisplayed;
					float isSelectedMask = saturate(1.0 - abs((float)_SelectedSpeciesID - (float)i));
					half4 speciesColor = tex2D(_ColorKeyTex, half2(speciesCoord, 0.5)) * (isSelectedMask + 0.5);
					half4 speciesDataSample = tex2D(_MainTex, float2(finalCoords.x, speciesCoord));
					float scoreValue = saturate((speciesDataSample.x - _MinimumValue) / (_MaximumValue - _MinimumValue));
					float dist = abs(finalCoords.y - scoreValue);
					if(dist < (lineWidth + lineFadeWidth) * (isSelectedMask + 1.0)) {
						float smoothDist = smoothstep(0.0, lineFadeWidth * (isSelectedMask + 1.0), dist - lineWidth * (isSelectedMask + 1.0));
						pixColor = lerp(saturate(speciesColor + 0.2), pixColor, smoothDist);
					}
				}

				int speciesIndexSel = _SelectedSpeciesID;
				float speciesCoordSel = ((float)speciesIndexSel + 0.5)/_NumDisplayed;
				//float isSelectedMask = saturate(1.0 - abs((float)_SelectedSpeciesID - (float)i));
				half4 speciesColorSel = tex2D(_ColorKeyTex, half2(speciesCoordSel, 0.5)); // * (isSelectedMask + 0.5);
				half4 speciesDataSampleSel = tex2D(_MainTex, float2(saturate(finalCoords.x), speciesCoordSel));
				float scoreValueSel = saturate((speciesDataSampleSel.x - _MinimumValue) / (_MaximumValue - _MinimumValue));
				float distSel = abs(finalCoords.y - scoreValueSel);
				if(distSel < (lineWidth + lineFadeWidth)) {
					float smoothDist = smoothstep(0.0, lineFadeWidth, distSel - lineWidth);
					//pixColor = lerp(float4(1,1,1,1), pixColor, smoothDist);
					pixColor = lerp(saturate(speciesColorSel + 0.75), pixColor, smoothDist);
				}
				
				//float distToSideScreenEdge = min((1.0 - finalCoords.x), finalCoords.x);
				//if(distToSideScreenEdge < 0.3) {
					//pixColor = lerp(bgColor, pixColor, smoothstep(0.0, 1.0, distToSideScreenEdge / 0.3));
				//}
				return pixColor;
			}
		ENDCG
		}
	}
}

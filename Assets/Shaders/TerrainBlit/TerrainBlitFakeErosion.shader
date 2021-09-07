Shader "TerrainBlit/TerrainBlitFakeErosion"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}  // Original Heights
		_NewTex  ("Texture", 2D) = "black" {}
		_MaskTex1 ("Texture", 2D) = "white" {}
		_MaskTex2 ("Texture", 2D) = "white" {}
		_FlowTex ("Texture", 2D) = "gray" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0

			// Default to Textures, which should have no effect if they are not Set (default values)
			#pragma shader_feature _USE_NEW_NOISE
			#pragma shader_feature _USE_MASK1_NOISE
			#pragma shader_feature _USE_MASK2_NOISE
			#pragma shader_feature _USE_FLOW_NOISE
						
			#include "UnityCG.cginc"
			#include "Assets/Shaders/Inc/NoiseShared.cginc"
			
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

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			struct GenomeNoiseOctaveData {
				float3 amplitude;
				float3 frequency;
				float3 offset;
				float rotation;
				float ridgeNoise;
			};

			sampler2D _MainTex;
			sampler2D _NewTex;
			sampler2D _MaskTex1;
			sampler2D _MaskTex2;
			sampler2D _FlowTex;

			StructuredBuffer<GenomeNoiseOctaveData> newTexSampleParamsCBuffer;
			StructuredBuffer<GenomeNoiseOctaveData> maskTex1SampleParamsCBuffer;
			StructuredBuffer<GenomeNoiseOctaveData> maskTex2SampleParamsCBuffer;
			StructuredBuffer<GenomeNoiseOctaveData> flowTexSampleParamsCBuffer;

			float4 _NewTexLevels;
			float _NewTexFlowAmount;
			float4 _MaskTex1Levels;
			float _MaskTex1FlowAmount;
			float4 _MaskTex2Levels;
			float _MaskTex2FlowAmount;
			//float4 _FlowTexLevels;
			

			int _PixelsWidth;
			int _PixelsHeight;
			float4 _GridBounds;	

			float4 GetTexSample(sampler2D TEXTURE, StructuredBuffer<GenomeNoiseOctaveData> samplerSettings, float2 coords) {
				
				float rotation = samplerSettings[0].rotation;
				float2 uvPos = float2(coords.x * cos(rotation) - coords.y * sin(rotation), coords.x * sin(rotation) + coords.y * cos(rotation)) * samplerSettings[0].frequency.xy + samplerSettings[0].offset.xy;
				float4 texSample = tex2D(TEXTURE, uvPos) * samplerSettings[0].amplitude.x;
				float4 ridgedValue = 1 - abs(texSample);
				texSample = lerp(texSample, ridgedValue, samplerSettings[0].ridgeNoise);
				
				return texSample;			
			}

			float4 GetNoiseSample(StructuredBuffer<GenomeNoiseOctaveData> samplerSettings, float2 coords) {
				
				uint elements;
				uint stride;
				samplerSettings.GetDimensions(elements, stride);	
				float noiseOctaveCount = (float)elements;
				
				//float accumulatedAltitude = 0;
				float accumulatedValue = 0;

				// NOISE!!!!
				for(int x = 0; x < 6; x++) {
					if(x < noiseOctaveCount) {
						float rotation = samplerSettings[x].rotation;
							
						//float2 uvPos = float2(coords.x * cos(rotation) - coords.y * sin(rotation), coords.x * sin(rotation) + coords.y * cos(rotation)) * samplerSettings[x].frequency.xy + samplerSettings[x].offset.xy;

						float2 samplePos = float2(coords.x * cos(rotation) - coords.y * sin(rotation), coords.x * sin(rotation) + coords.y * cos(rotation))
																				* samplerSettings[x].frequency.xy + samplerSettings[x].offset.xy;
						float3 noiseSample = Value2D(samplePos, 1);

						float value = noiseSample.x;
						float invertedValue = 1 - abs(value);

						accumulatedValue += lerp(value, invertedValue, samplerSettings[x].ridgeNoise) * samplerSettings[x].amplitude.x; 

						//float altitude = noiseSample.x;							
						//float ridge = altitude = 1 - abs(altitude);
							
						//accumulatedAltitude += altitude; //lerp(altitude, ridge, 0) * samplerSettings[x].amplitude.x;
					}	
				}

				//return float4(1,1,1,1);
				return float4(accumulatedValue,accumulatedValue,accumulatedValue,1);
			}

			float4 GetNoiseSampleDerivative(StructuredBuffer<GenomeNoiseOctaveData> samplerSettings, float2 coords) {
				
				uint elements;
				uint stride;
				samplerSettings.GetDimensions(elements, stride);	
				float noiseOctaveCount = (float)elements;
				
				//float accumulatedAltitude = 0;
				float4 accumulatedValue = 0;

				// NOISE!!!!
				for(int x = 0; x < 6; x++) {
					if(x < noiseOctaveCount) {
						float rotation = samplerSettings[x].rotation;
							
						//float2 uvPos = float2(coords.x * cos(rotation) - coords.y * sin(rotation), coords.x * sin(rotation) + coords.y * cos(rotation)) * samplerSettings[x].frequency.xy + samplerSettings[x].offset.xy;

						float3 samplePos = float3(coords.x * cos(rotation) - coords.y * sin(rotation), 0, coords.x * sin(rotation) + coords.y * cos(rotation))
																				* samplerSettings[x].frequency.xyz + samplerSettings[x].offset.xyz;
						float4 noiseSample = Value3D(samplePos, 1);

						float4 value = float4(noiseSample.x * samplerSettings[x].amplitude.x, noiseSample.yzw * samplerSettings[x].amplitude.xyz);						

						accumulatedValue += value;

						//float altitude = noiseSample.x;							
						//float ridge = altitude = 1 - abs(altitude);
							
						//accumulatedAltitude += altitude; //lerp(altitude, ridge, 0) * samplerSettings[x].amplitude.x;
					}	
				}

				float3 dir = normalize(accumulatedValue.yzw);
				return float4(dir,1);
				//return accumulatedValue;
			}
		
			fixed4 frag (v2f i) : SV_Target
			{	
				
				float2 _Pixels = float2((float)_PixelsWidth, (float)_PixelsHeight);  // why the fuck did I need to mult by 2?? -- something weird with Bilinear Sampling?
				
				 // Neighbour cells
				float s = 1 / _Pixels;
				float pixelWorldSize = (_GridBounds.y - _GridBounds.x) / (float)_PixelsWidth;

				//_GridBounds
				//float2 coords = float2(i.uv.x * (_GridBounds.y - _GridBounds.x) + _GridBounds.x, i.uv.y * (_GridBounds.w - _GridBounds.z) + _GridBounds.z);

				float4 cl = tex2D(_MainTex, i.uv + float2(-s, 0)); // Centre Left
				float4 tc = tex2D(_MainTex, i.uv + float2(0, -s)); // Top Centre
				float4 cc = tex2D(_MainTex, i.uv + float2(0, 0)); // Centre Centre
				float4 bc = tex2D(_MainTex, i.uv + float2(0, +s)); // Bottom Centre
				float4 cr = tex2D(_MainTex, i.uv + float2(+s, 0)); // Centre Right

				float talusAngle = 0.5; // ~~ 30 degrees?
				
				float deltaHeightLeft = cc.x - cl.x;
				float deltaHeightTop = cc.x - tc.x;
				float deltaHeightRight = cc.x - cr.x;
				float deltaHeightBottom = cc.x - bc.x;
				float minDelta = min(min(min(deltaHeightLeft, deltaHeightTop), deltaHeightRight), deltaHeightBottom);  // most inflow potential
				float maxDelta = max(max(max(deltaHeightLeft, deltaHeightTop), deltaHeightRight), deltaHeightBottom);  // most outflow potential

				float slopeLeft = atan(deltaHeightLeft / pixelWorldSize / 1024);
				float slopeTop = atan(deltaHeightTop / pixelWorldSize / 1024);
				float slopeRight = atan(deltaHeightRight / pixelWorldSize / 1024);
				float slopeBottom = atan(deltaHeightBottom / pixelWorldSize / 1024);

				float outTalusLeft = saturate(sign(-slopeLeft - talusAngle));
				float outTalusTop = saturate(sign(-slopeTop - talusAngle));
				float outTalusRight = saturate(sign(-slopeRight - talusAngle));
				float outTalusBottom = saturate(sign(-slopeBottom - talusAngle));

				float totalOutFlow = (outTalusLeft * deltaHeightLeft + outTalusTop * deltaHeightTop + outTalusRight * deltaHeightRight + outTalusBottom * deltaHeightBottom) * 0.5;
				
				float inTalusLeft = saturate(sign(slopeLeft - talusAngle));
				float inTalusTop = saturate(sign(slopeTop - talusAngle));
				float inTalusRight = saturate(sign(slopeRight - talusAngle));
				float inTalusBottom = saturate(sign(slopeBottom - talusAngle));

				float totalInFlow = (inTalusLeft * deltaHeightLeft + inTalusTop * deltaHeightTop + inTalusRight * deltaHeightRight + inTalusBottom * deltaHeightBottom) * 0.5;

				cc.x += (totalInFlow + totalOutFlow) * 0.1;
				return cc;
				
				//float outflowLeft = talusLeft * deltaHeightLeft

				/*if(talusLeft + talusTop + talusRight + talusBottom > 0) {
					// Outflow first:
					float totalOutflowDeltaHeight = max(0,deltaHeightLeft) * talusLeft + max(0,deltaHeightTop) * talusTop + max(0,deltaHeightRight) * talusRight + max(0,deltaHeightBottom) * talusBottom;
					float outWeightLeft = max(0,-slopeLeft - talusAngle) / totalOutflowDeltaHeight * talusLeft;
					float outWeightTop = max(0,-slopeTop - talusAngle) / totalOutflowDeltaHeight * talusTop;
					float outWeightRight = max(0,-slopeRight - talusAngle) / totalOutflowDeltaHeight * talusRight;
					float outWeightBottom = max(0,-slopeBottom - talusAngle) / totalOutflowDeltaHeight * talusBottom;

					totalOutFlow = (outWeightLeft + outWeightTop + outWeightRight + outWeightBottom) * maxDelta * 0.5;
				}*/
				
				
				
				
				
				//float inflowAmount = max(0, max(0, slopeLeft) - talusSlope) + max(0, max(0, slopeTop) - talusSlope) + max(0, max(0, slopeRight) - talusSlope) + max(0, max(0, slopeBottom) - talusSlope);
				//float outflowAmount = (max(0, -slopeLeft - talusSlope) + max(0, -slopeTop - talusSlope) + max(0, -slopeRight - talusSlope) + max(0, -slopeBottom - talusSlope));
				
				//float newValue = (cl.x + tc.x + cc.x + bc.x + cr.x) * 0.2;

				//cc.x += inflowAmount * 0.1; //(inflowAmount - outflowAmount) * 0.5;;

				
								
				/*float newValue = dot(cl.xyz, weight5MatrixCBuffer[0]) +
								 dot(tc.xyz, weight5MatrixCBuffer[1]) +
								 dot(cc.xyz, weight5MatrixCBuffer[2]) +
								 dot(bc.xyz, weight5MatrixCBuffer[3]) +
								 dot(cr.xyz, weight5MatrixCBuffer[4]);*/

				//float4 baseHeight = tex2D(_MainTex, i.uv);

				//return baseHeight;

				
								
			}



			ENDCG
		}
	}
}

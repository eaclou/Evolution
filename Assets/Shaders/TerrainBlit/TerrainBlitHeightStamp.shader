Shader "TerrainBlit/TerrainBlitHeightStamp"
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
			#pragma shader_feature _HEIGHT_ADD _HEIGHT_SUBTRACT _HEIGHT_MULTIPLY _HEIGHT_AVERAGE
						
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

			struct HeightStampData {
				float radiusStartFade;
				float radiusEndFade;
				float4 stampPivot;
				int heightOperation;
				float maskNoiseFreq;
				float altitudeOffset;

				int numOctaves;
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

			//StructuredBuffer<StampNoiseData> newTexSampleParamsCBuffer;
			StructuredBuffer<HeightStampData> heightStampDataCBuffer;
			//StructuredBuffer<GenomeNoiseOctaveData> maskTex1SampleParamsCBuffer;
			//StructuredBuffer<GenomeNoiseOctaveData> maskTex2SampleParamsCBuffer;
			//StructuredBuffer<GenomeNoiseOctaveData> flowTexSampleParamsCBuffer;

			float4 _NewTexLevels;
			float _NewTexFlowAmount;
			float4 _MaskTex1Levels;
			float _MaskTex1FlowAmount;
			float4 _MaskTex2Levels;
			float _MaskTex2FlowAmount;
			//float4 _FlowTexLevels;
			
			//float4 _WorldPivot;
			//float _RadiusStartFade;
			//float _RadiusEndFade;
			//float _AltitudeOffset;
			//int _PixelsWidth;
			//int _PixelsHeight;
			float4 _GridBounds;	

			float4 GetTexSample(sampler2D TEXTURE, HeightStampData samplerSettings, float2 coords) {
				
				float rotation = samplerSettings.rotation;
				float2 uvPos = float2(coords.x * cos(rotation) - coords.y * sin(rotation), coords.x * sin(rotation) + coords.y * cos(rotation)) * samplerSettings.frequency.xy + samplerSettings.offset.xy;
				float4 texSample = tex2D(TEXTURE, uvPos) * samplerSettings.amplitude.x;
				float4 ridgedValue = 1 - abs(texSample);
				texSample = lerp(texSample, ridgedValue, samplerSettings.ridgeNoise);
				
				return texSample;			
			}

			float4 GetNoiseSample(HeightStampData samplerSettings, float2 coords) {
				
				//uint elements;
				//uint stride;
				//samplerSettings.GetDimensions(elements, stride);	
				float noiseOctaveCount = samplerSettings.numOctaves;
				
				//float accumulatedAltitude = 0;
				float accumulatedValue = 0;

				// NOISE!!!!
				for(int x = 0; x < 6; x++) {
					if(x < noiseOctaveCount) {
						float rotation = samplerSettings.rotation * x;
						
						float2 samplePos = float2(coords.x * cos(rotation) - coords.y * sin(rotation), coords.x * sin(rotation) + coords.y * cos(rotation))
																				* samplerSettings.frequency.xy * pow(2, x) + samplerSettings.offset.xy;
						float3 noiseSample = Value2D(samplePos, 1);

						float value = noiseSample.x;
						float invertedValue = 1 - abs(value);

						accumulatedValue += lerp(value, invertedValue, samplerSettings.ridgeNoise) * samplerSettings.amplitude.x / pow(2, x); 

					}	
				}

				//return float4(1,1,1,1);
				return float4(accumulatedValue,accumulatedValue,accumulatedValue,1);
			}

			float4 GetNoiseSampleDerivative(HeightStampData samplerSettings, float2 coords) {
				
				//uint elements;
				//uint stride;
				//samplerSettings.GetDimensions(elements, stride);	
				//float noiseOctaveCount = (float)elements;
				float noiseOctaveCount = samplerSettings.numOctaves;
				
				//float accumulatedAltitude = 0;
				float4 accumulatedValue = 0;

				// NOISE!!!!
				for(int x = 0; x < 6; x++) {
					if(x < noiseOctaveCount) {
						float rotation = samplerSettings.rotation;
						
						float3 samplePos = float3(coords.x * cos(rotation) - coords.y * sin(rotation), 0, coords.x * sin(rotation) + coords.y * cos(rotation))
																				* samplerSettings.frequency.xyz + samplerSettings.offset.xyz;
						float4 noiseSample = Value3D(samplePos, 1);

						float4 value = float4(noiseSample.x * samplerSettings.amplitude.x, noiseSample.yzw * samplerSettings.amplitude.xyz);						

						accumulatedValue += value;
					}	
				}

				float3 dir = normalize(accumulatedValue.yzw);
				return float4(dir,1);
			}
		
			fixed4 frag (v2f i) : SV_Target
			{	
				
				 // Neighbour cells
				//float s = 1 / _Pixels;
				float4 baseHeight = tex2D(_MainTex, i.uv);

				//_GridBounds
				float2 coords = float2(i.uv.x * (_GridBounds.y - _GridBounds.x) + _GridBounds.x, i.uv.y * (_GridBounds.w - _GridBounds.z) + _GridBounds.z);

				float4 finalHeight = baseHeight;

				uint elements;
				uint stride;
				heightStampDataCBuffer.GetDimensions(elements, stride);	
				float numStamps = (float)elements;
				for(int x = 0; x < 8; x++) {
					if(x < numStamps) {
						float2 stampPosition = heightStampDataCBuffer[x].stampPivot.xz;
						// FLOW MAP:
						//float4 flowSample = 0.0;
						//flowSample = GetNoiseSampleDerivative(flowTexSampleParamsCBuffer, coords);
						// Apply flow uv warp:
						//float2 flowCoords = coords + flowSample.xz * 0.5;
						float4 newHeight = float4(0,0,0,0);
						newHeight = GetNoiseSample(heightStampDataCBuffer[x], lerp(coords - stampPosition, coords - stampPosition, _NewTexFlowAmount));				
						//float mask1Value = 1;
						//mask1Value *= GetNoiseSample(maskTex1SampleParamsCBuffer, lerp(coords, flowCoords, _MaskTex1FlowAmount)).x * 0.5 + 0.5;  // get in 0-1 range				
						//float mask2Value = 1;
						//mask2Value *= GetNoiseSample(maskTex2SampleParamsCBuffer, lerp(coords, flowCoords, _MaskTex2FlowAmount)).x * 0.5 + 0.5;  // get in 0-1 range
				
						// LEVELS:
						//mask1Value = min(max(mask1Value - _MaskTex1Levels.x, 0) / (_MaskTex1Levels.y - _MaskTex1Levels.x), 1);
						//mask1Value = mask1Value * ((_MaskTex1Levels.w - _MaskTex1Levels.z)) + _MaskTex1Levels.z;
						//mask2Value = min(max(mask2Value - _MaskTex2Levels.x, 0) / (_MaskTex2Levels.y - _MaskTex2Levels.x), 1);
						//mask2Value = mask2Value * ((_MaskTex2Levels.w - _MaskTex2Levels.z)) + _MaskTex2Levels.z;

						float distanceMask = smoothstep(heightStampDataCBuffer[x].radiusStartFade, heightStampDataCBuffer[x].radiusEndFade, length(coords - stampPosition) + Value2D(coords, heightStampDataCBuffer[x].maskNoiseFreq).yz * 0.01);
				
						float newRockHeight = newHeight.x + heightStampDataCBuffer[x].altitudeOffset; // * saturate(mask1Value.x) * saturate(mask2Value.x);

						newRockHeight = lerp(0, newRockHeight, 1.0 - distanceMask);
				
				
						// Operation:
						#if defined(_HEIGHT_ADD)
						// Addition:
						//baseHeight.x += newRockHeight;
							finalHeight.x += newRockHeight;
						#endif
						#if defined(_HEIGHT_SUBTRACT)
						// Subtraction:
							finalHeight.x -= newRockHeight;
						//baseHeight.x -= newRockHeight;
						#endif
						#if defined(_HEIGHT_MULTIPLY)
						// Multiplication:          /// USE as CRATER?
							finalHeight.x += newRockHeight * 0.45;

							float distanceMask2 = smoothstep(heightStampDataCBuffer[x].radiusStartFade * 0.4, heightStampDataCBuffer[x].radiusEndFade * 0.95, length(coords - stampPosition) + Value2D(coords, heightStampDataCBuffer[x].maskNoiseFreq).yz * 0.025);
				
							float newRockHeight2 = newHeight.x + heightStampDataCBuffer[x].altitudeOffset; // * saturate(mask1Value.x) * saturate(mask2Value.x);

							newRockHeight2 = lerp(0, newRockHeight2, 1.0 - distanceMask2);

							finalHeight.x -= newRockHeight2 * 1.5;
						//baseHeight.x *= newRockHeight;
						#endif
						#if defined(_HEIGHT_AVERAGE)
						// Average:
							finalHeight.x = lerp(finalHeight, newRockHeight, 1.0 - distanceMask);
						//baseHeight.x = (newRockHeight + baseHeight.x) * 0.5;
						#endif


					}
				}
				
					
				return finalHeight; //newRockHeight;
			}



			ENDCG
		}
	}
}

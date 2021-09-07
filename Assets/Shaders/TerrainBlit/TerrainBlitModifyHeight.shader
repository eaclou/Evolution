Shader "TerrainBlit/TerrainBlitModifyHeight"
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
			#pragma shader_feature _USE_NEW_TEX
			#pragma shader_feature _ _USE_MASK1_TEX _USE_MASK1_NOISE
			#pragma shader_feature _ _USE_MASK2_TEX _USE_MASK2_NOISE
			#pragma shader_feature _ _USE_FLOW_TEX _USE_FLOW_NOISE
						
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
				float use_ridged_noise;
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

			float _Mask1BlackPointIn;
			float _Mask1WhitePointIn;
			float _Mask1Gamma;
			float _Mask2BlackPointIn;
			float _Mask2WhitePointIn;
			float _Mask2Gamma;

			int _PixelsWidth;
			int _PixelsHeight;
			float4 _GridBounds;	

			float4 GetNewTexSample(float2 coords) {
				float4 texSample = float4(0,0,0,0);
				#if defined(_USE_NEW_TEX)
					float rotation = newTexSampleParamsCBuffer[0].rotation;
					float2 uvPos = float2(coords.x * cos(rotation) - coords.y * sin(rotation), coords.x * sin(rotation) + coords.y * cos(rotation)) * newTexSampleParamsCBuffer[0].frequency.xy + newTexSampleParamsCBuffer[0].offset.xy;
					texSample = tex2D(_NewTex, uvPos) * newTexSampleParamsCBuffer[0].amplitude.x;
					float4 ridgedValue = 1 - abs(texSample);
					texSample = lerp(texSample, ridgedValue, newTexSampleParamsCBuffer[0].use_ridged_noise);
				#else
					uint elements;
					uint stride;
					newTexSampleParamsCBuffer.GetDimensions(elements, stride);	
					float noiseOctaveCount = (float)elements;
				
					float accumulatedAltitude = 0;

					// NOISE!!!!
					for(int x = 0; x < 4; x++) {
						if(x < noiseOctaveCount) {
							float rotation = newTexSampleParamsCBuffer[x].rotation;
							
							float2 uvPos = float2(coords.x * cos(rotation) - coords.y * sin(rotation), coords.x * sin(rotation) + coords.y * cos(rotation)) * newTexSampleParamsCBuffer[x].frequency.xy + newTexSampleParamsCBuffer[x].offset.xy;

							//float2 pos = float2(i.uv.x * cos(rotation) - i.uv.y * sin(rotation), i.uv.x * sin(rotation) + i.uv.y * cos(rotation)); //i.uv.x * xAxis + i.uv.y * yAxis;
							float3 noiseSample = Value2D(uvPos, 1);

							float altitude = noiseSample.x;							
							float ridgedAltitude = altitude = 1 - abs(altitude);
							
							accumulatedAltitude += lerp(altitude, ridgedAltitude, newTexSampleParamsCBuffer[x].use_ridged_noise) * newTexSampleParamsCBuffer[x].amplitude.y;
						}	
					}
					texSample = float4(accumulatedAltitude,accumulatedAltitude,accumulatedAltitude,1);
				#endif

				return texSample;
			}

			float4 GetMaskTex1Sample(float2 coords) {
				float4 texSample = float4(0,0,0,0);
				#if defined(_USE_MASK1_TEX)
					float rotation = maskTex1SampleParamsCBuffer[0].rotation;
					float2 uvPos = float2(coords.x * cos(rotation) - coords.y * sin(rotation), coords.x * sin(rotation) + coords.y * cos(rotation)) * maskTex1SampleParamsCBuffer[0].frequency.xy + maskTex1SampleParamsCBuffer[0].offset.xy;
					texSample = tex2D(_MaskTex1, uvPos) * maskTex1SampleParamsCBuffer[0].amplitude.x;
					float4 ridgedValue = 1 - abs(texSample);
					texSample = lerp(texSample, ridgedValue, maskTex1SampleParamsCBuffer[0].use_ridged_noise);
				#endif
				#if defined(_USE_MASK1_NOISE)
					uint elements;
					uint stride;
					maskTex1SampleParamsCBuffer.GetDimensions(elements, stride);	
					float noiseOctaveCount = (float)elements;
				
					float accumulatedAltitude = 0;

					// NOISE!!!!
					for(int x = 0; x < 4; x++) {
						if(x < noiseOctaveCount) {
							float rotation = maskTex1SampleParamsCBuffer[x].rotation;
							
							float2 uvPos = float2(coords.x * cos(rotation) - coords.y * sin(rotation), coords.x * sin(rotation) + coords.y * cos(rotation)) * maskTex1SampleParamsCBuffer[x].frequency.xy + maskTex1SampleParamsCBuffer[x].offset.xy;

							//float2 pos = float2(i.uv.x * cos(rotation) - i.uv.y * sin(rotation), i.uv.x * sin(rotation) + i.uv.y * cos(rotation)); //i.uv.x * xAxis + i.uv.y * yAxis;
							float3 noiseSample = Value2D(uvPos, 1);

							float altitude = noiseSample.x;							
							float ridgedAltitude = altitude = 1 - abs(altitude);
							
							accumulatedAltitude += lerp(altitude, ridgedAltitude, maskTex1SampleParamsCBuffer[x].use_ridged_noise) * maskTex1SampleParamsCBuffer[x].amplitude.y;
						}	
					}
					texSample = float4(accumulatedAltitude,accumulatedAltitude,accumulatedAltitude,1);
				#endif

				return texSample;
			}

			float4 GetMaskTex2Sample(float2 coords) {
				float4 texSample = float4(0,0,0,0);
				#if defined(_USE_MASK2_TEX)
					float rotation = maskTex2SampleParamsCBuffer[0].rotation;
					float2 uvPos = float2(coords.x * cos(rotation) - coords.y * sin(rotation), coords.x * sin(rotation) + coords.y * cos(rotation)) * maskTex2SampleParamsCBuffer[0].frequency.xy + maskTex2SampleParamsCBuffer[0].offset.xy;
					texSample = tex2D(_MaskTex2, uvPos) * maskTex2SampleParamsCBuffer[0].amplitude.x;
					float4 ridgedValue = 1 - abs(texSample);
					texSample = lerp(texSample, ridgedValue, maskTex2SampleParamsCBuffer[0].use_ridged_noise);
				#endif
				#if defined(_USE_MASK2_NOISE)
					uint elements;
					uint stride;
					maskTex2SampleParamsCBuffer.GetDimensions(elements, stride);	
					float noiseOctaveCount = (float)elements;
				
					float accumulatedAltitude = 0;

					// NOISE!!!!
					for(int x = 0; x < 4; x++) {
						if(x < noiseOctaveCount) {
							float rotation = maskTex2SampleParamsCBuffer[x].rotation;
							
							float2 uvPos = float2(coords.x * cos(rotation) - coords.y * sin(rotation), coords.x * sin(rotation) + coords.y * cos(rotation)) * maskTex2SampleParamsCBuffer[x].frequency.xy + maskTex2SampleParamsCBuffer[x].offset.xy;

							//float2 pos = float2(i.uv.x * cos(rotation) - i.uv.y * sin(rotation), i.uv.x * sin(rotation) + i.uv.y * cos(rotation)); //i.uv.x * xAxis + i.uv.y * yAxis;
							float3 noiseSample = Value2D(uvPos, 1);

							float altitude = noiseSample.x;							
							float ridgedAltitude = altitude = 1 - abs(altitude);
							
							accumulatedAltitude += lerp(altitude, ridgedAltitude, maskTex2SampleParamsCBuffer[x].use_ridged_noise) * maskTex2SampleParamsCBuffer[x].amplitude.y;
						}	
					}
					texSample = float4(accumulatedAltitude,accumulatedAltitude,accumulatedAltitude,1);
				#endif

				return texSample;
			}

			float4 GetFlowTexSample(float2 coords) {
				float4 texSample = float4(0,0,0,0);
				#if defined(_USE_FLOW_TEX)
					float rotation = flowTexSampleParamsCBuffer[0].rotation;
					float2 uvPos = float2(coords.x * cos(rotation) - coords.y * sin(rotation), coords.x * sin(rotation) + coords.y * cos(rotation)) * flowTexSampleParamsCBuffer[0].frequency.xy + flowTexSampleParamsCBuffer[0].offset.xy;
					texSample = tex2D(_FlowTex, uvPos) * flowTexSampleParamsCBuffer[0].amplitude.x;
					float4 ridgedValue = 1 - abs(texSample);
					texSample = lerp(texSample, ridgedValue, flowTexSampleParamsCBuffer[0].use_ridged_noise);
				#else
					uint elements;
					uint stride;
					flowTexSampleParamsCBuffer.GetDimensions(elements, stride);	
					float noiseOctaveCount = (float)elements;
				
					float accumulatedAltitude = 0;

					// NOISE!!!!
					for(int x = 0; x < 4; x++) {
						if(x < noiseOctaveCount) {
							float rotation = flowTexSampleParamsCBuffer[x].rotation;
							
							float2 uvPos = float2(coords.x * cos(rotation) - coords.y * sin(rotation), coords.x * sin(rotation) + coords.y * cos(rotation)) * flowTexSampleParamsCBuffer[x].frequency.xy + flowTexSampleParamsCBuffer[x].offset.xy;

							//float2 pos = float2(i.uv.x * cos(rotation) - i.uv.y * sin(rotation), i.uv.x * sin(rotation) + i.uv.y * cos(rotation)); //i.uv.x * xAxis + i.uv.y * yAxis;
							float3 noiseSample = Value2D(uvPos, 1);

							float altitude = noiseSample.x;							
							float ridgedAltitude = altitude = 1 - abs(altitude);
							
							accumulatedAltitude += lerp(altitude, ridgedAltitude, flowTexSampleParamsCBuffer[x].use_ridged_noise) * flowTexSampleParamsCBuffer[x].amplitude.y;
						}	
					}
					texSample = float4(accumulatedAltitude,accumulatedAltitude,accumulatedAltitude,1);
				#endif

				return texSample;
			}
			
			
			fixed4 frag (v2f i) : SV_Target
			{	
				
				float2 _Pixels = float2((float)_PixelsWidth, (float)_PixelsHeight);  // why the fuck did I need to mult by 2?? -- something weird with Bilinear Sampling?
				
				 // Neighbour cells
				float s = 1 / _Pixels;
				float4 baseHeight = tex2D(_MainTex, i.uv);
				float2 gridSize = float2(_GridBounds.y - _GridBounds.x, _GridBounds.w - _GridBounds.z);				
				float2 vertexUV = (i.uv - float2(s * 0.5, s * 0.5)) * ((_Pixels.x + 1.0) / _Pixels.x);  //float2(,);
				float2 coords = float2(_GridBounds.x + vertexUV.x * gridSize.x, _GridBounds.z + vertexUV.y * gridSize.y) / float2(680,680);

				float2 uvFlow = float2(0,0);
				#if defined(_USE_FLOW_TEX)
					float4 flowTexSample = GetFlowTexSample(coords);
					coords += flowTexSample.xy;
				#endif
				#if defined(_USE_FLOW_NOISE)
					float4 flowTexSample = GetFlowTexSample(coords);
					coords += flowTexSample.xy;
				#endif

				float4 newHeightSample = GetNewTexSample(coords);				
				
				float4 maskValue = float4(1,1,1,1);
				#if defined(_USE_MASK1_TEX)
					maskValue = GetMaskTex1Sample(coords);
					float val = min(max(maskValue.x - _Mask1BlackPointIn, 0) / (_Mask1WhitePointIn - _Mask1BlackPointIn), 1);
					val = pow(val, 1.0 / _Mask1Gamma);

					// Levels:
					//float inColor = min(max(maskValue.x – 0, 0) / (1 – 0.9), 1);
					//float gamma = pow(maskValue.x, 1.0 / _Mask1Gamma);
					//float inColor = min(max(maskValue.x – 0, 0) / (1 – 0), 1);
					//inColor = pow(inColor, 1.0 / _Mask1Gamma);
					maskValue = val; //float4(0,0,0,0); //inColor;

				#endif
				#if defined(_USE_MASK1_NOISE)
					maskValue = GetMaskTex1Sample(coords);
				#endif
				#if defined(_USE_MASK2_TEX)
					maskValue *= GetMaskTex2Sample(coords);
				#endif
				#if defined(_USE_MASK2_NOISE)
					maskValue *= GetMaskTex2Sample(coords);
				#endif

				baseHeight.x += newHeightSample.x * maskValue.x;

				return baseHeight;
			}



			ENDCG
		}
	}
}

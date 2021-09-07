Shader "TerrainBlit/TerrainBlitTectonicWarp"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}  // Original Heights
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
			sampler2D _FlowTex;

			StructuredBuffer<GenomeNoiseOctaveData> flowTexSampleParamsCBuffer;
			
			int _PixelsWidth;
			int _PixelsHeight;
			float4 _GridBounds;	
			
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
				
				float4 baseHeight = tex2D(_MainTex, i.uv);

				return baseHeight;

				
								
			}



			ENDCG
		}
	}
}

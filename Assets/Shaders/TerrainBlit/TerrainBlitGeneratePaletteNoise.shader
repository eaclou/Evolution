Shader "TerrainBlit/TerrainBlitGeneratePaletteNoise"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}  // Original Heights		
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

			#pragma shader_feature RIDGED_NOISE_ON
			
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
			float4 _MainTex_TexelSize;
			int _PixelsWidth;
			int _PixelsHeight;

			int _GenomeIndex;

			StructuredBuffer<GenomeNoiseOctaveData> terrainGenomeCBuffer;
			

			fixed4 frag (v2f i) : SV_Target
			{	
				
				uint elements;
				uint stride;
				terrainGenomeCBuffer.GetDimensions(elements, stride);	
				float noiseOctaveCount = (float)elements;
				
				float accumulatedAltitude = 0;

				// NOISE!!!!
				for(int x = 0; x < 4; x++) {
					if(x < noiseOctaveCount) {
						float rotation = terrainGenomeCBuffer[x].rotation;
						float2 xAxis = float2(cos(rotation), sin(rotation));
						float2 yAxis = float2(sin(rotation), cos(rotation));
						float2 pos = i.uv.x * xAxis + i.uv.y * yAxis;
						float3 noiseSample = Value2D(pos * terrainGenomeCBuffer[x].frequency + terrainGenomeCBuffer[x].offset.xz, 1);
						float altitude = noiseSample.x;
						#if defined(RIDGED_NOISE_ON)
							altitude = 1 - abs(altitude);
						#endif
						accumulatedAltitude += altitude * terrainGenomeCBuffer[x].amplitude.y;
					}	
				}

				float4 col = float4(accumulatedAltitude,accumulatedAltitude,accumulatedAltitude,1);

				return col;
				
			}
			ENDCG
		}
	}
}

Shader "TerrainBlit/TerrainBlitStrataAdjustments"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}  // Original Heights
		//_RemapTex  ("Texture", 2D) = "gray" {}
		//_MaskTex1 ("Texture", 2D) = "white" {}
		//_MaskTex2 ("Texture", 2D) = "white" {}
		//_FlowTex ("Texture", 2D) = "gray" {}
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
			#include "Assets/Shaders/Inc/StrataRemap.cginc"
			
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

			struct RockStrataData {
				float3 color;
				float hardness;
			};

			sampler2D _MainTex;
			//sampler2D _RemapTex;
			StructuredBuffer<RockStrataData> rockStrataDataCBuffer;

			//int _PixelsWidth;
			//int _PixelsHeight;
			//float4 _GridBounds;	

			float _MinAltitude;
			float _AvgAltitude;
			float _MaxAltitude;
			float _RemapOffset;
			float _StrataIndex;
			float _NumStrata;
						
		
			fixed4 frag (v2f i) : SV_Target
			{	
				float4 baseHeight = tex2D(_MainTex, i.uv);

				float normalizedAltitude = (GetRemappedAltitude(float3(i.uv.x * 2 - 1, baseHeight.x, i.uv.y * 2 - 1), _MinAltitude, _MaxAltitude) - _MinAltitude) / (_MaxAltitude - _MinAltitude);
				float pixelAltitude = GetRemappedAltitude(float3(i.uv.x * 2 - 1, baseHeight.x, i.uv.y * 2 - 1), _MinAltitude, _MaxAltitude);

				float strataHardness = rockStrataDataCBuffer[_StrataIndex].hardness;

				float thisStrataStartAltitude = GetRemappedAltitude((floor(float3(i.uv.x * 2 - 1, normalizedAltitude, i.uv.y * 2 - 1) * (_NumStrata - 1)) / (_NumStrata - 1)) * (_MaxAltitude - _MinAltitude) + _MinAltitude, _MinAltitude, _MaxAltitude);
				float thisStrataEndAltitude = GetRemappedAltitude((ceil(float3(i.uv.x * 2 - 1, normalizedAltitude, i.uv.y * 2 - 1) * (_NumStrata - 1)) / (_NumStrata - 1)) * (_MaxAltitude - _MinAltitude) + _MinAltitude, _MinAltitude, _MaxAltitude);

				float strataStartAltitude = GetRemappedAltitude(float3(i.uv.x * 2 - 1 ,(_StrataIndex / (_NumStrata - 1)) * (_MaxAltitude - _MinAltitude) + _MinAltitude, i.uv.y * 2 - 1), _MinAltitude, _MaxAltitude);
				float strataTopAltitude = GetRemappedAltitude(float3(i.uv.x * 2 - 1 ,((_StrataIndex + 1) / (_NumStrata - 1)) * (_MaxAltitude - _MinAltitude) + _MinAltitude, i.uv.y * 2 - 1), _MinAltitude, _MaxAltitude);

				float cliffMask = 0;
				
				float centerOfStrataAltitude = (strataTopAltitude + strataStartAltitude) * 0.5;
				float heightDeltaSoft = 0;
				if(pixelAltitude < centerOfStrataAltitude) {
					float midPointAltitude = (centerOfStrataAltitude - _MinAltitude) * 0.5;
					float distToMidAltitude = abs(midPointAltitude - pixelAltitude);

					float distToStrataCenter = centerOfStrataAltitude - pixelAltitude;
					cliffMask = 1.0 - saturate(distToStrataCenter / ((centerOfStrataAltitude - _MinAltitude) * 0.5));

					heightDeltaSoft += distToStrataCenter * cliffMask;
				}
				else {
					float midPointAltitude = (_MaxAltitude - centerOfStrataAltitude) * 0.5;
					float distToMidAltitude = abs(midPointAltitude - pixelAltitude);

					float distToStrataCenter = pixelAltitude - centerOfStrataAltitude;
					cliffMask = 1.0 - saturate(distToStrataCenter / ((_MaxAltitude - centerOfStrataAltitude) * 0.5));
					
					heightDeltaSoft -= distToStrataCenter * cliffMask;
				}


				float cliffMaskHard = (pixelAltitude - strataStartAltitude) / (_MaxAltitude - strataStartAltitude);
				cliffMaskHard = 1.0 - saturate(cliffMaskHard);
				if(pixelAltitude < strataStartAltitude)
					cliffMaskHard = 0;

				float heightDeltaHard = (strataTopAltitude - strataStartAltitude) * cliffMaskHard * saturate(strataHardness - 0.5) * 2;
				
				baseHeight.x += lerp(heightDeltaSoft, heightDeltaHard, 1); // only using hardening for now

				//baseHeight.x = thisStrataEndAltitude; //lerp(0, thisStrataEndAltitude - pixelAltitude, cliffMaskHard);
				return baseHeight;
				

				
			}



			ENDCG
		}
	}
}

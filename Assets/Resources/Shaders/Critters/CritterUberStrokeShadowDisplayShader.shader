Shader "Critter/CritterUberStrokeShadowDisplayShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_AltitudeTex ("_AltitudeTex", 2D) = "gray" {}
		_WaterSurfaceTex ("_WaterSurfaceTex", 2D) = "black" {}
		_TerrainColorTex ("_TerrainColorTex", 2D) = "black" {}
	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" }
		ZWrite Off
		//Cull Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag			
			#pragma target 5.0
			#include "UnityCG.cginc"
			#include "Assets/Resources/Shaders/Inc/CritterBodyAnimation.cginc"
			#include "Assets/Resources/Shaders/Inc/TerrainShared.cginc"

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				float2 altitudeUV : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float4 screenUV : TEXCOORD3;
			};

			sampler2D _MainTex;
			//float4 _MainTex_ST;
			sampler2D _AltitudeTex;
			sampler2D _WaterSurfaceTex;
			sampler2D _TerrainColorTex;

			uniform float _MapSize;
			uniform float _MaxAltitude;
			uniform float _GlobalWaterLevel;

			//sampler2D _RenderedSceneRT;  // Provided by CommandBuffer -- global tex??? seems confusing... ** revisit this

			StructuredBuffer<float3> quadVerticesCBuffer;			
			StructuredBuffer<CritterInitData> critterInitDataCBuffer;
			StructuredBuffer<CritterSimData> critterSimDataCBuffer;
			StructuredBuffer<CritterGenericStrokeData> critterGenericStrokesCBuffer;
			
			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
				
				CritterGenericStrokeData genericStrokeData = critterGenericStrokesCBuffer[inst];
				uint agentIndex = genericStrokeData.parentIndex;
				CritterInitData critterInitData = critterInitDataCBuffer[agentIndex];
				CritterSimData critterSimData = critterSimDataCBuffer[agentIndex];

				float3 critterWorldPos = critterSimData.worldPos;

				float3 neighborWorldPos = critterGenericStrokesCBuffer[genericStrokeData.neighborIndex + agentIndex * 1856].worldPos;
				float3 neighborAlignTangent = neighborWorldPos - genericStrokeData.worldPos;

				// Decay color bleach value:
				float decayTimeRange = 0.4;
				float decayAmount = saturate((critterSimData.decayPercentage) / decayTimeRange - genericStrokeData.thresholdValue * 2);
				float4 decayColor = float4(0.5, 0.4, 0.3, 1);
				
				float3 brushScale = float3(genericStrokeData.scale, 1);
								
				float3 worldNormal = genericStrokeData.worldNormal;
				float3 worldTangent = normalize(lerp(genericStrokeData.worldTangent, -neighborAlignTangent, genericStrokeData.neighborAlign));
				float3 worldBitangent = cross(worldNormal, worldTangent);
				
				float3 quadVertexOffset = quadVerticesCBuffer[id].x * worldBitangent * genericStrokeData.scale.x + quadVerticesCBuffer[id].y * worldTangent * genericStrokeData.scale.y;
				quadVertexOffset *= (1 - critterSimData.decayPercentage);
				quadVertexOffset *= 2;
				// old //float3 vertexWorldPos = critterWorldPos + strokeBindPos + quadVerticesCBuffer[id] * 0.645 * length(genericStrokeData.scale);
				float3 vertexWorldPos = genericStrokeData.worldPos + quadVertexOffset * 1.25 * lerp(critterInitData.spawnSizePercentage, 1, critterSimData.growthPercentage) * 1;
								
				// REFRACTION:							
				float3 surfaceNormal = tex2Dlod(_WaterSurfaceTex, float4(genericStrokeData.worldPos.xy / _MapSize, 0, 0)).yzw;
				float refractionStrength = 1;
				vertexWorldPos.xy += -surfaceNormal.xy * refractionStrength;		
				
				float2 altUV = vertexWorldPos.xy / _MapSize;
				o.altitudeUV = altUV;
				float altitudeRaw = tex2Dlod(_AltitudeTex, float4(altUV.xy, 0, 0));
				float seaFloorAltitude = -altitudeRaw * _MaxAltitude;
				vertexWorldPos.z = seaFloorAltitude;
				
				//vertexWorldPos.z = ;
				o.worldPos = vertexWorldPos;
				
				//vertexWorldPos.z += 1.0;
				o.vertex = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(vertexWorldPos, 1.0)));
				o.uv = quadVerticesCBuffer[id].xy + 0.5;	
				float4 screenUV = ComputeScreenPos(o.vertex);
				o.screenUV = screenUV;
		
				float alpha = saturate((critterSimData.embryoPercentage - 0.995) * 200);

				o.color = float4(1,1,1,alpha);
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				
				float4 brushColor = tex2D(_MainTex, i.uv);	
				
				//float2 screenUV = i.screenUV.xy / i.screenUV.w;
				//float4 frameBufferColor = tex2D(_RenderedSceneRT, screenUV);  //  Color of brushtroke source					
				float4 altitudeTex = tex2D(_AltitudeTex, i.altitudeUV); //i.worldPos.z / 10; // [-1,1] range
				float depth = saturate((_GlobalWaterLevel - altitudeTex.x) * 1);
				float4 waterSurfaceTex = tex2D(_WaterSurfaceTex, i.altitudeUV);
				float4 terrainColorTex = tex2D(_TerrainColorTex, i.altitudeUV);

				//frameBufferColor.rgb *= 0.75; // = lerp(frameBufferColor.rgb, particleColor, 0.25);
				float4 finalColor = float4(0,0,0,1);
				finalColor.rgb = lerp(finalColor.rgb, terrainColorTex.rgb * 0.75, depth); //GetGroundColor(i.worldPos, frameBufferColor, altitudeTex, waterSurfaceTex, float4(0,0,0,0));
				//finalColor.a = brushColor.a * 0.175;
				//finalColor.rgb *= 0.55;
				return finalColor;
			}
			ENDCG
		}
	}
}

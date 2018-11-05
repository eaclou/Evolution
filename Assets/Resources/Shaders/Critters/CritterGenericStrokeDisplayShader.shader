Shader "Critter/CritterGenericStrokeDisplayShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_PatternTex ("Pattern Texture", 2D) = "white" {}
		_WaterSurfaceTex ("_WaterSurfaceTex", 2D) = "black" {}
	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" }
		ZWrite Off
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag			
			#pragma target 5.0
			#include "UnityCG.cginc"
			#include "Assets/Resources/Shaders/Inc/CritterBodyAnimation.cginc"
			

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				//float2 bodyUV : TEXCOORD1;
			};

			sampler2D _MainTex;
			//float4 _MainTex_ST;
			sampler2D _PatternTex;
			sampler2D _WaterSurfaceTex;

			uniform float _MapSize;

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

				// WEIRD COORDINATES!!! Positive Z = DEEPER!!!
				//float3 strokeBindPos = genericStrokeData.bindPos; //float3(genericStrokeData.bindPos.x, genericStrokeData.bindPos.z, -genericStrokeData.bindPos.y) * 0.8;

				//Temp align with creatures:
				//float3 critterForwardDir = float3(critterSimData.heading, 0);
				//float3 critterRightDir = float3(critterForwardDir.y, -critterForwardDir.x, 0);
								
				//strokeBindPos = critterRightDir * strokeBindPos.x + critterForwardDir * strokeBindPos.y;
				//strokeBindPos.z = genericStrokeData.bindPos.z;

				float3 brushScale = float3(genericStrokeData.scale, 1);
								
				float3 worldNormal = genericStrokeData.worldNormal;
				float3 worldTangent = genericStrokeData.worldTangent;
				float3 worldBitangent = cross(worldNormal, worldTangent);

				float3 quadVertexOffset = quadVerticesCBuffer[id].x * worldBitangent * genericStrokeData.scale.x + quadVerticesCBuffer[id].y * worldTangent * genericStrokeData.scale.y;

				// old //float3 vertexWorldPos = critterWorldPos + strokeBindPos + quadVerticesCBuffer[id] * 0.645 * length(genericStrokeData.scale);
				float3 vertexWorldPos = genericStrokeData.worldPos + quadVertexOffset * 1.25 * lerp(critterInitData.spawnSizePercentage, 1, critterSimData.growthPercentage) * 1;

				// REFRACTION:							
				float3 surfaceNormal = tex2Dlod(_WaterSurfaceTex, float4(genericStrokeData.worldPos.xy /  _MapSize, 0, 0)).yzw;
				float refractionStrength = 1;
				vertexWorldPos.xy += -surfaceNormal.xy * refractionStrength;
				

				o.vertex = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(vertexWorldPos, 1.0)));
				o.uv = quadVerticesCBuffer[id].xy + 0.5;	

				const float tilePercentage = (1.0 / 8.0);
				float2 patternUV = genericStrokeData.uv;
				float randPatternIDX = critterInitData.bodyPatternX; // ** This is now inside CritterInitData!!!!!
				float randPatternIDY = critterInitData.bodyPatternY; //  fmod(bodyStrokeData.brushTypeY, 4); // ********** UPDATE!!! **************
				patternUV *= tilePercentage; // randVariation eventually
				patternUV.x += tilePercentage * randPatternIDX;
				patternUV.y += tilePercentage * randPatternIDY;				
				
				fixed4 col = tex2Dlod(_PatternTex, float4(patternUV, 0, 0));
								
				float crudeDiffuse = dot(normalize(worldNormal), normalize(float3(-0.52, 0.35, 1))) * 0.75 + 0.25;
				o.color = float4(lerp(critterInitData.secondaryHue, critterInitData.primaryHue, col.x) * crudeDiffuse, 1); //genericStrokeData.bindPos.x * 0.5 + 0.5, genericStrokeData.bindPos.z * 0.33 + 0.5, genericStrokeData.bindPos.y * 0.5 + 0.5, 1);
				//o.color.rgb = ;
				//o.color.rgb *= 0.4;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				//return i.color;
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv) * i.color;
				//fixed4 col = tex2D(_MainTex, i.bodyUV) * i.color;
				return col;
			}
			ENDCG
		}
	}
}

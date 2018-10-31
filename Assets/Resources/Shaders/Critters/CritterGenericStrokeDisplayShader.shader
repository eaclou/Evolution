Shader "Critter/CritterGenericStrokeDisplayShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_PatternTex ("Pattern Texture", 2D) = "white" {}
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
				float3 strokeBindPos = float3(genericStrokeData.bindPos.x, genericStrokeData.bindPos.z, -genericStrokeData.bindPos.y) * 0.8;

				//Temp align with creatures:
				float2 critterForwardDir = critterSimData.heading;
				float2 critterRightDir = float2(critterForwardDir.y, -critterForwardDir.x);
								
				strokeBindPos.xy = critterRightDir * strokeBindPos.x + critterForwardDir * strokeBindPos.y;

				float3 brushScale = float3(genericStrokeData.scale, 1);
				float3 bitangent = cross(genericStrokeData.normal, genericStrokeData.tangent);
								
				float3 worldNormal = float3(genericStrokeData.normal.x, genericStrokeData.normal.z, -genericStrokeData.normal.y);
				worldNormal.xy = critterRightDir * worldNormal.x + critterForwardDir * worldNormal.y;
				float3 worldTangent = float3(genericStrokeData.tangent.x, genericStrokeData.tangent.z, -genericStrokeData.tangent.y);
				worldTangent.xy = critterRightDir * worldTangent.x + critterForwardDir * worldTangent.y;
				float3 worldBitangent = float3(bitangent.x, bitangent.z, -bitangent.y);
				worldBitangent.xy = critterRightDir * worldBitangent.x + critterForwardDir * worldBitangent.y;

				float2 quadPoints = float2((quadVerticesCBuffer[id].x + 0.5) * 2, quadVerticesCBuffer[id].y * 3.5);
				float3 brushSpriteVertexPos = (quadPoints.x + 0.5) * worldBitangent * brushScale.x + (quadPoints.y) * worldTangent * brushScale.y;

				float3 vertexWorldPos = critterWorldPos + strokeBindPos + brushSpriteVertexPos * 1.25;

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
								
				float crudeDiffuse = dot(normalize(worldNormal), normalize(float3(-0.52, 0.35, -1))) * 0.75 + 0.25;
				o.color = float4(lerp(critterInitData.secondaryHue, critterInitData.primaryHue, col.x) * crudeDiffuse, 1); //genericStrokeData.bindPos.x * 0.5 + 0.5, genericStrokeData.bindPos.z * 0.33 + 0.5, genericStrokeData.bindPos.y * 0.5 + 0.5, 1);
				//o.color.rgb = ;
				//o.color.rgb *= 0.4;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				//return i.color;
				// sample the texture
				fixed4 col = tex2D(_MainTex, float2(i.uv.y, i.uv.x)) * i.color;
				//fixed4 col = tex2D(_MainTex, i.bodyUV) * i.color;
				return col;
			}
			ENDCG
		}
	}
}

Shader "Critter/CritterGenericStrokeDisplayShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_PatternTex ("Pattern Texture", 2D) = "white" {}
		_AltitudeTex ("_AltitudeTex", 2D) = "gray" {}
		_WaterSurfaceTex ("_WaterSurfaceTex", 2D) = "black" {}
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
			

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				float4 worldPos : TEXCOORD1;
				float4 highlight : TEXCOORD2;
				//float2 bodyUV : TEXCOORD1;
			};

			sampler2D _MainTex;
			//float4 _MainTex_ST;
			sampler2D _PatternTex;
			sampler2D _AltitudeTex;
			sampler2D _WaterSurfaceTex;

			uniform float _MapSize;

			uniform float _HighlightOn;

			uniform int _HoverID;
			uniform int _SelectedID;
			uniform float _IsHover;			
			uniform float _IsSelected;

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

				// WEIRD COORDINATES!!! Positive Z = DEEPER!!!
				//float3 strokeBindPos = genericStrokeData.bindPos; //float3(genericStrokeData.bindPos.x, genericStrokeData.bindPos.z, -genericStrokeData.bindPos.y) * 0.8;

				//Temp align with creatures:
				//float3 critterForwardDir = float3(critterSimData.heading, 0);
				//float3 critterRightDir = float3(critterForwardDir.y, -critterForwardDir.x, 0);
								
				//strokeBindPos = critterRightDir * strokeBindPos.x + critterForwardDir * strokeBindPos.y;
				//strokeBindPos.z = genericStrokeData.bindPos.z;

				// Decay color bleach value:
				float decayTimeRange = 0.4;
				float decayAmount = saturate((critterSimData.decayPercentage) / decayTimeRange - genericStrokeData.thresholdValue * 2);
				float4 decayColor = float4(0.25, 0.14, 0.052, 0.9);
				

				float3 brushScale = float3(genericStrokeData.scale, 1);
								
				float3 worldNormal = genericStrokeData.worldNormal;
				float3 worldTangent = normalize(lerp(genericStrokeData.worldTangent, -neighborAlignTangent, genericStrokeData.neighborAlign));
				float3 worldBitangent = cross(worldNormal, worldTangent);
				
				float3 quadVertexOffset = quadVerticesCBuffer[id].x * worldBitangent * genericStrokeData.scale.x + quadVerticesCBuffer[id].y * worldTangent * genericStrokeData.scale.y;
				quadVertexOffset *= (1 - critterSimData.decayPercentage);
				// old //float3 vertexWorldPos = critterWorldPos + strokeBindPos + quadVerticesCBuffer[id] * 0.645 * length(genericStrokeData.scale);
				float3 vertexWorldPos = genericStrokeData.worldPos + quadVertexOffset * 1.25 * lerp(critterInitData.spawnSizePercentage, 1, critterSimData.growthPercentage) * 1;
				
				float2 altUV = vertexWorldPos.xy / _MapSize;					
				float altitudeRaw = tex2Dlod(_AltitudeTex, float4(altUV.xy, 0, 0)).x;
				float seaFloorAltitude = -(altitudeRaw * 2 - 1) * 10;

				vertexWorldPos.z = lerp(vertexWorldPos.z, seaFloorAltitude, decayAmount);
				// REFRACTION:							
				float3 surfaceNormal = tex2Dlod(_WaterSurfaceTex, float4(genericStrokeData.worldPos.xy /  _MapSize, 0, 0)).yzw;
				float refractionStrength = 0.4;
				vertexWorldPos.xy += -surfaceNormal.xy * refractionStrength;				
				
				float3 lightDir = float3(-0.52, -0.35, -1);
				lightDir.xy += -surfaceNormal.xy * 2.25;
				lightDir = normalize(lightDir);
				float3 viewDir = normalize(_WorldSpaceCameraPos - genericStrokeData.worldPos);
				float3 reflectionDir = reflect(-lightDir, worldNormal);
				float specTest = pow(saturate(dot(viewDir, reflectionDir)), 17);

				//vertexWorldPos.z += 1.0;
				o.vertex = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(vertexWorldPos, 1.0)));
				o.uv = quadVerticesCBuffer[id].xy + 0.5;	

				const float tilePercentage = (1.0 / 8.0);
				float2 patternUV = genericStrokeData.uv;
				float randPatternIDX = critterInitData.bodyPatternX; // ** This is now inside CritterInitData!!!!!
				float randPatternIDY = critterInitData.bodyPatternY; //  fmod(bodyStrokeData.brushTypeY, 4); // ********** UPDATE!!! **************
				patternUV *= tilePercentage; // randVariation eventually
				patternUV.x += tilePercentage * randPatternIDX;
				patternUV.y += tilePercentage * randPatternIDY;				
				
				fixed4 patternTexSample = tex2Dlod(_PatternTex, float4(patternUV, 0, 0));
								
				float crudeDiffuse = dot(normalize(worldNormal), lightDir) * 0.75 + 0.25;
				float3 hue = lerp(critterInitData.secondaryHue, critterInitData.primaryHue, patternTexSample.x);
				
				hue = lerp(hue, genericStrokeData.color.rgb, genericStrokeData.color.a);
				//float damagedThreshold = saturate(abs((sin((float)inst * -198264.1111927) * 319287419.8) % 100) / 100);
				float damagedThreshold = saturate(float(inst % 1024) / 1024.0);
				float damagedMask = saturate((critterSimData.health - damagedThreshold) * 9999);
				hue = lerp(float3(0.75,0.25,0.1), hue, damagedMask);
				
				float alpha = saturate((critterSimData.embryoPercentage - 0.995) * 200) * saturate((0.9 - critterSimData.decayPercentage) * 999);
								
				o.color = float4(specTest * 0.65 + hue * crudeDiffuse, alpha); //genericStrokeData.bindPos.x * 0.5 + 0.5, genericStrokeData.bindPos.z * 0.33 + 0.5, genericStrokeData.bindPos.y * 0.5 + 0.5, 1);
				o.color = lerp(o.color, decayColor, saturate(decayAmount + saturate(critterSimData.decayPercentage * 50) * 0.25));
				o.color.a *= alpha;

				o.worldPos = float4(vertexWorldPos, 1.0);
				//o.color.rgb = ;
				//o.color.rgb *= 0.4;
								
				//float debugColorVal = detachAmount;
				//o.color.rgb = float3(debugColorVal, debugColorVal, debugColorVal);
				float hoverMask = 1.0 - saturate(abs(agentIndex - _HoverID));
				float selectedMask = 1.0 - saturate(abs(agentIndex - _SelectedID));

				o.highlight = float4(hoverMask, selectedMask, 0, 0);

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				//return i.color;
				// sample the texture
				float3 waterFogColor = float3(0.03,0.4,0.3) * 0.4;

				fixed4 col = tex2D(_MainTex, i.uv) * i.color;
				col.rgb = lerp(col.rgb, waterFogColor, 0.5 * saturate((i.worldPos.z - 0.75) * 0.5));

				float highlightBoost = saturate(i.highlight.x * _IsHover + i.highlight.y * _IsSelected * 0.25) * _HighlightOn;
				col.rgb = col.rgb * (1 + highlightBoost) + highlightBoost * 0.1;

				//fixed4 col = tex2D(_MainTex, i.bodyUV) * i.color;
				return col;
			}
			ENDCG
		}
	}
}

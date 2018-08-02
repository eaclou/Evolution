Shader "Critter/CritterEyesStrokesShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_AltitudeTex ("_AltitudeTex", 2D) = "gray" {}
		_VelocityTex ("_VelocityTex", 2D) = "black" {}
		_SkyTex ("_SkyTex", 2D) = "white" {}
		_WaterSurfaceTex ("_WaterSurfaceTex", 2D) = "black" {}
		//_Tint("Color", Color) = (1,1,1,1)
		//_Size("Size", vector) = (1,1,1,1)
	}
	SubShader
	{		
		Tags{ "RenderType" = "Transparent" }
		ZWrite Off
		Cull Off
		//Blend SrcAlpha One
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#include "UnityCG.cginc"
			#include "Assets/Resources/Shaders/Inc/CritterBodyAnimation.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _AltitudeTex;			
			sampler2D _VelocityTex;
			sampler2D _SkyTex;
			sampler2D _WaterSurfaceTex;
			//float4 _Tint;
			//float4 _Size;

			struct AgentEyeStrokeData {
				int parentIndex;  // what agent/object is this attached to?				
				float2 localPos;
				float2 localDir;
				float2 localScale;
				float3 irisHue;
				float3 pupilHue;
				float strength;  // abstraction for pressure of brushstroke + amount of paint 
				int brushType;  // what texture/mask/brush pattern to use
			};
			
			StructuredBuffer<CritterInitData> critterInitDataCBuffer;
			StructuredBuffer<CritterSimData> critterSimDataCBuffer;
			StructuredBuffer<AgentEyeStrokeData> agentEyesStrokesCBuffer;
			
			StructuredBuffer<float3> quadVerticesCBuffer;
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				float4 color : TEXCOORD1;
				float animFrameBlendLerp : TEXCOORD2;
				int2 bufferIndices : TEXCOORD3;  // agent then eye
				float3 worldPos : TEXCOORD4;
				float2 altitudeUV : TEXCOORD5;
			};

			float rand(float2 co){   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
								
				AgentEyeStrokeData eyeData = agentEyesStrokesCBuffer[inst];
				int agentIndex = eyeData.parentIndex;
				CritterInitData critterInitData = critterInitDataCBuffer[agentIndex];
				CritterSimData critterSimData = critterSimDataCBuffer[agentIndex];
				o.bufferIndices = int2(agentIndex, inst);


				float3 critterWorldPos = critterSimData.worldPos;
				//float growthScale = lerp(critterInitData.spawnSizePercentage, 1, critterSimData.growthPercentage);
				float3 critterCurScale = critterInitData.boundingBoxSize * lerp(critterInitData.spawnSizePercentage, 1, critterSimData.growthPercentage) * 0.5;
				//float2 curAgentSize = critterInitData.boundingBoxSize * growthScale;
				
				float3 spriteLocalPos = float3(eyeData.localPos.x, eyeData.localPos.y * 1, -0.25) * critterCurScale;
				float3 vertexWorldOffset = quadVerticesCBuffer[id];

				
				vertexWorldOffset.xy = vertexWorldOffset.xy * critterCurScale.x * saturate(0.99 - critterSimData.decayPercentage * 2) * 0.5;
				
				
				float3 worldPosition = critterWorldPos + GetAnimatedPos(spriteLocalPos + vertexWorldOffset, float3(0,0,0), critterInitData, critterSimData, float3(eyeData.localPos, 0));
				
				//spriteLocalPos

				//float3 worldPosition = critterWorldPos + spriteLocalPos + vertexWorldOffset;
				// REFRACTION:
				//float3 offset = skinStrokeData.worldPos;				
				float3 surfaceNormal = tex2Dlod(_WaterSurfaceTex, float4(worldPosition.xy / 256, 0, 0)).yzw;
				float refractionStrength = 2.5;
				worldPosition.xy += -surfaceNormal.xy * refractionStrength;

				float embryoStatus = critterSimData.embryoPercentage;

				//float3 worldPosition = offset + vertexWorldOffset; //critterWorldPos + vertexWorldOffset; //
				
				worldPosition = lerp(critterSimData.worldPos, worldPosition, embryoStatus);



				//worldPosition = critterWorldPos + quadVerticesCBuffer[id]

				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)));
				o.worldPos = worldPosition;

				float2 altUV = (worldPosition.xy + 128) / 512;
				o.altitudeUV = altUV;

				/*
				float2 centerPosition = eyeData.localPos;				
				float bodyAspectRatio = critterInitData.boundingBoxSize.y / critterInitData.boundingBoxSize.x;
				float bendStrength = 0.5 * saturate(bodyAspectRatio * 0.5 - 0.4);				
				float aspect = eyeData.localScale.y;				
				float2 eyeLocalScale = float2(eyeData.localScale.x * aspect, eyeData.localScale.x * (1.0 / aspect));
				centerToVertexOffset *= eyeLocalScale * growthScale * (critterInitData.boundingBoxSize.x + critterInitData.boundingBoxSize.y) * 0.5 * (1.0 - critterSimData.decayPercentage);
				float2 forwardGaze = normalize(critterSimData.velocity);
				if(length(critterSimData.velocity) < 0.0001) {
					forwardGaze = critterSimData.heading;
				}
				float3 worldPosition = float3(critterPosition + centerPosition + centerToVertexOffset, 1.0);
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)));
				*/

				o.color = float4(1, 1, 1, 1);	// change color of eyes if dead X's?'
				
				const float tilePercentage = (1.0 / 8.0);

				float2 uv0 = quadVerticesCBuffer[id] + 0.5f; // full texture
				float2 uv1 = uv0;
				// Which Brush? (uv.X) :::::
				float eyeBrush = 0;  // What eye type variation? horizontal axis <-->
				uv0.x *= tilePercentage; 
				uv0.x += tilePercentage * eyeBrush;
				uv1.x *= tilePercentage; 
				uv1.x += tilePercentage * eyeBrush;

				// Figure out how much to blur:
				// Y coords = animation frame!  // 0 = normal, 1 = closed, 2 = dead
				float animFrame = 0; // sin(agentSimData.eatingStatus * 3.141592) * 0.99;	
				if(critterSimData.decayPercentage > 0) {
					animFrame = 2;
				}
				float frameRow0 = floor(animFrame);  // 0-2
				float frameRow1 = ceil(animFrame); // 1-3
				float animFrameBlendLerp = animFrame - frameRow0;
				// calculate UV's to sample from correct rows:
				uv0.y = uv0.y * tilePercentage + tilePercentage * frameRow0;
				uv1.y = uv1.y * tilePercentage + tilePercentage * frameRow1;
				
				// What percentage of frame interpolation btw current and next frame
				o.animFrameBlendLerp = animFrameBlendLerp;				
				o.uv = float4(uv0, uv1);
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//return float4(1,1,1,1);

				AgentEyeStrokeData eyeData = agentEyesStrokesCBuffer[i.bufferIndices.y];
				CritterInitData critterInitData = critterInitDataCBuffer[i.bufferIndices.x];
				CritterSimData critterSimData = critterSimDataCBuffer[i.bufferIndices.x];
				
				float4 texColor0 = tex2D(_MainTex, i.uv.xy);  // Read Brush Texture start Row
				float4 texColor1 = tex2D(_MainTex, i.uv.zw);  // Read Brush Texture end Row
				
				float4 brushColor = lerp(texColor0, texColor1, i.animFrameBlendLerp);

				float3 rgb = critterInitData.primaryHue;
				rgb = lerp(rgb, float3(1,1,1), brushColor.r);  // White of the eye
				rgb = lerp(rgb, eyeData.irisHue, brushColor.g);  // Iris
				rgb = lerp(rgb, eyeData.pupilHue, brushColor.b);  // Pupil
				
				float4 finalColor = float4(rgb * 1.0, brushColor.a);
				//finalColor.rgb = lerp(float3(0.45, 0.45, 0.45), finalColor.rgb, saturate(critterSimData.energy * 5));
				
				float fogAmount = 0.25; //saturate((i.worldPos.z + 1) * 0.5);				
				//finalColor.rgb *= saturate(1.0 - i.color.w * 32) * 0.5 + 0.5;
				//finalColor.rgb = lerp(finalColor.rgb, backgroundColor, i.color.a);
				float3 surfaceNormal = tex2D(_WaterSurfaceTex, (i.altitudeUV - 0.25) * 2).yzw;
				float dotLight = dot(surfaceNormal, _WorldSpaceLightPos0.xyz);
				dotLight = dotLight * dotLight;

				float diffuseLight = 0.75;
				// FAKE CAUSTICS:::				
				dotLight *= saturate(diffuseLight * 2.5);
				finalColor.rgb = lerp(finalColor.rgb, finalColor.rgb * (dotLight * 0.6 + 0.4) + dotLight * 1.1, 1); //dotLight * 1.0;
				float3 waterFogColor = float3(0.03,0.4,0.3) * 0.4;
				finalColor.rgb = lerp(finalColor.rgb, waterFogColor, fogAmount);

				//return float4(1,1,1,1);
				return finalColor;
				
			}
		ENDCG
		}
	}
}

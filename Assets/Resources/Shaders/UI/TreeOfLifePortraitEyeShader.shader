Shader "UI/TreeOfLifePortraitEyeShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_AltitudeTex ("_AltitudeTex", 2D) = "gray" {}
		_VelocityTex ("_VelocityTex", 2D) = "black" {}
		_SkyTex ("_SkyTex", 2D) = "white" {}
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
			#include "Assets/Resources/Shaders/Inc/NoiseShared.cginc"

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

			sampler2D _MainTex;
			sampler2D _PatternTex;
			sampler2D _AltitudeTex;			
			sampler2D _VelocityTex;
			sampler2D _SkyTex;
			sampler2D _WaterSurfaceTex;
			
			sampler2D _RenderedSceneRT;  // Provided by CommandBuffer -- global tex??? seems confusing... ** revisit this
			
			uniform float _MapSize;

			uniform float4 _TopLeftCornerWorldPos;
			uniform float4 _CamRightDir;
			uniform float4 _CamUpDir;

			uniform float _CamScale;

			uniform float _AnimatedScale1;  // signal from main CPU program of whether the panel is active, hidden, or interpolating
			uniform float _AnimatedScale2; // signal from main CPU program of whether the panel is active, hidden, or interpolating

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

				float2 forward = critterSimData.heading;
				float2 right = float2(forward.y, -forward.x); // perpendicular to forward vector

				float3 critterWorldPos = critterSimData.worldPos;
				
				float3 critterCurScale = critterInitData.boundingBoxSize * lerp(critterInitData.spawnSizePercentage, 1, critterSimData.growthPercentage) * 0.5;
					
				float3 spriteLocalPos = float3(eyeData.localPos.x, eyeData.localPos.y * 0.5, -0.5) * critterCurScale;
				float3 vertexWorldOffset = quadVerticesCBuffer[id];

				// EGG EMBRYO MASK:
				float eggMask = saturate(saturate(critterSimData.embryoPercentage - 0.9) * 10);
				spriteLocalPos *= eggMask;
				vertexWorldOffset *= eggMask;				
				vertexWorldOffset.xy = vertexWorldOffset.xy * critterCurScale.x * saturate(0.99 - critterSimData.decayPercentage * 2) * 0.5;				
				
				float3 pivot = _TopLeftCornerWorldPos.xyz + (_CamRightDir.xyz * 0.5f - _CamUpDir.xyz * 0.5f) * _CamScale; 
				spriteLocalPos = GetAnimatedPos(spriteLocalPos, float3(0,0,0), critterInitData, critterSimData, float3(eyeData.localPos, 0));
				//float3 localPos = GetAnimatedPos(spriteLocalPos, float3(0,0,0), critterInitData, critterSimData, float3(eyeData.localPos, 0));
				
				float3 worldPosition = pivot + spriteLocalPos;
				
				// REFRACTION:			
				//float3 surfaceNormal = tex2Dlod(_WaterSurfaceTex, float4(float2(128, 128) / 256, 0, 0)).yzw;
				//float refractionStrength = 2.5;
				//worldPosition.xy += -surfaceNormal.xy * refractionStrength;

				float embryoStatus = critterSimData.embryoPercentage;

				
				vertexWorldOffset = float3(vertexWorldOffset.x * right + vertexWorldOffset.y * forward, vertexWorldOffset.z);  // Rotate localRotation by AgentRotation
				//spriteLocalPos = float3(spriteLocalPos.x * right + spriteLocalPos.y * forward, spriteLocalPos.z);
				//worldPosition = pivot + spriteLocalPos * 0.75 + vertexWorldOffset; //quadVerticesCBuffer[id];
				worldPosition += vertexWorldOffset;
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)));
				o.worldPos = worldPosition;

				float2 altUV = (worldPosition.xy + 128) / 512;
				o.altitudeUV = altUV;

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
			
			fixed4 frag (v2f i) : SV_Target
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
				
				float fogAmount = 0.25; //saturate((i.worldPos.z + 1) * 0.5);		
				float3 surfaceNormal = tex2D(_WaterSurfaceTex, (i.altitudeUV - 0.25) * 2).yzw;
				float dotLight = dot(surfaceNormal, _WorldSpaceLightPos0.xyz);
				dotLight = dotLight * dotLight;

				float diffuseLight = 0.75;
				// FAKE CAUSTICS:::				
				dotLight *= saturate(diffuseLight * 2.5);
				finalColor.rgb = lerp(finalColor.rgb, finalColor.rgb * (dotLight * 0.6 + 0.4) + dotLight * 1.1, 1); //dotLight * 1.0;
				float3 waterFogColor = float3(0.03,0.4,0.3) * 0.4;
				
				return finalColor;

			}
			ENDCG
		}
	}
}

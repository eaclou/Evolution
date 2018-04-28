Shader "Unlit/AgentBodyDisplayShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_PatternTex ("Pattern Texture", 2D) = "white" {}
		_BumpMap("Normal Map", 2D) = "bump" {}		
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
			#include "Assets/Resources/Shaders/Inc/NoiseShared.cginc"

			sampler2D _MainTex;
			sampler2D _PatternTex;
			sampler2D _BumpMap;
			
			struct BodyStrokeData {
				int parentIndex;  // what agent/object is this attached to?				
				float2 localPos;
				float2 localDir;
				float2 localScale;
				float strength;  // abstraction for pressure of brushstroke + amount of paint 
				int brushTypeX;  // what texture/mask/brush pattern to use
				int brushTypeY;
			};

			struct AgentSimData {
				float2 worldPos;
				float2 velocity;
				float2 heading;
				float2 size;
				float3 primaryHue;  // can eventually pull these static variables out of here to avoid per-frame updates on non-dynamic attributes
				float3 secondaryHue;
				float maturity;
				float decay;
				float eatingStatus;
				float foodAmount;
			};

			StructuredBuffer<AgentSimData> agentSimDataCBuffer;
			StructuredBuffer<BodyStrokeData> bodyStrokesCBuffer;
			StructuredBuffer<float3> quadVerticesCBuffer;
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				float4 color : TEXCOORD1;
				float frameBlendLerp : TEXCOORD2;
				half3 tspace0 : TEXCOORD3;
				half3 tspace1 : TEXCOORD4;
				half3 tspace2 : TEXCOORD5;
				float3 worldPos : TEXCOORD6;
				float2 quadUV : TEXCOORD7;
				float2 uvPattern : TEXCOORD8;
				int2 bufferIndices : TEXCOORD9;
			};

			float rand(float2 co){   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
								
				BodyStrokeData bodyStrokeData = bodyStrokesCBuffer[inst];
				AgentSimData agentSimData = agentSimDataCBuffer[bodyStrokeData.parentIndex];
				o.bufferIndices = int2(inst, bodyStrokeData.parentIndex);
								
				float3 worldPosition = float3(agentSimData.worldPos, -0.5);
				// Rotation of Billboard center around Agent's Center (no effect if localPos and localDir are zero/default)'
				float2 forwardAgent = agentSimData.heading;
				float2 rightAgent = float2(forwardAgent.y, -forwardAgent.x);
				float2 positionOffset = float2(bodyStrokeData.localPos.x * agentSimData.size.x * rightAgent + bodyStrokeData.localPos.y * agentSimData.size.y * forwardAgent) * 0.5;
				worldPosition.xy += positionOffset; // Place properly

				// NoiseWobble offset:
				//float clock = _Time.y;

				float3 quadPoint = quadVerticesCBuffer[id];
				float2 quadUV = quadVerticesCBuffer[id].xy + 0.5;
				

				half3 wNormal = float3(0,0,-1); // -1 or 1 ???                
				half3 wTangent = float3(rightAgent, 0); //UnityObjectToWorldDir(tangent.xyz); // need to rotate this manually
                // compute bitangent from cross product of normal and tangent
                half tangentSign = -1 * unity_WorldTransformParams.w;  // 1 or -1 ????
                half3 wBitangent = cross(wNormal, wTangent) * tangentSign;
                // output the tangent space matrix
                o.tspace0 = half3(wTangent.x, wBitangent.x, wNormal.x);
                o.tspace1 = half3(wTangent.y, wBitangent.y, wNormal.y);
                o.tspace2 = half3(wTangent.z, wBitangent.z, wNormal.z);

				
				float random1 = rand(float2(inst, inst));
				float random2 = rand(float2(random1, random1));
				//float randomAspect = lerp(0.75, 1.33, random1);
				//float randomValue = 1; //rand(float2(inst, randomAspect * 10));

				//float velMag = saturate(length(agentSimData.velocity)) * 0.5;
				//float aspect = bodyStrokeData.localScale.y;				
				//float2 scale = float2(eyeData.localScale.x * (1.0 / aspect), eyeData.localScale.x * aspect) * agentSimData.maturity;
				float2 scale = agentSimData.size * agentSimData.maturity;  // assume bodyStroke is 1:1 agentSize
				//scale.y *= (velMag + 1);
				quadPoint *= float3(scale, 1.0);

				// Figure out final facing Vectors!!!
				float2 forward0 = agentSimData.heading;
				float2 right0 = float2(forward0.y, -forward0.x); // perpendicular to forward vector
				float2 rotatedPoint0 = float2(bodyStrokeData.localDir.x * right0 + bodyStrokeData.localDir.y * forward0);  // Rotate localRotation by AgentRotation

				float2 forward1 = rotatedPoint0;
				float2 right1 = float2(forward1.y, -forward1.x);
				// With final facing Vectors, find rotation of QuadPoints:
				float3 rotatedPoint1 = float3(quadPoint.x * right1 + quadPoint.y * forward1,
											 quadPoint.z);

				//float freq = 1000;
				//float amp = 0.05;
				//float3 noiseOffset = Value2D(worldPosition.x * 0.002 * length(agentSimData.velocity), freq) * amp;
				//rotatedPoint1.xy += noiseOffset.yz;

				float alpha = 1;
				//alpha = alpha * agentSimData.maturity;
				alpha = alpha * (1.0 - agentSimData.decay);
				float activeColorLerp = 1;
				activeColorLerp = activeColorLerp * floor(agentSimData.maturity + 0.01);
				activeColorLerp = activeColorLerp * floor((1.0 - agentSimData.decay) + 0.01);
				
				//o.pos = mul(UNITY_MATRIX_VP, float4(rotatedPoint, 0.0f));
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(rotatedPoint1, 0.0f));
				o.worldPos = worldPosition + rotatedPoint1;
				o.color = lerp(float4(0.5, 0.5, 0.5, alpha), float4(0,0,1,alpha), activeColorLerp);	

				const float tilePercentage = (1.0 / 8.0);

				float2 baseUV = quadVerticesCBuffer[id] + 0.5f;				

				o.quadUV = baseUV;

				// PATTERNS UV:
				float2 patternUV = baseUV;
				float randPatternIDX = bodyStrokeData.brushTypeX;
				float randPatternIDY = fmod(bodyStrokeData.brushTypeY, 4); // ********** UPDATE!!! **************
				patternUV *= tilePercentage; // randVariation eventually
				patternUV.x += tilePercentage * randPatternIDX;
				patternUV.y += tilePercentage * randPatternIDY;
				o.uvPattern = patternUV;
				
				// BodyAnimFrame UVS:
				float2 uv0 = baseUV; // full texture
				float2 uv1 = uv0;							
				float randBrush = 0; //pointStrokeData.brushType; //floor(rand(float2(random2, inst)) * 3.99); // 0-3
				uv0.x *= tilePercentage;  // 5 brushes on texture
				uv0.x += tilePercentage * randBrush;
				uv1.x *= tilePercentage;  // 5 brushes on texture
				uv1.x += tilePercentage * randBrush;

				// Figure out how much to blur:
				float animFrame = agentSimData.maturity * 3 + agentSimData.decay * 4; //saturate(length(agentSimData.velocity)) * 2.99;				
				float animRow0 = floor(animFrame);  // 0-6
				float animRow1 = ceil(animFrame); // 1-7
				//float blurLerp = animFrame - blurRow0;
				// calculate UV's to sample from correct rows:
				uv0.y = uv0.y * tilePercentage + tilePercentage * animRow0;
				uv1.y = uv1.y * tilePercentage + tilePercentage * animRow1;
				// motion blur sampling:
				o.frameBlendLerp = animFrame - animRow0; //blurLerp;
				
				o.uv = float4(uv0, uv1);
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				BodyStrokeData bodyStrokeData = bodyStrokesCBuffer[i.bufferIndices.x];
				AgentSimData agentSimData = agentSimDataCBuffer[i.bufferIndices.y];

				//noise offset UVs slightly:
				float clock = _Time.y;
				float freq = 1.25;
				float amp = 0.003;
				float3 noiseOffset = Value2D(i.worldPos.xy * 1 * saturate(length(agentSimData.velocity) * 1), freq) * amp;
				//i.uv.xy += noiseOffset.yz;
				//i.uv.zw += noiseOffset.yz;
				//i.uvPattern += noiseOffset.yz;
				//i.quadUV += noiseOffset.yz * 8;
				
				half3 tnormal = UnpackNormal(tex2D(_BumpMap, i.quadUV)); // ***  *** Move this to 8x8 tiles to match Albedo map
                // transform normal from tangent to world space
                half3 worldNormal;
                worldNormal.x = dot(i.tspace0, tnormal);
                worldNormal.y = dot(i.tspace1, tnormal);
                worldNormal.z = dot(i.tspace2, tnormal);


				float4 texColor0 = tex2D(_MainTex, i.uv.xy);  // Read Brush Texture start Row
				float4 texColor1 = tex2D(_MainTex, i.uv.zw);  // Read Brush Texture end Row

				float4 patternSample = tex2D(_PatternTex, i.uvPattern);
				
				float4 brushColor = lerp(texColor0, texColor1, i.frameBlendLerp);

				float4 finalColor = float4(lerp(agentSimData.primaryHue, agentSimData.secondaryHue, patternSample.x), 1);
				finalColor.a = brushColor.a * i.color.a;

				finalColor.rgb = lerp(float3(0.5, 0.5, 0.5), finalColor.rgb, saturate(agentSimData.maturity * 4 - 3));
				finalColor.rgb = lerp(float3(0.5, 0.5, 0.5), finalColor.rgb, saturate(agentSimData.foodAmount * 4));
				//finalColor.a = min(finalColor.a, lerp(0.33, finalColor.a, saturate(agentSimData.foodAmount * 3.5)));

				float dotNml = dot(normalize(float3(-0.2,1,-0.75)), worldNormal);
				finalColor.xyz += dotNml * 0.35 + 0.005;
				//finalColor.yz = 0;
				
				return finalColor;
				
			}
		ENDCG
		}
	}
}

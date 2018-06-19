Shader "Unlit/TestSwimmingBodyShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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

			struct AgentMovementAnimData {
				float animCycle;
				float turnAmount;
				float accel;
				float smoothedThrottle;
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

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture				
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			StructuredBuffer<AgentSimData> agentSimDataCBuffer;
			StructuredBuffer<AgentMovementAnimData> agentMovementAnimDataCBuffer;
			StructuredBuffer<float3> meshVerticesCBuffer;
			Texture2D<float4> widthsTex;

			float2 rotate_point(float2 pivot,float angle, float2 p)
			{
				float2 rotatedPoint = p;
				float s = sin(angle);
				float c = cos(angle);

				// translate point back to origin:
				rotatedPoint -= pivot;
			  
				// rotate point
				float xnew = rotatedPoint.x * c - rotatedPoint.y * s;
				float ynew = rotatedPoint.x * s + rotatedPoint.y * c;

				rotatedPoint = float2(xnew, ynew);

				// translate point back:
				rotatedPoint += pivot;

				return rotatedPoint;
			}

			float2 getWarpedPoint(float2 originalPoint, float v, float turnAmount, float warpStrength, float pivot, float animCycle, float accel, float throttle) {
				// --------------Swim Anim:-----------------------
				float animSpeed = 30;
				float accelAnimSpeed = 55;
				float offsetMask = saturate(1 - v * 0.75);
				//float2 horOffset = float2(0,0);
				
				// panning yaw:
				//float bodyAspectRatio = agentSimData.size.y / agentSimData.size.x;
				float panningYawStrength = warpStrength; //0.5 * saturate(bodyAspectRatio * 0.5 - 0.4);
				float turningAngle = turnAmount;
				float2 warpedPoint = rotate_point(float2(0,pivot), clamp(turningAngle * -1, -1, 1) * offsetMask + panningYawStrength * sin(v * 3.2 + animCycle * animSpeed + accel * accelAnimSpeed) * offsetMask * throttle, originalPoint);
				
				return warpedPoint;
			}
			
			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
								
				
				AgentSimData agentSimData = agentSimDataCBuffer[inst];
				AgentMovementAnimData animData = agentMovementAnimDataCBuffer[inst];
												
				float3 worldPosition = float3(agentSimData.worldPos, -0.5);
				// Rotation of Billboard center around Agent's Center (no effect if localPos and localDir are zero/default)'
				float2 forwardAgent = agentSimData.heading;
				float2 rightAgent = float2(forwardAgent.y, -forwardAgent.x);
				float2 positionOffset = float2(0, 0); // float2(agentSimData.size.x * rightAgent + agentSimData.size.y * forwardAgent) * 0.5;
				worldPosition.xy += positionOffset; // Place properly
								
				float3 quadPoint = meshVerticesCBuffer[id];				
				float2 quadUV = meshVerticesCBuffer[id].xy;
				quadUV.x += 0.5;
				
				float2 scale = agentSimData.size * agentSimData.maturity;  // assume bodyStroke is 1:1 agentSize

				float4 texWidth = widthsTex[int2(floor(quadPoint.y * 16), inst)];
				scale.x = texWidth.x * agentSimData.maturity;

				quadPoint.y -= 0.5;
				quadPoint *= float3(scale, 1.0);
				
				// --------------Swim Anim:-----------------------
				/*float animSpeed = 30;
				float offsetMask = saturate(1 - quadUV.y * 0.75);
				float2 horOffset = float2(0,0);
				
				float2 turningOffset = rotate_point(float2(0,agentSimData.size.y * 0.2), clamp(animData.turnAmount * 5, -1, 1) * -1.4 * saturate(1 - quadUV.y), quadPoint.xy);
				
				// hor translation:
				horOffset.x += sin(animData.animCycle * animSpeed + animData.accel * 55) * 0.25;
				horOffset *= 0.0;
				// yaw:
				float2 yawOffset = rotate_point(float2(0,agentSimData.size.y * 0.4), sin(animData.animCycle * animSpeed) * 0.25, quadPoint.xy) - quadPoint.xy;
				yawOffset *= 0.0;
				// panning yaw:
				float bodyAspectRatio = agentSimData.size.y / agentSimData.size.x;
				float panningYawStrength = 0.5 * saturate(bodyAspectRatio * 0.5 - 0.4);
				float turningAngle = animData.turnAmount;
				float2 panningYawOffset = rotate_point(float2(0,agentSimData.size.y * 0.25), clamp(turningAngle * -1, -1, 1) * offsetMask + panningYawStrength * sin(quadUV.y * 3.2 + animData.animCycle * animSpeed + animData.accel * 55) * offsetMask * animData.smoothedThrottle, quadPoint.xy) - quadPoint.xy;
				//panningYawOffset *= panningYawStrength;

				float2 totalOffset = horOffset + yawOffset + panningYawOffset;
				quadPoint.xy += totalOffset; // * offsetMask * animData.smoothedThrottle;
				//quadPoint.xy = lerp(quadPoint.xy, turningOffset, saturate(abs(animData.turnAmount)));
				*/

				float bodyAspectRatio = agentSimData.size.y / agentSimData.size.x;
				float panningYawStrength = 0.5 * saturate(bodyAspectRatio * 0.5 - 0.4);
				quadPoint.xy = getWarpedPoint(quadPoint.xy, quadUV.y, animData.turnAmount, panningYawStrength, agentSimData.size.y * 0.25, animData.animCycle, animData.accel, animData.smoothedThrottle);
				
				float3 rotatedPoint1 = float3(quadPoint.x * rightAgent + quadPoint.y * forwardAgent,
											 quadPoint.z);

				float alpha = 1;
				
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)) + float4(rotatedPoint1, 0.0));
				o.uv = quadUV;
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{				
				float4 texColor = tex2D(_MainTex, i.uv);				
				float4 finalColor = float4(1,1,0,texColor.a);
				
				return finalColor;
				
			}
			ENDCG
		}
	}
}

Shader "Unlit/FrameBufferSampleStrokeDisplayShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		//_Tint("Color", Color) = (1,1,1,1)
		//_Size("Size", vector) = (1,1,1,1)
	}
	SubShader
	{		
		Tags { "RenderType"="Transparent" }
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

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			sampler2D _RenderedSceneRT;  // Provided by CommandBuffer -- global tex??? seems confusing... ** revisit this
			//float4 _Tint;
			//float4 _Size;

			struct FrameBufferStrokeData {
				float3 worldPos;
			};

			StructuredBuffer<FrameBufferStrokeData> frameBufferStrokesCBuffer;
			StructuredBuffer<float3> quadVerticesCBuffer;			

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture
				float2 centerUV : TEXCOORD1;
			};

			float rand(float2 co){   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
								
				FrameBufferStrokeData strokeData = frameBufferStrokesCBuffer[inst];

				float3 worldPosition = strokeData.worldPos;
				float3 quadPoint = quadVerticesCBuffer[id];
				
				float random1 = rand(float2(inst, inst));
				float random2 = rand(float2(random1, random1));
				float randomAspect = lerp(0.75, 1.33, random1);
				//float randomValue = 1; //rand(float2(inst, randomAspect * 10));

				//float velMag = saturate(length(agentSimData.velocity)) * 0.5;
				
				float2 scale = float2(2,2) * randomAspect;
				quadPoint *= float3(scale, 1.0);

				// &&&& Screen-space UV of center of brushstroke:
				// Magic to get proper UV's for sampling from GBuffers:
				float4 pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0)); // *** Revisit to better understand!!!! ***
				float4 centerUV = ComputeScreenPos(pos);
				o.centerUV = centerUV.xy / centerUV.w;

				// Figure out final facing Vectors!!!
				//float2 forward0 = agentSimData.heading;
				//float2 right0 = float2(forward0.y, -forward0.x); // perpendicular to forward vector
				//float2 rotatedPoint0 = float2(pointStrokeData.localDir.x * right0 + pointStrokeData.localDir.y * forward0);  // Rotate localRotation by AgentRotation

				//float2 forward1 = rotatedPoint0;
				//float2 right1 = float2(forward1.y, -forward1.x);
				// With final facing Vectors, find rotation of QuadPoints:
				//float3 rotatedPoint1 = float3(quadPoint.x * right1 + quadPoint.y * forward1,
				//							 quadPoint.z);
				
				//o.pos = mul(UNITY_MATRIX_VP, float4(rotatedPoint, 0.0f));
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(quadPoint, 0.0f));
				//o.color = float4(pointStrokeData.hue,1);
				
				//float2 uv0 = quadVerticesCBuffer[id] + 0.5f; // full texture
				//float2 uv1 = uv0;
				// Which Brush? (uv.X) :::::
				//float randBrush = pointStrokeData.brushType; //floor(rand(float2(random2, inst)) * 3.99); // 0-3
				//uv0.x *= 0.2;  // 5 brushes on texture
				//uv0.x += 0.2 * randBrush;
				//uv1.x *= 0.2;  // 5 brushes on texture
				//uv1.x += 0.2 * randBrush;

				// Figure out how much to blur:
				//float blurMag = saturate(length(agentSimData.velocity)) * 2.99;				
				//float blurRow0 = floor(blurMag);  // 0-2
				//float blurRow1 = ceil(blurMag); // 1-3
				//float blurLerp = blurMag - blurRow0;
				// calculate UV's to sample from correct rows:
				//uv0.y = uv0.y * 0.25 + 0.25 * blurRow0;
				//uv1.y = uv1.y * 0.25 + 0.25 * blurRow1;
				// motion blur sampling:
				//o.motionBlurLerp = blurLerp;
				
				o.uv = quadVerticesCBuffer[id] + 0.5f; // full texture
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				
				//float4 texColor0 = tex2D(_MainTex, i.uv.xy);  // Read Brush Texture start Row
				//float4 texColor1 = tex2D(_MainTex, i.uv.zw);  // Read Brush Texture end Row
				
				float4 brushColor = tex2D(_MainTex, i.uv);				
				float4 frameBufferColor = tex2D(_RenderedSceneRT, i.centerUV);  //  Color of brushtroke source	
				
				float4 finalColor = frameBufferColor;
				finalColor.a = brushColor.a;
				
				return finalColor;
				
			}
		ENDCG
		}
	}
}

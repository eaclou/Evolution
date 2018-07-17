Shader "Unlit/WaterTestStrokeDisplayShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_AltitudeTex ("_AltitudeTex", 2D) = "gray" {}
		_VelocityTex ("_VelocityTex", 2D) = "black" {}
		_Cube("Reflection Map", CUBE) = "" {}
		//_FluidColorTex ("_FluidColorTex", 2D) = "black" {}
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
			sampler2D _AltitudeTex;			
			sampler2D _VelocityTex;
			samplerCUBE _Cube;
			
			sampler2D _RenderedSceneRT;  // Provided by CommandBuffer -- global tex??? seems confusing... ** revisit this
			
			struct FrameBufferStrokeData {
				float3 worldPos;
				float2 scale;
				float2 heading;
				int brushType;
			};

			StructuredBuffer<FrameBufferStrokeData> frameBufferStrokesCBuffer;
			StructuredBuffer<float3> quadVerticesCBuffer;			

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 quadUV : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture
				float2 centerUV : TEXCOORD1;
				float4 screenUV : TEXCOORD2;
				float3 worldPos : TEXCOORD3;
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

				o.quadUV = quadPoint + 0.5;
				o.worldPos = worldPosition;
				float2 uv = (worldPosition.xy + 128) / 512;
				o.centerUV = uv;
				
				float2 scale = strokeData.scale * 3;
				scale.x *= 0.5;
				quadPoint *= float3(scale, 1.0);
				
				float4 fluidVelocity = tex2Dlod(_VelocityTex, float4(worldPosition.xy / 256, 0, 3));
				float2 fluidDir = float2(0,1); //normalize(fluidVelocity.xy);
				if(length(fluidVelocity) > 0.0000001) {
					fluidDir = normalize(fluidVelocity.xy);
				}

				// Figure out final facing Vectors!!!
				float2 forward = fluidDir; //strokeData.heading;
				float2 right = float2(forward.y, -forward.x); // perpendicular to forward vector
				float2 rotatedPoint = float2(quadPoint.x * right + quadPoint.y * forward);  // Rotate localRotation by AgentRotation

				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)) + float4(rotatedPoint, 0, 0));


				float4 pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0)); // *** Revisit to better understand!!!! ***
				float4 screenUV = ComputeScreenPos(pos);
				o.screenUV = screenUV; //centerUV.xy / centerUV.w;
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float2 screenUV = i.screenUV.xy / i.screenUV.w;

				float distFromScreenCenter = length(screenUV * 2 - 1);
				float vignetteMask = smoothstep(0.5, 2.0, distFromScreenCenter) * 0.8;

				// get view space position of vertex
                //float3 viewPos = UnityObjectToViewPos(i.worldPos);
				float3 cameraToVertex = i.worldPos - _WorldSpaceCameraPos;
                float3 cameraToVertexDir = normalize(cameraToVertex);

				float time = _Time.y;
				float3 waterWorldNormal = normalize(float3(sin(i.worldPos.x * 0.5 + time * 6) * 0.005, 
														   cos(i.worldPos.y * 0.9 + time * 4.2 + 12) * 0.005,
														   -1));  // -Z is UP

				float3 reflectedViewDir = cameraToVertexDir + 2 * waterWorldNormal;

				float3 coords = reflectedViewDir; //normalize(float3(i.centerUV, 1));
				float4 cubeMapColor = texCUBE(_Cube, coords);
                //finalColor.xyz = DecodeHDR(val, unity_SpecCube0_HDR);

				//float4 texColor0 = tex2D(_MainTex, i.uv.xy);  // Read Brush Texture start Row
				//float4 texColor1 = tex2D(_MainTex, i.uv.zw);  // Read Brush Texture end Row
				
				float4 brushColor = tex2D(_MainTex, i.quadUV);	
				//float4 frameBufferColor = tex2D(_RenderedSceneRT, screenUV);  //  Color of brushtroke source	
				// use separate camera?
								
				float altitude = tex2D(_AltitudeTex, i.centerUV); // [-1,1] range
				float isUnderwater = saturate((-altitude + 0.5) * 10000);

				float val = 1;
				float3 valColor = float3(val, val, val);

				float4 finalColor = float4(cubeMapColor.rgb, isUnderwater * brushColor.a);
				finalColor.a *= vignetteMask;

				//finalColor.xyz = lerp(finalColor.xyz, float3(0.02,0.17,0.67), 0.25 * (saturate(altitude * 20) * 0.2 + 0.4 * (saturate(altitude * 4))));
				//finalColor.a = saturate((altitude + 0.08) * 7) * brushColor.a;
				
				//return float4(1,1,1,finalColor.a);

				//finalColor.rgb *= (sin(altitude * 20) * 0.5 + 0.5) * 0.5 + 0.5;
				return finalColor;
				
			}
		ENDCG
		}
	}
}

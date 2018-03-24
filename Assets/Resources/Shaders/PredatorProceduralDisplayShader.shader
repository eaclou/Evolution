Shader "Unlit/PredatorProceduralDisplayShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_BumpMap("Normal Map", 2D) = "bump" {}	
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

			sampler2D _MainTex;
			sampler2D _BumpMap;
			
			struct PredatorSimData {
				float2 worldPos;
				float2 velocity;
				float scale;
			};

			StructuredBuffer<PredatorSimData> predatorSimDataCBuffer;
			StructuredBuffer<float3> quadVerticesCBuffer;

			

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				float4 color : TEXCOORD1;
				half3 tspace0 : TEXCOORD2;
				half3 tspace1 : TEXCOORD3;
				half3 tspace2 : TEXCOORD4;
			};

			float rand(float2 co){   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
				
				//o.color = floatingGlowyBitsCBuffer[inst].color;
				PredatorSimData rawData = predatorSimDataCBuffer[inst];
				float3 worldPosition = float3(rawData.worldPos, -0.25);  // -25 arbitrary to be visible above floaty bits & BG
				float3 quadPoint = quadVerticesCBuffer[id];

				float random1 = rand(float2(inst, inst));
				float random2 = rand(float2(random1, random1));

				float2 scale = float2(1.3, 1.3) * rawData.scale;
				quadPoint *= float3(scale, 1.0);

				float clock = _Time.y;

				// ROTATION:
				float rotationAngle = random1 * 10.0 * 3.141592 + clock;  // radians
				float3 rotatedPoint = float3(quadPoint.x * cos(rotationAngle) - quadPoint.y * sin(rotationAngle),
											 quadPoint.x * sin(rotationAngle) + quadPoint.y * cos(rotationAngle),
											 quadPoint.z);
				float2 rotatedTangent = float2(1 * cos(rotationAngle) - 0 * sin(rotationAngle), 1 * sin(rotationAngle) + 0 * cos(rotationAngle));

				half3 wNormal = float3(0,0,-1); // -1 or 1 ???                
				half3 wTangent = float3(rotatedTangent, 0);
                // compute bitangent from cross product of normal and tangent
                half tangentSign = -1 * unity_WorldTransformParams.w;  // 1 or -1 ????
                half3 wBitangent = cross(wNormal, wTangent) * tangentSign;
                // output the tangent space matrix
                o.tspace0 = half3(wTangent.x, wBitangent.x, wNormal.x);
                o.tspace1 = half3(wTangent.y, wBitangent.y, wNormal.y);
                o.tspace2 = half3(wTangent.z, wBitangent.z, wNormal.z);

				//float2 velocity = rawData.velocity;
				// Scaling!!!  _Size.zw is min/max aspect ratio, _Size.xy is min/max overall size				
				//float randomAspect = lerp(_Size.z, _Size.w, random1);
				//float randomAspect = lerp(0.67, 1.33, random1);
				//float randomScale = lerp(_Size.x, _Size.y, random2);
				//float randomValue = rand(float2(inst, randomAspect * 10));
				//float randomScale = lerp(0.033, 0.09, random2);
				//float2 scale = float2(randomAspect * randomScale, (1.0 / randomAspect) * randomScale * (length(velocity) * 50 + 1));				
				//float2 forward = normalize(velocity);
				//float2 right = float2(forward.y, -forward.x); // perpendicular to forward vector
				//float3 rotatedPoint = float3(quadPoint.x * right + quadPoint.y * forward,
				//							 quadPoint.z);
				//float3 rotatedPoint = quadPoint; // TEMP!!!

				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(rotatedPoint, 0.0f));
				o.color = float4(1, 1, 1, 1);
				o.uv = quadVerticesCBuffer[id] + 0.5f;
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				half3 tnormal = UnpackNormal(tex2D(_BumpMap, i.uv)); // ***  *** Move this to 8x8 tiles to match Albedo map
                // transform normal from tangent to world space
                half3 worldNormal;
                worldNormal.x = dot(i.tspace0, tnormal);
                worldNormal.y = dot(i.tspace1, tnormal);
                worldNormal.z = dot(i.tspace2, tnormal);

				float4 texColor = tex2D(_MainTex, i.uv);  // Read Brush Texture
				float4 finalColor = float4(i.color) * texColor;

				float dotNml = dot(normalize(float3(-0.2,1,-0.75)), worldNormal);
				finalColor.xyz += dotNml * texColor.a * texColor.a * 0.36 + 0.000;

				return finalColor;
				
			}
		ENDCG
		}
	}
}

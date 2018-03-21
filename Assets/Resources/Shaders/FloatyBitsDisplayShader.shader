Shader "Unlit/FloatyBitsDisplayShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
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
			float4 _MainTex_ST;
			//float4 _Tint;
			//float4 _Size;

			//struct GlowyBitData {
			//	float3 worldPos;
			//	float3 color;
			//};

			StructuredBuffer<float4> floatyBitsCBuffer;
			StructuredBuffer<float3> quadVerticesCBuffer;

			

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				float4 color : TEXCOORD1;
			};

			float rand(float2 co){   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
				
				//o.color = floatingGlowyBitsCBuffer[inst].color;
				float4 floatyBitData = floatyBitsCBuffer[inst];
				float3 worldPosition = float3(floatyBitData.xy * 140 - 70, -0.05);
				float3 quadPoint = quadVerticesCBuffer[id];

				float2 velocity = floatyBitData.zw;
				float velMag = length(velocity);

				float random1 = rand(float2(inst, inst));
				float random2 = rand(float2(random1, random1));

				// Scaling!!!  _Size.zw is min/max aspect ratio, _Size.xy is min/max overall size				
				//float randomAspect = lerp(_Size.z, _Size.w, random1);
				float randomAspect = lerp(0.75, 1.33, random1);
				//float randomScale = lerp(_Size.x, _Size.y, random2);
				float randomValue = rand(float2(inst, randomAspect * 10));
				float randomScale = lerp(0.14, 0.22, random2);
				float2 scale = float2(randomAspect * randomScale, (1.0 / randomAspect) * randomScale * (length(velocity) * 55 + 1));
				//float2 scale = float2(1, 1) * randomScale;
				quadPoint *= float3(scale, 1.0);

				// ROTATION:
				//float rotationAngle = random1 * 10.0 * 3.141592;  // radians
				//float3 rotatedPoint = float3(quadPoint.x * cos(rotationAngle) - quadPoint.y * sin(rotationAngle),
				//							 quadPoint.x * sin(rotationAngle) + quadPoint.y * cos(rotationAngle),
				//							 quadPoint.z);
				float2 forward = normalize(velocity);
				float2 right = float2(forward.y, -forward.x); // perpendicular to forward vector
				float3 rotatedPoint = float3(quadPoint.x * right + quadPoint.y * forward,
											 quadPoint.z);

				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(rotatedPoint, 0.0f));
				o.color = float4(random1,random1,random1,randomValue * 2 * velMag + 0.15); // float4(randomValue, randomValue, randomValue, 1 / (length(velocity) * 50 + 1.15));
				o.uv = quadVerticesCBuffer[id] + 0.5f;
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				
				float4 texColor = tex2D(_MainTex, i.uv);  // Read Brush Texture
				float4 finalColor = float4(i.color) * texColor; //texColor * _Tint * float4(i.color, 1);
				//finalColor.a *= 0.5;
				return finalColor;
				
			}
		ENDCG
		}
	}
}

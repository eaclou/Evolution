Shader "Unlit/TestDisplayFloatingGlowyBitsA"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_Tint("Color", Color) = (1,1,1,1)
		_Size("Size", vector) = (1,1,1,1)
	}
	SubShader
	{
		//Tags { "RenderType"="Opaque" }
		//LOD 100
		Tags{ "RenderType" = "Transparant" }
		//ZTest Off
		ZWrite Off
		Cull Off
		Blend SrcAlpha One
		//Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Tint;
			float4 _Size;

			struct GlowyBitData {
				float3 worldPos;
				float3 color;
			};

			StructuredBuffer<GlowyBitData> floatingGlowyBitsCBuffer;
			StructuredBuffer<float3> quadVerticesCBuffer;

			

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				float3 color : TEXCOORD1;
			};

			float rand(float2 co){   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;


				o.color = floatingGlowyBitsCBuffer[inst].color;
				//o.color = lerp(inNeuronsBaseColor, outNeuronsBaseColor, floatingGlowyBitsCBuffer[inst].type) * floatingGlowyBitsCBuffer[inst].brightness;

				//Only transform world pos by view matrix
				//To Create a billboarding effect
				float3 worldPosition = floatingGlowyBitsCBuffer[inst].worldPos;
				float3 quadPoint = quadVerticesCBuffer[id];

				float random1 = rand(float2(inst, inst));
				float random2 = rand(float2(random1, random1));

				// Scaling!!!  _Size.zw is min/max aspect ratio, _Size.xy is min/max overall size				
				float randomAspect = lerp(_Size.z, _Size.w, random1);
				float randomScale = lerp(_Size.x, _Size.y, random2);
				float2 scale = float2(randomAspect * randomScale, (1.0 / randomAspect) * randomScale);
				
				quadPoint *= float3(scale, 1.0);

				// ROTATION:
				float rotationAngle = random1 * 10.0 * 3.141592;  // radians
				float3 rotatedPoint = float3(quadPoint.x * cos(rotationAngle) - quadPoint.y * sin(rotationAngle),
											 quadPoint.x * sin(rotationAngle) + quadPoint.y * cos(rotationAngle),
											 quadPoint.z);

				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(rotatedPoint, 0.0f));
				
				o.uv = quadVerticesCBuffer[id] + 0.5f;
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				
				float4 texColor = tex2D(_MainTex, i.uv);  // Read Brush Texture
				float4 finalColor = texColor * _Tint * float4(i.color, 1);
				return finalColor;
				
			}
		ENDCG
		}
	}
}

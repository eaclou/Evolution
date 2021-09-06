Shader "Unlit/TestDisplayExtraBallsA"
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
		//Tags{ "RenderType" = "Transparant" }
		//ZTest Off
		//ZWrite Off
		//Cull Off
		//Blend SrcAlpha One
		//Blend SrcAlpha OneMinusSrcAlpha

		Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" "IgnoreProjector"="True" }
		AlphaToMask On

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#include "UnityCG.cginc"
			// make fog work
			#pragma multi_compile_fog

			struct BallData {
				float3 worldPos;
				float value;
				float inOut;  // 0 = inColor, 1 = outColor;
				float scale;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Tint;
			float4 _Size;

			StructuredBuffer<BallData> extraBallsCBuffer;
			StructuredBuffer<float3> quadVerticesCBuffer;

			struct v2f
			{
				float4 renderPos : SV_POSITION;
				float4 worldPivotPos : TEXCOORD1;
				float4 worldLocalVertexPos : TEXCOORD2;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				float3 colorData : TEXCOORD3;
				UNITY_FOG_COORDS(4)
			};

			float rand(float2 co){   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;

				//Only transform world pos by view matrix
				//To Create a billboarding effect
				BallData data;
				data = extraBallsCBuffer[inst];
				float3 worldPosition = data.worldPos;
				float3 quadPoint = quadVerticesCBuffer[id];				

				float random1 = rand(float2(inst, inst));
				float random2 = rand(float2(random1, random1));

				// Scaling!!!  _Size.zw is min/max aspect ratio, _Size.xy is min/max overall size				
				float randomAspect = lerp(_Size.z, _Size.w, random1);
				float randomScale = lerp(_Size.x, _Size.y, random2);
				float2 scale = data.scale * float2(randomAspect * randomScale, (1.0 / randomAspect) * randomScale);
				
				quadPoint *= float3(scale, 1.0);

				o.worldPivotPos = float4(worldPosition, 1);

				// ROTATION:
				float rotationAngle = random1 * 0.0 * 3.141592;  // radians
				float3 rotatedPoint = float3(quadPoint.x * cos(rotationAngle) - quadPoint.y * sin(rotationAngle),
											 quadPoint.x * sin(rotationAngle) + quadPoint.y * cos(rotationAngle),
											 quadPoint.z);

				o.worldLocalVertexPos = float4(rotatedPoint, 1.0);  // .w should be 0.0????

				o.renderPos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(rotatedPoint, 0.0f));
				
				o.uv = quadVerticesCBuffer[id] + 0.5f;
				o.colorData = float3(data.value, data.inOut, 1.0);   // (value, in/out color, __)

				UNITY_TRANSFER_FOG(o,o.renderPos);
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				
				float4 texColor = tex2D(_MainTex, i.uv);  // Read Brush Texture

				float radius = length(i.uv * 2.0 - 1.0);
				float3 normalXYZ = normalize(float3(sin(i.uv.x * 2.0 - 1.0), sin(i.uv.y * 2.0 - 1.0), cos(radius * 0.5 * 3.141592))); 				
				float cutoff = 1.0 - floor(radius);  // assumes default quad UVs...
				float alpha = saturate(cutoff);

				//float4 testCol = float4(1,1,1,alpha) * _Tint;
				//UNITY_APPLY_FOG(i.fogCoord, testCol);
				//return testCol;
				//return float4(1,1,1,alpha);

				float normalZ = 1.0; // cos(radius * 0.5 * 3.141592);
				//normalXY *= sin(radius);

				float3 worldNormal = normalize(normalXYZ);

				//return float4(normal, alpha);
				//o.renderPos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(rotatedPoint, 0.0f));				
				float3 worldPosition = i.worldPivotPos;
				float3 worldViewDir = UnityWorldSpaceViewDir(worldPosition);
				float3 viewDir = normalize(mul(UNITY_MATRIX_VP, worldViewDir));

				float3 fakeLightDir = float3(0, 1.0, 0);
				float angleDot = saturate(dot(float3(0,0,1), worldNormal) + 0.25);
				float lightDot = saturate(dot(fakeLightDir, normalize(worldNormal)));

				float3 baseColorIn = float3(0.3, 0.4, 0.5);
				float3 baseColorOut = float3(0.5, 0.5, 0.3);
				float3 baseColor = lerp(baseColorIn, baseColorOut, i.colorData.y);
				
				float3 colValue = float3(angleDot, angleDot, angleDot) * _Tint.rgb * baseColor * i.colorData.x * 4 + 0.0;
								
				//clip(alpha - alphaCutoffValue);
				float4 finalCol = lerp(float4(1,1,1,alpha * _Tint.a), float4(colValue, alpha * _Tint.a), 0.5);
				return finalCol;
				//return float4(colValue * (worldNormal.x * 0.5 + 0.5), colValue * (worldNormal.y * 0.5 + 0.5), colValue * (worldNormal.z * 0.5 + 0.5), alpha);
				
				//float4 finalColor = float4(colValue, colValue, colValue, 1.0) * _Tint;
				//return finalColor;
				
			}
		ENDCG
		}
	}
}

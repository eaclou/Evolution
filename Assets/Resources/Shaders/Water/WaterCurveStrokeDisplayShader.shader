Shader "Water/WaterCurveStrokeDisplayShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_FluidColorTex ("_FluidColorTex", 2D) = "black" {}
		_AltitudeTex ("_AltitudeTex", 2D) = "gray" {}
		_VelocityTex ("_VelocityTex", 2D) = "black" {}
		_SkyTex("_SkyTex", 2D) = "white" {}
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
			#include "Assets/Resources/Shaders/Inc/NoiseShared.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 color : TEXCOORD0;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD1;
				float2 fluidTexUV : TEXCOORD2;
				float3 worldPos : TEXCOORD3;
				float4 screenUV : TEXCOORD4;
			};

			struct WaterCurveData {   // 2 ints, 17 floats
				int index;    
				float2 p0;
				float2 p1;
				float2 p2;
				float2 p3;
				float4 widths;
				float3 hue;
				float strength;   // extra data to use however
				float age;  // to allow for fade-in and fade-out
				int brushType;  // brush texture mask
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _FluidColorTex; 
			sampler2D _SkyTex;
			
			uniform float _MapSize;

			StructuredBuffer<WaterCurveData> waterCurveStrokesCBuffer;
			StructuredBuffer<float3> verticesCBuffer;

			float rand(float2 co){   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}
			// CUBIC:
			float2 GetPoint2D (float2 p0, float2 p1, float2 p2, float2 p3, float t) {
				t = saturate(t);
				float oneMinusT = 1.0 - t;
				return oneMinusT * oneMinusT * oneMinusT * p0 +	3.0 * oneMinusT * oneMinusT * t * p1 + 3.0 * oneMinusT * t * t * p2 +	t * t * t * p3;
			}
			float2 GetPoint1D (float p0, float p1, float p2, float p3, float t) {
				t = saturate(t);
				float oneMinusT = 1.0 - t;
				return oneMinusT * oneMinusT * oneMinusT * p0 +	3.0 * oneMinusT * oneMinusT * t * p1 + 3.0 * oneMinusT * t * t * p2 +	t * t * t * p3;
			}
			// CUBIC
			float2 GetFirstDerivative (float2 p0, float2 p1, float2 p2, float2 p3, float t) {
				t = saturate(t);
				float oneMinusT = 1.0 - t;
				return 3.0 * oneMinusT * oneMinusT * (p1 - p0) + 6.0 * oneMinusT * t * (p2 - p1) + 3.0 * t * t * (p3 - p2);
			}
			
			v2f vert (uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
				WaterCurveData waterCurveData = waterCurveStrokesCBuffer[inst];

				float3 worldPosition = float3(waterCurveData.p0, 0);
				
				float3 vertexPos = verticesCBuffer[id];
				float2 uv = verticesCBuffer[id];
				uv.x += 0.5;
								
				o.fluidTexUV = worldPosition.xy / _MapSize + float2(-0.25, 0.08) * 0.034 * _Time.y;

				float random1 = rand(float2(inst, inst));
				float random2 = rand(float2(random1, random1));
				float randomWidth = lerp(0.75, 1.3, random1) * 2.65;				

				// Figure out worldPosition by constructing a bezierCurve between the 4 points
				// and adjusting vertPosition based on curve output.
				// curve t = vertex UV.x
				// find tangent and bitangent of curve and use to project vertex UV.y
				float t = uv.y;
				uv.y = 1.0 - uv.y;

				float2 curvePos = GetPoint2D(waterCurveData.p0, waterCurveData.p1, waterCurveData.p2, waterCurveData.p3, t);
				float2 curveTangent = normalize(GetFirstDerivative(waterCurveData.p0, waterCurveData.p1, waterCurveData.p2, waterCurveData.p3, t));
				float2 curveBitangent = float2(curveTangent.y, -curveTangent.x);

				float width = GetPoint1D(waterCurveData.widths.x, waterCurveData.widths.y, waterCurveData.widths.z, waterCurveData.widths.w, t);
				float2 offset = curveBitangent * -verticesCBuffer[id].x * width * randomWidth; // *** support full vec4 widths!!!

				float fadeDuration = 0.1;
				float fadeIn = saturate(waterCurveData.age / fadeDuration);  // fade time = 0.1
				float fadeOut = saturate((1 - waterCurveData.age) / fadeDuration);
				float alpha = fadeIn * fadeOut;

				float4 waveNoise = Value3D(float3(waterCurveData.p0, _Time.y), 1);

				o.normal = waveNoise;
				o.pos = UnityObjectToClipPos(float4(curvePos, 0, 1.0) + float4(offset, 0.0, 0.0));

				//float4 pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0)); // *** Revisit to better understand!!!! ***
				float4 screenUV = ComputeScreenPos(o.pos);
				o.screenUV = screenUV; //centerUV.xy / centerUV.w;

				o.worldPos = float3(curvePos,0) + float4(offset, 0.0, 0.0);
				o.color = float4(waterCurveData.hue, alpha);				
				o.uv = uv;

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float2 screenUV = i.screenUV.xy / i.screenUV.w;

				float distFromScreenCenter = length(screenUV * 2 - 1);
				float vignetteMask = smoothstep(0.35, 1.6, distFromScreenCenter) * 1;

				// sample the texture
				//float4 fluidDensityColor = tex2D(_FluidColorTex, i.fluidTexUV);  //  Color of brushtroke source					
				fixed4 col = tex2D(_MainTex, i.uv) * i.color;
				// get view space position of vertex
                //float3 viewPos = UnityObjectToViewPos(i.worldPos);
				float3 cameraToVertex = i.worldPos - _WorldSpaceCameraPos;
                float3 cameraToVertexDir = normalize(cameraToVertex);

				//float3 waterWorldNormal = normalize(float3(i.normal.xy * 1,-1));

				//float3 reflectedViewDir = cameraToVertexDir + 2 * waterWorldNormal;
				//float4 finalCol = lerp(col, fluidDensityColor, 0);				
				//finalCol.a = col.a;
				//finalCol.rgb *= 1.125;
				float4 finalColor = float4(tex2D(_SkyTex, i.fluidTexUV + i.normal.xy * 0.01).rgb, col.a); //col;
				finalColor.a *= vignetteMask;

				return finalColor;

				//return finalCol;
			}
			ENDCG
		}
	}
}

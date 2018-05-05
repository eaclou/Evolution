Shader "Unlit/WaterSplinesShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_FluidColorTex ("_FluidColorTex", 2D) = "black" {}
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

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 color : TEXCOORD0;
				float2 uv : TEXCOORD1;
				float2 fluidTexUV : TEXCOORD2;
			};

			struct WaterSplineData {   // 2 ints, 17 floats
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

			StructuredBuffer<WaterSplineData> waterSplinesReadCBuffer;
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
				WaterSplineData waterSplineData = waterSplinesReadCBuffer[inst];

				float3 worldPosition = float3(waterSplineData.p0, -0.1);
				float3 vertexPos = verticesCBuffer[id];
				float2 uv = verticesCBuffer[id];
				uv.x += 0.5;
								
				o.fluidTexUV = (worldPosition.xy + 70) / 140;

				float random1 = rand(float2(inst, inst));
				float random2 = rand(float2(random1, random1));
				float randomWidth = lerp(0.75, 1.3, random1) * 2.65;				

				// Figure out worldPosition by constructing a bezierCurve between the 4 points
				// and adjusting vertPosition based on curve output.
				// curve t = vertex UV.x
				// find tangent and bitangent of curve and use to project vertex UV.y
				float t = uv.y;
				uv.y = 1.0 - uv.y;

				float2 curvePos = GetPoint2D(waterSplineData.p0, waterSplineData.p1, waterSplineData.p2, waterSplineData.p3, t);
				float2 curveTangent = normalize(GetFirstDerivative(waterSplineData.p0, waterSplineData.p1, waterSplineData.p2, waterSplineData.p3, t));
				float2 curveBitangent = float2(curveTangent.y, -curveTangent.x);

				float width = GetPoint1D(waterSplineData.widths.x, waterSplineData.widths.y, waterSplineData.widths.z, waterSplineData.widths.w, t);
				float2 offset = curveBitangent * -verticesCBuffer[id].x * width * randomWidth; // *** support full vec4 widths!!!

				float fadeDuration = 0.1;
				float fadeIn = saturate(waterSplineData.age / fadeDuration);  // fade time = 0.1
				float fadeOut = saturate((1 - waterSplineData.age) / fadeDuration);
				float alpha = fadeIn * fadeOut;

				o.pos = UnityObjectToClipPos(float4(curvePos, 0, 1.0) + float4(offset, 0.0, 0.0));
				o.color = float4(waterSplineData.hue, alpha);				
				o.uv = uv;

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				float4 fluidDensityColor = tex2D(_FluidColorTex, i.fluidTexUV);  //  Color of brushtroke source					
				fixed4 col = tex2D(_MainTex, i.uv) * i.color;

				float4 finalCol = lerp(col, fluidDensityColor, 0);				
				finalCol.a = col.a;
				finalCol.rgb *= 1.125;
				return finalCol;
			}
			ENDCG
		}
	}
}

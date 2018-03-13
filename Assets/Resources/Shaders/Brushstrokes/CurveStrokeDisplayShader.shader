Shader "Unlit/CurveStrokeDisplayShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		//_SourceTex ("Source Texture", 2D) = "black" {}
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
			
			#include "UnityCG.cginc"

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 color : TEXCOORD0;
				float2 uv : TEXCOORD1;
				float2 centerUV : TEXCOORD2;
			};

			struct CurveStrokeData {
				int parentIndex;
				float3 hue;
				float2 p0;
				float2 p1;
				float2 p2;
				float2 p3;
			};
			
			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			StructuredBuffer<CurveStrokeData> agentCurveStrokesReadCBuffer;
			StructuredBuffer<float3> curveRibbonVerticesCBuffer;

			// CUBIC:
			float2 GetPoint (float2 p0, float2 p1, float2 p2, float2 p3, float t) {
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
				CurveStrokeData curveStrokeData = agentCurveStrokesReadCBuffer[inst];
				//AgentSimData agentSimData = agentSimDataCBuffer[curveStrokeData.parentIndex];

				float3 worldPosition = float3(curveStrokeData.p0, -0.6);
				float3 vertexPos = curveRibbonVerticesCBuffer[id];
				float2 uv = curveRibbonVerticesCBuffer[id];
				uv.x += 0.5;

				// Figure out worldPosition by constructing a bezierCurve between the 4 points
				// and adjusting vertPosition based on curve output.
				// curve t = vertex UV.x
				// find tangent and bitangent of curve and use to project vertex UV.y
				float t = uv.y;
				uv.y = 1.0 - uv.y;

				float2 curvePos = GetPoint(curveStrokeData.p0, curveStrokeData.p1, curveStrokeData.p2, curveStrokeData.p3, t);
				float2 curveTangent = normalize(GetFirstDerivative(curveStrokeData.p0, curveStrokeData.p1, curveStrokeData.p2, curveStrokeData.p3, t));
				float2 curveBitangent = float2(curveTangent.y, -curveTangent.x);

				float2 offset = curveBitangent * -curveRibbonVerticesCBuffer[id].x;

				// &&&& Screen-space UV of center of brushstroke:
				// Magic to get proper UV's for sampling from GBuffers:
				float4 pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0)); // *** Revisit to better understand!!!! ***
				float4 centerUV = ComputeScreenPos(pos);
				o.centerUV = centerUV.xy / centerUV.w;

				//o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(vertexPos, 0.0f));
				o.pos = UnityObjectToClipPos(float4(curvePos, 0, 1.0) + float4(offset, 0.0, 0.0));
				o.color = float4(curveStrokeData.hue,1);				
				o.uv = uv;
				

				return o;


				//o.vertex = UnityObjectToClipPos(v.vertex);
				//o.uv = TRANSFORM_TEX(v.uv, _MainTex);				
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv) * i.color;
				
				return col;
			}
			ENDCG
		}
	}
}

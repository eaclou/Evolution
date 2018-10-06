Shader "UI/TreeOfLifeBackdropShader"
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
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct BasicStrokeData {  // fluidSim Render -- Obstacles + ColorInjection
				float2 worldPos;
				float2 localDir;
				float2 scale;
				float4 color;
			};

			StructuredBuffer<float3> quadVerticesCBuffer;
			StructuredBuffer<BasicStrokeData> treeOfLifeBasicStrokeDataCBuffer;
			
			uniform float4 _TopLeftCornerWorldPos;
			uniform float4 _CamRightDir;
			uniform float4 _CamUpDir;

			uniform float _CamScale;

			uniform float _AnimatedScale1;  // signal from main CPU program of whether the panel is active, hidden, or interpolating
			uniform float _AnimatedScale2; // signal from main CPU program of whether the panel is active, hidden, or interpolating

			sampler2D _MainTex;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
				//float4 colorSec : TEXCOORD1;
			};
			
			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;

				BasicStrokeData data = treeOfLifeBasicStrokeDataCBuffer[inst];

				//_AnimatedScale1 = 0.5;

				float3 localPos = float3(data.worldPos, 0);
				float3 pivot = _TopLeftCornerWorldPos.xyz - _CamUpDir.xyz * data.scale.y * _CamScale; // + (_CamRightDir.xyz * 0.5 - _CamUpDir.xyz * 0.5) * _CamScale;
				float2 uvCoords = quadVerticesCBuffer[id].xy + 0.5;
				//float3 vertexWorldOffset = float3(quadVerticesCBuffer[id].xy * data.scale, 0);

				//float2 forward = data.localDir;
				//float2 right = float2(forward.y, -forward.x); // perpendicular to forward vector
				//vertexWorldOffset = float3(vertexWorldOffset.x * right + vertexWorldOffset.y * forward, 0);  // Rotate localRotation by AgentRotation

				//float3 orthoPos = (vertexWorldOffset + localPos) * _CamScale * _AnimatedScale1;
				float3 worldPosition = pivot + (_CamRightDir.xyz * uvCoords.x * data.scale.x + _CamUpDir.xyz * uvCoords.y * data.scale.y) * _CamScale; //_CamRightDir.xyz * quadVerticesCBuffer[id].x * 1.0 * _CamScale + _CamUpDir.xyz * quadVerticesCBuffer[id].y * 1.0 * _CamScale;
				
				o.uv = quadVerticesCBuffer[id].xy + 0.5;
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0))); //float4(quadVerticesCBuffer[id], 1);
				o.color = data.color;

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				//return(1, 1, 0.25, 1);
				/*float uvDist = length((i.uv - 0.5) * 2);
				float radiusOneMask = saturate((1.0 - saturate(uvDist)) * 256);
				float radiusHalfMask = saturate((1.0 - saturate(uvDist * 2)) * 256);
				
				float4 finalColor = float4(i.colorPri.rgb, radiusOneMask);
				finalColor.rgb = lerp(finalColor.rgb, i.colorSec.rgb, radiusHalfMask);
				
				return finalColor;
				*/
				// sample the texture
				//float brightness = ((i.color.r + i.color.g + i.color.b) / 3.0) * 0.1 + 0.01;
				fixed4 col = tex2D(_MainTex, i.uv);
				//col.rgb *= brightness;
				//col.a *= 0.25;
				return col;
			}
			ENDCG
		}
	}
}

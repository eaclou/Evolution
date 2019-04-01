Shader "Gizmos/GizmoStirToolShader"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_AltitudeTex ("_AltitudeTex", 2D) = "gray" {}
		_WaterSurfaceTex ("_WaterSurfaceTex", 2D) = "black" {}
		
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
			#include "Assets/Resources/Shaders/Inc/StructsCritterData.cginc"

			sampler2D _MainTex;
			sampler2D _AltitudeTex;
			sampler2D _WaterSurfaceTex;			
			
			StructuredBuffer<float4> gizmoStirToolPosCBuffer;

			//StructuredBuffer<CritterInitData> critterInitDataCBuffer;
			//StructuredBuffer<CritterSimData> critterSimDataCBuffer;
			//StructuredBuffer<float4> agentHoverHighlightData;
			StructuredBuffer<float3> quadVerticesCBuffer;
			
			uniform float _IsVisible;
			uniform float _IsStirring;
			uniform float _Radius;
			uniform float _CamDistNormalized;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture
				float4 screenUV : TEXCOORD1;
				float2 altitudeUV : TEXCOORD2;
				float3 worldPos : TEXCOORD3;
				float4 color : COLOR;
			};

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
								
				//float4 data = agentHoverHighlightData[_HoverAgentIndex];
				//uint agentIndex = inst.x; //data.parentIndex;
				//CritterInitData critterInitData = critterInitDataCBuffer[agentIndex];
				//CritterSimData critterSimData = critterSimDataCBuffer[agentIndex];

				float4 toolData = gizmoStirToolPosCBuffer[0];

				float3 worldPosition = float3(toolData.xy, 0);
				float3 quadPoint = quadVerticesCBuffer[id];

				float radiusMult = _Radius;
				radiusMult *= (1.0 + _IsStirring * 0.25 + _CamDistNormalized * 1.25);
				quadPoint *= radiusMult;
				
				
				float2 altUV = (worldPosition.xy + 128) / 512;
				o.altitudeUV = altUV;

				float4 centerPos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0)); // *** Revisit to better understand!!!! ***
				float4 screenUV = ComputeScreenPos(centerPos);
				o.screenUV = screenUV;

				worldPosition.xy += quadPoint.xy;
				
				//temp:
				worldPosition = float3(quadVerticesCBuffer[id].xy * 10 + toolData.xy, 0);
				float4 pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0)); // *** Revisit to better understand!!!! ***
				
				o.worldPos = worldPosition;
				o.pos = pos;
				o.uv = quadVerticesCBuffer[id] + 0.5; // full texture

				float alpha = 0.75;
				alpha *= _IsVisible;
				alpha *= (1.0 + _IsStirring);
				
				float4 col = float4(1,1,1,alpha);
				
				o.color = col;
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//return float4(1,1,1,1);

				float4 idleColor = float4(0.25, 0.9, 1.4, 1);
				float4 activeColor = float4(1.75, 1.4, 0.75, 1);

				float4 finalColor = lerp(idleColor, activeColor, _IsStirring);
				
				float4 texColor = tex2D(_MainTex, i.uv);
				texColor.a = texColor.a * texColor.a;
				finalColor = finalColor * i.color * texColor;
				finalColor.a *= 0.5;
				//return texColor; // temp debug
				return finalColor;

			}
		ENDCG
		}
	}
}

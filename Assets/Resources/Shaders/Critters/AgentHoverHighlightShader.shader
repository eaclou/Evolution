Shader "Critter/AgentHoverHighlightShader"
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
			
			StructuredBuffer<CritterInitData> critterInitDataCBuffer;
			StructuredBuffer<CritterSimData> critterSimDataCBuffer;

			StructuredBuffer<float4> agentHoverHighlightData;
			StructuredBuffer<float3> quadVerticesCBuffer;
			
			uniform int _HoverAgentIndex;
			uniform float _IsHover;

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
								
				float4 data = agentHoverHighlightData[_HoverAgentIndex];
				uint agentIndex = _HoverAgentIndex; //data.parentIndex;
				CritterInitData critterInitData = critterInitDataCBuffer[agentIndex];
				CritterSimData critterSimData = critterSimDataCBuffer[agentIndex];

				float3 worldPosition = critterSimData.worldPos;
				float3 quadPoint = quadVerticesCBuffer[id];

				float radiusMult = 4;
				quadPoint *= radiusMult;
				
				float3 critterWorldPos = critterSimData.worldPos;
				//critterWorldPos
				float3 critterCurScale = ((critterInitData.boundingBoxSize.x + critterInitData.boundingBoxSize.y) * 0.5) * lerp(critterInitData.spawnSizePercentage, 1, critterSimData.growthPercentage);
				quadPoint *= critterCurScale;

				float2 altUV = (worldPosition.xy + 128) / 512;
				o.altitudeUV = altUV;

				float2 forward = critterSimData.heading;
				float2 right = float2(forward.y, -forward.x);
				quadPoint.xy = quadPoint.x * right + quadPoint.y * forward;				

				float4 centerPos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0)); // *** Revisit to better understand!!!! ***
				float4 screenUV = ComputeScreenPos(centerPos);
				o.screenUV = screenUV;

				worldPosition.xy += quadPoint.xy;
				
				float4 pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0)); // *** Revisit to better understand!!!! ***
				
				o.worldPos = pos;
				o.pos = pos;
				o.uv = quadVerticesCBuffer[id] + 0.5; // full texture

				o.color = float4(_IsHover, 1, 1, 1);
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				
				float4 finalColor = float4(1, 1, 1, 0.1);
				
				float4 texColor = tex2D(_MainTex, i.uv);

				finalColor.a *= i.color.x * texColor.a;  // hide if mouse not hovering over
				finalColor.rgb *= 1;

				
				return finalColor;

			}
		ENDCG
		}
	}
}

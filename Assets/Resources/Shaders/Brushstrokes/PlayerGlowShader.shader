Shader "Unlit/PlayerGlowShader"
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

			struct BasicStrokeData {				
				float2 worldPos;
				float2 localDir;
				float2 scale;
				float4 color;
			};
			
			struct AgentSimData {
				float2 worldPos;
				float2 velocity;
				float2 heading;
				float2 size;
				float3 primaryHue;  // can eventually pull these static variables out of here to avoid per-frame updates on non-dynamic attributes
				float3 secondaryHue;
				float maturity;
				float decay;
				float eatingStatus;
				float foodAmount;
			};

			StructuredBuffer<AgentSimData> agentSimDataCBuffer;
			StructuredBuffer<BasicStrokeData> basicStrokesCBuffer;
			StructuredBuffer<float3> quadVerticesCBuffer;
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;  // uv of the brushstroke quad itself, particle texture	
				float4 color : TEXCOORD1;
			};

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
								
				BasicStrokeData strokeData = basicStrokesCBuffer[inst];

				float3 worldPosition = float3(agentSimDataCBuffer[0].worldPos, 0.0);
				float3 quadPoint = quadVerticesCBuffer[id];
				
				float2 scale = lerp(strokeData.scale * 2.4, max(strokeData.scale.x, strokeData.scale.y) * 2.4, 0.5)	; //max(strokeData.scale.x, strokeData.scale.y) * 1.45;				
				quadPoint *= float3(scale, 1.0);

				// Figure out final facing Vectors!!!
				float2 forward0 = agentSimDataCBuffer[0].heading;
				float2 right0 = float2(forward0.y, -forward0.x); // perpendicular to forward vector
				float3 rotatedPoint0 = float3(quadPoint.x * right0 + quadPoint.y * forward0,
											 quadPoint.z);
				
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(rotatedPoint0, 0.0f));
				
				o.color = strokeData.color;
				o.color.a = o.color.a * agentSimDataCBuffer[0].maturity * (1.0 - agentSimDataCBuffer[0].decay);
				//o.color.rgb = 0;

				float2 uv = quadVerticesCBuffer[id] + 0.5f;

				o.uv = uv;
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				
				float4 brushColor = tex2D(_MainTex, i.uv);

				float4 finalColor = float4(i.color) * brushColor;
				//finalColor.a = i.color.a;
				
				return finalColor;
				
			}
		ENDCG
		}
	}
}

Shader "Critter/CritterGenericStrokeDisplayShader"
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
			#pragma target 5.0
			#include "UnityCG.cginc"
			#include "Assets/Resources/Shaders/Inc/CritterBodyAnimation.cginc"
			

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			StructuredBuffer<float3> quadVerticesCBuffer;			
			StructuredBuffer<CritterInitData> critterInitDataCBuffer;
			StructuredBuffer<CritterSimData> critterSimDataCBuffer;
			StructuredBuffer<CritterGenericStrokeData> critterGenericStrokesCBuffer;
			
			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
				
				CritterGenericStrokeData genericStrokeData = critterGenericStrokesCBuffer[inst];
				uint agentIndex = genericStrokeData.parentIndex;
				CritterInitData critterInitData = critterInitDataCBuffer[agentIndex];
				CritterSimData critterSimData = critterSimDataCBuffer[agentIndex];

				float3 critterWorldPos = critterSimData.worldPos;

				// WEIRD COORDINATES!!! Positive Z = DEEPER!!!
				float3 strokeBindPos = float3(genericStrokeData.bindPos.x, genericStrokeData.bindPos.z, -genericStrokeData.bindPos.y) * 0.8;

				//Temp align with creatures:
				float2 critterForwardDir = critterSimData.heading;
				float2 critterRightDir = float2(critterForwardDir.y, -critterForwardDir.x);

				strokeBindPos.xy = critterRightDir * strokeBindPos.x + critterForwardDir * strokeBindPos.y;

				float3 vertexWorldPos = critterWorldPos + strokeBindPos + quadVerticesCBuffer[id] * 0.125;

				o.vertex = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(vertexWorldPos, 1.0)));
				o.uv = quadVerticesCBuffer[id].xy;
				o.color = float4(genericStrokeData.bindPos.x * 0.5 + 0.5, genericStrokeData.bindPos.z * 0.33 + 0.5, genericStrokeData.bindPos.y * 0.5 + 0.5, 1);
				//o.color.rgb *= 0.4;
				return o;
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

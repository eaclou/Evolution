Shader "UI/WorldTree/ClockOrbitLineDataShader"
{
	Properties
	{
		_DataTex ("_DataTex", 2D) = "black" {}
		_KeyTex ("_KeyTex", 2D) = "black" {}
		_BrushTex ("_BrushTex", 2D) = "white" {}
	}
	SubShader
	{
		//Tags { "RenderType"="Opaque" }
		//LOD 100
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
			//#include "Assets/Resources/Shaders/Inc/StructsTreeOfLife.cginc"
			//#include "Assets/Resources/Shaders/Inc/NoiseShared.cginc"

			struct ClockOrbitLineData {
				float3 worldPos;
				float radius;
				float4 color;
				float animPhase;
				float rotateZ;
			};
			StructuredBuffer<float3> quadVerticesCBuffer;
			// Change to 2x texture2D's ((1)time series data + (2)key) for data input?
			StructuredBuffer<ClockOrbitLineData> clockOrbitLineDataCBuffer;
						
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			sampler2D _DataTex; // needed?
			sampler2D _KeyTex;
			sampler2D _BrushTex;

			uniform int _SelectedWorldStatsID;

			uniform float _IsOn;

			uniform float _GraphCoordStatsStart;

			uniform float _MouseCoordX;
			uniform float _MouseCoordY;
			uniform float _MouseOn;

			uniform float _CurFrame;
			uniform float _NumRows;
			uniform float _NumColumns;


			v2f vert (uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
				ClockOrbitLineData data = clockOrbitLineDataCBuffer[inst];
				float3 quadData = quadVerticesCBuffer[id];
				
				float angle = data.rotateZ * 0.001;// + 3.14159 * 0.5;
				float2 forward = float2(cos(angle),sin(angle));
				float2 right = float2(forward.y, -forward.x); // perpendicular to forward vector
				float2 rotatedPoint = float2(quadData.x * right + quadData.y * forward);
				//o.uv = quadData.xy + 0.5;

				float rows = _NumRows;
				float cols = _NumColumns;
				float frame = floor(data.animPhase * 16);// floor(inst) % (rows * cols);
				
				float2 newUV = quadData.xy + 0.5;
				newUV.x = newUV.x / cols;
				newUV.y = newUV.y / rows;				
				float column = (float)(frame % cols);
				float row = (float)floor((frame) / cols);
				newUV.x += column * (1.0 / cols);
				newUV.y += row * (1.0 / rows);
				
				float ballRadius = data.radius;
				float3 worldPosition = data.worldPos + float3(rotatedPoint.x, rotatedPoint.y, 0) * ballRadius;

				o.uv = newUV;
				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)));
				o.color = data.color; // tex2Dlod(_KeyTex, float4(0,((float)_SelectedWorldStatsID + 0.5) / 32.0,0,0));

				//float distToMouse = 1.0 - saturate(abs(vertexCoord.x - _MouseCoordX) * 15);
				//o.color.xyz = lerp(o.color.xyz, float3(1, 1, 1), _MouseOn * distToMouse * 1);

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float4 col = tex2D(_BrushTex, i.uv);
				
				return col;// * i.color;

				//old:
				//float fade = (1.0 - i.uv.x);
				//float4 finalColor = float4(i.color.rgb, fade * fade * 1);					
				//return finalColor;
			}
			ENDCG
		}
	}
}

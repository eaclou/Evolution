Shader "Unlit/TestDisplayShaderDrawProceduralIndirectA"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom
			#pragma target 5.0   // SUPER FUCKING IMPORTANT!!!!!!
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			float4 _MainTex_ST;

			struct Triangle {
				float3 vertA;
				float3 vertB;
				float3 vertC;
			};

			#if SHADER_TARGET >= 45
            StructuredBuffer<Triangle> appendTrianglesBuffer;
			#endif

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				uint id : TEXCOORD1;
				//UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};
			
			v2f vert (appdata v, uint vID : SV_VertexID )
			{
				

				
				float3 worldPosition = float3(0, 0, 0);

				v2f o;
				o.vertex = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));
				o.id = vID;
				//o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				//UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			[maxvertexcount(3)]
			void geom(point v2f p[1], inout TriangleStream<v2f> triStream) {
				
				v2f pIn;
				pIn.id = p[0].id;

				#if SHADER_TARGET >= 45
					Triangle triData = appendTrianglesBuffer[p[0].id];				
				#endif

				pIn.vertex = mul(UNITY_MATRIX_VP, float4(triData.vertA, 1));
				pIn.uv = float2(0, 0);
				//UNITY_TRANSFER_FOG(pIn, pIn.pos);
				triStream.Append(pIn);

				pIn.vertex = mul(UNITY_MATRIX_VP, float4(triData.vertB, 1));
				pIn.uv = float2(0, 1);
				//UNITY_TRANSFER_FOG(pIn, pIn.pos);
				triStream.Append(pIn);

				pIn.vertex = mul(UNITY_MATRIX_VP, float4(triData.vertC, 1));
				pIn.uv = float2(1, 0);
				//UNITY_TRANSFER_FOG(pIn, pIn.pos);
				triStream.Append(pIn);

				triStream.RestartStrip();
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return float4(1,0.2,0.2,1);

				// sample the texture
				//fixed4 col = tex2D(_MainTex, i.uv);
				// apply fog
				//UNITY_APPLY_FOG(i.fogCoord, col);
				//return col;
			}
			ENDCG
		}
	}
}

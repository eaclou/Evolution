Shader "Water/WaterMeshRenderShader"
{
	Properties
	{
		_MainTex ("_MainTex", 2D) = "white" {}
		//_DensityTex ("_DensityTex", 2D) = "white" {}
		//_VelocityTex ("_VelocityTex", 2D) = "white" {}
		//_PressureTex ("_PressureTex", 2D) = "white" {}
		//_DivergenceTex ("_DivergenceTex", 2D) = "white" {}
		//_ObstaclesTex ("_ObstaclesTex", 2D) = "white" {}
		//_TerrainHeightTex ("_TerrainHeightTex", 2D) = "grey" {}
		//_DebugTex ("_DebugTex", 2D) = "black" {}
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		ZWrite Off
		Cull Off
		//Blend SrcAlpha One
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			//sampler2D _DensityTex;
			//sampler2D _VelocityTex;
			//sampler2D _PressureTex;
			//sampler2D _DivergenceTex;
			//sampler2D _ObstaclesTex;
			//sampler2D _TerrainHeightTex;
			//sampler2D _DebugTex;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 main = tex2D(_MainTex, i.uv);
				//fixed4 density = tex2D(_DensityTex, i.uv);
				//fixed4 velocity = tex2D(_VelocityTex, i.uv);
				//fixed4 pressure = tex2D(_PressureTex, i.uv);
				//fixed4 divergence = tex2D(_DivergenceTex, i.uv);
				//fixed4 obstacles = tex2D(_ObstaclesTex, i.uv);

				fixed4 finalColor = float4(0,0,1,1);
				

				return finalColor;
			}
			ENDCG
		}
	}
}

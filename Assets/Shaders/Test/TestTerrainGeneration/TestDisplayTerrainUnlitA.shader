Shader "Unlit/TestDisplayTerrainUnlitA"
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
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal: NORMAL;
				float2 uv : TEXCOORD0;
				float3 color : COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float4 worldPos : TEXCOORD2;
				float3 normal: NORMAL;
				float3 vertexColor: COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.worldPos = v.vertex;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.normal = v.color;
				o.vertexColor = v.color;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = float4(0,0,i.vertexColor.z, 1); //tex2D(_MainTex, i.uv) * float4(i.vertexColor, 1);

				//float altitudeColor = fmod(i.worldPos.y, 4);
				//col *= altitudeColor;
				// apply fog
				//UNITY_APPLY_FOG(i.fogCoord, col);
				//col = float4(i.vertexColor.z, i.vertexColor.z, i.vertexColor.z, 1);
				return col;
			}
			ENDCG
		}
	}
}

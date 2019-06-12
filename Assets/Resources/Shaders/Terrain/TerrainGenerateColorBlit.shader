Shader "Terrain/TerrainGenerateColorBlit"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_DeltaTex ("_DeltaTex", 2D) = "black" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			
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

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _DeltaTex;

			uniform float4 _Color0;
			uniform float4 _Color1;
			uniform float4 _Color2;
			uniform float4 _Color3;

			uniform float _AddSubtractSign = 1; 

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv); // altitude height data
				//return col;
				float4 deltaCol = tex2D(_DeltaTex, i.uv); // extra
				// just invert the colors
				//col.rgb = saturate(col.rgb + deltaCol.rgb * _AddSubtractSign);
				float4 finalColor = saturate(_Color0);
				finalColor = lerp(finalColor, _Color1, saturate(col.y));
				finalColor = lerp(finalColor, _Color2, saturate(col.z));
				finalColor = lerp(finalColor, _Color3, saturate(col.w));
				//finalColor.rgb = float3(0.37, 0.3, 0.3) * 0.75;
				//finalColor.rgb *= (col.x * 0.5 + 0.5);
				
				return finalColor;

				//return float4(col.rgb,1);
				//return col;
			}
			ENDCG
		}
	}
}

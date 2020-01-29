Shader "UI/KnowledgeMinimap"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}		
		_AltitudeTex ("Texture", 2D) = "gray" {}	
		_ResourceGridTex ("Texture", 2D) = "black" {}	
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		//ZWrite Off
		//Cull Off
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
			float4 _MainTex_ST;
			sampler2D _AltitudeTex;
			sampler2D _ResourceGridTex;
			
			uniform float4 _Zoom;
			uniform float _Amplitude;
			uniform float4 _ChannelMask;
			uniform int _ChannelSoloIndex;
			uniform float _IsChannelSolo;
			uniform float _Gamma;
			uniform float _WaterLevel;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				
				float4 bgColor;
				float4 terrainHeightTex = tex2D(_AltitudeTex, i.uv * _Zoom.xy);
				if(terrainHeightTex.x < _WaterLevel) {
					bgColor = float4(58.0 / 255, 67 / 255.0, 75 / 255.0, 1);
					//col.rgb = lerp(col.rgb, float3(0.3,0.3,1), 0.1);
				}
				else {
					bgColor = float4(62.0 / 255.0, 55.0 / 255, 48.0 / 255, 1);
					//col.rgb = lerp(col.rgb, float3(1,0.8,0.3), 0.1);
				}
				bgColor.rgb *= 0.6165;
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv * _Zoom.xy);
								
				float xChannelSolo = 1.0 - saturate(abs((float)_ChannelSoloIndex));
				float yChannelSolo = 1.0 - saturate(abs((float)_ChannelSoloIndex - 1.0));
				float zChannelSolo = 1.0 - saturate(abs((float)_ChannelSoloIndex - 2.0));
				float wChannelSolo = 1.0 - saturate(abs((float)_ChannelSoloIndex - 3.0));

				float soloChannelCol = 0;
				soloChannelCol = lerp(soloChannelCol, col.x, xChannelSolo);
				soloChannelCol = lerp(soloChannelCol, col.y, yChannelSolo);
				soloChannelCol = lerp(soloChannelCol, col.z, zChannelSolo);
				soloChannelCol = lerp(soloChannelCol, col.w, wChannelSolo);

				float4 soloChannelColor = bgColor + float4(soloChannelCol, soloChannelCol, soloChannelCol, 1);

				//soloChannelColor = lerp(bgColor, soloChannelColor, 0.3);

				float4 finalColor = lerp(col, soloChannelColor, _IsChannelSolo);

				finalColor.rgb *= _Amplitude;
				finalColor.rgb = pow(finalColor.rgb, _Gamma);

				
				finalColor.a *= terrainHeightTex.a;

				return finalColor;
				
				//return col;
			}
			ENDCG
		}
	}
}

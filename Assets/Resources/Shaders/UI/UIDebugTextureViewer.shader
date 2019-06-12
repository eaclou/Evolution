Shader "UI/UIDebugTextureViewer"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}		
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		//ZWrite Off
		//Cull Off
		//Blend SrcAlpha OneMinusSrcAlpha

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
			
			uniform float4 _Zoom;
			uniform float _Amplitude;
			uniform float4 _ChannelMask;
			uniform int _ChannelSoloIndex;
			uniform float _IsChannelSolo;
			uniform float _Gamma;

			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				

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

				float4 finalColor = lerp(col, float4(soloChannelCol, soloChannelCol, soloChannelCol, 1), _IsChannelSolo);

				finalColor.rgb *= _Amplitude;
				finalColor.rgb = pow(finalColor.rgb, _Gamma);

				return finalColor;
				
				//return col;
			}
			ENDCG
		}
	}
}

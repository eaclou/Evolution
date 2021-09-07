Shader "Effects/TestBlitImageEffectShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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
			float4 _MainTex_TexelSize;
			int _PixelsWidth;
			int _PixelsHeight;

			fixed4 frag (v2f i) : SV_Target
			{	
				//float w, h;
				//_MainTex.GetDimensions(w, h);

				float2 _Pixels = float2((float)_PixelsWidth, (float)_PixelsHeight);  // why the fuck did I need to mult by 2?? -- something weird with Bilinear Sampling?
				
				// Cell centre
				//float2 uv = round(i.uv * _Pixels) / _Pixels;
 
				 // Neighbour cells
				float s = 1 / _Pixels;
				float cl = tex2D(_MainTex, i.uv + float2(-s, 0)).x; // Centre Left
				float tc = tex2D(_MainTex, i.uv + float2(-0, -s)).x; // Top Centre
				float cc = tex2D(_MainTex, i.uv + float2(0, 0)).x; // Centre Centre
				float bc = tex2D(_MainTex, i.uv + float2(0, +s)).x; // Bottom Centre
				float cr = tex2D(_MainTex, i.uv + float2(+s, 0)).x; // Centre Right
 
				 // Diffusion step
				//float factor = 1 * (	0.25 * (cl + tc + bc + cr)	- cc);
				//cc += factor;

				cc = 0.2 * (cl + tc + bc + cr + cc);

				fixed4 col = tex2D(_MainTex, i.uv);
				// just invert the colors
				col = float4(cc,cc,cc,1); //1 - col;
				return col;
			}
			ENDCG
		}
	}
}

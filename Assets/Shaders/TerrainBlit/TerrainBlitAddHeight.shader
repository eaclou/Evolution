Shader "TerrainBlit/TerrainBlitAddHeight"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}  // Original Heights
		_MaskTex ("Texture", 2D) = "white" {}
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

			#pragma shader_feature USE_MASK
			
			#include "UnityCG.cginc"
			#include "Assets/Shaders/Inc/NoiseShared.cginc"
			
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
			sampler2D _MaskTex;
			float4 _MainTex_TexelSize;
			float4 _MaskTex_TexelSize;
			int _PixelsWidth;
			int _PixelsHeight;
			float _NoiseAmplitude;
			float _NoiseFrequency;
			float4 _NoiseOffset;
			float4 _GridBounds;
			float _FilterStrength;
			

			fixed4 frag (v2f i) : SV_Target
			{	
				
				float2 _Pixels = float2((float)_PixelsWidth, (float)_PixelsHeight);  // why the fuck did I need to mult by 2?? -- something weird with Bilinear Sampling?
				
				 // Neighbour cells
				float s = 1 / _Pixels;
				float strength = _FilterStrength;

				float4 baseHeight = tex2D(_MainTex, i.uv);

				float2 gridSize = float2(_GridBounds.y - _GridBounds.x, _GridBounds.w - _GridBounds.z);
				
				float2 vertexUV = (i.uv - float2(s * 0.5, s * 0.5)) * ((_Pixels.x + 1.0) / _Pixels.x);  //float2(,);
				float2 coords = float2(_GridBounds.x + vertexUV.x * gridSize.x, _GridBounds.z + vertexUV.y * gridSize.y);
				float3 noiseSample = Value2D(coords, _NoiseFrequency * 0.05);

				noiseSample = 1.0 - abs(noiseSample);

				float4 maskValue = float4(1,1,1,1);
#if defined(USE_MASK)
				float4 maskSample = tex2D(_MaskTex, coords * 0.003);
				maskValue = maskSample;


				baseHeight.x += noiseSample.x * 6 * maskValue.x;
				baseHeight.y = maskValue.x; //frac(vertexUV.x * 1);
				baseHeight.z = 1; //frac(vertexUV.y * 1);
#endif
				// just invert the colors
				//col = float4(0,0,0,1); //1 - col;
				return baseHeight;
			}
			ENDCG
		}
	}
}

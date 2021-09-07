Shader "Effects/TestErosionBlitShader"
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
			

			StructuredBuffer<float3> weight5MatrixCBuffer;
			StructuredBuffer<float3> weight9MatrixCBuffer;
			StructuredBuffer<float3> weight13MatrixCBuffer;

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			int _PixelsWidth;
			int _PixelsHeight;
			float _FilterStrength;

			float GetValue5(float s, float2 uv) {
				float4 cl = tex2D(_MainTex, uv + float2(-s * 2, 0)); // Centre Left
				float4 tc = tex2D(_MainTex, uv + float2(0, -s * 2)); // Top Centre
				float4 cc = tex2D(_MainTex, uv + float2(0, 0)); // Centre Centre
				float4 bc = tex2D(_MainTex, uv + float2(0, +s * 2)); // Bottom Centre
				float4 cr = tex2D(_MainTex, uv + float2(+s * 2, 0)); // Centre Right
								
				float newValue = dot(cl.xyz, weight5MatrixCBuffer[0]) +
								 dot(tc.xyz, weight5MatrixCBuffer[1]) +
								 dot(cc.xyz, weight5MatrixCBuffer[2]) +
								 dot(bc.xyz, weight5MatrixCBuffer[3]) +
								 dot(cr.xyz, weight5MatrixCBuffer[4]);

				return newValue;
			}

			float GetValue9(float s, float2 uv) {
				float4 cc = tex2D(_MainTex, uv + float2(0, 0)); // Centre Centre
				float4 cl = tex2D(_MainTex, uv + float2(-s, 0)); // Centre Left
				float4 tl = tex2D(_MainTex, uv + float2(-s * 2, -s * 2)); // Top Left
				float4 tc = tex2D(_MainTex, uv + float2(0, -s)); // Top Centre
				float4 tr = tex2D(_MainTex, uv + float2(+s * 2, -s * 2)); // Top Right
				float4 cr = tex2D(_MainTex, uv + float2(+s, 0)); // Centre Right
				float4 br = tex2D(_MainTex, uv + float2(+s * 2, +s * 2)); // Bottom Right
				float4 bc = tex2D(_MainTex, uv + float2(0, +s)); // Bottom Centre
				float4 bl = tex2D(_MainTex, uv + float2(-s * 2, +s * 2)); // Bottom Left
				
								
				float newValue = dot(cc.xyz, weight9MatrixCBuffer[0]) +
								 dot(cl.xyz, weight9MatrixCBuffer[1]) +
								 dot(tl.xyz, weight9MatrixCBuffer[2]) +
								 dot(tc.xyz, weight9MatrixCBuffer[3]) +
								 dot(tr.xyz, weight9MatrixCBuffer[4]) +
								 dot(cr.xyz, weight9MatrixCBuffer[5]) +
								 dot(br.xyz, weight9MatrixCBuffer[6]) +
								 dot(bc.xyz, weight9MatrixCBuffer[7]) +
								 dot(bl.xyz, weight9MatrixCBuffer[8]);

				return newValue;
			}

			float GetValue13(float s, float2 uv) {
				float4 cc = tex2D(_MainTex, uv + float2(0, 0)); // Centre Centre
				float4 cl = tex2D(_MainTex, uv + float2(-s * 1, 0)); //float2(-s * 4, 0)); // Centre Left
				float4 tl = tex2D(_MainTex, uv + float2(0, s * 1)); //float2(-s, -s * 2)); // Top Left
				float4 tc = tex2D(_MainTex, uv + float2(s * 1, 0)); //float2(0, -s * 4)); // Top Centre
				float4 tr = tex2D(_MainTex, uv + float2(0, -s * 1)); //float2(+s * 2, -s)); // Top Right

				float4 cr = tex2D(_MainTex, uv + float2(-s * 1, -s * 1)); //float2(+s * 4, 0)); // Centre Right
				float4 br = tex2D(_MainTex, uv + float2(-s * 1, s * 1)); //float2(+s * 2, +s)); // Bottom Right
				float4 bc = tex2D(_MainTex, uv + float2(-s * 1, s * 1)); //float2(0, +s * 4)); // Bottom Centre
				float4 bl = tex2D(_MainTex, uv + float2(s * 1, -s * 1)); //float2(-s, +s * 2)); // Bottom Left

				float4 ll = tex2D(_MainTex, uv + float2(-s * 2, -s * 1)); //float2(-s * 8, 0)); // Left Left
				float4 tt = tex2D(_MainTex, uv + float2(-s * 1, s * 2)); //float2(0, -s * 8)); // Top Top
				float4 rr = tex2D(_MainTex, uv + float2(-s * 2, s * 1)); //float2(+s * 8, 0)); // Right Right
				float4 bb = tex2D(_MainTex, uv + float2(s * 1, -s * 2)); //float2(0, +s * 8)); // Bottom Bottom

				float4 p0 = tex2D(_MainTex, uv + float2(-s * 1, -s * 2));
				float4 p1 = tex2D(_MainTex, uv + float2(-s * 2, s * 1)); 
				float4 p2 = tex2D(_MainTex, uv + float2(s * 1, -s * 2));
				float4 p3 = tex2D(_MainTex, uv + float2(s * 2, s * 1));

				float4 p4 = tex2D(_MainTex, uv + float2(-s * 2, -s * 2)); 
				float4 p5 = tex2D(_MainTex, uv + float2(-s * 2, s * 2)); 
				float4 p6 = tex2D(_MainTex, uv + float2(s * 2, -s * 2)); 
				float4 p7 = 0; //tex2D(_MainTex, uv + float2(s * 2, s * 2)); 
				
								
				float newValue = dot(cc.xyz, weight13MatrixCBuffer[0]) +
								 dot(cl.xyz, weight13MatrixCBuffer[1]) +
								 dot(tl.xyz, weight13MatrixCBuffer[2]) +
								 dot(tc.xyz, weight13MatrixCBuffer[3]) +
								 dot(tr.xyz, weight13MatrixCBuffer[4]) +
								 dot(cr.xyz, weight13MatrixCBuffer[5]) +
								 dot(br.xyz, weight13MatrixCBuffer[6]) +
								 dot(bc.xyz, weight13MatrixCBuffer[7]) +
								 dot(bl.xyz, weight13MatrixCBuffer[8]) +
								 dot(ll.xyz, weight13MatrixCBuffer[9]) +
								 dot(tt.xyz, weight13MatrixCBuffer[10]) +
								 dot(rr.xyz, weight13MatrixCBuffer[11]) +
								 dot(bb.xyz, weight13MatrixCBuffer[12]) +
								 dot(p0.xyz, weight5MatrixCBuffer[0]) +
								 dot(p1.xyz, weight5MatrixCBuffer[1]) +
								 dot(p2.xyz, weight5MatrixCBuffer[2]) +
								 dot(p3.xyz, weight5MatrixCBuffer[3]) +
								 dot(p4.xyz, weight9MatrixCBuffer[4]) +
								 dot(p5.xyz, weight9MatrixCBuffer[5]) +
								 dot(p6.xyz, weight9MatrixCBuffer[6]) +
								 dot(p7.xyz, weight9MatrixCBuffer[7]);

				return newValue;
			}

			fixed4 frag (v2f i) : SV_Target
			{	
				//float w, h;
				//_MainTex.GetDimensions(w, h);

				float2 _Pixels = float2((float)_PixelsWidth, (float)_PixelsHeight);  // why the fuck did I need to mult by 2?? -- something weird with Bilinear Sampling?
				
				// Cell centre
				//float2 uv = round(i.uv * _Pixels) / _Pixels;
 
				 // Neighbour cells
				float s = 1 / _Pixels;
				float4 cc = tex2D(_MainTex, i.uv + float2(0, 0)); // Centre Centre

				float3 oldValue = cc.xyz;
				float newValueR = GetValue13(s, i.uv);
				float newValueG = GetValue9(s, i.uv);
				float newValueB = GetValue5(s, i.uv);

				float strength = _FilterStrength;
				float valR = lerp(clamp(oldValue.x, -1, 1), clamp(newValueR, -1, 1), strength);
				float valG = lerp(clamp(oldValue.x, -1, 1), clamp(newValueR, -1, 1), strength);
				float valB = lerp(clamp(oldValue.x, -1, 1), clamp(newValueR, -1, 1), strength);

				fixed4 col = tex2D(_MainTex, i.uv);
				// just invert the colors
				col = float4(valR,valG,valB,1); //1 - col;
				return col;
			}
			ENDCG
		}
	}
}

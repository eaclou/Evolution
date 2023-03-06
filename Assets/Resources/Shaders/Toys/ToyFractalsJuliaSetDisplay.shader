Shader "Unlit/ToyFractalsJuliaSetDisplay"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_ZoomScale ("_ZoomScale", Float) = 1
		_ConstantMin ("_ConstantMin", Float) = 0.347
		_ConstantMax ("_ConstantMax", Float) = 0.41
		_AnimSpeed ("_AnimSpeed", Float) = 0.41
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
			float _ZoomScale;
			float _Constant;
			float _ConstantMin;
			float _ConstantMax;
			float _AnimSpeed;

			float2 iterate(float2 coords, float constant) {
				const float zr = coords.x * coords.x - coords.y * coords.y;
				const float zi = 2.0 * coords.x * coords.y;

				return float2(zr, zi) + constant;
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                
				//could add antialiasing by super-sampling 
				int numPixels = 1024;
				float offsetLinearDist = 1/numPixels * 0.167;
				float2 uvTL = i.uv + float2(-offsetLinearDist, offsetLinearDist);
				float2 uvTR = i.uv + float2(offsetLinearDist, offsetLinearDist);
				float2 uvBL = i.uv + float2(-offsetLinearDist, -offsetLinearDist);
				float2 uvBR = i.uv + float2(offsetLinearDist, -offsetLinearDist);
				float constant = lerp(_ConstantMin, _ConstantMax, sin(_Time.y*_AnimSpeed)*0.5 + 0.5);


				float2 startCoordsTL = (uvTL - 0.5) * _ZoomScale + _MainTex_ST.zw;
				float2 finalCoordsTL = startCoordsTL;
				float2 startCoordsTR = (uvTR - 0.5) * _ZoomScale + _MainTex_ST.zw;
				float2 finalCoordsTR = startCoordsTR;
				float2 startCoordsBL = (uvBL - 0.5) * _ZoomScale + _MainTex_ST.zw;
				float2 finalCoordsBL = startCoordsBL;
				float2 startCoordsBR = (uvBR - 0.5) * _ZoomScale + _MainTex_ST.zw;
				float2 finalCoordsBR = startCoordsBR;
				
				float iterCounterTL = 0;
				float iterCounterTR = 0;
				float iterCounterBL = 0;
				float iterCounterBR = 0;
								
				for(int tl = 0; tl < 256; tl++) {
					finalCoordsTL = iterate(finalCoordsTL, constant);
					iterCounterTL = iterCounterTL + 1.0;
					float distTL = finalCoordsTL.x*finalCoordsTL.x + finalCoordsTL.y*finalCoordsTL.y;
					if(distTL > 2.0) {						
						break;
					}
				}
				for(int tr = 0; tr < 256; tr++) {
					finalCoordsTR = iterate(finalCoordsTR, constant);
					iterCounterTR = iterCounterTR + 1.0;
					float distTR = finalCoordsTR.x*finalCoordsTR.x + finalCoordsTR.y*finalCoordsTR.y;
					if(distTR > 2.0) {						
						break;
					}
				}
				for(int bl = 0; bl < 256; bl++) {
					finalCoordsBL = iterate(finalCoordsBL, constant);
					iterCounterBL = iterCounterBL + 1.0;
					float distBL = finalCoordsBL.x*finalCoordsBL.x + finalCoordsBL.y*finalCoordsBL.y;
					if(distBL > 2.0) {						
						break;
					}
				}
				for(int br = 0; br < 256; br++) {
					finalCoordsBR = iterate(finalCoordsBR, constant);
					iterCounterBR = iterCounterBR + 1.0;
					float distBR = finalCoordsBR.x*finalCoordsBR.x + finalCoordsBR.y*finalCoordsBR.y;
					if(distBR > 2.0) {						
						break;
					}
				}
				
				float averageIterCount = (iterCounterTL + iterCounterTR + iterCounterBL + iterCounterBR) / 4.0;
				// sample the texture
                fixed4 col = tex2D(_MainTex, saturate(float2(averageIterCount / 256, sin(_Time.y * _AnimSpeed * 0.1) * 0.25 + 0.5)));
                
				//col.rgb = iterCounter / 256;
				col.a = 1;
				//col.rg = finalCoords;
				//col.b = 0;
				if(i.uv.y - _MainTex_ST.w < 0.035) {
					return tex2D(_MainTex, saturate(float2(i.uv.x - _MainTex_ST.z, sin(_Time.y * _AnimSpeed * 0.1) * 0.25 + 0.5)));
				}
                return col;
            }
            ENDCG
        }
    }
}

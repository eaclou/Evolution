Shader "UI/SpeciesCoatOfArms"
{
    Properties
    {
        _MainTex ("_MainTex", 2D) = "white" {}
		_PatternTex ("_PatternTex", 2D) = "white" {}
		_TintPri ("_TintPri", Color) = (1,1,1,1)
		_TintSec ("_TintSec", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags{ "RenderType" = "Transparent" }
		ZWrite Off
		//Cull Off
		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
           // #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                //UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			sampler2D _PatternTex;
			uniform float _PatternX;
			uniform float _PatternY;
			uniform float4 _TintPri;
			uniform float4 _TintSec;
			uniform float _IsSelected;
            uniform float _IsEndangered;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //UNITY_TRANSFER_FOG(o,o.vertex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                const float tilePercentage = (1.0 / 8.0);
				float2 patternUV = i.uv; // ????
				float randPatternIDX = _PatternX; 
				float randPatternIDY = _PatternY; 
				patternUV *= tilePercentage; // randVariation eventually
				patternUV.x += tilePercentage * randPatternIDX;
				patternUV.y += tilePercentage * randPatternIDY;
				fixed4 patternTexSample = tex2D(_PatternTex, patternUV);
				float4 col = lerp(_TintPri, _TintSec, patternTexSample.x);
				float4 maskColor = tex2D(_MainTex, i.uv);
				col.a = maskColor.a;
				float3 borderHue = lerp(float3(0,0,0), float3(1,1,1), _IsSelected);
                if (_IsEndangered > 0.5) {
                    borderHue = float3(1, 0, 0);
                }
				col.rgb = lerp(col.rgb, borderHue, maskColor.x);
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}

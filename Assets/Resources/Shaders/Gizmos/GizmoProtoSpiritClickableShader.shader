Shader "Gizmos/GizmoProtoSpiritClickableShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_AltitudeTex ("_AltitudeTex", 2D) = "gray" {}
		_WaterSurfaceTex ("_WaterSurfaceTex", 2D) = "black" {}
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
			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "Assets/Resources/Shaders/Inc/TerrainShared.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD1;
				float3 objectPos : TEXCOORD2;
				float4 screenUV : TEXCOORD3;
				float2 altitudeUV : TEXCOORD4;				
				fixed4 color : COLOR;
			};

			sampler2D _MainTex;
			fixed4 _MainTex_ST;
			sampler2D _AltitudeTex;
			sampler2D _WaterSurfaceTex;
			
			uniform float _MapSize;

			uniform float _IsVisible;
			uniform float _IsStirring;
			uniform float _Radius;
			uniform float _CamDistNormalized;

			
			v2f vert (appdata v)
			{
				v2f o;
				
				o.objectPos = v.vertex;
								
				v.vertex = mul(unity_ObjectToWorld, v.vertex); //float4(pos, 1.0)), //UnityObjectToWorldSpace v.vertex.xyz;				
				
				o.vertex = mul(UNITY_MATRIX_VP, v.vertex); // UnityObjectToClipPos(v.vertex);
				o.worldPos = v.vertex.xyz;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.altitudeUV = v.uv;
				float3 worldNormal = mul(unity_ObjectToWorld, float4(v.normal, 0)).xyz;
				
				o.color = _FogColor; //float4(0,0.75,0,1); // float4(saturate(dot(worldNormal, lightDir)), 0, 0, 1);

				float4 pos = mul(UNITY_MATRIX_VP, float4(o.worldPos, 1.0)); // *** Revisit to better understand!!!! ***
				float4 screenUV = ComputeScreenPos(pos);
				o.screenUV = screenUV; //centerUV.xy / centerUV.w;

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				
				float4 finalColor = i.color; // float4(1,1,1,1); // = lerp(frameBufferColor.rgb, particleColor, 0.25);
				
				return finalColor;
			}
			ENDCG
		}
	}
}

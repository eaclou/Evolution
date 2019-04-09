Shader "Gizmos/GizmoStirStickShader"
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
			
			#include "UnityCG.cginc"

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
				fixed4 color : COLOR;
			};

			sampler2D _MainTex;
			fixed4 _MainTex_ST;

			uniform float _IsVisible;
			uniform float _IsStirring;
			uniform float _Radius;
			uniform float _CamDistNormalized;

			uniform float _Turbidity;
			uniform float _CausticsStrength;
			uniform float _MinFog;
			uniform float4 _FogColor;
			
			v2f vert (appdata v)
			{
				v2f o;
				
				v.vertex *= _IsVisible;
				o.objectPos = v.vertex;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz; //float4(pos, 1.0)), //UnityObjectToWorldSpace v.vertex.xyz;				
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				float3 worldNormal = mul(unity_ObjectToWorld, float4(v.normal, 0)).xyz;
				float3 lightDir = float3(-0.52, -0.35, -1);
				o.color = float4(saturate(dot(worldNormal, lightDir)), 0, 0, 1);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);

				col = lerp(col, float4(0.714,0.651,0.44,1), 0.87) * 0.9;

				float diffuseVal = i.color.x;
				col.rgb *= diffuseVal * 0.5 + 0.5;
				
				float wetMask = 1.0 - saturate((i.objectPos.z + 1) * 1);
				col.rgb *= wetMask * 0.2 + 0.8;

				float depth = i.worldPos.z;
				float isUnderwater = 1.0 - saturate(depth * 25);
				col.rgb *= isUnderwater * 0.33 + 0.67;

				
				/*
				float depthNormalized = GetDepthNormalized(rawAltitude);
				float altitude = (rawAltitude * 2 - 1) * -1;
				float isUnderwater = saturate(altitude * 10000);
				float3 waterFogColor = _FogColor.rgb;
	
				return lerp(sourceColor, waterFogColor, (saturate(depthNormalized + _MinFog) * isUnderwater));
				*/

				float depthNormalized = saturate(depth / 10.0);
				col.rgb = lerp(col.rgb, _FogColor.rgb, saturate(depthNormalized + _MinFog) * _Turbidity); //(saturate(depthNormalized + _MinFog) * isUnderwater));
				//col.rgb *= isUnderwater;

				//col.rgb = i.color;
				return col;
			}
			ENDCG
		}
	}
}

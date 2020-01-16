Shader "Gizmos/GizmoProtoSpiritClickableShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_AltitudeTex ("_AltitudeTex", 2D) = "gray" {}
		_WaterSurfaceTex ("_WaterSurfaceTex", 2D) = "black" {}
		_ResourceTex ("_ResourceTex", 2D) = "black" {}
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
			sampler2D _ResourceTex;
			
			sampler2D _RenderedSceneRT;  // Provided by CommandBuffer -- global tex??? seems confusing... ** revisit this
			
			uniform float _MapSize;

			uniform float _IsVisible;
			uniform float _IsStirring;
			uniform float _Radius;
			uniform float _CamDistNormalized;

			
			v2f vert (appdata v)
			{
				v2f o;
				
				//v.vertex *= _IsVisible;
				o.objectPos = v.vertex;
								
				v.vertex = mul(unity_ObjectToWorld, v.vertex); //float4(pos, 1.0)), //UnityObjectToWorldSpace v.vertex.xyz;				
				// convert to worldPos ^
				//float vertexInitWorldZ = v.vertex.z - 2.5;
				//float3 lightDir = float3(-0.82, -0.35, -1);

				/*
								
				float2 altUV = v.vertex.xy / 256;  // Find pond floor at this alt
				o.altitudeUV = altUV;				
				float altitudeRaw = tex2Dlod(_AltitudeTex, float4(altUV.xy, 0, 0)).x;
				float depth = saturate(-altitudeRaw + 0.5);
				v.vertex.xyz += (v.normal * 0.15) * depth;
				v.vertex.xy += lightDir.xy * vertexInitWorldZ * 0.25;
				float3 surfaceNormal = tex2Dlod(_WaterSurfaceTex, float4(v.vertex.xy / 256, 0, 0)).yzw;
				
				float refractionStrength = depth * 5.5;
				v.vertex.xy += surfaceNormal.xy * refractionStrength;
				
				
				v.vertex.z = -(altitudeRaw * 2 - 1) * 10;  // drop to level of pond floor in worldspace
				*/
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
				
				/*col = lerp(col, float4(0.714,0.651,0.44,1), 0.87) * 0.9;
				float diffuseVal = i.color.x;
				col.rgb *= diffuseVal * 0.5 + 0.5;				
				float wetMask = 1.0 - saturate((i.objectPos.z + 1) * 1);
				col.rgb *= wetMask * 0.2 + 0.8;
				
				float isUnderwater = 1.0 - saturate(depth * 25);
				col.rgb *= isUnderwater * 0.33 + 0.67;
				*/
				//float depth = i.worldPos.z;
				
				/*
				float depthNormalized = GetDepthNormalized(rawAltitude);
				float altitude = (rawAltitude * 2 - 1) * -1;
				float isUnderwater = saturate(altitude * 10000);
				float3 waterFogColor = _FogColor.rgb;
	
				return lerp(sourceColor, waterFogColor, (saturate(depthNormalized + _MinFog) * isUnderwater));
				*/

				//float depthNormalized = saturate(depth / 10.0);
				//col.rgb = lerp(col.rgb, _FogColor.rgb, saturate(depthNormalized + _MinFog) * _Turbidity); //(saturate(depthNormalized + _MinFog) * isUnderwater));
				//col.rgb *= isUnderwater;

				//col.rgb = i.color;
				col.rgb = float3(1,0,0);


				//float2 screenUV = i.screenUV.xy / i.screenUV.w;
				//float4 frameBufferColor = tex2D(_RenderedSceneRT, screenUV);  //  Color of brushtroke source					
				//float4 altitudeTex = tex2D(_AltitudeTex, i.altitudeUV); //i.worldPos.z / 10; // [-1,1] range
				//float4 waterSurfaceTex = tex2D(_WaterSurfaceTex, i.altitudeUV);
				//float4 resourceTex = tex2D(_ResourceTex, (i.altitudeUV - 0.25) * 2);					
				//float4 finalColor = GetGroundColor(i.worldPos, frameBufferColor, altitudeTex, waterSurfaceTex, float4(1,1,1,1));
				//finalColor.a = col.a;

				float4 finalColor = i.color; // float4(1,1,1,1); // = lerp(frameBufferColor.rgb, particleColor, 0.25);
				//float4 finalColor = GetGroundColor(i.worldPos, frameBufferColor, altitudeTex, waterSurfaceTex, float4(0,0,0,0));
				//finalColor.a = col.a * 0.5;

				return finalColor;
			}
			ENDCG
		}
	}
}

// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Custom/doodadTVWalkerBScreen" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap ("Bumpmap", 2D) = "bump" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Luminance ("Luminance", Float) = 1
		_LightTint ("LightTint", Color) = (1,1,1,1)
		_RimPower ("Rim Power", Range(0.1, 10)) = 3.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpMap;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float3 viewDir;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float _Luminance;
		float _RimPower;
		float4 _LightTint;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));

			// This shader mostly just controls the emissive screen image, and won't be touching albedo/metallic/etc. until later for dirt/noise maps'

			//half rim = saturate(dot(normalize(IN.viewDir), o.Normal));
			//fixed4 emissive = tex2D(_EmissionMap, IN.uv_EmissionMap);
			
			float3 screenColor = float3(0,0,0);

				float3 gridLineColor = float3(0.5, 1, 0.66);

				// gridlines:
				float gridLineThickness = 0.1;
				float distToGridLineX = abs(frac(IN.uv_MainTex.x * 6) - gridLineThickness);
				float distToGridLineY = abs(frac(IN.uv_MainTex.y * 6) - gridLineThickness);

				float linesVal = 1.0 - smoothstep(0.0, gridLineThickness, distToGridLineX) + 1.0 - smoothstep(0.0, gridLineThickness, distToGridLineY);
				screenColor += gridLineColor * linesVal * 0.5;


				// SinWave!
				float frequency = 16; //lerp(4, 64, cos(_Time.y) * 0.5 + 0.5);
				float edgeReduce = 1.0 - smoothstep(0, 0.4, abs(IN.uv_MainTex.x - 0.5));
				float distToSinY = abs(IN.uv_MainTex.y - 0.5 - sin((IN.uv_MainTex.x + _Time.y) * frequency) * 0.25 * edgeReduce);
				float sinLineIntensity = 1.0 - smoothstep(0, 0.12, distToSinY);

				screenColor += gridLineColor * sinLineIntensity * 1;



				float distToCenter = length(IN.uv_MainTex - float2(0.5, 0.5));
				float radialMask = 1.0 - smoothstep(0.36, 0.5, distToCenter);
				screenColor *= radialMask;

			o.Emission = screenColor; //emissive.rgb * _LightTint.rgb * pow(rim, _RimPower) * _Luminance + emissive.rgb * _LightTint.rgb * pow(rim, _RimPower * 0.1) * (_Luminance * 0.15);
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}

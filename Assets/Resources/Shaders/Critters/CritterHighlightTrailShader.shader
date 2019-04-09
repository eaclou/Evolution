Shader "Unlit/CritterHighlightTrailShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		ZWrite Off
		Cull Off
		Blend SrcAlpha One
		//Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#include "UnityCG.cginc"
			#include "Assets/Resources/Shaders/Inc/NoiseShared.cginc"
			#include "Assets/Resources/Shaders/Inc/StructsCritterData.cginc"

			struct HighlightTrailData {
				float2 worldPos;
				float2 vel;
				float2 heading;
				float age;
				float strength;
			};

			sampler2D _MainTex;

			StructuredBuffer<HighlightTrailData> highlightTrailDataCBuffer;			
			StructuredBuffer<float3> quadVerticesCBuffer;
			StructuredBuffer<CritterInitData> critterInitDataCBuffer;	
			StructuredBuffer<CritterSimData> critterSimDataCBuffer;		
			
			uniform float _HighlightOn;
			uniform float _CamDistNormalized;
			
			uniform int _HoverID;
			uniform int _SelectedID;
			uniform float _IsHover;			
			uniform float _IsSelected;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;	
				float4 color : COLOR;
				float4 highlight : TEXCOORD1;
				float3 hue : TEXCOORD2;
			};

			float rand(float2 co) {   // OUTPUT is in [0,1] RANGE!!!
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}
			

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;

				float3 quadPoint = quadVerticesCBuffer[id];
				o.uv = quadVerticesCBuffer[id].xy + 0.5f;

				//int particleIndex = floor((float)inst / 128.0);
				
				float rand0 = rand(float2(inst, inst) * 10);
				//float rand1 = rand(float2(rand0, rand0) * 10);
				//float rand2 = rand(float2(rand1, rand1) * 10);
				int critterIndex = inst / 1024;
				CritterSimData critterSimData = critterSimDataCBuffer[critterIndex];

				float hoverMask = 1.0 - saturate(abs(critterIndex - _HoverID));
				float selectedMask = 1.0 - saturate(abs(critterIndex - _SelectedID));
							
				//AlgaeParticleData particleData = foodParticleDataCBuffer[particleIndex];

				HighlightTrailData data = highlightTrailDataCBuffer[inst];
				float3 worldPosition = float3(data.worldPos, 1.0);    //float3(rawData.worldPos, -random2);
				
				float fadeInTime = 0.2;
				float fadeOutTime = 0.3;
				float fadeMask = saturate(data.age) * (1.0 / fadeInTime);
				fadeMask *= saturate(1.0 - data.age) * (1.0 / fadeOutTime);
				/*float2 offsetRaw = (float2(rand0, rand1) * 2 - 1);				
				float2 offset = offsetRaw * (16 * particleData.biomass + 0.2) * 0.66;
				
				float masterFreq = 1;
				float spatialFreq = 0.55285;
				float timeMult = 0.013;
				float4 noiseSample = Value3D(worldPosition * spatialFreq + _Time * timeMult, masterFreq); //float3(0, 0, _Time * timeMult) + 
				float noiseMag = 0.520;
				float3 noiseOffset = noiseSample.yzw * noiseMag;

				worldPosition.xyz += noiseOffset;
				*/
				
				//quadPoint.y *= 1 + data.vel * 10;
				float2 forward = float2(0,1); //normalize(data.vel);
				float2 right = float2(forward.y, -forward.x); // perpendicular to forward vector
				float3 rotatedPoint = float3(quadPoint.x * right + quadPoint.y * forward, 0);  // Rotate localRotation by AgentRotation
				
				
				worldPosition = worldPosition + _HighlightOn * rotatedPoint * 0.33 * critterSimData.embryoPercentage * (_CamDistNormalized * 0.9 + 0.1); // * (_CamDistNormalized * 0.9 + 0.1) * (data.strength * _HighlightOn * 0.25 + 0.75); // * lerp(0.55, 0.85, data.age) * 0.12; // rotatedPoint * particleData.isActive;

				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0)));				
				o.color = float4(critterSimData.embryoPercentage, (1.0 - saturate(critterSimData.decayPercentage)),saturate(critterSimData.growthPercentage * 2.5),(1.0 - critterSimData.decayPercentage) * critterSimData.embryoPercentage); //float4(rand0 * 0.3 + 0.5, data.strength * _HighlightOn, hoverMask * _IsHover, selectedMask * _IsSelected);	
				o.highlight = float4(data.strength, saturate(hoverMask * _IsHover), selectedMask * _IsSelected, saturate(fadeMask));
				o.hue = critterInitDataCBuffer[critterIndex].primaryHue;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{				
				float4 texColor = tex2D(_MainTex, i.uv);
				float4 finalColor = float4(1, 1, 1, 1); //texColor.a); //texColor.a);
				float brightnessLerp = i.highlight.x * saturate(i.highlight.y * 0.75 + i.highlight.z) * i.highlight.w;

				finalColor.rgb = saturate(brightnessLerp);
				finalColor.rgb = lerp(finalColor.rgb, i.hue, 0.75);
				finalColor.a *= i.highlight.w;
				finalColor.a += i.highlight.y * 0.2;
				finalColor.a = finalColor.a * (saturate(i.highlight.y * 1 + i.highlight.z) * 0.5 + 0.5);

				finalColor.a *= i.color.x * i.color.y * i.color.z;
				
				finalColor.a *= texColor.a;
				//texColor.rgb = lerp(texColor.rgb, float3(0.6,1,0.45), 0.9);
				//texColor.rgb *= i.color.r;
				//texColor.a *= 0.212555 * i.color.a * (i.color.z * 0.75 + 0.25);

				//texColor.rgb = lerp(texColor.rgb, float3(0.5,0.5,0.5), i.color.y * 0.5 + 0.5);

				//texColor.a *= 1.0 - i.color.a;
				//texColor = 1.0;
				//texColor.rgb = float3(1,1,1) * i.color.a;
				//texColor.a = 0.05;
				//texColor.rgb = float3( i.color.z;
				//texColor.rgb += _SelectedID;
				return finalColor;
			}

			
			ENDCG
		}
	}
}

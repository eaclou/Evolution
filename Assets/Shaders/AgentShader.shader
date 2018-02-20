Shader "Unlit/AgentShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_IsPlayer ("_IsPlayer", Float) = 0.0
		_VelX ("_VelX", Float) = 0.0
		_VelY ("_VelY", Float) = 0.0
	}
	SubShader
	{
		Tags {"Queue" = "Transparent" "RenderType" = "Transparent"}
		LOD 100
		ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			//#pragma multi_compile_fog
			
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
			uniform float _IsPlayer;
			uniform float _VelX;
			uniform float _VelY;

			float rand(float2 co){
				return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex + float4(0,0,-0.1,0));
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				//UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				//fixed4 texSample = tex2D(_MainTex, i.uv);

				/*float4 bgColor = float4(0.2, 0.85, 0.75, 1.0);				
				float4 damageColor = float4(0.6, 0.4, 0.3,1);
				float damageRadius = 0.8;
				
				float3 negColor = float3(1, 0.6, 0.0) * 1.05;
				float3 posColor = float3(0.25, 0.6, 1) * 1.25;
				if(_IsPlayer > 0.5) {
					bgColor = float4(0.9, 1.25, 0.2, 1.0);
					damageColor = float4(0.8, 0.3, 0.2,1);
					outCommRadius *= 0.75;
				}*/

				const float PI = 3.141592;
				float2 facingDirection = normalize(float2(_VelX, _VelY));
				float facingAngle = atan(facingDirection.y / facingDirection.x);
				if (facingDirection.x < 0 && facingDirection.y < 0) // quadrant Ⅲ
					facingAngle = PI + facingAngle;
				else if (facingDirection.x < 0) // quadrant Ⅱ
					facingAngle = PI + facingAngle; // it actually substracts
				else if (facingDirection.y < 0) // quadrant Ⅳ
					facingAngle = 2 * PI + facingAngle; // it actually substracts
				/*if(facingDirection.x == 0) {
					facingAngle = PI * 0.5;
					if(facingDirection.y < 0)
						facingAngle = -facingAngle;
				}
				if(facingDirection.y == 0) {
					facingAngle = 0;
					if(facingDirection.x < 0)
						facingAngle = PI;
				}*/
				//facingAngle = PI * 0.5;

				float3 eyeColor = float3(1,0.9,0.6);
				float3 pupilColor = float3(0,0,0);
				
				float2 coords = float2((i.uv - 0.5) * 2.0);
				float circle = (1.0 - saturate(length(coords))) * 15;
				float distToOrigin = length(coords);

				float4 outCol = float4(0.15, 0.5, 0.55, saturate(circle));				

				//float foodRadius = 0.8;
				//float foodEnergy = (tex2D(_MainTex, float2(0.375, 0.333)).r + tex2D(_MainTex, float2(0.625, 0.333)).r + tex2D(_MainTex, float2(0.875, 0.333)).r) / 3.0;
								
				//float hitPoints = tex2D(_MainTex, float2(0.125, 0.333)).r;
				
				//float healthValue = saturate(min(foodEnergy, hitPoints));
				//if(healthValue < 0.50) {
					//outCol.rgb = lerp(damageColor, bgColor, healthValue * 2);
					//outCommRadius *= healthValue * 3;
					//negColor *= 0.5;
					//posColor *= 0.5;
				//}
				//outCol.rgb = lerp(outCol.rgb, float3(0.8, 0.75, 0.9), 0.45);
				float eyesOpen = saturate(length(float2(_VelX, _VelY)) * 20);
				float eyeRadius = 0.4;			
				float2 outCommOrigin0 = float2(0.6, 0.5);
				float2 outComCoords0 = float2(outCommOrigin0.x * cos(facingAngle) - outCommOrigin0.y * sin(facingAngle),
											  outCommOrigin0.y * cos(facingAngle) + outCommOrigin0.x * sin(facingAngle));
				float outCommDist0 = length(coords - outComCoords0);
				if(outCommDist0 < eyeRadius) {
					outCol.rgb = lerp(outCol.rgb, eyeColor, eyesOpen);
					outCol.a = 1;
				}
				
				float2 outCommOrigin1 = float2(0.6, -0.5);
				float2 outComCoords1 = float2(outCommOrigin1.x * cos(facingAngle) - outCommOrigin1.y * sin(facingAngle),
											  outCommOrigin1.y * cos(facingAngle) + outCommOrigin1.x * sin(facingAngle));
				float outCommDist1 = length(coords - outComCoords1);
				if(outCommDist1 < eyeRadius) {
					outCol.rgb = lerp(outCol.rgb, eyeColor, eyesOpen);
					outCol.a = 1;
				}

				float pupilRadius = 0.24;			
				float2 outCommOrigin2 = float2(0.75, 0.55);
				float2 outComCoords2 = float2(outCommOrigin2.x * cos(facingAngle) - outCommOrigin2.y * sin(facingAngle),
											  outCommOrigin2.y * cos(facingAngle) + outCommOrigin2.x * sin(facingAngle));
				float outCommDist2 = length(coords - outComCoords2);
				if(outCommDist2 < pupilRadius) {
					outCol.rgb = lerp(outCol.rgb, pupilColor, eyesOpen);
					outCol.a = 1;
				}
				
				float2 outCommOrigin3 = float2(0.75, -0.55);
				float2 outComCoords3 = float2(outCommOrigin3.x * cos(facingAngle) - outCommOrigin3.y * sin(facingAngle),
											  outCommOrigin3.y * cos(facingAngle) + outCommOrigin3.x * sin(facingAngle));
				float outCommDist3 = length(coords - outComCoords3);
				if(outCommDist3 < pupilRadius) {
					outCol.rgb = lerp(outCol.rgb, pupilColor, eyesOpen);
					outCol.a = 1;
				}
				

				/*float cellSize = 4;
				coords = floor(coords * cellSize) / cellSize;

				float randVal = lerp(0, rand(coords), (distToOrigin));//rand(coords) - (saturate((1.0 - distToCenter))) * 0.25;
				if(randVal > hitPoints * 1.5) {
					//outCol.a = min(outCol.a, hitPoints * 0.5);					
				}
				if(hitPoints < 0.60) {
						outCol.rgb = lerp(damageColor, outCol.rgb, hitPoints * 1.5);
					}*/
				
				
				//outCol.a = circle;
				outCol.rgb = float3(0, 2, 6);
				// inside CGPROGRAM in the fragment Shader:
				//float alphaCutoffValue = 0.1;
				//clip(outCol.a - alphaCutoffValue);
				return outCol;
			}
			ENDCG
		}
	}
}

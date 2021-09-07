Shader "Unlit/TestDisplayShaderBrainVisualizationCables"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BumpMap("Normal Map", 2D) = "bump" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		// inside SubShader
		//Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }

		// inside Pass
		//ZWrite Off
		//Blend One One
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom
			#pragma target 5.0   // SUPER FUCKING IMPORTANT!!!!!!
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			float4 _MainTex_ST;
			// normal map texture from shader properties
            sampler2D _BumpMap;

			struct Triangle {
				float3 vertA;
				float3 normA;
				float3 tanA;
				float3 uvwA;
				float3 colorA;

				float3 vertB;
				float3 normB;
				float3 tanB;
				float3 uvwB;
				float3 colorB;	

				float3 vertC;
				float3 normC;
				float3 tanC;
				float3 uvwC;
				float3 colorC;
			};

			#if SHADER_TARGET >= 45
            StructuredBuffer<Triangle> appendTrianglesCablesCBuffer;
			#endif

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;				
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				uint id : TEXCOORD1;
				UNITY_FOG_COORDS(3)
				float3 nml : NORMAL0;
				float3 tan : TANGENT;
				float3 col : COLOR0;
				float4 vertex : SV_POSITION;
				float4 worldPos : TEXCOORD4;
				float angleDot : TEXCOORD2;

				// these three vectors will hold a 3x3 rotation matrix
                // that transforms from tangent to world space
                half3 tspace0 : TEXCOORD5; // tangent.x, bitangent.x, normal.x
                half3 tspace1 : TEXCOORD6; // tangent.y, bitangent.y, normal.y
                half3 tspace2 : TEXCOORD7; // tangent.z, bitangent.z, normal.z
			};
			
			v2f vert (appdata v, uint vID : SV_VertexID )
			{
				
				float3 worldPosition = float3(0, 0, 0);

				v2f o;
				o.vertex = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));
				o.id = vID;
				o.nml = float3(0,0,0);
				o.tan = float3(0,0,0);
				o.col = float3(0,0,0);
				o.angleDot = 0;
				o.worldPos = float4(0, 0, 0, 1);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.tspace0 = half3(0,0,0);
                o.tspace1 = half3(0,0,0);
                o.tspace2 = half3(0,0,0);

				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			[maxvertexcount(3)]
			void geom(point v2f p[1], inout TriangleStream<v2f> triStream) {
				
				v2f pIn;
				pIn.id = p[0].id;

				#if SHADER_TARGET >= 45
					Triangle triData = appendTrianglesCablesCBuffer[p[0].id];				
				#endif

				float3 fakeLightDir = float3(0.45,1,-0.1);

				// How can I compress these 3 identical chunks into one function so I only have to change it once?
				// Try changing the Triangle struct to be arranged per-vertex, inside sub-structs called verts ?? would this break the code?
				// 

				pIn.nml = triData.normA;
				pIn.tan = triData.tanA;
				half3 wNormal = UnityObjectToWorldNormal(pIn.nml);
                half3 wTangent = UnityObjectToWorldDir(pIn.tan);
                // compute bitangent from cross product of normal and tangent
                half tangentSign = 1; //tangent.w * unity_WorldTransformParams.w;
                half3 wBitangent = cross(wNormal, wTangent) * tangentSign;
                // output the tangent space matrix
                pIn.tspace0 = half3(wTangent.x, wBitangent.x, wNormal.x);
                pIn.tspace1 = half3(wTangent.y, wBitangent.y, wNormal.y);
                pIn.tspace2 = half3(wTangent.z, wBitangent.z, wNormal.z);
				////////
				float3 viewDir = normalize(WorldSpaceViewDir(float4(triData.vertA, 1)));
				pIn.worldPos = float4(triData.vertA, 1);
				pIn.vertex = mul(UNITY_MATRIX_VP, pIn.worldPos);
				pIn.uv = TRANSFORM_TEX(triData.uvwA.xy, _MainTex);				
				float angle = (dot(normalize(viewDir), normalize(pIn.nml)));
				pIn.angleDot = angle; //float3(angle*angle,angle*angle,angle*angle);
				pIn.col = triData.colorA;
				UNITY_TRANSFER_FOG(pIn, pIn.vertex); // think this function might need vertex to already be in 'clip'
				triStream.Append(pIn);

				pIn.nml = triData.normB;
				pIn.tan = triData.tanB;
				wNormal = UnityObjectToWorldNormal(pIn.nml);
                wTangent = UnityObjectToWorldDir(pIn.tan);
                // compute bitangent from cross product of normal and tangent
                tangentSign = 1; //tangent.w * unity_WorldTransformParams.w;
                wBitangent = cross(wNormal, wTangent) * tangentSign;
                // output the tangent space matrix
                pIn.tspace0 = half3(wTangent.x, wBitangent.x, wNormal.x);
                pIn.tspace1 = half3(wTangent.y, wBitangent.y, wNormal.y);
                pIn.tspace2 = half3(wTangent.z, wBitangent.z, wNormal.z);
				/////////////
				viewDir = normalize(WorldSpaceViewDir(float4(triData.vertB, 1)));
				pIn.worldPos = float4(triData.vertB, 1);
				pIn.vertex = mul(UNITY_MATRIX_VP, pIn.worldPos);
				pIn.uv = TRANSFORM_TEX(triData.uvwB.xy, _MainTex);
				angle = (dot(normalize(viewDir), normalize(pIn.nml)));
				pIn.angleDot = angle; //float3(angle*angle,angle*angle,angle*angle);
				pIn.col = triData.colorB;								
				UNITY_TRANSFER_FOG(pIn, pIn.vertex);
				triStream.Append(pIn);

				pIn.nml = triData.normC;
				pIn.tan = triData.tanC;
				wNormal = UnityObjectToWorldNormal(pIn.nml);
                wTangent = UnityObjectToWorldDir(pIn.tan);
                // compute bitangent from cross product of normal and tangent
                tangentSign = 1; //tangent.w * unity_WorldTransformParams.w;
                wBitangent = cross(wNormal, wTangent) * tangentSign;
                // output the tangent space matrix
                pIn.tspace0 = half3(wTangent.x, wBitangent.x, wNormal.x);
                pIn.tspace1 = half3(wTangent.y, wBitangent.y, wNormal.y);
                pIn.tspace2 = half3(wTangent.z, wBitangent.z, wNormal.z);
				/////////////
				viewDir = normalize(WorldSpaceViewDir(float4(triData.vertC, 1)));				
				pIn.worldPos = float4(triData.vertC, 1);
				pIn.vertex = mul(UNITY_MATRIX_VP, pIn.worldPos);
				pIn.uv = TRANSFORM_TEX(triData.uvwC.xy, _MainTex);
				angle = (dot(normalize(viewDir), normalize(pIn.nml)));
				pIn.angleDot = angle; //float3(angle*angle,angle*angle,angle*angle);
				pIn.col = triData.colorC;		
				UNITY_TRANSFER_FOG(pIn, pIn.vertex);
				triStream.Append(pIn);

				//triStream.RestartStrip();
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				//float4 testCol = lerp(float4(1,1,1,1), float4(i.col,1), 0.5);
				//UNITY_APPLY_FOG(i.fogCoord, testCol);
				//return testCol;

				float normalStrength = 5;
				half3 tnormal = lerp(float4(normalize(float3(0.0, 0.0, 1.0)), 1.0), UnpackNormal(tex2D(_BumpMap, i.uv)), normalStrength);

				float4 texColor = tex2D(_MainTex, i.uv);
								
				half3 worldNormal;
                worldNormal.x = dot(i.tspace0, tnormal);
                worldNormal.y = dot(i.tspace1, tnormal);
                worldNormal.z = dot(i.tspace2, tnormal);
				//return float4(tnormal, 1.0); //tex2D(_BumpMap, i.uv); 


				//return float4(tex2D(_MainTex, i.uv));
				//return UnpackNormal(tex2D(_MainTex, i.uv));

				float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
				float3 fakeLightDir = float3(0, 1.0, 0);
				float angleDot = saturate(dot(viewDir, normalize(worldNormal)) + 0);
				float lightDot = saturate(dot(fakeLightDir, normalize(worldNormal)));		
				//float3 c = reflect(-viewDir, normalize(worldNormal)); //triData.colorB;
				// sample the default reflection cubemap, using the reflection vector
                //half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, c);
                // decode cubemap data into actual color
                //half3 skyColor = DecodeHDR (skyData, unity_SpecCube0_HDR);
				
				//return float4(skyData.xyz, 1); //===============================================================================================================

				// sample the texture
				//i.angleDot = saturate(i.angleDot);
				//i.angleDot = i.angleDot * 1;

				float fakeFog = 0; //1.2 - (i.worldPos.x * 0.5 + 0.5) * 1.2;

				float3 textureMaskColor = lerp(0, 1.0 - texColor, pow(1.0 - angleDot, 1.0/1.0));
				fixed4 col = fixed4(i.col * lightDot, 1.0); //lerp(fixed4(i.col, 1.0), fixed4(textureMaskColor, 1), 1);
				//fixed4 col = fixed4(i.col.r * textureMaskColor, i.col.g * textureMaskColor, i.col.b * textureMaskColor, 1.0) + lightDot * 0.25 + 0.1; //float4(0.6, .95, .7,1); //tex2D(_MainTex, i.uv);
				col = lerp(float4(textureMaskColor,1), col * 0.1, 1);
				col = lerp(col, float4(1,1,1,1), fakeFog);
				// apply fog
				//UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}

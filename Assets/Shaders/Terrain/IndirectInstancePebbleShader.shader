Shader "Instanced/IndirectInstancePebbleShader" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_BaseColorPrimary ("_BaseColorPrimary", Color) = (1,1,1,1)
		_BaseColorSecondary ("_BaseColorSecondary", Color) = (1,1,1,1)
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model
        #pragma surface surf Standard addshadow fullforwardshadows
        #pragma multi_compile_instancing
        #pragma instancing_options procedural:setup

        sampler2D _MainTex;
		float4 _BaseColorPrimary;
		float4 _BaseColorSecondary;

        struct Input {
            float2 uv_MainTex;
			float3 color;
        };

		struct TransformData {
			float4 worldPos;
			float3 scale;
			float4 rotation;
		};

		
    #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
        StructuredBuffer<TransformData> instancedPebblesCBuffer;
    #endif

        void rotate2D(inout float2 v, float r)
        {
            float s, c;
            sincos(r, s, c);
            v = float2(v.x * c - v.y * s, v.x * s + v.y * c);
        }

		float3 qtransform(float4 q, float3 v){ 
			return v + 2.0*cross(cross(v, q.xyz ) + q.w*v, q.xyz);
		}

		float rand(in float2 uv)
		{
			float2 noise = (frac(sin(dot(uv ,float2(12.9898,78.233)*2.0)) * 43758.5453));
			return abs(noise.x + noise.y) * 0.5;
		}

        void setup()
        {
        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            TransformData data = instancedPebblesCBuffer[unity_InstanceID];
			
			//float4x3 dataM;
			//dataM.transform._11_21_31  ??????

            //float rotation = data.w * data.w * _Time.y * 0.5f;
            //rotate2D(data.xz, rotation);
			float3 wPosition = data.worldPos.xyz;
			//wPosition = qtransform(data.rotation, wPosition);

			float3 tempScale = float3(0.1,0.1,0.1);
            unity_ObjectToWorld._11_21_31_41 = float4(data.scale.x, 0, 0, 0);
            unity_ObjectToWorld._12_22_32_42 = float4(0, data.scale.x, 0, 0);
            unity_ObjectToWorld._13_23_33_43 = float4(0, 0, data.scale.x, 0);
            unity_ObjectToWorld._14_24_34_44 = float4(wPosition, 1);

			// rotation
			float rx = data.rotation.x * 0.6;
			float ry = data.rotation.y * 2;
			float rz = data.rotation.z * 0.6;
            float4x4 rotationMatrix = float4x4(  // XYZ:
				cos(ry)*cos(rz), -cos(ry)*sin(rz), sin(ry), 0,
				cos(rx)*sin(rz)+sin(rx)*sin(ry)*cos(rz), cos(rx)*cos(rz)-sin(rx)*sin(ry)*sin(rz), -sin(rx)*cos(ry), 0,
				sin(rx)*sin(rz)-cos(rx)*sin(ry)*cos(rz), sin(rx)*cos(rz)+cos(rx)*sin(ry)*sin(rz), cos(rx)*cos(ry), 0,
				0, 0, 0, 1);
            unity_ObjectToWorld = mul(unity_ObjectToWorld, rotationMatrix);

            unity_WorldToObject = unity_ObjectToWorld;
            unity_WorldToObject._14_24_34 *= -1;
            unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;
        #endif
        }

        half _Glossiness;
        half _Metallic;

        void surf (Input IN, inout SurfaceOutputStandard o) {
			float4 col = 1.0f;
			float randColorLerp = 0;
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			//col.gb = (float)(unity_InstanceID % 256) / 255.0f;
			col = instancedPebblesCBuffer[unity_InstanceID].worldPos;	
			randColorLerp = rand(col.xz);
			//col = col.xxx1;
#else
			//col.gb = float4(0, 0, 1, 1);
			col = float4(0, 0, 1, 1);
#endif

			//float3 hue = float3(rand(col.xy), rand(col.yz), rand(col.zw));
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);			
            o.Albedo = c.rgb * lerp(_BaseColorPrimary, _BaseColorSecondary, randColorLerp); //float3(c.r * hue.x, c.y * hue.y, c.z * hue.z);
			
			float grayVal = o.Albedo.x * 0.299 + o.Albedo.y * 0.587 + o.Albedo.z * 0.114;
			o.Albedo = float3(1,1,1); //float3(grayVal, grayVal, grayVal);

            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
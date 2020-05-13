Shader "Custom/TriMap" {
	Properties {
		[Toggle(WORLDSPACE)] _WORLDSPACE("Use Worldspace Position", Float) = 1
		
		_Color ("Color", Color) = (1,1,1,1)
		[NoScaleOffset] _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Scale ("Texture Scale", Float) = 1
		_Offset ("Texture Offset (xyz * w)", Vector) = (0, 0, 0, 1)
		[NoScaleOffset] _Detail ("Detail", 2D) = "gray" {}
		_DetScale ("Detail Scale", Float) = 4
		_DetOffset ("Detail Offset (xyz * w)", Vector) = (0, 0, 0, 1)
		
		
		[NoScaleOffset] _BumpMap ("Bump", 2D) = "bump" {}
		[NoScaleOffset] _Occlusion ("Occlusion", 2D) = "white" {}
		[NoScaleOffset] _Height ("Height", 2D) = "gray" {}
		
		_DepthLayers ("Depth Layers", Range(1, 32)) = 8
		_Parallax ("Parallax ammount", Float) = .02
		_Bias ("Parallax bias", Float) = 0
		
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
	}
	SubShader {
		Tags { 
			"Queue"="Geometry" 
			"RenderType"="Opaque"
		}
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard vertex:vert fullforwardshadows 
		#pragma target 4.0
		#pragma multi_compile __ WORLDSPACE

		struct Input {
			float3 worldPos;
			float3 viewDir;
			
			float3 wNormal;
			float3 wTangent;
			float3 wBinormal;
		};

		fixed4 _Emission;
		
		float _Spec;
		float _EmissionAmt;
		float _Parallax;
		float _Bias;
		
		float _Scale;
		float _DetScale;

		int _DepthLayers;
		
		sampler2D _MainTex;
		sampler2D _Detail;
		sampler2D _BumpMap;
		sampler2D _Height;
		sampler2D _Occlusion;
		sampler2D _EmissionTex;
		
		half _Glossiness;
		half _Metallic;
		
		float4 _Offset;
		float4 _DetOffset;
		float4 _Color;
		
		
		void vert(inout appdata_full v, out Input data) {
			UNITY_INITIALIZE_OUTPUT(Input, data);
			data.worldPos = v.vertex;
			data.viewDir = WorldSpaceViewDir(v.vertex);
			
			float3x3 o2w = (float3x3)unity_ObjectToWorld;
			data.wNormal = normalize(mul(o2w, v.normal));
			data.wTangent = normalize(mul(o2w, v.tangent));
			data.wBinormal = cross(data.wNormal, data.wTangent);
		}
		
		
		float3 Parallax3D(Input IN, float depth) {
			float3 p = -IN.viewDir * (depth * .001 * _Parallax - _Bias * .001);
			
			return p.x * IN.wTangent + p.y * -IN.wBinormal + p.z * IN.wNormal;
		}
		
		inline fixed Depth3D(float3 pos, float3 blend) {
			const fixed hx = tex2D(_Height, pos.zy);
			const fixed hy = tex2D(_Height, pos.xz);
			const fixed hz = tex2D(_Height, pos.xy);
			return (hx * blend.x + hy * blend.y + hz * blend.z);
		}
		
		int layers;
		inline float3 Parallax3D_Layered(Input IN, float3 start) {
			half3 blend = abs(IN.wNormal);
			//const float numLayers = lerp(8.0, _DepthLayers, abs(dot(float3(0,0,1), IN.viewDir)));
			const float layerDepth = 1.0 / _DepthLayers;
			
			const float3 p = -IN.viewDir * (.001 * _Parallax - _Bias * .001);
			const float3 delta = (p.x * IN.wTangent + p.y * -IN.wBinormal + p.z * IN.wNormal) / _DepthLayers;
			
			float3 pos = start;
			fixed curDepth = 0.0;
			fixed curTexDepth = Depth3D(pos, blend);
			
			int iter = 0;
			layers = _DepthLayers;
			
			for (iter = 0; iter < layers; iter++) {
				if (curDepth < curTexDepth) { 
					pos += delta;
					curTexDepth = Depth3D(pos, blend);
					curDepth += layerDepth;
				}
			}
				
			return pos;
		}
		
		inline float3 Parallax3D_Occ(Input IN, float3 start) {
			//const half3 blend = normalize(abs(IN.wNormal * IN.wNormal * IN.wNormal));
			half3 blend = abs(IN.wNormal);
			//const float numLayers = lerp(8.0, _DepthLayers, abs(dot(float3(0,0,1), IN.viewDir)));
			const float layerDepth = 1.0 / _DepthLayers;
			
			const float3 p = -IN.viewDir * (.001 * _Parallax - _Bias * .001);
			const float3 delta = (p.x * IN.wTangent + p.y * -IN.wBinormal + p.z * IN.wNormal) / _DepthLayers;
			
			float3 pos = start;
			fixed curDepth = 0.0;
			fixed prevDepth = 0.0;
			fixed curTexDepth = Depth3D(pos, blend);
			
			int iter = 0;
			layers = _DepthLayers;
			
			for (iter = 0; iter < layers; iter++) {
				if (curDepth < curTexDepth) { 
					prevDepth = curTexDepth;
					pos += delta;
					curTexDepth = Depth3D(pos, blend);
					curDepth += layerDepth;
				}
			}
			
			float3 prev = pos - delta;
			fixed after = curTexDepth - curDepth;
			fixed before = prevDepth - curTexDepth + layerDepth;
			
			fixed weight = after / (after - before);
			
			return prev * weight + pos * (1.0 - weight);
		}
		
		
		void surf(Input IN, inout SurfaceOutputStandard o) {
			// o.Emission = abs(IN.wNormal.xyz);
			// o.Emission = abs(IN.wBinormal.xyz);
			// o.Emission = abs(IN.wTangent.xyz);
			// o.Emission = Parallax3D(IN, 15);
			
			// o.Albedo = 0;
			// o.Alpha = 1;
			// o.Metallic = _Metallic;
			// o.Smoothness = _Glossiness;
			// return;
			
			// Unfortunately, we have to do tangent space per pixel...
			/* float4x4 tbn = float3x3(
				float4(IN.wTangent.xyz, 0), 
				float4(IN.wBinormal.xyz, 0), 
				float4(IN.wNormal.xyz, 0), 
				float4(0,0,0,1));
			//
			*/	
			//float3 tView = normalize(mul(tbn, IN.viewDir).xyz);
			
			
			const float4 wPos = float4(IN.worldPos, 1);
			#ifdef WORLDSPACE
				const float4 pos = wPos;
			#else 
				const float4 pos = mul(unity_WorldToObject, wPos);
			#endif
			
			const float3 offset =  _Offset.xyz * _Offset.w;
			//half3 blend = normalize(abs(IN.wNormal * IN.wNormal * IN.wNormal));
			half3 blend = abs(IN.wNormal);
			
			const half sum = (blend.x + blend.y + blend.z);
			if (sum != 0) { blend /= sum; }
			const float3 preCoords = pos * _Scale + offset;
			
			// const fixed hx = tex2D(_Height, preCoords.zy);
			// const fixed hy = tex2D(_Height, preCoords.xz);
			// const fixed hz = tex2D(_Height, preCoords.xy);
			// const fixed h = (hx * blend.x + hy * blend.y + hz * blend.z);
			//const float3 p = Parallax3D(IN, h);
			
			/*
			fixed h = -.5 + .5 *(hx * blend.x + hy * blend.y + hz * blend.z);
			float v = h * _Parallax - _Bias;
			float3 eye = IN.viewDir;//normalize(IN.viewDir);
			float3 p = -eye * v;
			*/
			
			const float3 coords = Parallax3D_Occ(IN, preCoords);
			const float3 p = coords - preCoords;
			
			const fixed4 cx = tex2D(_MainTex, coords.zy);
			const fixed4 cy = tex2D(_MainTex, coords.xz);
			const fixed4 cz = tex2D(_MainTex, coords.xy);
			const fixed4 c = cx * blend.x + cy * blend.y + cz * blend.z;
			
			const fixed3 ox = tex2D(_Occlusion, coords.zy);
			const fixed3 oy = tex2D(_Occlusion, coords.xz);
			const fixed3 oz = tex2D(_Occlusion, coords.xy);
			const fixed3 occ = ox * blend.x + oy * blend.y + oz * blend.z;
			
			const float3 detCoords = pos * _DetScale + p + _DetOffset.xyz * _DetOffset.w;
			const fixed4 dx = tex2D(_Detail, detCoords.zy);
			const fixed4 dy = tex2D(_Detail, detCoords.xz);
			const fixed4 dz = tex2D(_Detail, detCoords.xy);
			const fixed4 d = (dx * blend.x + dy * blend.y + dz * blend.z);
			
			const fixed3 nx = UnpackNormal(tex2D(_BumpMap, coords.zy));
			const fixed3 ny = UnpackNormal(tex2D(_BumpMap, coords.xz));
			const fixed3 nz = UnpackNormal(tex2D(_BumpMap, coords.xy));
			const fixed3 n = nx * blend.x + ny * blend.y + nz * blend.z;
			
			
			o.Occlusion = occ;
			o.Normal = n;
			o.Albedo = 2.0 * c.rgb * d.rgb * _Color;
			
			
			//o.Emission = c.rgb;
			//o.Normal = UnpackNormal(n);
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}

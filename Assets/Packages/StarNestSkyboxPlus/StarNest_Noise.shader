//Star Nest algorithm by Pablo Román Andrioli
//Unity 5.x shader by Jonathan Cohen
//This content is under the MIT License.
//
//Original Shader:
//https://www.shadertoy.com/view/XlfGRj
//
//This shader uses the same algorithm in 3d space to render a skybox.

Shader "FX/StarNest Full" {
	Properties {
		_MainTex ("Texture Sampler (UV Only)", 2D) = "grey" {}
		
		//2D: Zoom level
		_Zoom ("Zoom", Float) = 800
		
		_Color ("Main Color", Color) = (1,1,1,1)	
		
		[Toggle(CLAMPOUT)] _CLAMPOUT("Clamp Output with Main Color", Float) = 0
		//Upper range for output. Negative means unlimited.
		_UpperRange ("Upper Range (Use to reduce ambient lighting or skybox brightness)", 
		Range(-1, 100)) = 15
		
		//Shift in hue
		_HueShift ("Post-Hue Shift", Range(0, 1)) = 0
		
		//Post-Adjust in saturation
		_PostSat ("Post-Saturation", Range(-1,1)) = 0
		
		//Scrolls in this direction over time. Set 'w' to zero to stop scrolling.
		_Scroll ("Scrolling direction (x,y,z) * w * time", Vector) = (3, 1, .6, .01)
		
		//Center position in space and time.
		_Center ("Center Position (x, y, z, time)", Vector) = (1, .3, .5, 0)
		
		//Rotation angles
		_Rotation ("Rotation (x,y,z)*w angles", Vector) = (35, 25, 75, .1)
		
		//Iterations of inner loop. 
		//The higher this is, the more distant objects get rendered.
		_Iterations ("Iterations", Range(1, 30)) = 17
		
		//Volumetric rendering steps. Each 'step' renders more objects at all distances.
		//This has a higher performance hit than iterations.
		_Volsteps ("Volumetric Steps", Range(1,40)) = 20
		
		//Magic number. Best values are around 400-600.
		_Formuparam ("Formuparam", Float) = 530
		
		//How much farther each volumestep goes
		_StepSize ("Step Size", Float) = 130
		
		//Fractal repeating rate
		//Low numbers are busy and give lots of repititio
		//High numbers are very sparce
		_Tile ("Tile", Float) = 700
		
		//Brightness scale.
		_Brightness ("Brightness", Float) = 2
		//Abundance of Dark matter (in the distance). 
		//Visible with Volsteps >= 8 (at 7 its really, really hard to see)
		_Darkmatter ("Dark Matter", Float) = 25
		//Brightness of distant objects (or dim) are distant objects
		//Ironically, Also affets brightness of 'darkmatter'
		_Distfading ("Distance Fading", Float) = 68
		//How much color is present?
		_Saturation ("Saturation", Float) = 85
		
		//Vector for additional fractal field
		_FieldVec ("Field Vector", Vector) = (-.7, -.8, -.8, 0)
		//Detail per Repetition
		_FieldPow ("Field Power", Range(.5, 3)) = 1.7
		//Reps for first layer of field
		_FieldReps ("Field Reps", Range(1, 30)) = 11
		//Reps for second layer of field
		_FieldReps2 ("Field Reps2", Range(1, 20)) = 11
		
		//Adjustment to input to first field
		_FieldSize ("Field Size", Range(.1, 10)) = 2.3
		//Adjustment to input to second field
		_FieldSize2 ("Field Size 2", Range(.1, 10)) = 1.8
		
		//Brighness of first field layer
		_FieldBrightness ("Field Brightness", Range(0, 20)) = 8
		//Brighness of second field layer
		_FieldBrightness2 ("Field Brightness 2", Range(0, 20)) = 3
		//Oscilation Frequency of first field layer
		_FieldFreq ("Field Frequency", Range(0, 12)) = 1
		//Oscilation Frequency of second field layer
		_FieldFreq2 ("Field Frequency 2", Range(0, 12)) = 2
		
		//Change in color of first field layer
		_FieldHue ("Field Hueshift", Range(0, 1)) = 0
		//Change in color of second field layer
		_FieldHue2 ("Field Hueshift 2", Range(0, 1)) = 0
		
		//How big are the 'nebulas'
		_NebulaPower ("Nebula Power", Range(0, .4)) = .2
		//How much light do the 'nebulas' block?
		_DarkNebula ("Nebula Darkness", Range(0, 4)) = 1
		
		_Seed ("Seed", Float) = 31337.1337
		_Octaves ("NoiseOctaves", Range(1, 32)) = 5.0
		_Persistence ("NoisePersistence", Range(0, 1)) = .95
		
		
		_Scale ("NoiseScale", Float) = 20.0
	}

	SubShader {
		Tags { "Queue"="Geometry" "RenderType"="Opaque" }
		Cull Off
		
		
		CGPROGRAM
		
		
		#pragma surface surf Lambert
		#pragma multi_compile __ CLAMPOUT
		#include "UnityCG.cginc"
		#include "noiseprims.cginc"
		#include "hsv.cginc"
		
		float _UpperRange;
		fixed4 _Color;
			
		float _HueShift;
		float _PostSat;
		
		int _Volsteps;
		int _Iterations;
		sampler2D _MainTex;
		
		float4 _Scroll;
		float4 _Center;
		float _CamScroll;
		float4 _Rotation;
		
		float _Formuparam;
		float _StepSize;
		
		float _Zoom;
		
		float _Tile;
		
		float _Brightness;
		float _Darkmatter;
		float _Distfading;
		float _Saturation;
		
		float _NebulaPower;
		float _DarkNebula;
		
		float3 _FieldVec;
		float _FieldPow;
		float _FieldSize;
		float _FieldSize2;
		float _FieldBrightness;
		float _FieldBrightness2;
		float _FieldFreq;
		float _FieldFreq2;
		float _FieldReps;
		float _FieldReps2;
		float _FieldHue;
		float _FieldHue2;
			
		struct Input {
			float2 uv_MainTex;
		};
		
		float3 noisedNebula(half3 pos, float3 dir) {
			float time = _Center.w + _Time.x;
			
			float sc = 2.3;
			float x1 = onoise(pos + dir);
			float x2 = onoise(pos + dir * sc);
			float x3 = onoise(pos + dir / sc);
			
			float n = x1 * x2 * x3;
			if (n < _NebulaPower) {
				float v = -(_NebulaPower-n);
				return float3(v,v,v) * _DarkNebula;
			}
			
			return float3(0, 0, 0);
		}
		
		float field(float3 p, float s, int idx) {
			float strength = 3.0 + .1 * sin(_Time.z);
			float accum = s/4.;
			float prev = 0.;
			float tw = 0.;
			float fact = pow(4, _FieldPow);
			
			
			for (int i = 0; i < idx; ++i) {
				float mag = dot(p, p);
				p = abs(p) / mag + _FieldVec.xyz;
				float w = exp(-float(i) / fact);
				accum += w * exp(-strength * pow(abs(mag - prev), 1.2));
				tw += w;
				prev = mag;
			}
			
			return max(0., 5. * accum / tw - .7);
		}
		
		void surf (Input IN, inout SurfaceOutput o) {
			resetNoise();
			half3 col = half3(0, 0, 0);
			float zoom = _Zoom / 1000;
			float2 uv = IN.uv_MainTex;
			uv -= .5;
			
			half3 dir = float3(uv * zoom, 1);
			
			float time = _Center.w + _Time.x;
			
			//Un-scale parameters (source parameters for these are mostly in 0...1 range)
			//Scaling them up makes it much easier to fine-tune shader in the inspector.
			float brightness = _Brightness / 1000;
			float stepSize = _StepSize / 1000;
			float3 tile = abs(float3(_Tile, _Tile, _Tile)) / 1000;
			float formparam = _Formuparam / 1000;
			
			float darkmatter = _Darkmatter / 100;
			float distFade = _Distfading / 100;
			
			float3 from = _Center.xyz;
			
			//scroll over time
			from += _Scroll.xyz * _Scroll.w * time;
			//scroll from camera position
			//from += _WorldSpaceCameraPos * _CamScroll * .00001;
			
			
			//Apply rotation if enabled
			float3 rot = _Rotation.xyz * _Rotation.w;
			if (length(rot) > 0) {
				float2x2 rx = float2x2(cos(rot.x), sin(rot.x), -sin(rot.x), cos(rot.x));
				float2x2 ry = float2x2(cos(rot.y), sin(rot.y), -sin(rot.y), cos(rot.y));
				float2x2 rz = float2x2(cos(rot.z), sin(rot.z), -sin(rot.z), cos(rot.z));
				
				dir.xy = mul(rz, dir.xy);
				dir.xz = mul(ry, dir.xz);
				dir.yz = mul(rx, dir.yz);
				from.xy = mul(rz, from.xy);
				from.xz = mul(ry, from.xz);
				from.yz = mul(rx, from.yz);
			}
			
			
			//volumetric rendering
			float s = 0.1, fade = 1.0;
			float3 v = float3(0, 0, 0);
			for (int r = 0; r < _Volsteps; r++) {
				float3 p = abs(from + s * dir * .5);
				
				p = abs(float3(tile - fmod(p, tile*2)));
				float pa,a = pa = 0.;
				for (int i = 0; i < _Iterations; i++) {
					p = abs(p) / dot(p, p) - formparam;
					a += abs(length(p) - pa);
					pa = length(p);
				}
				//Dark matter
				float dm = max(0., darkmatter - a * a * .001);
				if (r > 6) { fade *= 1. - dm; } // Render distant darkmatter
				a *= a * a; //add contrast
				
				v += fade;
				
				// coloring based on distance
				v += float3(s, s*s, s*s*s*s) * a * brightness * fade;
				
				// distance fading
				fade *= distFade;
				s += stepSize;
			}
			
			float len = length(v);
			//Quick color saturate
			v = lerp(float3(len, len, len), v, _Saturation / 100);
			v.xyz *= _Color.xyz * .01;
			
			v.xyz += noisedNebula(from, dir);
			
			float fieldPos = (from / (10000 + (5555 * sin(_FieldFreq * time * 6.11)) ) )
							+ (dir / (100 + (55.45 * sin(_FieldFreq * time * 2.23)) ) ); 
			fieldPos *= _FieldSize;
			
			float fv1 = field(fieldPos + dir, .7, _FieldReps);
			float div = 1.0 + (sin(_FieldFreq2 * time * 7.11) * 0.2 + sin(_FieldFreq2 * time * 2.53) * 0.3);
			fieldPos = fieldPos / float3(div, div, div) + float3(.3, -.2, .4);
			fieldPos /= _FieldSize2;
			float fv2 = field(fieldPos + dir, .7, _FieldReps2);
			
			float4 fb1 = float4(.2,.2,.2,.6) 
				* float4(.45 * fv1 * fv1 * fv1, .36 * fv1 * fv1, .7 * fv1, fv1 * .1);
			fb1.xyz = shiftHue(fb1.xyz, _FieldHue);
			
			float4 fb2 = float4(.2,.2,.2,.2) 
				* float4(1.3 * fv2 * fv2 * fv2, 1.8 * fv2 * fv2, fv2 * 0.05, fv2 * .1);
			fb2.xyz = shiftHue(fb2.xyz, _FieldHue2);
				
			v.xyz += fb1.xyz * fb1.a * _FieldBrightness + fb2.xyz * fb2.a * _FieldBrightness2;
			
			#ifdef CLAMPOUT
				v = clamp(v, float3(0,0,0), _Color.xyz);
			#else
				if (_UpperRange > 0) { 
					v = clamp(v, float3(0,0,0), float3(_UpperRange, _UpperRange, _UpperRange));
				}
			#endif
			
			float3 hsv = toHSV(v);
			hsv.r += _HueShift;
			hsv.g += _PostSat;
			v.xyz = toRGB(hsv);
			o.Emission = float3(v * 1.0);
		}
		
		ENDCG
	
	}

	Fallback Off
}

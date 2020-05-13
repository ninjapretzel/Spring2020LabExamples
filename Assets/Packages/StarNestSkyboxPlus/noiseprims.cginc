//Nvidia CG include
//Noise Primitives
//Jonathan Cohen
//2016
//Distributed under MIT license
//This content is under the MIT License.


#ifndef NOISEPRIMS_INCLUDED
#define NOISEPRIMS_INCLUDED
//LOL INCLUDE GUARDS

//Standard random generation variables
//values are from hooks into material system
						//Recommended hooks are
int _Octaves; 			//_Octaves ("NoiseOctaves", Range(1, 32)) = 5.0
float _Seed; 			//_Seed ("Seed", Float) = 31337.1337
float _Persistence;		//_Persistence ("NoisePersistence", Range(0, 1)) = .95
float _Scale;			//_Scale ("NoiseScale", Float) = 20.0

int octaves;			//Number of 'octaves' for perlin noise
float seed;				//'seed' value for generation of randomness.
float persistence;		//
float scale;			//

///Resets the octaves, seed, persistence and scale to the 'hooked' values
///(_Octaves, _Seed, _Persistence, _Scale)
///Typically, this function should be called once at the begining of procedural texture code.
void resetNoise() { 
	octaves = _Octaves;
	seed = _Seed;
	persistence = _Persistence;
	scale = _Scale;
}

///Sets the values for noise
///Order of parameters is 
///seed, scale, persistence, octaves
///if parameters are ommitted, they are not changed.
void setNoise(float sd) { seed = sd; }
void setNoise(float sd, float sc) { seed = sd; scale = sc; }
void setNoise(float sd, float sc, float per) { seed = sd; scale = sc; persistence = per; }
void setNoise(float sd, float sc, float per, int oc) { 
	seed = sd; 
	scale = sc; 
	persistence = per; 
	octaves = oc;
}

//Quick hash method 1
float hash(float n) { return frac(sin(n)*seed); }
//Quick hash method 2
float hash2(float n) { return frac(frac(n/1e4)*n-1e6); }
//Quick float2 hash
float2 rhash(float2 uv) {
	const float2x2 t = float2x2(.12121212,.13131313,-.13131313,.12121212);
	const float2 s = float2(1e4, 1e6);
	uv = mul(t, uv);
	uv *= s;
	return  frac(frac(uv/s)*uv);
}

// 1D noise
float noise1(float p) {
	float fl = floor(p);
	float fc = frac(p);
	return lerp(hash(fl), hash(fl + 1.0), fc);
}

float3 hash3(float3 p) {
    float3 q = float3(dot(p, float3(127.1, 311.7, 189.2)),
                  dot(p, float3(269.5, 183.3, 324.7)),
                  dot(p, float3(419.2, 371.9, 128.5)));
    return frac(sin(q) * seed);
}


//Quick smoothing
float2 smooth(float2 uv) { return uv*uv*(3.-2.*uv); }

///Quick, standard 3d noise.
float noise(float3 x) {
	float3 p = floor(x);
	float3 f = frac(x);
	f       = f*f*(3.0-2.0*f);
	float n = p.x + p.y*157.0 + 113.0*p.z;

	return lerp(lerp(	lerp( hash(n+0.0), hash(n+1.0),f.x),
						lerp( hash(n+157.0), hash(n+158.0),f.x),f.y),
				lerp(	lerp( hash(n+113.0), hash(n+114.0),f.x),
						lerp( hash(n+270.0), hash(n+271.0),f.x),f.y),f.z);
}

///Octave-3d noise  function
///Output is in range [0...1], but, clusters mostly between .4 and .6
///Use nnoise for values that take up the whole range.
float onoise(float3 pos) {
	float total = 0.0
		, frequency = scale
		, amplitude = 1.0
		, maxAmplitude = 0.0;
	
	for (int i = 0; i < octaves; i++) {
		total += noise(pos * frequency) * amplitude;
		frequency *= 2.0, maxAmplitude += amplitude;
		amplitude *= persistence;
	}
	
	
	return total / maxAmplitude;
}



///normalized octave noise function, output is in range [0...1]
///factor parameter controls how 'tight' the noise is.
///Default is (factor = 0.5)
float nnoise(float3 pos, float factor) {
	float total = 0.0
		, frequency = scale
		, amplitude = 1.0
		, maxAmplitude = 0.0;
	
	for (int i = 0; i < octaves; i++) {
		total += noise(pos * frequency) * amplitude;
		frequency *= 2.0, maxAmplitude += amplitude;
		amplitude *= persistence;
	}
	
	float avg = maxAmplitude * .5;
	if (factor != 0) {
	float range = avg * clamp(factor, 0, 1);
		float mmin = avg - range;
		float mmax = avg + range;
		
		float val = clamp(total, mmin, mmax);
		return val = (val - mmin) / (mmax - mmin);
	} 
	
	if (total > avg) { return 1; }
	return 0;
}
float nnoise(float3 pos) { return nnoise(pos, 0.5); }



// voronoi distance noise
float voroni2(float2 x) {
	float2 p = floor(x);
	float2 f = frac(x);
	
	float2 res = float2(1.0, 1.0);
	for (int j = -1; j <= 1; j ++) {
		for (int i = -1; i <= 1; i ++) {
			float2 b = float2(i, j);
			float2 r = float2(b) - f + rhash(p + b);
			
			float d = max(abs(r.x), abs(r.y));
			
			if(d < res.x) 		{ 	res.y = res.x;	res.x = d; }
			else if(d < res.y) 	{	res.y = d; 	}
		}
	}
	return res.y - res.x;
}

// voronoi distance noise
float voroni3(float3 x, float v) {
	float3 p = floor(x);
	float3 f = frac(x);
	
	float3 res = float3(1.0, 1.0, 1.0);
	for (int k = -2; k <= 1; k++) {
		for (int j = -2; j <= 1; j++) {
			for (int i = -2; i <= 1; i++) {
				float3 b = float3(i, j, k);
				float3 r = b - f + hash3(p + b);
				
				float d = max(max(abs(r.x), abs(r.y)), abs(r.z));
				if (d < res.x) {
					res.z = res.y;
					res.y = res.x;
					res.x = d;
				} else if (d < res.y) {
					res.z = res.y;
					res.y = d;
				} else if (d < res.z) {
					res.z = d;
				}
			}
		}
	}
	return res.y - res.x;
}
float voroni3(float3 x) { return voroni3(x, 1.0); }


float ovoroni3(float3 p, float v) {
    float total = 0.0
		, frequency = scale
		, amplitude = 1.0
		, maxAmplitude = 0;
		
    for(int i = 0; i < octaves; i++) {
        total += amplitude * voroni3(p * frequency, v);
        frequency *= 2.0, maxAmplitude += amplitude;
        amplitude *= persistence;
    }
    return total / maxAmplitude;
}
float ovoroni3(float3 p) { return ovoroni3(p, 1.0); }

float nvoroni3(float3 p, float v, float factor) {
    float total = 0.0
		, frequency = scale
		, amplitude = 1.0
		, maxAmplitude = 0;
		
    for(int i = 0; i < octaves; i++) {
        total += amplitude * voroni3(p * frequency, v);
        frequency *= 2.0, maxAmplitude += amplitude;
        amplitude *= persistence;
    }
	
	float avg = maxAmplitude * .5;
	if (factor != 0) {
		float range = avg * clamp(factor, 0, 1);
		float mmin = avg - range;
		float mmax = avg + range;
		
		float val = clamp(total, mmin, mmax);
		return val = (val - mmin) / (mmax - mmin);
	} 
	
	if (total > avg) { return 1; }
	return 0;
}
float nvoroni3(float3 pos, float v) { return nvoroni3(pos, v, 0.5); }
float nvoroni3(float3 pos) { return nvoroni3(pos, 1.0, 0.5); }



#endif




















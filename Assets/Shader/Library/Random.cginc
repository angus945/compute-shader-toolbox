#ifndef RandomIncloud
#define RandomIncloud


float rnd1To1(float seed)
{
    return frac(sin(dot(float2(seed, sin(seed)), float2(12.9898, 78.233))) * 43758.5453);
}
float2 rnd1To2(float seed)
{
    float x = rnd1To1(seed);
    float y = rnd1To1(x);
    return float2(x, y);
}
float3 rnd1To3(float seed)
{
    float x = rnd1To1(seed);
    float y = rnd1To1(x);
    float z = rnd1To1(y);
    return float3(x, y, z);
}
float rnd2to1(float2 seed)
{
    return frac(sin(dot(seed, float2(12.9898,78.233))) * 43758.5453123);
}
float2 rnd2to2(float2 uv)
{
	float vec = dot(uv, float2(127.1, 311.7));
	return -1.0 + 2.0 * frac(sin(vec) * 43758.5453123);
}

float perlinNoise(float2 uv) 
{				
	float2 pi = floor(uv);
	float2 pf = uv - pi;
	float2 w = pf * pf * (3.0 - 2.0 *  pf);

	float2 lerp1 = lerp(
		dot(rnd2to2(pi + float2(0.0, 0.0)), pf - float2(0.0, 0.0)),
		dot(rnd2to2(pi + float2(1.0, 0.0)), pf - float2(1.0, 0.0)), w.x);
                
 	float2 lerp2 = lerp(
		dot(rnd2to2(pi + float2(0.0, 1.0)), pf - float2(0.0, 1.0)),
		dot(rnd2to2(pi + float2(1.0, 1.0)), pf - float2(1.0, 1.0)), w.x);
		
	return lerp(lerp1, lerp2, w.y).x;
}


#endif




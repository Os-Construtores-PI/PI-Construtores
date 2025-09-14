Shader "Unlit/NewUnlitShader"
{
	Properties
	{
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		Pass
		{
			Blend One One
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			#include "UnityCG.cginc"
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};
#define hlsl_atan(x,y) atan2(x, y)
#define mod(x,y) ((x)-(y)*floor((x)/(y)))
inline float4 textureLod(sampler2D tex, float2 uv, float lod) {
    return tex2D(tex, uv);
}
// int vector funtions
inline int2 toint2(int x) {
    return int2(x, x);
}
inline int2 toint2(int x, int y) {
    return int2(x, y);
}
inline int3 toint3(int x) {
    return int3(x, x, x);
}
inline int3 toint3(int x, int y, int z) {
    return int3(x, y, z);
}
inline int3 toint3(int2 xy, int z) {
    return int3(xy.x, xy.y, z);
}
inline int3 toint3(int x, int2 yz) {
    return int3(x, yz.x, yz.y);
}
inline int4 toint4(int x, int y, int z, int w) {
    return int4(x, y, z, w);
}
inline int4 toint4(int x) {
    return int4(x, x, x, x);
}
inline int4 toint4(int x, int3 yzw) {
    return int4(x, yzw.x, yzw.y, yzw.z);
}
inline int4 toint4(int2 xy, int2 zw) {
    return int4(xy.x, xy.y, zw.x, zw.y);
}
inline int4 toint4(int3 xyz, int w) {
    return int4(xyz.x, xyz.y, xyz.z, w);
}
inline int4 toint4(int2 xy, int z, int w) {
    return int4(xy.x, xy.y, z, w);
}
// float vector funtions
inline float2 tofloat2(float x) {
    return float2(x, x);
}
inline float2 tofloat2(float x, float y) {
    return float2(x, y);
}
inline float3 tofloat3(float x) {
    return float3(x, x, x);
}
inline float3 tofloat3(float x, float y, float z) {
    return float3(x, y, z);
}
inline float3 tofloat3(float2 xy, float z) {
    return float3(xy.x, xy.y, z);
}
inline float3 tofloat3(float x, float2 yz) {
    return float3(x, yz.x, yz.y);
}
inline float4 tofloat4(float x, float y, float z, float w) {
    return float4(x, y, z, w);
}
inline float4 tofloat4(float x) {
    return float4(x, x, x, x);
}
inline float4 tofloat4(float x, float3 yzw) {
    return float4(x, yzw.x, yzw.y, yzw.z);
}
inline float4 tofloat4(float2 xy, float2 zw) {
    return float4(xy.x, xy.y, zw.x, zw.y);
}
inline float4 tofloat4(float3 xyz, float w) {
    return float4(xyz.x, xyz.y, xyz.z, w);
}
inline float4 tofloat4(float2 xy, float z, float w) {
    return float4(xy.x, xy.y, z, w);
}
inline float2x2 tofloat2x2(float2 v1, float2 v2) {
    return float2x2(v1.x, v1.y, v2.x, v2.y);
}
// EngineSpecificDefinitions
float dot2(float2 x) {
	return dot(x, x);
}
float rand(float2 x) {
    return frac(cos(mod(dot(x, tofloat2(13.9898, 8.141)), 3.14)) * 43758.5);
}
float2 rand2(float2 x) {
    return frac(cos(mod(tofloat2(dot(x, tofloat2(13.9898, 8.141)),
						      dot(x, tofloat2(3.4562, 17.398))), tofloat2(3.14))) * 43758.5);
}
float3 rand3(float2 x) {
    return frac(cos(mod(tofloat3(dot(x, tofloat2(13.9898, 8.141)),
							  dot(x, tofloat2(3.4562, 17.398)),
                              dot(x, tofloat2(13.254, 5.867))), tofloat3(3.14))) * 43758.5);
}
float param_rnd(float minimum, float maximum, float seed) {
	return minimum+(maximum-minimum)*rand(tofloat2(seed));
}
float param_rndi(float minimum, float maximum, float seed) {
	return floor(param_rnd(minimum, maximum + 1.0, seed));
}
float3 rgb2hsv(float3 c) {
	float4 K = tofloat4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	float4 p = c.g < c.b ? tofloat4(c.bg, K.wz) : tofloat4(c.gb, K.xy);
	float4 q = c.r < p.x ? tofloat4(p.xyw, c.r) : tofloat4(c.r, p.yzx);
	float d = q.x - min(q.w, q.y);
	float e = 1.0e-10;
	return tofloat3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}
float3 hsv2rgb(float3 c) {
	float4 K = tofloat4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
	float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
	return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}
static const float p_o934373501988_gradient_pos[2] = {  0.000000000, 1.000000000  };
static const float4 p_o934373501988_gradient_col[2] = {  tofloat4(0.000000000, 0.000000000, 0.000000000, 1.000000000), tofloat4(0.000000000, 0.000000000, 0.000000000, 1.000000000)  };
static const float p_o11085579038739_amount = 0.355000000;
static const float p_o11085579038739_eps = 0.030000000;
static const float p_o12657620953688_default_in1 = 50.000000000;
static const float p_o934356724770_default_in1 = 0.000000000;
static const float p_o933836631062_sx = 3.000000000;
static const float p_o933836631062_sy = 4.000000000;
static const float p_o933836631062_angle = 33.950000000;
static const float p_o933836631062_round = 0.000000000;
static const float p_o9886662406134_Distort = 0.580000000;
static const float seed_o10489098678253 = 0.000000000;
static const float p_o10489098678253_scale_x = 7.000000000;
static const float p_o10489098678253_scale_y = 8.000000000;
static const float p_o10489098678253_scale_z = 1.000000000;
static const float p_o10489098678253_iterations = 1.000000000;
static const float p_o10489098678253_persistence = 0.250000000;
static const float p_o933903739930_g_pos[2] = {  0.368630052, 1.000000000  };
static const float4 p_o933903739930_g_col[2] = {  tofloat4(0.514718056, 0.166625977, 0.656250000, 1.000000000), tofloat4(0.078125000, 0.078125000, 0.078125000, 1.000000000)  };
static const float p_o933886962713_s = 1.000000000;
static const float seed_o933870185496 = 0.747882366;
static const float p_o933870185496_scale_x = 1.000000000;
static const float p_o933870185496_scale_y = 0.600000000;
static const float p_o933870185496_scale_z = 1.000000000;
static const float p_o933870185496_iterations = 7.000000000;
static const float p_o933870185496_persistence = 1.000000000;
// #globals: tex3d_apply (o5014609209764)
// #globals: math_2 (o12657620953688)
float pingpong(float a, float b)
{
  return (b != 0.0) ? abs(frac((a - b) / (b * 2.0)) * b * 2.0 - b) : 0.0;
}
// #globals: cairo (o933836631062)
float cairo_round(float2 uv, float angle, float k) {
	float2 cell = floor(uv);
	float ca = cos(angle);
	float sa = sin(angle);
	float2 corner = frac(uv)-0.5;
	uv = 0.5-abs(corner);
	uv = lerp(uv, uv.yx, mod(cell.x+cell.y, 2.0));
	float side = dot(tofloat2(-sa, ca), uv);
	float d1 = abs(side);
	float d2 = abs(dot(tofloat2(-sa, ca), lerp(tofloat2(uv.x, 1.0-uv.y), tofloat2(1.0-uv.x, uv.y), step(side, 0.0))));
	float d3 = abs(dot(tofloat2(ca, sa), uv));
	float d4 = lerp(0.5-uv.x, 0.5-uv.y, step(side, 0.0));
	return clamp(-log2(exp2(-k*d1)+exp2(-k*d2)+exp2(-k*d3)+exp2(-k*d4))/k, 0.0, 1.0);
}
float4 cairo_bbox(float2 uv, float angle) {
	float2 cell = floor(uv);
	float cell_type = mod(cell.x+cell.y, 2.0);
	float ca = cos(angle);
	float sa = sin(angle);
	float l = 0.0;
	float r = 1.0;
	float b = 0.0;
	float t = 1.0;
	float2 corner = frac(uv)-0.5;
	uv = 0.5-abs(corner);
	uv = lerp(uv, uv.yx, cell_type);
	float side = dot(tofloat2(-sa, ca), uv);
	float ta = tan(angle);
	float c = min(0.5, 0.5/ta);
	float s = min(0.5, 0.5*ta);
	if (cell_type > 0.5) {
		if (side > 0.0) {
			if (corner.y > 0.0) {
				t = 1.0+s;
				b = 1.0-c;
			} else {
				t = c;
				b = -s;
			}
		} else {
			if (corner.x > 0.0) {
				l = 1.0-s;
				r = 1.0+c;
			} else {
				l = -c;
				r = s;
			}
		}
	} else {
		if (side > 0.0) {
			if (corner.x > 0.0) {
				l = 1.0-c;
				r = 1.0+s;
			} else {
				l = -s;
				r = c;
			}
		} else {
			if (corner.y > 0.0) {
				t = 1.0+c;
				b = 1.0-s;
			} else {
				t = s;
				b = -c;
			}
		}
	}
	float d1 = abs(side);
	float d3 = abs(dot(tofloat2(ca, sa), uv));
	float d4 = lerp(0.5-uv.x, 0.5-uv.y, step(side, 0.0));
	return tofloat4(cell.x+l, cell.y+b, r-l, t-b);
}
// #globals: tex3d_fbm_2 (o10489098678253)
float rand31(float3 p) {
	return frac(sin(dot(p,tofloat3(127.1,311.7, 74.7)))*43758.5453123);
}
float3 rand33(float3 p){
	p = tofloat3( dot(p,tofloat3(127.1,311.7, 74.7)),
			  dot(p,tofloat3(269.5,183.3,246.1)),
			  dot(p,tofloat3(113.5,271.9,124.6)));
	return -1.0 + 2.0*frac(sin(p)*43758.5453123);
}
float tex3d_fbm_value(float3 coord, float3 size, float seed) {
	float3 o = floor(coord)+rand3(tofloat2(seed, 1.0-seed))+size;
	float3 f = frac(coord);
	float p000 = rand31(mod(o, size));
	float p001 = rand31(mod(o + tofloat3(0.0, 0.0, 1.0), size));
	float p010 = rand31(mod(o + tofloat3(0.0, 1.0, 0.0), size));
	float p011 = rand31(mod(o + tofloat3(0.0, 1.0, 1.0), size));
	float p100 = rand31(mod(o + tofloat3(1.0, 0.0, 0.0), size));
	float p101 = rand31(mod(o + tofloat3(1.0, 0.0, 1.0), size));
	float p110 = rand31(mod(o + tofloat3(1.0, 1.0, 0.0), size));
	float p111 = rand31(mod(o + tofloat3(1.0, 1.0, 1.0), size));
	float3 t =  f * f * f * (f * (f * 6.0 - 15.0) + 10.0);
	return lerp(lerp(lerp(p000, p100, t.x), lerp(p010, p110, t.x), t.y), lerp(lerp(p001, p101, t.x), lerp(p011, p111, t.x), t.y), t.z);
}
float fbm3d_value(float3 coord, float3 size, int octaves, float persistence, float seed) {
	float normalize_factor = 0.0;
	float value = 0.0;
	float scale = 1.0;
	for (int i = 0; i < octaves; i++) {
		value += tex3d_fbm_value(coord*size, size, seed) * scale;
		normalize_factor += scale;
		size *= 2.0;
		scale *= persistence;
	}
	return value / normalize_factor;
}
float tex3d_fbm_value_nowrap(float3 coord, float3 size, float seed) {
	float3 o = floor(coord)+rand3(tofloat2(seed, 1.0-seed))+size;
	float3 f = frac(coord);
	float p000 = rand31(o);
	float p001 = rand31(o + tofloat3(0.0, 0.0, 1.0));
	float p010 = rand31(o + tofloat3(0.0, 1.0, 0.0));
	float p011 = rand31(o + tofloat3(0.0, 1.0, 1.0));
	float p100 = rand31(o + tofloat3(1.0, 0.0, 0.0));
	float p101 = rand31(o + tofloat3(1.0, 0.0, 1.0));
	float p110 = rand31(o + tofloat3(1.0, 1.0, 0.0));
	float p111 = rand31(o + tofloat3(1.0, 1.0, 1.0));
	float3 t =  f * f * f * (f * (f * 6.0 - 15.0) + 10.0);
	return lerp(lerp(lerp(p000, p100, t.x), lerp(p010, p110, t.x), t.y), lerp(lerp(p001, p101, t.x), lerp(p011, p111, t.x), t.y), t.z);
}
float fbm3d_value_nowrap(float3 coord, float3 size, int octaves, float persistence, float seed) {
	float normalize_factor = 0.0;
	float value = 0.0;
	float scale = 1.0;
	for (int i = 0; i < octaves; i++) {
		value += tex3d_fbm_value_nowrap(coord*size, size, seed) * scale;
		normalize_factor += scale;
		size *= 2.0;
		scale *= persistence;
	}
	return value / normalize_factor;
}
float tex3d_fbm_perlin(float3 coord, float3 size, float seed) {
	float3 o = floor(coord)+rand3(tofloat2(seed, 1.0-seed))+size;
	float3 f = frac(coord);
	float3 v000 = normalize(rand33(mod(o, size))-tofloat3(0.5));
	float3 v001 = normalize(rand33(mod(o + tofloat3(0.0, 0.0, 1.0), size))-tofloat3(0.5));
	float3 v010 = normalize(rand33(mod(o + tofloat3(0.0, 1.0, 0.0), size))-tofloat3(0.5));
	float3 v011 = normalize(rand33(mod(o + tofloat3(0.0, 1.0, 1.0), size))-tofloat3(0.5));
	float3 v100 = normalize(rand33(mod(o + tofloat3(1.0, 0.0, 0.0), size))-tofloat3(0.5));
	float3 v101 = normalize(rand33(mod(o + tofloat3(1.0, 0.0, 1.0), size))-tofloat3(0.5));
	float3 v110 = normalize(rand33(mod(o + tofloat3(1.0, 1.0, 0.0), size))-tofloat3(0.5));
	float3 v111 = normalize(rand33(mod(o + tofloat3(1.0, 1.0, 1.0), size))-tofloat3(0.5));
	float p000 = dot(v000, f);
	float p001 = dot(v001, f - tofloat3(0.0, 0.0, 1.0));
	float p010 = dot(v010, f - tofloat3(0.0, 1.0, 0.0));
	float p011 = dot(v011, f - tofloat3(0.0, 1.0, 1.0));
	float p100 = dot(v100, f - tofloat3(1.0, 0.0, 0.0));
	float p101 = dot(v101, f - tofloat3(1.0, 0.0, 1.0));
	float p110 = dot(v110, f - tofloat3(1.0, 1.0, 0.0));
	float p111 = dot(v111, f - tofloat3(1.0, 1.0, 1.0));
	float3 t =  f * f * f * (f * (f * 6.0 - 15.0) + 10.0);
	return 0.5 + lerp(lerp(lerp(p000, p100, t.x), lerp(p010, p110, t.x), t.y), lerp(lerp(p001, p101, t.x), lerp(p011, p111, t.x), t.y), t.z);
}
float fbm3d_perlin(float3 coord, float3 size, int octaves, float persistence, float seed) {
	float normalize_factor = 0.0;
	float value = 0.0;
	float scale = 1.0;
	for (int i = 0; i < octaves; i++) {
		value += tex3d_fbm_perlin(coord*size, size, seed) * scale;
		normalize_factor += scale;
		size *= 2.0;
		scale *= persistence;
	}
	return value / normalize_factor;
}
float tex3d_fbm_perlin_nowrap(float3 coord, float3 size, float seed) {
	float3 o = floor(coord)+rand3(tofloat2(seed, 1.0-seed))+size;
	float3 f = frac(coord);
	float3 v000 = normalize(rand33(o)-tofloat3(0.5));
	float3 v001 = normalize(rand33(o + tofloat3(0.0, 0.0, 1.0))-tofloat3(0.5));
	float3 v010 = normalize(rand33(o + tofloat3(0.0, 1.0, 0.0))-tofloat3(0.5));
	float3 v011 = normalize(rand33(o + tofloat3(0.0, 1.0, 1.0))-tofloat3(0.5));
	float3 v100 = normalize(rand33(o + tofloat3(1.0, 0.0, 0.0))-tofloat3(0.5));
	float3 v101 = normalize(rand33(o + tofloat3(1.0, 0.0, 1.0))-tofloat3(0.5));
	float3 v110 = normalize(rand33(o + tofloat3(1.0, 1.0, 0.0))-tofloat3(0.5));
	float3 v111 = normalize(rand33(o + tofloat3(1.0, 1.0, 1.0))-tofloat3(0.5));
	float p000 = dot(v000, f);
	float p001 = dot(v001, f - tofloat3(0.0, 0.0, 1.0));
	float p010 = dot(v010, f - tofloat3(0.0, 1.0, 0.0));
	float p011 = dot(v011, f - tofloat3(0.0, 1.0, 1.0));
	float p100 = dot(v100, f - tofloat3(1.0, 0.0, 0.0));
	float p101 = dot(v101, f - tofloat3(1.0, 0.0, 1.0));
	float p110 = dot(v110, f - tofloat3(1.0, 1.0, 0.0));
	float p111 = dot(v111, f - tofloat3(1.0, 1.0, 1.0));
	float3 t =  f * f * f * (f * (f * 6.0 - 15.0) + 10.0);
	return 0.5 + lerp(lerp(lerp(p000, p100, t.x), lerp(p010, p110, t.x), t.y), lerp(lerp(p001, p101, t.x), lerp(p011, p111, t.x), t.y), t.z);
}
float fbm3d_perlin_nowrap(float3 coord, float3 size, int octaves, float persistence, float seed) {
	float normalize_factor = 0.0;
	float value = 0.0;
	float scale = 1.0;
	for (int i = 0; i < octaves; i++) {
		value += tex3d_fbm_perlin_nowrap(coord*size, size, seed) * scale;
		normalize_factor += scale;
		size *= 2.0;
		scale *= persistence;
	}
	return value / normalize_factor;
}
float tex3d_fbm_cellular(float3 coord, float3 size, float seed) {
	float3 o = floor(coord)+rand3(tofloat2(seed, 1.0-seed))+size;
	float3 f = frac(coord);
	float min_dist = 3.0;
	for (float x = -1.0; x <= 1.0; x++) {
		for (float y = -1.0; y <= 1.0; y++) {
			for (float z = -1.0; z <= 1.0; z++) {
				float3 node = 0.4*rand33(mod(o + tofloat3(x, y, z), size)) + tofloat3(x, y, z);
				float dist = sqrt((f - node).x * (f - node).x + (f - node).y * (f - node).y + (f - node).z * (f - node).z);
				min_dist = min(min_dist, dist);
			}
		}
	}
	return min_dist;
}
float fbm3d_cellular(float3 coord, float3 size, int octaves, float persistence, float seed) {
	float normalize_factor = 0.0;
	float value = 0.0;
	float scale = 1.0;
	for (int i = 0; i < octaves; i++) {
		value += tex3d_fbm_cellular(coord*size, size, seed) * scale;
		normalize_factor += scale;
		size *= 2.0;
		scale *= persistence;
	}
	return value / normalize_factor;
}
float tex3d_fbm_cellular_nowrap(float3 coord, float3 size, float seed) {
	float3 o = floor(coord)+rand3(tofloat2(seed, 1.0-seed))+size;
	float3 f = frac(coord);
	float min_dist = 3.0;
	for (float x = -1.0; x <= 1.0; x++) {
		for (float y = -1.0; y <= 1.0; y++) {
			for (float z = -1.0; z <= 1.0; z++) {
				float3 node = 0.4*rand33(o + tofloat3(x, y, z)) + tofloat3(x, y, z);
				float dist = sqrt((f - node).x * (f - node).x + (f - node).y * (f - node).y + (f - node).z * (f - node).z);
				min_dist = min(min_dist, dist);
			}
		}
	}
	return min_dist;
}
float fbm3d_cellular_nowrap(float3 coord, float3 size, int octaves, float persistence, float seed) {
	float normalize_factor = 0.0;
	float value = 0.0;
	float scale = 1.0;
	for (int i = 0; i < octaves; i++) {
		value += tex3d_fbm_cellular_nowrap(coord*size, size, seed) * scale;
		normalize_factor += scale;
		size *= 2.0;
		scale *= persistence;
	}
	return value / normalize_factor;
}
float4 o934373501988_gradient_gradient_fct(float x) {
  if (x < p_o934373501988_gradient_pos[0]) {
    return p_o934373501988_gradient_col[0];
  } else if (x < p_o934373501988_gradient_pos[1]) {
    return lerp(p_o934373501988_gradient_col[0], p_o934373501988_gradient_col[1], ((x-p_o934373501988_gradient_pos[0])/(p_o934373501988_gradient_pos[1]-p_o934373501988_gradient_pos[0])));
  }
  return p_o934373501988_gradient_col[1];
}
float o11085579038739_input_d(float2 uv, float _seed_variation_) {
// #code: math_2 (o12657620953688)
float o12657620953688_0_clamp_false = p_o12657620953688_default_in1*(sin(_Time.y));
float o12657620953688_0_clamp_true = clamp(o12657620953688_0_clamp_false, 0.0, 1.0);
// #output0: math_2 (o12657620953688)
float o12657620953688_0_1_f = o12657620953688_0_clamp_false;
return o12657620953688_0_1_f;
}
// #instance: warp (o11085579038739)
float2 o11085579038739_slope(float2 uv, float epsilon, float _seed_variation_) {
	return tofloat2(o11085579038739_input_d((frac(uv+tofloat2(epsilon, 0.0))), _seed_variation_)-o11085579038739_input_d((frac(uv-tofloat2(epsilon, 0.0))), _seed_variation_), o11085579038739_input_d((frac(uv+tofloat2(0.0, epsilon))), _seed_variation_)-o11085579038739_input_d((frac(uv-tofloat2(0.0, epsilon))), _seed_variation_));
}
float4 o933903739930_g_gradient_fct(float x) {
  if (x < p_o933903739930_g_pos[0]) {
    return p_o933903739930_g_col[0];
  } else if (x < p_o933903739930_g_pos[1]) {
    return lerp(p_o933903739930_g_col[0], p_o933903739930_g_col[1], ((x-p_o933903739930_g_pos[0])/(p_o933903739930_g_pos[1]-p_o933903739930_g_pos[0])));
  }
  return p_o933903739930_g_col[1];
}
		
			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			fixed4 frag (v2f i) : SV_Target {
				float _seed_variation_ = 0.0;
				float2 uv = i.uv;

// #output0: colorize_2 (o934373501988)
float4 o934373501988_0_1_rgba = o934373501988_gradient_gradient_fct((uv).x);

// #code: warp (o11085579038739)
float2 o11085579038739_0_slope = o11085579038739_slope((uv), p_o11085579038739_eps, _seed_variation_);
float2 o11085579038739_0_warp = o11085579038739_0_slope*(1.0-o11085579038739_input_d((uv), _seed_variation_));
// #output0: cairo (o933836631062)
float o933836631062_0_1_f = cairo_round(((uv)+p_o11085579038739_amount*o11085579038739_0_warp)*tofloat2(p_o933836631062_sx, p_o933836631062_sy), p_o933836631062_angle*0.01745329251, 200.0-190.0*p_o933836631062_round);

// #code: math (o934356724770)
float o934356724770_0_clamp_false = o933836631062_0_1_f+(_Time.y * .03);
float o934356724770_0_clamp_true = clamp(o934356724770_0_clamp_false, 0.0, 1.0);
// #output0: math (o934356724770)
float o934356724770_0_1_f = o934356724770_0_clamp_false;

// #output0: warp (o11085579038739)
float4 o11085579038739_0_1_rgba = tofloat4(tofloat3(o934356724770_0_1_f), 1.0);

// #output0: tex3d_fbm_2 (o10489098678253)
float o10489098678253_0_1_tex3d_gs = fbm3d_cellular((tofloat4((uv)-tofloat2(0.5), (dot((o11085579038739_0_1_rgba).rgb, tofloat3(1.0))/3.0), (dot((o934373501988_0_1_rgba).rgb, tofloat3(1.0))/3.0))).xyz, tofloat3(p_o10489098678253_scale_x, p_o10489098678253_scale_y, p_o10489098678253_scale_z), int(p_o10489098678253_iterations), p_o10489098678253_persistence, float((seed_o10489098678253+frac(_seed_variation_))));

// #output0: tex3d_fbm_4 (o933870185496)
float o933870185496_0_1_tex3d_gs = fbm3d_value((tofloat4((tofloat4((tofloat4((uv)-tofloat2(0.5), (dot((o11085579038739_0_1_rgba).rgb, tofloat3(1.0))/3.0), (dot((o934373501988_0_1_rgba).rgb, tofloat3(1.0))/3.0))).xyz+(tofloat3(o10489098678253_0_1_tex3d_gs)*p_o9886662406134_Distort*0.5-0.5), 0.0)).xyz/max(p_o933886962713_s, 0.00001), (tofloat4((tofloat4((uv)-tofloat2(0.5), (dot((o11085579038739_0_1_rgba).rgb, tofloat3(1.0))/3.0), (dot((o934373501988_0_1_rgba).rgb, tofloat3(1.0))/3.0))).xyz+(tofloat3(o10489098678253_0_1_tex3d_gs)*p_o9886662406134_Distort*0.5-0.5), 0.0)).w)).xyz, tofloat3(p_o933870185496_scale_x, p_o933870185496_scale_y, p_o933870185496_scale_z), int(p_o933870185496_iterations), p_o933870185496_persistence, float((seed_o933870185496+frac(_seed_variation_))));

// #output0: tex3d_scale (o933886962713)
float3 o933886962713_0_1_tex3d = tofloat3(o933870185496_0_1_tex3d_gs);

// #output0: tex3d_colorize_3 (o933903739930)
float3 o933903739930_0_1_tex3d = o933903739930_g_gradient_fct(dot(o933886962713_0_1_tex3d, tofloat3(1.0))/3.0).rgb;

// #output0: tex3d_distort_2 (o9886662406134)
float3 o9886662406134_0_1_tex3d = o933903739930_0_1_tex3d;

// #output0: tex3d_apply (o5014609209764)
float3 o5014609209764_0_1_rgb = o9886662406134_0_1_tex3d;

				// sample the generated texture
				fixed4 col = tofloat4(o5014609209764_0_1_rgb, 1.0);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}




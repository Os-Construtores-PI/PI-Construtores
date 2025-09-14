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
static const float p_o187076813269750_amount1 = 1.000000000;
static const float p_o200068116260987_cx = -0.152085662;
static const float p_o200068116260987_cy = -0.119414598;
static const float p_o200068116260987_scale_x = 1.339626789;
static const float p_o200068116260987_scale_y = 1.456968904;
static const float p_o188887964068934_gradient_pos[2] = {  0.000000000, 1.000000000  };
static const float4 p_o188887964068934_gradient_col[2] = {  tofloat4(0.505882382, 0.000000000, 0.450980395, 1.000000000), tofloat4(0.666666687, 0.000000000, 0.549019635, 0.690196097)  };
static const float p_o186275046893320_steps = 2.000000000;
static const float seed_o150551505415811 = 0.000000000;
static const float p_o150551505415811_scale_x = 4.000000000;
static const float p_o150551505415811_scale_y = 4.000000000;
static const float p_o150551505415811_folds = 0.000000000;
static const float p_o150551505415811_iterations = 5.000000000;
static const float p_o150551505415811_persistence = 0.000000000;
static const float p_o150551505415811_offset = 0.000000000;
static const float p_o51591902145254_default_in2 = 15.000000000;
static const float p_o194488265880262_gradient_pos[2] = {  0.000000000, 0.055952638  };
static const float4 p_o194488265880262_gradient_col[2] = {  tofloat4(0.000000000, 0.000000000, 0.000000000, 0.858823538), tofloat4(0.617187500, 0.617187500, 0.617187500, 1.000000000)  };
static const float p_o196261550499600_cx = -0.277478158;
static const float p_o196261550499600_cy = -0.229239285;
static const float p_o196261550499600_scale_x = 1.000000000;
static const float p_o196261550499600_scale_y = 1.000000000;
static const float p_o185735189638695_steps = 3.000000000;
static const float seed_o149238621155305 = 0.000000000;
static const float p_o149238621155305_scale_x = 3.000000000;
static const float p_o149238621155305_scale_y = 5.000000000;
static const float p_o149238621155305_folds = -1.000000000;
static const float p_o149238621155305_iterations = 5.000000000;
static const float p_o149238621155305_persistence = 0.040000000;
static const float p_o149238621155305_offset = 0.000000000;
static const float p_o51062362875199_default_in2 = 5.000000000;
// #globals: blend
float3 blend_normal(float2 uv, float3 c1, float3 c2, float opacity) {
	return opacity*c1 + (1.0-opacity)*c2;
}
float3 blend_dissolve(float2 uv, float3 c1, float3 c2, float opacity) {
	if (rand(uv) < opacity) {
		return c1;
	} else {
		return c2;
	}
}
float3 blend_multiply(float2 uv, float3 c1, float3 c2, float opacity) {
	return opacity*c1*c2 + (1.0-opacity)*c2;
}
float3 blend_screen(float2 uv, float3 c1, float3 c2, float opacity) {
	return opacity*(1.0-(1.0-c1)*(1.0-c2)) + (1.0-opacity)*c2;
}
float blend_overlay_f(float c1, float c2) {
	return (c1 < 0.5) ? (2.0*c1*c2) : (1.0-2.0*(1.0-c1)*(1.0-c2));
}
float3 blend_overlay(float2 uv, float3 c1, float3 c2, float opacity) {
	return opacity*tofloat3(blend_overlay_f(c1.x, c2.x), blend_overlay_f(c1.y, c2.y), blend_overlay_f(c1.z, c2.z)) + (1.0-opacity)*c2;
}
float3 blend_hard_light(float2 uv, float3 c1, float3 c2, float opacity) {
	return opacity*0.5*(c1*c2+blend_overlay(uv, c1, c2, 1.0)) + (1.0-opacity)*c2;
}
float blend_soft_light_f(float c1, float c2) {
	return (c2 < 0.5) ? (2.0*c1*c2+c1*c1*(1.0-2.0*c2)) : 2.0*c1*(1.0-c2)+sqrt(c1)*(2.0*c2-1.0);
}
float3 blend_soft_light(float2 uv, float3 c1, float3 c2, float opacity) {
	return opacity*tofloat3(blend_soft_light_f(c1.x, c2.x), blend_soft_light_f(c1.y, c2.y), blend_soft_light_f(c1.z, c2.z)) + (1.0-opacity)*c2;
}
float blend_burn_f(float c1, float c2) {
	return (c1==0.0)?c1:max((1.0-((1.0-c2)/c1)),0.0);
}
float3 blend_burn(float2 uv, float3 c1, float3 c2, float opacity) {
	return opacity*tofloat3(blend_burn_f(c1.x, c2.x), blend_burn_f(c1.y, c2.y), blend_burn_f(c1.z, c2.z)) + (1.0-opacity)*c2;
}
float blend_dodge_f(float c1, float c2) {
	return (c1==1.0)?c1:min(c2/(1.0-c1),1.0);
}
float3 blend_dodge(float2 uv, float3 c1, float3 c2, float opacity) {
	return opacity*tofloat3(blend_dodge_f(c1.x, c2.x), blend_dodge_f(c1.y, c2.y), blend_dodge_f(c1.z, c2.z)) + (1.0-opacity)*c2;
}
float3 blend_lighten(float2 uv, float3 c1, float3 c2, float opacity) {
	return opacity*max(c1, c2) + (1.0-opacity)*c2;
}
float3 blend_darken(float2 uv, float3 c1, float3 c2, float opacity) {
	return opacity*min(c1, c2) + (1.0-opacity)*c2;
}
float3 blend_difference(float2 uv, float3 c1, float3 c2, float opacity) {
	return opacity*clamp(c2-c1, tofloat3(0.0), tofloat3(1.0)) + (1.0-opacity)*c2;
}
float3 blend_additive(float2 uv, float3 c1, float3 c2, float oppacity) {
	return c2 + c1 * oppacity;
}
float3 blend_addsub(float2 uv, float3 c1, float3 c2, float oppacity) {
	return c2 + (c1 - .5) * 2.0 * oppacity;
}
// #globals: adjust_hsv
float3 rgb_to_hsv(float3 c) {
	float4 K = tofloat4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	float4 p = c.g < c.b ? tofloat4(c.bg, K.wz) : tofloat4(c.gb, K.xy);
	float4 q = c.r < p.x ? tofloat4(p.xyw, c.r) : tofloat4(c.r, p.yzx);
	float d = q.x - min(q.w, q.y);
	float e = 1.0e-10;
	return tofloat3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}
float3 hsv_to_rgb(float3 c) {
	float4 K = tofloat4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
	float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
	return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}
// #globals: blend2 (o187076813269750)
float blend_linear_light_f(float c1, float c2) {
	return (c1 + 2.0 * c2) - 1.0;
}
float3 blend_linear_light(float2 uv, float3 c1, float3 c2, float opacity) {
return opacity*tofloat3(blend_linear_light_f(c1.x, c2.x), blend_linear_light_f(c1.y, c2.y), blend_linear_light_f(c1.z, c2.z)) + (1.0-opacity)*c2;
}
float blend_vivid_light_f(float c1, float c2) {
	return (c1 < 0.5) ? 1.0 - (1.0 - c2) / (2.0 * c1) : c2 / (2.0 * (1.0 - c1));
}
float3 blend_vivid_light(float2 uv, float3 c1, float3 c2, float opacity) {
	return opacity*tofloat3(blend_vivid_light_f(c1.x, c2.x), blend_vivid_light_f(c1.y, c2.y), blend_vivid_light_f(c1.z, c2.z)) + (1.0-opacity)*c2;
}
float blend_pin_light_f( float c1, float c2) {
	return (2.0 * c1 - 1.0 > c2) ? 2.0 * c1 - 1.0 : ((c1 < 0.5 * c2) ? 2.0 * c1 : c2);
}
float3 blend_pin_light(float2 uv, float3 c1, float3 c2, float opacity) {
	return opacity*tofloat3(blend_pin_light_f(c1.x, c2.x), blend_pin_light_f(c1.y, c2.y), blend_pin_light_f(c1.z, c2.z)) + (1.0-opacity)*c2;
}
float blend_hard_lerp_f(float c1, float c2) {
	return floor(c1 + c2);
}
float3 blend_hard_lerp(float2 uv, float3 c1, float3 c2, float opacity) {
		return opacity*tofloat3(blend_hard_lerp_f(c1.x, c2.x), blend_hard_lerp_f(c1.y, c2.y), blend_hard_lerp_f(c1.z, c2.z)) + (1.0-opacity)*c2;
}
float blend_exclusion_f(float c1, float c2) {
	return c1 + c2 - 2.0 * c1 * c2;
}
float3 blend_exclusion(float2 uv, float3 c1, float3 c2, float opacity) {
	return opacity*tofloat3(blend_exclusion_f(c1.x, c2.x), blend_exclusion_f(c1.y, c2.y), blend_exclusion_f(c1.z, c2.z)) + (1.0-opacity)*c2;
}
float3 blend_hue(float2 uv, float3 c1, float3 c2, float opacity) {
	float3 outcol = c2;
	float3 hsv, hsv2, tmp;
	hsv2 = rgb_to_hsv(c1);
	if (hsv2.y != 0.0) {
		hsv = rgb_to_hsv(outcol);
		hsv.x = hsv2.x;
		tmp = hsv_to_rgb(hsv);
		outcol = lerp(outcol, tmp, opacity);
	}
	return outcol;
}
float3 blend_saturation(float2 uv, float3 c1, float3 c2, float opacity) {
	float facm = 1.0 - opacity;
	float3 outcol = c2;
	float3 hsv, hsv2;
	hsv = rgb_to_hsv(outcol);
	if (hsv.y != 0.0) {
		hsv2 = rgb_to_hsv(c1);
		hsv.y = facm * hsv.y + opacity * hsv2.y;
		outcol = hsv_to_rgb(hsv);
	}
	return outcol;
}
float3 blend_color(float2 uv, float3 c1, float3 c2, float opacity) {
	float facm = 1.0 - opacity;
	float3 outcol = c2;
	float3 hsv, hsv2, tmp;
	hsv2 = rgb_to_hsv(c1);
	if (hsv2.y != 0.0) {
		hsv = rgb_to_hsv(outcol);
		hsv.x = hsv2.x;
		hsv.y = hsv2.y;
		tmp = hsv_to_rgb(hsv);
		outcol = lerp(outcol, tmp, opacity);
	}
	return outcol;
}
float3 blend_value(float2 uv, float3 c1, float3 c2, float opacity) {
	float facm = 1.0 - opacity;
	float3 hsv, hsv2;
	hsv = rgb_to_hsv(c2);
	hsv2 = rgb_to_hsv(c1);
	hsv.z = facm * hsv.z + opacity * hsv2.z;
	return hsv_to_rgb(hsv);
}
// #globals: scale_2 (o200068116260987)
float2 scale(float2 uv, float2 center, float2 scale) {
	uv -= center;
	uv /= scale;
	uv += center;
	return uv;
}
// #globals: colorize (o188887964068934)
// #globals: fbm3_2 (o150551505415811)
float value_noise_2d(float2 coord, float2 size, float offset, float seed) {
	float2 o = floor(coord)+rand2(tofloat2(seed, 1.0-seed))+size;
	float2 f = frac(coord);
	float p00 = rand(mod(o, size));
	float p01 = rand(mod(o + tofloat2(0.0, 1.0), size));
	float p10 = rand(mod(o + tofloat2(1.0, 0.0), size));
	float p11 = rand(mod(o + tofloat2(1.0, 1.0), size));
	p00 = sin(p00 * 6.28318530718 + offset * 6.28318530718) / 2.0 + 0.5;
	p01 = sin(p01 * 6.28318530718 + offset * 6.28318530718) / 2.0 + 0.5;
	p10 = sin(p10 * 6.28318530718 + offset * 6.28318530718) / 2.0 + 0.5;
	p11 = sin(p11 * 6.28318530718 + offset * 6.28318530718) / 2.0 + 0.5;
	float2 t =  f * f * f * (f * (f * 6.0 - 15.0) + 10.0);
	return lerp(lerp(p00, p10, t.x), lerp(p01, p11, t.x), t.y);
}
float fbm_2d_value(float2 coord, float2 size, int folds, int octaves, float persistence, float offset, float seed) {
	float normalize_factor = 0.0;
	float value = 0.0;
	float scale = 1.0;
	for (int i = 0; i < octaves; i++) {
		float noise = value_noise_2d(coord*size, size, offset, seed+float(i));
		for (int f = 0; f < folds; ++f) {
			noise = abs(2.0*noise-1.0);
		}
		value += noise * scale;
		normalize_factor += scale;
		size *= 2.0;
		scale *= persistence;
	}
	return value / normalize_factor;
}
float perlin_noise_2d(float2 coord, float2 size, float offset, float seed) {
	float2 o = floor(coord)+rand2(tofloat2(seed, 1.0-seed))+size;
	float2 f = frac(coord);
	float a00 = rand(mod(o, size)) * 6.28318530718 + offset * 6.28318530718;
	float a01 = rand(mod(o + tofloat2(0.0, 1.0), size)) * 6.28318530718 + offset * 6.28318530718;
	float a10 = rand(mod(o + tofloat2(1.0, 0.0), size)) * 6.28318530718 + offset * 6.28318530718;
	float a11 = rand(mod(o + tofloat2(1.0, 1.0), size)) * 6.28318530718 + offset * 6.28318530718;
	float2 v00 = tofloat2(cos(a00), sin(a00));
	float2 v01 = tofloat2(cos(a01), sin(a01));
	float2 v10 = tofloat2(cos(a10), sin(a10));
	float2 v11 = tofloat2(cos(a11), sin(a11));
	float p00 = dot(v00, f);
	float p01 = dot(v01, f - tofloat2(0.0, 1.0));
	float p10 = dot(v10, f - tofloat2(1.0, 0.0));
	float p11 = dot(v11, f - tofloat2(1.0, 1.0));
	float2 t =  f * f * f * (f * (f * 6.0 - 15.0) + 10.0);
	return 0.5 + lerp(lerp(p00, p10, t.x), lerp(p01, p11, t.x), t.y);
}
float fbm_2d_perlin(float2 coord, float2 size, int folds, int octaves, float persistence, float offset, float seed) {
	float normalize_factor = 0.0;
	float value = 0.0;
	float scale = 1.0;
	for (int i = 0; i < octaves; i++) {
		float noise = perlin_noise_2d(coord*size, size, offset, seed+float(i));
		for (int f = 0; f < folds; ++f) {
			noise = abs(2.0*noise-1.0);
		}
		value += noise * scale;
		normalize_factor += scale;
		size *= 2.0;
		scale *= persistence;
	}
	return value / normalize_factor;
}
float perlinabs_noise_2d(float2 coord, float2 size, float offset, float seed) {
	return abs(2.0*perlin_noise_2d(coord, size, offset, seed)-1.0);
}
float fbm_2d_perlinabs(float2 coord, float2 size, int folds, int octaves, float persistence, float offset, float seed) {
	float normalize_factor = 0.0;
	float value = 0.0;
	float scale = 1.0;
	for (int i = 0; i < octaves; i++) {
		float noise = perlinabs_noise_2d(coord*size, size, offset, seed+float(i));
		for (int f = 0; f < folds; ++f) {
			noise = abs(2.0*noise-1.0);
		}
		value += noise * scale;
		normalize_factor += scale;
		size *= 2.0;
		scale *= persistence;
	}
	return value / normalize_factor;
}
float fbm_2d_mod289(float x) {
	return x - floor(x * (1.0 / 289.0)) * 289.0;
}
float fbm_2d_permute(float x) {
	return fbm_2d_mod289(((x * 34.0) + 1.0) * x);
}
float2 fbm_2d_rgrad2(float2 p, float rot, float seed) {
	float u = fbm_2d_permute(fbm_2d_permute(p.x) + p.y) * 0.0243902439 + rot; // Rotate by shift
	u = frac(u+seed) * 6.28318530718; // 2*pi
	return tofloat2(cos(u), sin(u));
}
float simplex_noise_2d(float2 coord, float2 size, float offset, float seed) {
	coord *= 2.0; // needed for it to tile
	coord += rand2(tofloat2(seed, 1.0-seed)) + size;
	size *= 2.0; // needed for it to tile
	coord.y += 0.001;
	float2 uv = tofloat2(coord.x + coord.y*0.5, coord.y);
	float2 i0 = floor(uv);
	float2 f0 = frac(uv);
	float2 i1 = (f0.x > f0.y) ? tofloat2(1.0, 0.0) : tofloat2(0.0, 1.0);
	float2 p0 = tofloat2(i0.x - i0.y * 0.5, i0.y);
	float2 p1 = tofloat2(p0.x + i1.x - i1.y * 0.5, p0.y + i1.y);
	float2 p2 = tofloat2(p0.x + 0.5, p0.y + 1.0);
	i1 = i0 + i1;
	float2 i2 = i0 + tofloat2(1.0, 1.0);
	float2 d0 = coord - p0;
	float2 d1 = coord - p1;
	float2 d2 = coord - p2;
	float3 xw = mod(tofloat3(p0.x, p1.x, p2.x), size.x);
	float3 yw = mod(tofloat3(p0.y, p1.y, p2.y), size.y);
	float3 iuw = xw + 0.5 * yw;
	float3 ivw = yw;
	float2 g0 = fbm_2d_rgrad2(tofloat2(iuw.x, ivw.x), offset, seed);
	float2 g1 = fbm_2d_rgrad2(tofloat2(iuw.y, ivw.y), offset, seed);
	float2 g2 = fbm_2d_rgrad2(tofloat2(iuw.z, ivw.z), offset, seed);
	float3 w = tofloat3(dot(g0, d0), dot(g1, d1), dot(g2, d2));
	float3 t = 0.8 - tofloat3(dot(d0, d0), dot(d1, d1), dot(d2, d2));
	t = max(t, tofloat3(0.0));
	float3 t2 = t * t;
	float3 t4 = t2 * t2;
	float n = dot(t4, w);
	return 0.5 + 5.5 * n;
}
float fbm_2d_simplex(float2 coord, float2 size, int folds, int octaves, float persistence, float offset, float seed) {
	float normalize_factor = 0.0;
	float value = 0.0;
	float scale = 1.0;
	for (int i = 0; i < octaves; i++) {
		float noise = simplex_noise_2d(coord*size, size, offset, seed+float(i));
		for (int f = 0; f < folds; ++f) {
			noise = abs(2.0*noise-1.0);
		}
		value += noise * scale;
		normalize_factor += scale;
		size *= 2.0;
		scale *= persistence;
	}
	return value / normalize_factor;
}
float cellular_noise_2d(float2 coord, float2 size, float offset, float seed) {
	float2 o = floor(coord)+rand2(tofloat2(seed, 1.0-seed))+size;
	float2 f = frac(coord);
	float min_dist = 2.0;
	for(float x = -1.0; x <= 1.0; x++) {
		for(float y = -1.0; y <= 1.0; y++) {
			float2 neighbor = tofloat2(float(x),float(y));
			float2 node = rand2(mod(o + tofloat2(x, y), size)) + tofloat2(x, y);
			node =  0.5 + 0.25 * sin(offset * 6.28318530718 + 6.28318530718 * node);
			float2 diff = neighbor + node - f;
			float dist = length(diff);
			min_dist = min(min_dist, dist);
		}
	}
	return min_dist;
}
float fbm_2d_cellular(float2 coord, float2 size, int folds, int octaves, float persistence, float offset, float seed) {
	float normalize_factor = 0.0;
	float value = 0.0;
	float scale = 1.0;
	for (int i = 0; i < octaves; i++) {
		float noise = cellular_noise_2d(coord*size, size, offset, seed+float(i));
		for (int f = 0; f < folds; ++f) {
			noise = abs(2.0*noise-1.0);
		}
		value += noise * scale;
		normalize_factor += scale;
		size *= 2.0;
		scale *= persistence;
	}
	return value / normalize_factor;
}
float cellular2_noise_2d(float2 coord, float2 size, float offset, float seed) {
	float2 o = floor(coord)+rand2(tofloat2(seed, 1.0-seed))+size;
	float2 f = frac(coord);
	float min_dist1 = 2.0;
	float min_dist2 = 2.0;
	for(float x = -1.0; x <= 1.0; x++) {
		for(float y = -1.0; y <= 1.0; y++) {
			float2 neighbor = tofloat2(float(x),float(y));
			float2 node = rand2(mod(o + tofloat2(x, y), size)) + tofloat2(x, y);
			node = 0.5 + 0.25 * sin(offset * 6.28318530718 + 6.28318530718*node);
			float2 diff = neighbor + node - f;
			float dist = length(diff);
			if (min_dist1 > dist) {
				min_dist2 = min_dist1;
				min_dist1 = dist;
			} else if (min_dist2 > dist) {
				min_dist2 = dist;
			}
		}
	}
	return min_dist2-min_dist1;
}
float fbm_2d_cellular2(float2 coord, float2 size, int folds, int octaves, float persistence, float offset, float seed) {
	float normalize_factor = 0.0;
	float value = 0.0;
	float scale = 1.0;
	for (int i = 0; i < octaves; i++) {
		float noise = cellular2_noise_2d(coord*size, size, offset, seed+float(i));
		for (int f = 0; f < folds; ++f) {
			noise = abs(2.0*noise-1.0);
		}
		value += noise * scale;
		normalize_factor += scale;
		size *= 2.0;
		scale *= persistence;
	}
	return value / normalize_factor;
}
float cellular3_noise_2d(float2 coord, float2 size, float offset, float seed) {
	float2 o = floor(coord)+rand2(tofloat2(seed, 1.0-seed))+size;
	float2 f = frac(coord);
	float min_dist = 2.0;
	for(float x = -1.0; x <= 1.0; x++) {
		for(float y = -1.0; y <= 1.0; y++) {
			float2 neighbor = tofloat2(float(x),float(y));
			float2 node = rand2(mod(o + tofloat2(x, y), size)) + tofloat2(x, y);
			node = 0.5 + 0.25 * sin(offset * 6.28318530718 + 6.28318530718*node);
			float2 diff = neighbor + node - f;
			float dist = abs((diff).x) + abs((diff).y);
			min_dist = min(min_dist, dist);
		}
	}
	return min_dist;
}
float fbm_2d_cellular3(float2 coord, float2 size, int folds, int octaves, float persistence, float offset, float seed) {
	float normalize_factor = 0.0;
	float value = 0.0;
	float scale = 1.0;
	for (int i = 0; i < octaves; i++) {
		float noise = cellular3_noise_2d(coord*size, size, offset, seed+float(i));
		for (int f = 0; f < folds; ++f) {
			noise = abs(2.0*noise-1.0);
		}
		value += noise * scale;
		normalize_factor += scale;
		size *= 2.0;
		scale *= persistence;
	}
	return value / normalize_factor;
}
float cellular4_noise_2d(float2 coord, float2 size, float offset, float seed) {
	float2 o = floor(coord)+rand2(tofloat2(seed, 1.0-seed))+size;
	float2 f = frac(coord);
	float min_dist1 = 2.0;
	float min_dist2 = 2.0;
	for(float x = -1.0; x <= 1.0; x++) {
		for(float y = -1.0; y <= 1.0; y++) {
			float2 neighbor = tofloat2(float(x),float(y));
			float2 node = rand2(mod(o + tofloat2(x, y), size)) + tofloat2(x, y);
			node = 0.5 + 0.25 * sin(offset * 6.28318530718 + 6.28318530718*node);
			float2 diff = neighbor + node - f;
			float dist = abs((diff).x) + abs((diff).y);
			if (min_dist1 > dist) {
				min_dist2 = min_dist1;
				min_dist1 = dist;
			} else if (min_dist2 > dist) {
				min_dist2 = dist;
			}
		}
	}
	return min_dist2-min_dist1;
}
float fbm_2d_cellular4(float2 coord, float2 size, int folds, int octaves, float persistence, float offset, float seed) {
	float normalize_factor = 0.0;
	float value = 0.0;
	float scale = 1.0;
	for (int i = 0; i < octaves; i++) {
		float noise = cellular4_noise_2d(coord*size, size, offset, seed+float(i));
		for (int f = 0; f < folds; ++f) {
			noise = abs(2.0*noise-1.0);
		}
		value += noise * scale;
		normalize_factor += scale;
		size *= 2.0;
		scale *= persistence;
	}
	return value / normalize_factor;
}
float cellular5_noise_2d(float2 coord, float2 size, float offset, float seed) {
	float2 o = floor(coord)+rand2(tofloat2(seed, 1.0-seed))+size;
	float2 f = frac(coord);
	float min_dist = 2.0;
	for(float x = -1.0; x <= 1.0; x++) {
		for(float y = -1.0; y <= 1.0; y++) {
			float2 neighbor = tofloat2(float(x),float(y));
			float2 node = rand2(mod(o + tofloat2(x, y), size)) + tofloat2(x, y);
			node = 0.5 + 0.5 * sin(offset * 6.28318530718 + 6.28318530718*node);
			float2 diff = neighbor + node - f;
			float dist = max(abs((diff).x), abs((diff).y));
			min_dist = min(min_dist, dist);
		}
	}
	return min_dist;
}
float fbm_2d_cellular5(float2 coord, float2 size, int folds, int octaves, float persistence, float offset, float seed) {
	float normalize_factor = 0.0;
	float value = 0.0;
	float scale = 1.0;
	for (int i = 0; i < octaves; i++) {
		float noise = cellular5_noise_2d(coord*size, size, offset, seed+float(i));
		for (int f = 0; f < folds; ++f) {
			noise = abs(2.0*noise-1.0);
		}
		value += noise * scale;
		normalize_factor += scale;
		size *= 2.0;
		scale *= persistence;
	}
	return value / normalize_factor;
}
float cellular6_noise_2d(float2 coord, float2 size, float offset, float seed) {
	float2 o = floor(coord)+rand2(tofloat2(seed, 1.0-seed))+size;
	float2 f = frac(coord);
	float min_dist1 = 2.0;
	float min_dist2 = 2.0;
	for(float x = -1.0; x <= 1.0; x++) {
		for(float y = -1.0; y <= 1.0; y++) {
			float2 neighbor = tofloat2(float(x),float(y));
			float2 node = rand2(mod(o + tofloat2(x, y), size)) + tofloat2(x, y);
			node = 0.5 + 0.25 * sin(offset * 6.28318530718 + 6.28318530718*node);
			float2 diff = neighbor + node - f;
			float dist = max(abs((diff).x), abs((diff).y));
			if (min_dist1 > dist) {
				min_dist2 = min_dist1;
				min_dist1 = dist;
			} else if (min_dist2 > dist) {
				min_dist2 = dist;
			}
		}
	}
	return min_dist2-min_dist1;
}
float fbm_2d_cellular6(float2 coord, float2 size, int folds, int octaves, float persistence, float offset, float seed) {
	float normalize_factor = 0.0;
	float value = 0.0;
	float scale = 1.0;
	for (int i = 0; i < octaves; i++) {
		float noise = cellular6_noise_2d(coord*size, size, offset, seed+float(i));
		for (int f = 0; f < folds; ++f) {
			noise = abs(2.0*noise-1.0);
		}
		value += noise * scale;
		normalize_factor += scale;
		size *= 2.0;
		scale *= persistence;
	}
	return value / normalize_factor;
}
// MIT License Inigo Quilez - https://www.shadertoy.com/view/Xd23Dh
float voronoise_noise_2d( float2 coord, float2 size, float offset, float seed) {
	float2 i = floor(coord) + rand2(tofloat2(seed, 1.0-seed)) + size;
	float2 f = frac(coord);
	
	float2 a = tofloat2(0.0);
	
	for( int y=-2; y<=2; y++ ) {
		for( int x=-2; x<=2; x++ ) {
			float2  g = tofloat2( float(x), float(y) );
			float3  o = rand3( mod(i + g, size) + tofloat2(seed) );
			o.xy += 0.25 * sin(offset * 6.28318530718 + 6.28318530718*o.xy);
			float2  d = g - f + o.xy;
			float w = pow( 1.0-smoothstep(0.0, 1.414, length(d)), 1.0 );
			a += tofloat2(o.z*w,w);
		}
	}
	
	return a.x/a.y;
}
float fbm_2d_voronoise(float2 coord, float2 size, int folds, int octaves, float persistence, float offset, float seed) {
	float normalize_factor = 0.0;
	float value = 0.0;
	float scale = 1.0;
	for (int i = 0; i < octaves; i++) {
		float noise = voronoise_noise_2d(coord*size, size, offset, seed+float(i));
		for (int f = 0; f < folds; ++f) {
			noise = abs(2.0*noise-1.0);
		}
		value += noise * scale;
		normalize_factor += scale;
		size *= 2.0;
		scale *= persistence;
	}
	return value / normalize_factor;
}
// #globals: math_2 (o51591902145254)
float pingpong(float a, float b)
{
  return (b != 0.0) ? abs(frac((a - b) / (b * 2.0)) * b * 2.0 - b) : 0.0;
}
float4 o188887964068934_gradient_gradient_fct(float x) {
  if (x < p_o188887964068934_gradient_pos[0]) {
    return p_o188887964068934_gradient_col[0];
  } else if (x < p_o188887964068934_gradient_pos[1]) {
    return lerp(p_o188887964068934_gradient_col[0], p_o188887964068934_gradient_col[1], (x-p_o188887964068934_gradient_pos[0])/(p_o188887964068934_gradient_pos[1]-p_o188887964068934_gradient_pos[0]));
  }
  return p_o188887964068934_gradient_col[1];
}
float4 o194488265880262_gradient_gradient_fct(float x) {
  if (x < p_o194488265880262_gradient_pos[0]) {
    return p_o194488265880262_gradient_col[0];
  } else if (x < p_o194488265880262_gradient_pos[1]) {
    return lerp(p_o194488265880262_gradient_col[0], p_o194488265880262_gradient_col[1], ((x-p_o194488265880262_gradient_pos[0])/(p_o194488265880262_gradient_pos[1]-p_o194488265880262_gradient_pos[0])));
  }
  return p_o194488265880262_gradient_col[1];
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

// #code: math_2 (o51591902145254)
float o51591902145254_0_clamp_false = (sin(_Time.y))+p_o51591902145254_default_in2;
float o51591902145254_0_clamp_true = clamp(o51591902145254_0_clamp_false, 0.0, 1.0);
// #output0: math_2 (o51591902145254)
float o51591902145254_0_1_f = o51591902145254_0_clamp_false;

// #output0: fbm3_2 (o150551505415811)
float o150551505415811_0_1_f = fbm_2d_perlin((scale((uv), tofloat2(0.5+p_o200068116260987_cx, 0.5+p_o200068116260987_cy), tofloat2(p_o200068116260987_scale_x, p_o200068116260987_scale_y))), tofloat2(p_o150551505415811_scale_x, p_o150551505415811_scale_y), int(p_o150551505415811_folds), int(p_o150551505415811_iterations), p_o150551505415811_persistence, o51591902145254_0_1_f, (seed_o150551505415811+frac(_seed_variation_)));

// #output0: quantize_2 (o186275046893320)
float4 o186275046893320_0_1_rgba = tofloat4(floor(tofloat4(tofloat3(o150551505415811_0_1_f), 1.0).rgb*p_o186275046893320_steps)/p_o186275046893320_steps, tofloat4(tofloat3(o150551505415811_0_1_f), 1.0).a);

// #output0: invert (o188564566454673)
float4 o188564566454673_0_1_rgba = tofloat4(tofloat3(1.0)-o186275046893320_0_1_rgba.rgb, o186275046893320_0_1_rgba.a);

// #output0: colorize (o188887964068934)
float4 o188887964068934_0_1_rgba = o188887964068934_gradient_gradient_fct((dot((o188564566454673_0_1_rgba).rgb, tofloat3(1.0))/3.0));

// #output0: scale_2 (o200068116260987)
float4 o200068116260987_0_1_rgba = o188887964068934_0_1_rgba;

// #code: math (o51062362875199)
float o51062362875199_0_clamp_false = (sin(_Time.y))+p_o51062362875199_default_in2;
float o51062362875199_0_clamp_true = clamp(o51062362875199_0_clamp_false, 0.0, 1.0);
// #output0: math (o51062362875199)
float o51062362875199_0_1_f = o51062362875199_0_clamp_false;

// #output0: fbm3 (o149238621155305)
float o149238621155305_0_1_f = fbm_2d_perlin((scale((uv), tofloat2(0.5+p_o196261550499600_cx, 0.5+p_o196261550499600_cy), tofloat2(p_o196261550499600_scale_x, p_o196261550499600_scale_y))), tofloat2(p_o149238621155305_scale_x, p_o149238621155305_scale_y), int(p_o149238621155305_folds), int(p_o149238621155305_iterations), p_o149238621155305_persistence, o51062362875199_0_1_f, (seed_o149238621155305+frac(_seed_variation_)));

// #output0: quantize (o185735189638695)
float4 o185735189638695_0_1_rgba = tofloat4(floor(tofloat4(tofloat3(o149238621155305_0_1_f), 1.0).rgb*p_o185735189638695_steps)/p_o185735189638695_steps, tofloat4(tofloat3(o149238621155305_0_1_f), 1.0).a);

// #output0: scale (o196261550499600)
float4 o196261550499600_0_1_rgba = o185735189638695_0_1_rgba;

// #output0: colorize_2 (o194488265880262)
float4 o194488265880262_0_1_rgba = o194488265880262_gradient_gradient_fct((dot((o196261550499600_0_1_rgba).rgb, tofloat3(1.0))/3.0));

// #code: blend2 (o187076813269750)
float4 o187076813269750_0_b = o194488265880262_0_1_rgba;
float4 o187076813269750_0_l;
float o187076813269750_0_a;

o187076813269750_0_l = o200068116260987_0_1_rgba;
o187076813269750_0_a = p_o187076813269750_amount1*1.0;
o187076813269750_0_b = tofloat4(blend_darken((uv), o187076813269750_0_l.rgb, o187076813269750_0_b.rgb, o187076813269750_0_a*o187076813269750_0_l.a), min(1.0, o187076813269750_0_b.a+o187076813269750_0_a*o187076813269750_0_l.a));
// #output0: blend2 (o187076813269750)
float4 o187076813269750_0_1_rgba = o187076813269750_0_b;

				// sample the generated texture
				fixed4 col = o187076813269750_0_1_rgba;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}




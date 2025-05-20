#include "UnityCG.cginc"

// The application data.
struct appdata 
{
	float4 vertex  : POSITION;
	float2 uv      : TEXCOORD0;
#if UNITY_VERSION >= 202020
	float4 uv1     : TEXCOORD1;
#else
	float2 uv1     : TEXCOORD1;
#endif
	float2 uv2     : TEXCOORD2;
	float2 uv3     : TEXCOORD3;
	// Set from Image component property.
	float4 color   : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

// From vertex to fragment data.
struct v2f 
{
	float2 uv            : TEXCOORD0;
#if UNITY_VERSION >= 202020
	float4 uv1           : TEXCOORD1;
#else
	float2 uv1           : TEXCOORD1;
#endif
	uint2 uv2            : TEXCOORD2;
	float2 uv3           : TEXCOORD3;
	float4 worldPosition : TEXCOORD4;
	float4 vertex        : SV_POSITION;
	float4 color         : COLOR;
    UNITY_VERTEX_OUTPUT_STEREO
};

#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
#pragma multi_compile_local _ UNITY_UI_ALPHACLIP

#include "UnityUI.cginc"

float4 _ClipRect;
sampler2D _MaskTex;

// Vertex program.
// Sends data to the fragment program.
v2f vert(appdata v) 
{
	v2f o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.worldPosition = v.vertex;
	o.uv = v.uv;
	o.uv1 = v.uv1;
	o.uv2 = asuint(v.uv2);
	o.uv3 = v.uv3;
	o.color = v.color;
	return o;
}

// Mixes the alpha based on the SDF result.
inline fixed4 mixAlpha(fixed4 mainTexColor, float2 uv, fixed4 color, float4 world, float sdfAlpha) 
{
	fixed4 col = mainTexColor * color;
	col.a = min(col.a, sdfAlpha);
	col.a *= tex2D(_MaskTex, uv).r;

#ifdef UNITY_UI_CLIP_RECT
	col.a *= UnityGet2DClipping(world.xy, _ClipRect);
#endif

#ifdef UNITY_UI_ALPHACLIP
	clip(color.a - 0.001);
#endif

	return col;
}

// Decodes an packed/encoded float.
// Encoded method is defined in DTT.Utility.RoundingUI.Unsafe.Encode
float2 decode2(uint value) 
{
	uint aInt = value & 0x0000ffff;
	uint bInt = (value & 0xffff0000) >> 16;

	return float2((float)aInt, bInt) / 0x0000ffff;
}

// Creates an SDF rounded box at 0, 0 with a size of s and where the corners are rounded with r.
float sdRoundedBox(in float2 p, in float2 s, in float4 r)
{
	r.xy = (p.x > 0.0) ? r.xy : r.zw;
	r.x = (p.y > 0.0) ? r.x : r.y;
	float2 q = abs(p) - s + r.x;
	return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r.x;
}

// Anti aliases based on the given amount.
float AntialiasedCutoff(float distance, float amount) 
{
	return smoothstep(amount, -amount, distance + amount);
}

// Calculates the alpha
float RoundedBox(float2 samplePosition, float2 size, float4 radius, float falloff) {
	// -.5 = translate origin of samplePositions from (0, 0) to (.5, .5)
	// because for Image component (0,0) is bottom-right, not a center
	// * size = scale samplePositions to localSpace of Image with this size.
	float2 samplePositionTranslated = (samplePosition - .5) * size;
	float distToRect = sdRoundedBox(samplePositionTranslated, size * .5, radius * .5 * min(size.x, size.y));
	return AntialiasedCutoff(distToRect, falloff);
}

/// Rounded borders the border.
float RoundedBoxBorder(float2 samplePosition, float2 size, float4 radius, float falloff, float distance)
{
	// -.5 = translate origin of samplePositions from (0, 0) to (.5, .5)
	// because for Image component (0,0) is bottom-right, not a center
	// * size = scale samplePositions to localSpace of Image with this size
	float2 samplePositionTranslated = (samplePosition - .5) * size;
	float distToRect = abs(sdRoundedBox(samplePositionTranslated, size * .5, radius * .5 * min(size.x, size.y)) + distance) - distance;
	return AntialiasedCutoff(distToRect, falloff);
}

// Dynamically checks the border value to make use of the more optimized rounded box SDF.
float DynamicRoundedBox(float2 samplePosition, float2 size, float4 radius, float falloff, float distance)
{
	// Recalculates the border data back to a range of 0..1.
	const float normalizedBorder = distance / 0.25 / min(size.x, size.y);
	return normalizedBorder > 0.999 ? RoundedBox(samplePosition, size, radius, falloff) : RoundedBoxBorder(samplePosition, size, radius, falloff, distance);
}
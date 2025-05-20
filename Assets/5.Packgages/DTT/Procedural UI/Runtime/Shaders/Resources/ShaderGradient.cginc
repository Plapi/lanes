#define M_PI 3.1415926535897932384626433832795

float4 Gradient(sampler2D color, float2 uv, float4 settings, uint type)
{
	float2 uvs = float2(0, 0);
	float alpha = settings.x * M_PI / 180.0;
    float sina, cosa;
    sincos(-alpha, sina, cosa);
    float2x2 m = float2x2(cosa, -sina, sina, cosa);
	uv -= settings.zw;
	uv -= 0.5;
	uv = mul(m, uv);
	uv /= settings.y;
	uv += 0.5;

	if(type == 0){
		uv -= 0.5f;
		uv *= 2.0f;
		uvs.x = length(uv);
	}
	else if (type == 1)
	{ // radial around
		uv -= 0.5f;
		uv *= 2.0f;
		uvs.x =((uv.y == 0.0 || uv.x == 0 ? 0 : atan2(uv.x,uv.y))/ M_PI + 1)/2;
	}
	else if (type == 2)
	{ // gradient
		uvs.x = uv.x;
	}
	else if (type == 3)
	{ // reflected
		uvs.x = abs((uv.x - 0.5) * 2);
	}
	else if (type == 4)
	{ // star
		uv -= 0.5f;
		uvs.x = (abs(uv.x) + abs(uv.y));
	}
	uvs.x = clamp(uvs.x, 0.01f, 0.99f);
	return tex2D(color, uvs);
}

// Mixes the alpha based on the SDF result.
inline fixed4 mixAlphaGradient(fixed4 mainTexColor, float2 uv, fixed4 color, float4 world, float sdfAlpha) 
{
	fixed4 mainColor = mainTexColor * color;
	mainColor.a = min(mainColor.a, sdfAlpha);
	mainColor.a *= tex2D(_MaskTex, uv).r;

#ifdef UNITY_UI_CLIP_RECT
	mainColor.a *= UnityGet2DClipping(world.xy, _ClipRect);
#endif

#ifdef UNITY_UI_ALPHACLIP
	clip(color.a - 0.001);
#endif

	return mainColor;
}
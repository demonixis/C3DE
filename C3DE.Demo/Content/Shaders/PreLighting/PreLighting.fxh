float2 Viewport;

Texture2D LightMap;
sampler2D lightSampler = sampler_state
{
	Texture = (LightMap);
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Point;
};

float2 PostProjToScreen(float4 position)
{		
	float2 screenPos = position.xy / position.w;
	return 0.5f * (float2(screenPos.x, -screenPos.y) + 1);
}

float2 HalfPixel()
{
	return 0.5f / Viewport;
}

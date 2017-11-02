float2 Viewport;

Texture2D LightMap;
sampler2D lightSampler = sampler_state
{
	Texture = (LightMap);
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Point;
};

float3 GetLightingValue(float4 position)
{
	if (Viewport.x <= 0 || Viewport.y <= 0)
		return float3(1, 1, 1);
		
	float2 texCoord = position.xy / position.w;
	texCoord.y *= -1.0;
	texCoord = 0.5f * (texCoord + 1.0);
	texCoord += 0.5f / Viewport;
	return tex2D(lightSampler, texCoord).xyz;
}

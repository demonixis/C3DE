float2 Viewport;

// Gets the 2D screen position of a 3D position
float2 PostProjectToScreen(float4 position)
{
	float2 sp = position.xy / position.w;
	return 0.5f * (float2(sp.x, -sp.y) + 1.0);
}

float2 HalfPixel()
{
	return 0.5f / Viewport;
}

// Constants
const float E = 2.71828;

// Fog [0] => Mode [1] => Density [2] => Start [3] => End
float4 FogData;
float3 FogColor;

float CalcFogFactor(float camDistance)
{
	float fogCoeff = 1.0;
	int mode = (int)FogData.x;
	float density = FogData.y;
	float start = FogData.z;
	float end = FogData.w;
	
	if (mode == 1)
		fogCoeff = (end - camDistance) / (end - start);

	else if (mode == 2)
		fogCoeff = 1.0 / pow(E, camDistance * density);

	else if (mode == 3)
		fogCoeff = 1.0 / pow(E, camDistance * camDistance * density * density);

	if (mode > 0)
		fogCoeff = clamp(fogCoeff, 0.0, 1.0);

	return fogCoeff;
}

float4 ApplyFog(float3 pixelColor, float fogDistance)
{
	if (FogData.x > 0)
	{
		float fog = CalcFogFactor(fogDistance);
		return float4((fog * pixelColor + (1.0 - fog)) * FogColor, 1.0);
	}

    return float4(pixelColor, 1.0);
}

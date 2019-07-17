// Constants
const float E = 2.71828;

// Fog [0] => Mode [1] => Density [2] => Start [3] => End
float4 FogData;
float3 FogColor;

float3 ApplyFog(float3 color, float fogDistance)
{
	if (FogData.x > 0)
	{
		float fogCoeff = 1.0;
		int mode = (int) FogData.x;
		float density = FogData.y;
		float start = FogData.z;
		float end = FogData.w;
	
		if (mode == 1)
			fogCoeff = (end - fogDistance) / (end - start);
		else if (mode == 2)
			fogCoeff = 1.0 / pow(E, fogDistance * density);
		else if (mode == 3)
			fogCoeff = 1.0 / pow(E, fogDistance * fogDistance * density * density);

		if (mode > 0)
			fogCoeff = clamp(fogCoeff, 0.0, 1.0);

		return float3((fogCoeff * color + (1.0 - fogCoeff)) * FogColor);
	}

    return color;
}

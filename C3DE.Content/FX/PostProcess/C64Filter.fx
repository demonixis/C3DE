static const float3 c64col[16] = {
	float3(0.0,0.0,0.0),
	float3(62.0,49.0,162.0),
	float3(87.0,66.0,0.0),
	float3(140.0,62.0,52.0),
	float3(84.0,84.0,84.0),
	float3(141.0,71.0,179.0),
	float3(144.0,95.0,37.0),
	float3(124.0,112.0,218.0),
	float3(128.0,128.0,129.0),
	float3(104.0,169.0,65.0),
	float3(187.0,119.0,109.0),
	float3(122.0,191.0,199.0),
	float3(171.0,171.0,171.0),
	float3(208.0,220.0,113.0),
	float3(172.0,234.0,136.0),
	float3(255.0,255.0,255.0)
};

texture targetTexture;
sampler colorMapSampler = sampler_state
{
	texture = (targetTexture);
	minfilter = linear;
	magfilter = linear;
	mipfilter = linear;
	addressu = wrap;
	addressv = wrap;
};

float4 C64PixelShader(float2 UV:TEXCOORD0) : COLOR
{
	float3 samp = tex2D(colorMapSampler, UV).xyz;
	float3 match = float3(0.0, 0.0, 0.0);
	float bestDot = 8.0;
	float cDot = 0.0;

	for (int c = 15; c >= 0; c--) 
	{
		cDot = distance(c64col[c] / 255.0, samp);

		if (cDot < bestDot) 
		{
			bestDot = cDot;
			match = c64col[c];
		}
	}
	
	return tex2D(colorMapSampler, UV);
}

technique PostProcess
{
    pass C64Filter
    {
#if SM4
		PixelShader = compile ps_4_0_level_9_3 C64PixelShader();
#else
		PixelShader = compile ps_3_0 C64PixelShader();
#endif
    }
}
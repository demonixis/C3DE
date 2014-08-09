float blurDistance;

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

float4 GreyScaleIntensityPixelShader(float2 textureCoord:TEXCOORD0) : COLOR
{
	float4 color = tex2D(colorMapSampler, textureCoord);
	color.rgb = dot(color.rgb, float3(0.3, 0.59, 0.11));
	color.a = 1.0f;
	return color;
}

technique Technique1
{
	pass GreyScaleIntensity
	{
#if SM4
		PixelShader = compile ps_4_0_level_9_3 GreyScaleIntensityPixelShader();
#else
		PixelShader = compile ps_3_0 GreyScaleIntensityPixelShader();
#endif
	}
}
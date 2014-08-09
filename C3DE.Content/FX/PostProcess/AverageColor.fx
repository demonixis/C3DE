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

float4 AverageColorPixelShader(float2 textureCoord:TEXCOORD0) : COLOR
{
	float4 color = tex2D(colorMapSampler, textureCoord);
	color.rgb = (color.r + color.g + color.b) / 3.0;
	color.a = 1.0f;
	return color;
}

technique Technique1
{
    pass AverageColor
    {
#if SM4
		PixelShader = compile ps_4_0_level_9_3 AverageColorPixelShader();
#else
		PixelShader = compile ps_3_0 AverageColorPixelShader();
#endif
    }
}
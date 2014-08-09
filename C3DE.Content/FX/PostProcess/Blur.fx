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

float4 BlurPixelShader(float2 textureCoord:TEXCOORD0):COLOR
{
	float4 color = tex2D(colorMapSampler, float2(textureCoord.x + blurDistance, textureCoord.y + blurDistance));
	color += tex2D(colorMapSampler, float2(textureCoord.x - blurDistance, textureCoord.y - blurDistance));
	color += tex2D(colorMapSampler, float2(textureCoord.x + blurDistance, textureCoord.y - blurDistance));
	color += tex2D(colorMapSampler, float2(textureCoord.x - blurDistance, textureCoord.y + blurDistance));
	return color / 4.0;
}

technique Technique1
{
	pass Blur
	{
#if SM4
		PixelShader = compile ps_4_0_level_9_3 BlurPixelShader();
#else
		PixelShader = compile ps_3_0 BlurPixelShader();
#endif
	}
}
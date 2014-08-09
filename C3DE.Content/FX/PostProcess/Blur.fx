float blurDistance;

texture TargetTexture;
sampler2D textureSampler = sampler_state
{
	Texture = (MainTexture);
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

#if SM4
float4 BlurPixelShader(float4 position : SV_Position, float4 color : COLOR0, float2 UV : TEXCOORD0) : COLOR
#else
float4 BlurPixelShader(float4 position : POSITION0, float4 color : COLOR0, float2 UV : TEXCOORD0) : COLOR
#endif
{
	float4 diffuse = tex2D(textureSampler, float2(UV.x + blurDistance, UV.y + blurDistance));
	diffuse += tex2D(textureSampler, float2(UV.x - blurDistance, UV.y - blurDistance));
	diffuse += tex2D(textureSampler, float2(UV.x + blurDistance, UV.y - blurDistance));
	diffuse += tex2D(textureSampler, float2(UV.x - blurDistance, UV.y + blurDistance));
	return diffuse / 4.0;
}

technique Technique1
{
	pass Blur
	{
#if SM4
		PixelShader = compile ps_4_0_level_9_1 BlurPixelShader();
#else
		PixelShader = compile ps_3_0 BlurPixelShader();
#endif
	}
}
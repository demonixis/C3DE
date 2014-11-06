texture TargetTexture;
sampler2D textureSampler = sampler_state
{
	Texture = <TargetTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

struct PixelShaderInput
{
#if SM4
	float4 Position : SV_Position;
#else
	float4 Position : POSITION0;
#endif
	float4 Color : COLOR0;
	float2 UV : TEXCOORD0;
};

float4 GrayScaleIntensityPixelShader(PixelShaderInput input) : COLOR
{
	float4 diffuse = tex2D(textureSampler, input.UV);
	diffuse.rgb = dot(diffuse.rgb, float3(0.3, 0.59, 0.11));
	diffuse.a = 1.0f;
	return diffuse;
}

technique Technique1
{
	pass GreyScaleIntensity
	{
#if SM4
		PixelShader = compile ps_4_0_level_9_1 GrayScaleIntensityPixelShader();
#else
		PixelShader = compile ps_3_0 GrayScaleIntensityPixelShader();
#endif
	}
}
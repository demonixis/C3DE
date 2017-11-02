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

float4 AverageColorPixelShader(PixelShaderInput input) : COLOR
{
	float4 diffuse = tex2D(textureSampler, input.UV);
	diffuse.rgb = (diffuse.r + diffuse.g + diffuse.b) / 3.0;
	diffuse.a = 1.0f;
	return diffuse;
}

technique Technique1
{
    pass AverageColor
    {
#if SM4
		PixelShader = compile ps_4_0_level_9_1 AverageColorPixelShader();
#else
		PixelShader = compile ps_3_0 AverageColorPixelShader();
#endif
    }
}
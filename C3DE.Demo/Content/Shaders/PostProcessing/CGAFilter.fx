float3 Palette[4];

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

float4 CGAFilterPixelShader(PixelShaderInput input) : COLOR
{
	float3 samp = tex2D(textureSampler, input.UV).xyz;
	float3 match = float3(0.0, 0.0, 0.0);
	float bestDot = 8.0;
	float cDot = 0.0;

	for (int c = 3; c >= 0; c--) 
	{
		cDot = distance(Palette[c] / 255.0, samp);

		if (cDot < bestDot) 
		{
			bestDot = cDot;
			match = Palette[c];
		}
	}
	
	return float4(match / 255.0, 1.0);
}

technique PostProcess
{
    pass CGAFilter
    {
#if SM4
		PixelShader = compile ps_4_0_level_9_1 CGAFilterPixelShader();
#else
		PixelShader = compile ps_3_0 CGAFilterPixelShader();
#endif
    }
}
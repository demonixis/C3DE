const float2 MIDDLE = float2(0.5, 0.5);
float ColorLevel = 1.0;
float Depth = 1.0;
float2 TextureTiling = float2(1, 1);

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

texture RefractionTexture;
sampler2D refractionSampler = sampler_state
{
	Texture = <RefractionTexture>;
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

float4 RefractionPixelShader(PixelShaderInput input) : COLOR
{
	float ref = 1.0 - tex2D(refractionSampler, input.UV * TextureTiling).r;
	float2 uv = input.UV - MIDDLE;
	float2 offset = uv * Depth * ref;
	float3 sourceColor = tex2D(textureSampler, input.UV - offset).rgb;
	
	return float4(sourceColor + sourceColor * ref * ColorLevel, 1.0);
}

technique Technique1
{
	pass Refraction
	{
#if SM4
		PixelShader = compile ps_4_0_level_9_1 RefractionPixelShader();
#else
		PixelShader = compile ps_3_0 RefractionPixelShader();
#endif
	}
}
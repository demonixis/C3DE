float Exposure = 1;

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

float4 TonemappingPixelShader(PixelShaderInput input) : COLOR
{
	float4 color = tex2D(textureSampler, input.UV);
	color *= Exposure;
	const half3 a = 2.51f;
	const half3 b = 0.03f;
	const half3 c = 2.43f;
	const half3 d = 0.59f;
	const half3 e = 0.14f;
	float3 tonemapp = saturate((color * (a * color + b)) / (color * (c * color + d) + e));
	return float4(tonemapp, 1);
}

technique Technique1
{
    pass AverageColor
    {
#if SM4
		PixelShader = compile ps_4_0_level_9_1 TonemappingPixelShader();
#else
		PixelShader = compile ps_3_0 TonemappingPixelShader();
#endif
    }
}
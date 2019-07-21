#include "../Common/Macros.fxh"

const float2 Samples[16] =
{
	float2(0.355512, -0.709318),
	float2(0.534186, 0.71511),
	float2(-0.87866, 0.157139),
	float2(0.140679, -0.475516),
	float2(-0.0796121,	0.158842),
	float2(-0.0759516,	-0.101676),
	float2(0.12493, -0.0223423),
	float2(-0.0720074, 0.243395),
	float2(-0.207641, 0.414286),
	float2(-0.277332, -0.371262),
	float2(0.63864, -0.114214),
	float2(-0.184051, 0.622119),
	float2(0.110007, -0.219486),
	float2(0.235085, 0.314707),
	float2(-0.290012, 0.0518654),
	float2(0.0975089, -0.329594)
};

float4 ViewportSize;
float2 HalfPixel;

DECLARE_TEXTURE(MainTexture, 1);
DECLARE_TEXTURE(SecondaryMap, 2);

struct PixelShaderInput
{
#if SM4
	float4 Position : SV_Position;
#else
	float4 Position : POSITION0;
#endif
	float2 UV : TEXCOORD;
};

struct PixelShaderOutput
{
	float4 Position : SV_Position;
	float2 UV : TEXCOORD;
};

PixelShaderOutput MainVS(PixelShaderInput input)
{
	PixelShaderOutput output = (PixelShaderOutput)0;
	output.Position = input.Position;
	output.UV = input.UV;
	return output;
}

float4 SSAOGeneratePS(PixelShaderInput input) : COLOR
{
	float2 uv = input.UV - HalfPixel;
	float depth = SAMPLE_TEXTURE(SecondaryMap, uv).r;
	float occlusion = 0.0;

	for (int i = 0; i < 16; i++)
	{
		float2 current = Samples[i];
		float2 newUV = uv + i * Samples[i] * ViewportSize.zw;
		float newDepth = SAMPLE_TEXTURE(SecondaryMap, newUV).r;
		float diff = newDepth - depth;

		if (newDepth > depth)
			occlusion += 1.0 / (1 + pow(diff, 3));
	}

	float a = 1.0 / (occlusion / 16.0);
	float r = depth > 0.0002 ? a : 1;

	return float4(r, r, r, 1);
}

float4 SSAOCombinePS(PixelShaderInput input) : COLOR
{
	float3 diffuse = SAMPLE_TEXTURE(MainTexture, input.UV);
	float3 ssao = SAMPLE_TEXTURE(SecondaryMap, input.UV);
	return float4(diffuse + ssao, 1);
}

technique SSAO
{
	PASS(SSAOCombine, MainVS, SSAOGeneratePS)
	PASS(SSAOCombine, MainVS, SSAOCombinePS)
}
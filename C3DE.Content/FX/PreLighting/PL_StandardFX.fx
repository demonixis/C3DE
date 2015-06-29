#include "PreLighting.fxh"

// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;
float3 AmbientColor;
float3 DiffuseColor;

Texture2D MainTexture;
sampler2D mainSampler = sampler_state
{
	Texture = (MainTexture);
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

struct VertexShaderInput
{
#if SM4
	float4 Position : SV_Position;
#else
	float4 Position : POSITION0;
#endif
	float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float4 CopyPosition : TEXCOORD3;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4x4 worldViewProjection = mul(World, mul(View, Projection));
	output.Position = mul(input.Position, worldViewProjection);
	output.CopyPosition = output.Position;
	output.UV = input.UV;
	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 texColor = tex2D(mainSampler, input.UV);
	float3 light = GetLightingValue(input.CopyPosition);
	light += AmbientColor;
	return float4(texColor * DiffuseColor * light, 1);
}

technique Basic
{
	pass Pass1
	{
#if SM4
		VertexShader = compile vs_4_0_level_9_1 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 PixelShaderFunction();
#else
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
#endif
	}
}
#include "../Common/Macros.fxh"

// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

// Material
float3 AmbientColor;
float3 DiffuseColor;

// Misc
float2 TextureTiling;

DECLARE_TEXTURE(MainTexture, 1);

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
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.UV = input.UV;
	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 diffuse = SAMPLE_TEXTURE(MainTexture, input.UV * TextureTiling);
	float4 finalColor = float4(AmbientColor + DiffuseColor * diffuse.xyz, 1.0);
	
	clip(diffuse.a < 0.1f ? -1 : 1);

	return finalColor;
}

TECHNIQUE_SM4(Transparent, VertexShaderFunction, PixelShaderFunction);
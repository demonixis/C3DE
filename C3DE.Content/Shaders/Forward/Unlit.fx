#include "../Common/Macros.fxh"

// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

float Cutout;
float3 DiffuseColor;
float2 TextureTilling;

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

float4 PSUnlitColored(VertexShaderOutput input) : COLOR0
{
    return float4(DiffuseColor, 1.0);
}

float4 PSUnlitTextured(VertexShaderOutput input) : COLOR0
{
    float4 albedo = float4(DiffuseColor * SAMPLE_TEXTURE(MainTexture, input.UV * TextureTilling).xyz, 1.0);
	
	if (Cutout < 1)
		clip(albedo.a <= Cutout ? -1 : 1);
	
	return albedo;
}

technique TexturedSimple
{
	PASS(UnlitColor, VertexShaderFunction, PSUnlitColored)
	PASS(UnlitTexture, VertexShaderFunction, PSUnlitTextured)
}
#include "../Common/Macros.fxh"

// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;
float3 EyePosition;

DECLARE_CUBEMAP(SkyboxCubeMap, 1);

struct VertexShaderInput
{
#if SM4
	float4 Position : SV_Position;
#else
    float4 Position : POSITION0;
#endif
    float3 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float3 UV : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.UV = worldPosition.xyz - EyePosition;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	return float4(SAMPLE_CUBEMAP(SkyboxCubeMap, normalize(input.UV)).xyz, 1);
}

TECHNIQUE(Skybox, VertexShaderFunction, PixelShaderFunction);
#include "../StandardLighting.fxh"

// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

// Material
float3 AmbientColor;
int SpecularPower;

// Misc
float3 EyePosition;

struct VertexShaderInput
{
#if SM4
	float4 Position : SV_Position;
#else
    float4 Position : POSITION0;
#endif
    float2 UV : TEXCOORD0;
    float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
    float3 WorldNormal : TEXCOORD1;
    float4 WorldPosition : TEXCOORD2;
};

VertexShaderOutput CommonVS(VertexShaderInput input, float4x4 instanceTransform)
{
	VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, instanceTransform);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.UV = input.UV;
    output.WorldNormal = mul(input.Normal, instanceTransform);
    output.WorldPosition = worldPosition;
    return output;
}

VertexShaderOutput MainVS(VertexShaderInput input)
{
	return CommonVS(input, World);
}

VertexShaderOutput MainVS_Instanced(VertexShaderInput input, float4x4 instanceTransform : BLENDWEIGHT)
{
	return CommonVS(input, mul(World, transpose(instanceTransform)));
}

float3 StandardPixelShader(float4 worldPosition, float3 normal, float3 specular, float3 albedo, float3 emissive)
{    
	float3 light = float3(0, 0, 0);
	
	int limit = min(MAX_LIGHT_COUNT, LightCount);
	
	for(int i = 0; i < limit; i++)
		light += CalculateOneLight(i, worldPosition, normal, EyePosition, albedo, specular, SpecularPower);

    return AmbientColor + (albedo * light) + emissive;
}
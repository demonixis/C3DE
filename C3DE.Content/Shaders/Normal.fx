// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

struct VertexShaderInput
{
#if SM4
	float4 Position : SV_Position;
#else
	float4 Position : POSITION0;
#endif
	float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	float4x4 viewProjection = mul(View, Projection);
	float4x4 worldViewProjection = mul(World, viewProjection);
	float4 worldPosition = mul(input.Position, worldViewProjection);
	output.Position = worldPosition;
	output.Normal = mul(input.Normal, World);
	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	return float4(input.Normal, 1);
}

technique Normal
{
	pass NormalPass
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
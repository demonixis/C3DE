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
	float2 Depth : TEXCOORD0;
	float3 Normal : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4x4 viewProjection = mul(View, Projection);
	float4x4 worldViewProjection = mul(World, viewProjection);
	
	output.Position = mul(input.Position, worldViewProjection);
	output.Normal = mul(input.Normal, World);
	output.Depth.xy = output.Position.zw;

	return output;
}

float4 PSNormalFunction(VertexShaderOutput input) : COLOR0
{
	return float4((normalize(input.Normal).xyz / 2.0) + 0.5, 1.0);
}

float4 PSDepthFunction(VertexShaderOutput input) : COLOR0
{
	// Distance from camera (0, 1)
	float d = input.Depth.x / input.Depth.y;
	return float4(d, d, d, 1.0);
}

technique Basic
{
	pass NormalPass
	{
#if SM4
		VertexShader = compile vs_4_0_level_9_1 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 PSNormalFunction();
#else
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PSNormalFunction();
#endif
	}
	
	pass DepthPass
	{
#if SM4
		VertexShader = compile vs_4_0_level_9_1 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 PSDepthFunction();
#else
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PSDepthFunction();
#endif
	}
}
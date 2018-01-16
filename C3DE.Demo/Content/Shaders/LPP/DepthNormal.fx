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
    float2 UV : TEXCOORD0;
	float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
	float2 Depth : TEXCOORD1;
	float3 Normal : TEXCOORD2;
};

struct PixelShaderOutput
{
	float4 Normal: COLOR0;
	float4 Depth : COLOR1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4x4 viewProjection = mul(View, Projection);
	float4x4 worldViewProjection = mul(World, viewProjection);
	
	output.Position = mul(input.Position, worldViewProjection);
	output.Normal = (float3)mul(input.Normal, World);
    output.UV = input.UV;
	output.Depth.xy = (float2)output.Position.zw;

	return output;
}

PixelShaderOutput PSNormalDepthFunction(VertexShaderOutput input)
{
	PixelShaderOutput output;
	
    output.Normal = float4((normalize(input.Normal).xyz / 2.0) + 0.5, 1);
	output.Depth = input.Depth.x / input.Depth.y;

	return output;
}

technique NormalDepth
{
	pass NormalDepthPass
	{
#if SM4
		VertexShader = compile vs_4_0_level_9_1 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 PSNormalDepthFunction();
#else
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PSNormalDepthFunction();
#endif
	}
}
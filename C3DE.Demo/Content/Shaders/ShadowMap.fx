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
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float Depth : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.Depth = output.Position.z / output.Position.w;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	return float4(input.Depth, 0, 0, 0);
}

technique Technique1
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

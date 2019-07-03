struct VertexShaderInput
{
#if SM4
	float3 Position : SV_Position;
#else
	float3 Position : POSITION0;
#endif
};

struct VertexShaderOutput
{
#if SM4
	float4 Position : SV_Position;
#else
    float4 Position : POSITION0;
#endif
};

struct PixelShaderOutput
{
    float4 Color : COLOR0;
	float4 Normal: COLOR1;
	float4 Depth : COLOR2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
    output.Position = float4(input.Position, 1);
	return output;
}

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
	PixelShaderOutput output;
	
    output.Color = 0.0f;
    output.Normal.rgb = 0.5f;
    output.Normal.a = 0.0f;
    output.Depth = 0.0f;

	return output;
}

technique Clear
{
	pass ClearColorNormalDepth
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
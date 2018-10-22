sampler TextureSampler : register(s0);

float BloomThreshold;

struct PixelShaderInput
{
#if SM4
	float4 Position : SV_Position;
#else
	float4 Position : POSITION0;
#endif
	float4 Color : COLOR0;
	float2 UV : TEXCOORD0;
};

float4 PixelShaderFunction(PixelShaderInput input) : COLOR
{
    float4 c = tex2D(TextureSampler, input.UV);
    return saturate((c - BloomThreshold) / (1 - BloomThreshold));
}

technique BloomExtract
{
    pass Pass1
    {
#if SM4
		PixelShader = compile ps_4_0_level_9_1 PixelShaderFunction();
#else
		PixelShader = compile ps_3_0 PixelShaderFunction();
#endif
    }
}

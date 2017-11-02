#define SAMPLE_COUNT 15

sampler TextureSampler : register(s0);

float2 SampleOffsets[SAMPLE_COUNT];
float SampleWeights[SAMPLE_COUNT];

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
    float4 c = 0;
    
    // Combine a number of weighted image filter taps.
    for (int i = 0; i < SAMPLE_COUNT; i++)
        c += tex2D(TextureSampler, input.UV + SampleOffsets[i]) * SampleWeights[i];
    
    return c;
}

technique GaussianBlur
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

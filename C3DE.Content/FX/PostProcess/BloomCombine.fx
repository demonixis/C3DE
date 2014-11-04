sampler BloomSampler : register(s0);
sampler BaseSampler : register(s1);

float BloomIntensity;
float BaseIntensity;
float BloomSaturation;
float BaseSaturation;

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

float4 AdjustSaturation(float4 color, float saturation)
{
    float grey = dot(color, float4(0.3, 0.59, 0.11, 1.0));
    return lerp(grey, color, saturation);
}

float4 PixelShaderFunction(PixelShaderInput input) : COLOR
{
    float4 bloom = tex2D(BloomSampler, input.UV);
    float4 base = tex2D(BaseSampler, input.UV);
    
    bloom = AdjustSaturation(bloom, BloomSaturation) * BloomIntensity;
    base = AdjustSaturation(base, BaseSaturation) * BaseIntensity;

    base *= (1 - saturate(bloom));
    
    return base + bloom;
}

technique BloomCombine
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

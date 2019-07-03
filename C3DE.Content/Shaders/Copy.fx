Texture2D SourceTexture;
SamplerState sourceSampler
{
    Texture = <SourceTexture>;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

struct PixelShaderInput
{
	float4 Position : SV_Position;
    float2 UV : TEXCOORD0;
};

float4 PixelShaderFunction(PixelShaderInput input) : COLOR0
{
    return tex2D(sourceSampler, input.UV);
}

technique Technique0
{
	pass Copy
	{
#if SM4
		PixelShader = compile ps_4_0_level_9_1 PixelShaderFunction();
#else
        PixelShader = compile ps_3_0 PixelShaderFunction();
#endif
	}
}
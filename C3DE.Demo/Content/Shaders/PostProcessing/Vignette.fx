float2 ViewportSize;
float2 Scale;
float Power;

texture TargetTexture;
sampler2D textureSampler = sampler_state
{
    Texture = <TargetTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct PixelShaderInput
{
#if SM4
	float4 Position : SV_Position;
#else
    float4 Position : POSITION0;
#endif
    float2 UV : TEXCOORD;
};

struct PixelShaderOutput
{
    float4 Position : SV_Position;
    float2 UV : TEXCOORD;
};


PixelShaderOutput VertexShaderFunction(PixelShaderInput input)
{
    PixelShaderOutput output = (PixelShaderOutput) 0;
    output.Position = input.Position;
    output.UV = input.UV;

#if !SM4
    output.Position.xy -= 0.5;
#endif

    output.Position.xy /= ViewportSize;
    output.Position.xy *= float2(2, -2);
    output.Position.xy -= float2(1, -1);
    return output;
}


float4 PixedShaderFunctionPS(PixelShaderOutput input) : COLOR0
{
    float4 color = tex2D(textureSampler, input.UV);
    float radius = length((input.UV - 0.5) * 2 / Scale);
    float vignette = pow(abs(0.0001 + radius), Power);
    return color * saturate(1 - vignette);
}

technique Technique0
{
    pass Vignette
    {
#if SM4
        VertexShader = compile vs_4_0_level_9_1 VertexShaderFunction();
        PixelShader = compile ps_4_0_level_9_1 PixedShaderFunctionPS();
#else
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixedShaderFunctionPS();
#endif
    }
}
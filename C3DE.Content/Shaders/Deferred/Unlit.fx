// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;
float3 DiffuseColor;
float2 TextureTiling;

texture MainTexture;
sampler2D textureSampler = sampler_state
{
    Texture = (MainTexture);
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VertexShaderInput
{
#if SM4
	float4 Position : SV_Position;
#else
    float4 Position : POSITION0;
#endif
    float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
};

struct PixelShaderOutput
{
    float4 Color : COLOR0;
    float4 Normal : COLOR1;
    float4 Depth : COLOR2;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output;
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.UV = input.UV;
    return output;
}

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
    PixelShaderOutput output = (PixelShaderOutput) 0;
    output.Color = float4(DiffuseColor, 1.0);
    output.Normal = 0;
    output.Depth = 0;
    return output;
}

PixelShaderOutput PixelShaderFunctionTexture(VertexShaderOutput input)
{
    PixelShaderOutput output = (PixelShaderOutput) 0;
    output.Color = float4(DiffuseColor * tex2D(textureSampler, input.UV * TextureTiling).xyz, 1.0);
    output.Normal = 0;
    output.Depth = 0;
    return output;
}

technique TexturedSimple
{
    pass UnlitColor
    {
#if SM4
		VertexShader = compile vs_4_0_level_9_1 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 PixelShaderFunction();
#else
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
#endif
    }

    pass UnlitTexture
    {
#if SM4
		VertexShader = compile vs_4_0_level_9_1 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 PixelShaderFunctionTexture();
#else
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunctionTexture();
#endif
    }
}

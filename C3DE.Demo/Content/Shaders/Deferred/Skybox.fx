// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;
float3 EyePosition;

texture Texture;
samplerCUBE SkyboxSampler = sampler_state
{
    Texture = <Texture>;
    MagFilter = Linear;
    MinFilter = Linear;
    MipFilter = Linear;
    AddressU = Mirror;
    AddressV = Mirror;
};

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
    float3 TextureCoordinate : TEXCOORD0;
    float2 Depth : TEXCOORD1;
};

struct PixelShaderOutput
{
    float4 Color : COLOR0;
    float4 Normal : COLOR1;
    float4 Depth : COLOR2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.TextureCoordinate = worldPosition.xyz - EyePosition;
    output.Depth.x = output.Position.z;
    output.Depth.y = output.Position.w;

    return output;
}

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    PixelShaderOutput output = (PixelShaderOutput) 0;
    float4 diffuse = texCUBE(SkyboxSampler, normalize(input.TextureCoordinate));

    output.Color = diffuse;
    output.Color.a = 0;
    output.Normal = float4(0, 0, 0, 0);
    output.Depth = input.Depth.x / input.Depth.y;

    return output;
}

technique Skybox
{
    pass AmbientPass
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
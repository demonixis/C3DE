// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;
float3 EyePosition;

texture SkyboxCubeMap;
samplerCUBE SkyboxSampler = sampler_state
{
    Texture = <SkyboxCubeMap>;
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
    float3 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float3 UV : TEXCOORD0;
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.UV = worldPosition.xyz - EyePosition;
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
	return float4(texCUBE(SkyboxSampler, normalize(input.UV)).xyz, 1);
}

technique Skybox
{
    pass AmbientPass
    {
#if SM4
		VertexShader = compile vs_4_0_level_9_1 MainVS();
		PixelShader = compile ps_4_0_level_9_1 MainPS();
#else
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 MainPS();
#endif
    }
}
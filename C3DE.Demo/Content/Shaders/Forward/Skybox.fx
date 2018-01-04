#if SM4
#include "Fog.fxh"
#endif
// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;
float3 EyePosition;

#if SM4
bool FogEnabled;
#endif

texture MainTexture;
samplerCUBE SkyboxSampler = sampler_state
{
    Texture = <MainTexture>;
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
#if SM4
    float FogDistance : FOG;
#endif
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.UV = worldPosition.xyz - EyePosition;
#if SM4
    output.FogDistance = distance(worldPosition.xyz, EyePosition);
#endif
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 diffuse = texCUBE(SkyboxSampler, normalize(input.UV));

#if SM4
    if (FogEnabled == true)
        return ApplyFog(diffuse.xyz, input.FogDistance);
#endif

    return diffuse;
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
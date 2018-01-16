#include "Lightmap.fxh"

// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;
float3 AmbientColor;
float3 DiffuseColor;

// Reflection
bool ReflectionTextureEnabled;

// Emission
float3 EmissiveColor;
float EmissiveIntensity;
bool EmissiveTextureEnabled;

// Misc
float3 EyePosition = float3(1, 1, 0);
float2 TextureTiling = float2(1, 1);

Texture2D MainTexture;
sampler2D mainSampler = sampler_state
{
    Texture = (MainTexture);
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture ReflectionTexture;
samplerCUBE reflectionSampler = sampler_state
{
    Texture = <ReflectionTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Mirror;
    AddressV = Mirror;
};

texture EmissiveTexture;
sampler2D emissiveSampler = sampler_state
{
    Texture = (EmissiveTexture);
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
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
    float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
    float4 CopyPosition : TEXCOORD1;
    float3 Reflection : TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4x4 worldViewProjection = mul(World, mul(View, Projection));
    output.Position = mul(input.Position, worldViewProjection);
    output.CopyPosition = output.Position;
    output.UV = input.UV;

    if (ReflectionTextureEnabled == true)
    {
        float4 worldPosition = mul(input.Position, World);
        float3 viewDirection = EyePosition - worldPosition.xyz;
        float3 normal = input.Normal;
        output.Reflection = reflect(-normalize(viewDirection), normalize(normal));
    }

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float3 texColor = tex2D(mainSampler, input.UV * TextureTiling).rgb * DiffuseColor;

    if (ReflectionTextureEnabled == true)
        texColor *= texCUBE(reflectionSampler, normalize(input.Reflection)).xyz;

    float3 light = GetLightingValue(input.CopyPosition);
    light += AmbientColor;
	
    float3 emissiveColor = EmissiveColor;

    if (EmissiveTextureEnabled == true)
        emissiveColor = tex2D(emissiveSampler, input.UV * TextureTiling).xyz;

    emissiveColor *= EmissiveIntensity;
	
    return float4((texColor * light) + emissiveColor, 1);
}

technique Basic
{
    pass Pass1
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
#include "ShadowMap.new.fxh"
#include "Fog.new.fxh"
#include "Lights.fxh"

// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

// Material
float3 AmbientColor;
float3 DiffuseColor;

// Emissive
float3 EmissiveColor;
float EmissiveIntensity;
bool EmissiveTextureEnabled;

// Reflection
bool ReflectionTextureEnabled;
float ReflectionIntensity;

// Misc
float3 EyePosition = float3(1, 1, 0);
float2 TextureTiling = float2(1, 1);

texture MainTexture;
sampler2D textureSampler = sampler_state
{
    Texture = (MainTexture);
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
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
    float3 WorldNormal : TEXCOORD1;
    float4 WorldPosition : TEXCOORD2;
    float3 Reflection : TEXCOORD3;
    float FogDistance : FOG;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.UV = input.UV;
    output.WorldNormal = mul(input.Normal, World);
    output.WorldPosition = worldPosition;
    output.FogDistance = distance(worldPosition, EyePosition);

    if (ReflectionTextureEnabled)
    {
        float3 viewDirection = EyePosition - worldPosition;
        float3 normal = input.Normal;
        output.Reflection = reflect(-normalize(viewDirection), normalize(normal));
    }

    return output;
}

float4 PixelShaderAmbient(VertexShaderOutput input) : COLOR0
{
    float3 diffuse = tex2D(textureSampler, input.UV * TextureTiling).xyz;
	
    if (ReflectionTextureEnabled)
    {
        float3 reflectColor = texCUBE(reflectionSampler, normalize(input.Reflection)).xyz;
        diffuse = lerp(diffuse, reflectColor, ReflectionIntensity);
    }
	
    return float4(AmbientColor * (DiffuseColor + diffuse), 1.0);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float3 lightFactor = CalcLightFactor(input.WorldPosition, input.WorldNormal);
    float shadow = CalcShadow(input.WorldPosition); 
    float3 diffuse = lightFactor * shadow;
    float3 specular = CalcSpecular(input.WorldPosition, input.WorldNormal, EyePosition, input.UV * TextureTiling);
    float3 compose = diffuse + specular;
    return ApplyFog(compose, input.FogDistance);
}

float4 PixelShaderEmissive(VertexShaderOutput input) : COLOR0
{
    float3 emissiveColor = EmissiveColor * EmissiveIntensity;

    if (EmissiveTextureEnabled)
		emissiveColor *= tex2D(emissiveSampler, input.UV * TextureTiling).xyz;

    return float4(emissiveColor, 1.0);
}

technique Standard
{
    pass AmbientPass
    {
#if SM4
		VertexShader = compile vs_4_0_level_9_3 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_3 PixelShaderAmbient();
#else
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderAmbient();
#endif
    }
	
    pass LightPass
    {
#if SM4
		PixelShader = compile ps_4_0_level_9_3 PixelShaderFunction();
#else
        PixelShader = compile ps_3_0 PixelShaderFunction();
#endif
    }
	
    pass EmissivePass
    {
#if SM4
		PixelShader = compile ps_4_0_level_9_3 PixelShaderEmissive();
#else
        PixelShader = compile ps_3_0 PixelShaderEmissive();
#endif
    }
}
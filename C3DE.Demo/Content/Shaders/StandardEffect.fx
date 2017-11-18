#include "ShadowMap.fxh"
#include "Fog.fxh"
#include "Lights.fxh"
#include "Emissive.fxh"

// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

// Material
float3 AmbientColor;
float3 DiffuseColor;
bool NormalTextureEnabled;

// Reflection
bool ReflectionTextureEnabled;

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

texture NormalTexture;
sampler2D normalSampler = sampler_state
{
    Texture = (NormalTexture);
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
    float3x3 WorldToTangentSpace : TEXCOORD4;
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
    output.FogDistance = distance(worldPosition.xyz, EyePosition);

    if (ReflectionTextureEnabled)
    {
        float3 viewDirection = EyePosition - worldPosition.xyz;
        float3 normal = input.Normal;
        output.Reflection = reflect(-normalize(viewDirection), normalize(normal));
    }

    float3 c1 = cross(input.Normal, float3(0.0, 0.0, 1.0));
    float3 c2 = cross(input.Normal, float3(0.0, 1.0, 0.0));

    // [0] Tangent / [1] Binormal / [2] Normal
    output.WorldToTangentSpace[0] = length(c1) > length(c2) ? c1 : c2;
    output.WorldToTangentSpace[1] = normalize(output.WorldToTangentSpace[0]);
    output.WorldToTangentSpace[2] = input.Normal;

    return output;
}

float3 CalcDiffuseColor(VertexShaderOutput input)
{
    float3 diffuse = tex2D(textureSampler, input.UV * TextureTiling).xyz;
	
    if (ReflectionTextureEnabled)
    {
        float3 reflectColor = texCUBE(reflectionSampler, normalize(input.Reflection)).xyz;
        diffuse *= texCUBE(reflectionSampler, normalize(input.Reflection)).xyz;
    }

    return diffuse * DiffuseColor;
}

float4 PixelShaderAmbient(VertexShaderOutput input) : COLOR0
{
    return float4(AmbientColor * CalcDiffuseColor(input), 1);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    if (NormalTextureEnabled == true)
    {
        float3 normalMap = (2.0 * (tex2D(normalSampler, input.UV * TextureTiling))) - 1.0;
        normalMap = normalize(mul(normalMap, input.WorldToTangentSpace));
        input.WorldNormal = normalMap;
    }
    
    float3 diffuse = CalcDiffuseColor(input);
    float3 lightFactor = CalcLightFactor(input.WorldPosition, input.WorldNormal);
    float shadow = CalcShadow(input.WorldPosition);
    float3 diffuse2 = lightFactor * shadow * diffuse;
    float3 specular = CalcSpecular(input.WorldPosition, input.WorldNormal, EyePosition, input.UV * TextureTiling);
    float3 compose = diffuse2 + specular;
    return ApplyFog(compose, input.FogDistance);
}

float4 PixelShaderEmissive(VertexShaderOutput input) : COLOR0
{
    return CalcEmissiveColor(input.UV * TextureTiling);
}

technique Standard
{
    pass AmbientPass
    {
#if SM4
		VertexShader = compile vs_4_0_level_9_1 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 PixelShaderAmbient();
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
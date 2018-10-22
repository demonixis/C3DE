#include "../Common/ShadowMap.fxh"
#include "../Common/Fog.fxh"
#include "Lights.fxh"

// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

// Material
float3 AmbientColor;
float3 DiffuseColor;

bool ReflectionTextureEnabled = false;
float3 ReflectionColor;

// Misc
float2 TextureTiling;
bool NormalTextureEnabled = false;
float3 EyePosition;
float TotalTime;

texture MainTexture;
sampler2D WaterMapSampler = sampler_state
{
    Texture = <MainTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture NormalTexture;
sampler2D NormalMapSampler = sampler_state
{
    Texture = <NormalTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture ReflectionTexture;
samplerCUBE reflectiveSampler = sampler_state
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

	// Wave
    input.Position.z += sin((TotalTime * 16.0) + (input.Position.y / 1.0)) / 16.0;
    input.Position.y += sin(1.0 * input.Position.y + (TotalTime * 5.0)) * 0.25;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.UV = input.UV;
    output.WorldNormal = mul(input.Normal, World);
    output.WorldPosition = worldPosition;
    output.FogDistance = distance(worldPosition.xyz, EyePosition);
	
    float3 normal = input.Normal;

    if (ReflectionTextureEnabled == true)
    {
        float3 viewDirection = EyePosition - worldPosition.xyz;
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
    input.UV.x = input.UV.x * 20.0 + sin(TotalTime * 3.0 + 10.0) / 256.0;
    input.UV.y = input.UV.y * 20.0;

    float3 diffuse = tex2D(WaterMapSampler, input.UV * TextureTiling).xyz;
	
    if (ReflectionTextureEnabled == true)
        return diffuse * ReflectionColor * texCUBE(reflectiveSampler, normalize(input.Reflection)).xyz;
	
    return diffuse * DiffuseColor;
}

float4 PixelShaderAmbient(VertexShaderOutput input) : COLOR0
{
    return float4(AmbientColor * CalcDiffuseColor(input), 1.0);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float3 normal = input.WorldNormal;

    if (NormalTextureEnabled == true)
    {
        input.UV.y += (sin(TotalTime * 3.0 + 10.0) / 256) + (TotalTime / 16);
        float3 normalMap = 2.0 * (tex2D(NormalMapSampler, input.UV * TextureTiling)) - 1.0;

        input.UV.y -= ((sin(TotalTime * 3.0 + 10) / 256.0) + (TotalTime / 16.0)) * 2.0;
        float3 normalMap2 = (2.0 * (tex2D(NormalMapSampler, input.UV * TextureTiling))) - 1.0;

        normalMap = (normalMap + normalMap2) / 2.0;
        normalMap = normalize(mul(normalMap, input.WorldToTangentSpace));
		
        normal = normalMap;
    }

    float3 diffuse = CalcDiffuseColor(input);
    float3 lightFactor = CalcLightFactor(input.WorldPosition, normal);
    float shadow = CalcShadow(input.WorldPosition);
    float3 diffuse2 = lightFactor * shadow * diffuse;
    float3 specular = CalcSpecular(input.WorldPosition, normal, EyePosition, input.UV * TextureTiling);
    float3 compose = diffuse2 + specular;
    float4 final = ApplyFog(compose, input.FogDistance);

    return final;
}

technique Water
{
    pass AmbientPass
    {
        AlphaBlendEnable = TRUE;
        DestBlend = INVSRCALPHA;
        SrcBlend = SRCALPHA;
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
        AlphaBlendEnable = TRUE;
        DestBlend = INVSRCALPHA;
        SrcBlend = SRCALPHA;
#if SM4
		PixelShader = compile ps_4_0_level_9_3 PixelShaderFunction();
#else
        PixelShader = compile ps_3_0 PixelShaderFunction();
#endif
    }
}
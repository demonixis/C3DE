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

// Misc
float2 TextureTiling;
float3 EyePosition;

#if SM4
int NormalMapEnabled = 0;
#endif

texture2D MainTexture;
sampler2D MainTextureSampler = sampler_state
{
    Texture = <MainTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture2D SandTexture;
sampler2D SandTextureSampler = sampler_state
{
    Texture = <SandTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture2D RockTexture;
sampler2D RockTextureSampler = sampler_state
{
    Texture = <RockTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture2D SnowTexture;
sampler2D SnowTextureSampler = sampler_state
{
    Texture = <SnowTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture2D WeightMap;
sampler2D WeightMapSampler = sampler_state
{
    Texture = <WeightMap>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

#if SM4
texture2D NormalMap;
sampler2D NormalSampler = sampler_state
{
	Texture = <NormalMap>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

texture2D SandNormalMap;
sampler2D SandSampler = sampler_state
{
	Texture = <SandNormalMap>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

texture2D RockNormalMap;
sampler2D RockSampler = sampler_state
{
	Texture = <RockNormalMap>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

texture2D SnowNormalMap;
sampler2D SnowSampler = sampler_state
{
	Texture = <SnowNormalMap>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};
#endif

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
#if SM4
	float3x3 WorldToTangentSpace : TEXCOORD3;
#endif
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

#if SM4
	float3 c1 = cross(input.Normal, float3(0.0, 0.0, 1.0));
	float3 c2 = cross(input.Normal, float3(0.0, 1.0, 0.0));

	// [0] Tangent / [1] Binormal / [2] Normal
	output.WorldToTangentSpace[0] = length(c1) > length(c2) ? c1 : c2;
	output.WorldToTangentSpace[1] = normalize(output.WorldToTangentSpace[0]);
	output.WorldToTangentSpace[2] = input.Normal;
#endif

    return output;
}

float3 CalcDiffuseColor(VertexShaderOutput input)
{
    float3 sandTex = tex2D(SandTextureSampler, input.UV * TextureTiling);
    float3 rockTex = tex2D(RockTextureSampler, input.UV * TextureTiling);
    float3 snowTex = tex2D(SnowTextureSampler, input.UV * TextureTiling);
    float3 mainTex = tex2D(MainTextureSampler, input.UV * TextureTiling);
    float3 weightTex = tex2D(WeightMapSampler, input.UV);
	
    float3 diffuse = clamp(1.0 - weightTex.r - weightTex.g - weightTex.b, 0, 1);
    diffuse *= mainTex;
    diffuse += weightTex.r * sandTex + weightTex.g * rockTex + weightTex.b * snowTex;
	
    return diffuse * DiffuseColor;
}

float3 CalcNormal(VertexShaderOutput input)
{
	float3 normal = input.WorldNormal;

	if (NormalMapEnabled == 1)
	{
		float3 sandTex = tex2D(SandSampler, input.UV * TextureTiling);
		float3 rockTex = tex2D(RockSampler, input.UV * TextureTiling);
		float3 snowTex = tex2D(SnowSampler, input.UV * TextureTiling);
		float3 mainTex = tex2D(NormalSampler, input.UV * TextureTiling);
		float3 weightTex = tex2D(WeightMapSampler, input.UV);

		float3 normalBlend = clamp(1.0 - weightTex.r - weightTex.g - weightTex.b, 0, 1);
		normalBlend *= mainTex;
		normalBlend += weightTex.r * sandTex + weightTex.g * rockTex + weightTex.b * snowTex;

		float3 normalMap = (2.0 * (normalBlend)) - 1.0;
		normal = normalize(mul(normalMap, input.WorldToTangentSpace));
	}

	return normal;
}

float4 PixelShaderAmbient(VertexShaderOutput input) : COLOR0
{
    return float4(AmbientColor * CalcDiffuseColor(input), 1);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float3 diffuse = CalcDiffuseColor(input);
	float3 normal = CalcNormal(input);
    float3 lightFactor = CalcLightFactor(input.WorldPosition, normal);
    float shadow = CalcShadow(input.WorldPosition);
    float3 diffuse2 = lightFactor * shadow * diffuse;
    float3 specular = CalcSpecular(input.WorldPosition, normal, EyePosition, input.UV * TextureTiling);
    float3 compose = diffuse2 + specular;
    return ApplyFog(compose, input.FogDistance);
}

technique Terrain
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
}
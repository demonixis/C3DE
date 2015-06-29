#include "ShadowMap.fxh"
#include "Fog.fxh"

// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

// Material
float3 AmbientColor = float3(1.0, 1.0, 1.0);
float3 DiffuseColor = float3(1.0, 1.0, 1.0);

// Light
float3 LightDirection = float3(1.0, 1.0, 0.0);
float LightIntensity = 1.0;
float3 LightColor = float3(1, 1, 1);

// Misc
float2 TextureTiling = float2(1, 1);
float2 TextureOffset = float2(0, 0);
float3 EyePosition = float3(1, 1, 0);

texture2D MainTexture;
sampler2D MainTextureSampler = sampler_state
{
	Texture = < MainTexture > ;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

texture2D SandTexture;
sampler2D SandTextureSampler = sampler_state
{
	Texture = < SandTexture > ;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

texture2D RockTexture;
sampler2D RockTextureSampler = sampler_state
{
	Texture = < RockTexture > ;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

texture2D SnowTexture;
sampler2D SnowTextureSampler = sampler_state
{
	Texture = < SnowTexture > ;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

texture2D WeightMap;
sampler2D WeightMapSampler = sampler_state
{
	Texture = < WeightMap > ;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
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
	float3 Normal : TEXCOORD1;
	float4 WorldPosition : TEXCOORD2;
	float4 CopyPosition : TEXCOORD3;
	float FogDistance : FOG;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.UV = input.UV;
	output.Normal = mul(input.Normal, World);
	output.WorldPosition = worldPosition;
	output.CopyPosition = input.Position;
	output.FogDistance = distance(worldPosition, EyePosition);

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float light = dot(normalize(input.Normal), normalize(LightDirection));
	light = saturate(light * LightColor * LightIntensity + 0.4);
	
	float shadowTerm = CalcShadow(input.WorldPosition);
	
	float3 sandTex = tex2D(SandTextureSampler, (input.UV + TextureOffset) * TextureTiling);
	float3 rockTex = tex2D(RockTextureSampler, (input.UV + TextureOffset) * TextureTiling);
	float3 snowTex = tex2D(SnowTextureSampler, (input.UV + TextureOffset) * TextureTiling);
	float3 mainTex = tex2D(MainTextureSampler, (input.UV + TextureOffset) * TextureTiling);
	float3 weightTex = tex2D(WeightMapSampler, input.UV);
	
	float3 baseCompose = clamp(1.0 - weightTex.r - weightTex.g - weightTex.b, 0, 1);
	baseCompose *= mainTex;
	baseCompose += weightTex.r * sandTex + weightTex.g * rockTex + weightTex.b * snowTex;
	
	float4 finalCompose = float4(AmbientColor + (DiffuseColor * float4(baseCompose, 1.0) * light * shadowTerm), 1.0);
	
	return ApplyFog(finalCompose, input.FogDistance);
}

technique Terrain
{
	pass Pass1
	{
#if SM4
		VertexShader = compile vs_4_0_level_9_3 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_3 PixelShaderFunction();
#else
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
#endif
	}
}
#include "StandardBase.fx"

// Misc
float2 TextureTiling;
float3 SpecularColor;
float SpecularIntensity;

texture WeightMap;
sampler weightSampler = sampler_state
{
    Texture = (WeightMap);
    MAGFILTER = LINEAR;
    MINFILTER = LINEAR;
    MIPFILTER = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture GrassMap;
sampler grassSampler = sampler_state
{
    Texture = (GrassMap);
    MAGFILTER = LINEAR;
    MINFILTER = LINEAR;
    MIPFILTER = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture SandMap;
sampler sandSampler = sampler_state
{
    Texture = (SandMap);
    MAGFILTER = LINEAR;
    MINFILTER = LINEAR;
    MIPFILTER = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture RockMap;
sampler rockSampler = sampler_state
{
    Texture = (RockMap);
    MAGFILTER = LINEAR;
    MINFILTER = LINEAR;
    MIPFILTER = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture SnowMap;
sampler snowSampler = sampler_state
{
    Texture = (SnowMap);
    MAGFILTER = LINEAR;
    MINFILTER = LINEAR;
    MIPFILTER = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

float4 MainPS(VertexShaderOutput input) : COLOR0
{
	float2 scaledUV = input.UV * TextureTiling;
	float3 mainTex = tex2D(grassSampler, scaledUV);
	float3 sandTex = tex2D(sandSampler, scaledUV);
	float3 rockTex = tex2D(rockSampler, scaledUV);
	float3 snowTex = tex2D(snowSampler, scaledUV);
	float3 weightTex = tex2D(weightSampler, input.UV);

	float3 albedo = clamp(1.0 - weightTex.r - weightTex.g - weightTex.b, 0, 1);
	albedo *= mainTex;
	albedo += weightTex.r * sandTex + weightTex.g * rockTex + weightTex.b * snowTex;

	float3 normal = input.WorldNormal;

	// Base Pixel Shader
	return float4(StandardPixelShader(input.WorldPosition, normal, SpecularColor * SpecularIntensity, albedo, float3(0, 0, 0)), 1.0);
}

technique StandardTerrain
{
	pass P0
	{
#if SM4
		VertexShader = compile vs_4_0 MainVS();
		PixelShader = compile ps_4_0 MainPS();
#else
		VertexShader = compile vs_3_0 MainVS();
		PixelShader = compile ps_3_0 MainPS();
#endif
	}
}
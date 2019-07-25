#include "StandardBase.fx"
#include "../Common/ShadowMap.fxh"

// Misc
float2 TextureTiling;
float2 Features;
float3 SpecularColor;
float SpecularIntensity;

DECLARE_TEXTURE(WeightMap, 1);
DECLARE_TEXTURE(GrassMap, 2);
DECLARE_TEXTURE(SandMap, 3);
DECLARE_TEXTURE(RockMap, 4);
DECLARE_TEXTURE(SnowMap, 5);

DECLARE_TEXTURE(GrassNormalMap, 6);
DECLARE_TEXTURE(SandNormalMap, 7);
DECLARE_TEXTURE(RockNormalMap, 8);
DECLARE_TEXTURE(SnowNormalMap, 9);

float3 BlendTextures(sampler2D grass, sampler2D sand, sampler2D rock, sampler2D snow, float2 uv)
{
	float2 scaledUV = uv * TextureTiling;
	float3 mainTex = SAMPLE_TEXTURE(grass, scaledUV);
	float3 sandTex = SAMPLE_TEXTURE(sand, scaledUV);
	float3 rockTex = SAMPLE_TEXTURE(rock, scaledUV);
	float3 snowTex = SAMPLE_TEXTURE(snow, scaledUV);
	float3 weightTex = SAMPLE_TEXTURE(WeightMap, uv);

	float3 blend = clamp(1.0 - weightTex.r - weightTex.g - weightTex.b, 0, 1);
	blend *= mainTex;
	blend += weightTex.r * sandTex + weightTex.g * rockTex + weightTex.b * snowTex;

	return blend;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 albedo = BlendTextures(GrassMap, SandMap, RockMap, SnowMap, input.UV);
	float3 normal = input.WorldNormal;

	if (Features.x > 0)
	{
		float3 normalBlend = BlendTextures(GrassNormalMap, SandNormalMap, RockNormalMap, SnowNormalMap, input.UV);
		float3 normalMap = (2.0 * (normalBlend)) - 1.0;
		normal = normalize(mul(normalMap, input.WorldToTangentSpace));
	}

	// Shadows
	float shadowTerm = CalcShadow(input.WorldPosition);

	float3 emission = float3(0, 0, 0);
	float4 reflection = float4(0, 0, 0, 0);

	// Base Pixel Shader
	return float4(StandardPixelShader(input.WorldPosition, normal, SpecularColor * SpecularIntensity, input.FogDistance, albedo, emission, shadowTerm, reflection), 1.0);
}

TECHNIQUE_SM4(Terrain, VertexShaderFunction, PixelShaderFunction);
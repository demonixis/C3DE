#include "PBRBase.fx"
#include "../Common/ShadowMap.fxh"

// Variables
float2 Features;
float2 TextureTiling;
float Roughness;
float Metallic;

// Textures
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
	float3 mainTex = SAMPLE_TEXTURE(grass, uv * TextureTiling);
	float3 sandTex = SAMPLE_TEXTURE(sand, uv * TextureTiling);
	float3 rockTex = SAMPLE_TEXTURE(rock, uv * TextureTiling);
	float3 snowTex = SAMPLE_TEXTURE(snow, uv * TextureTiling);
	float3 weightTex = SAMPLE_TEXTURE(WeightMap, uv);

	float3 blend = clamp(1.0 - weightTex.r - weightTex.g - weightTex.b, 0, 1);
	blend *= mainTex;
	blend += weightTex.r * sandTex + weightTex.g * rockTex + weightTex.b * snowTex;

	return blend;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 albedo = pow(BlendTextures(GrassMap, SandMap, RockMap, SnowMap, input.UV), TO_LINEAR);

	float3 normal = input.WorldNormal;
	if (Features.x > 0)
	{
		float3 normalBlend = BlendTextures(GrassNormalMap, SandNormalMap, RockNormalMap, SnowNormalMap, input.UV);
		float3 normalMap = (2.0 * (normalBlend)) - 1.0;
		normal = normalize(mul(normalMap, input.WorldToTangentSpace));
	}

	// Shadows
	float shadowTerm = CalcShadow(input.WorldPosition);

	// PBR Lighting
	return float4(PBRPixelShader(input.WorldPosition, normal, albedo, float3(Roughness, Metallic, 1), float3(0, 0, 0), shadowTerm), 1);
}

TECHNIQUE_SM4(PBR, VertexShaderFunction, PixelShaderFunction);

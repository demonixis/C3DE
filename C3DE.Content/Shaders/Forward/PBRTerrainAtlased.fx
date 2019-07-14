#include "PBRBase.fx"
#include "../Common/ShadowMap.fxh"

// Variables
float2 Features;
float2 TextureTiling;

// Textures
DECLARE_TEXTURE(CombinedAlbedos, 1);
DECLARE_TEXTURE(CombinedNormals, 2);
DECLARE_TEXTURE(CombinedRMAOs, 3);
DECLARE_TEXTURE(WeightMap, 4);

// Four textures merged into one.
// Grass: rect(0, 0, 0.5, 0.5)
// Sand: rect(0.5, 0, 0.5, 0.5)
// Rock: rect(0, 0.5, 0.5, 0.5)
// Snow: rect(0.5, 0.5, 0.5, 0.5)
// ----------------
// |       |      |
// | Grass | Sand |
// |       |      |
// ----------------
// |       |      |
// | rock  | Snow |
// |       |      |
// -----------------
float3 BlendTextures(sampler2D target, float2 uv)
{
	float2 uv2 = uv * 0.5;
	float2 upperLeft = float2(0.0 + uv2.x, 0.0 + uv2.y);
	float2 upperRight = float2(0.5 + uv2.x, 0.0 + uv2.y);
	float2 bottomLeft = float2(0.0 + uv2.x, 0.5 + uv2.y);
	float2 bottomRight = float2(0.5 + uv2.x, 0.5 + uv2.y);

	float3 mainTex = tex2D(target, upperLeft);
	float3 sandTex = tex2D(target, upperRight);
	float3 rockTex = tex2D(target, bottomLeft);
	float3 snowTex = tex2D(target, bottomRight);
	float3 weightTex = SAMPLE_TEXTURE(WeightMap, uv);

	float3 blend = clamp(1.0 - weightTex.r - weightTex.g - weightTex.b, 0, 1);
	blend *= mainTex;
	blend += weightTex.r * sandTex + weightTex.g * rockTex + weightTex.b * snowTex;

	return blend;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 albedo = pow(BlendTextures(CombinedAlbedos, input.UV), TO_LINEAR);
	float3 rmao = BlendTextures(CombinedRMAOs, input.UV);

	float3 normal = input.WorldNormal;
	if (Features.x > 0)
	{
		float3 normalBlend = BlendTextures(CombinedNormals, input.UV);
		float3 normalMap = (2.0 * (normalBlend)) - 1.0;
		normal = normalize(mul(normalMap, input.WorldToTangentSpace));
	}

	// Shadows
	float shadowTerm = CalcShadow(input.WorldPosition);

	// PBR Lighting
	return float4(PBRPixelShader(input.WorldPosition, normal, albedo, rmao, float3(0, 0, 0), shadowTerm), 1);
}

TECHNIQUE_SM4(PBR, VertexShaderFunction, PixelShaderFunction);

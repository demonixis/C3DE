#include "PBRBase.fx"
#include "../Common/ShadowMap.fxh"

// Variables
float3 Features;
float2 TextureTiling;
float3 DiffuseColor;
float Cutout;

// Textures
DECLARE_TEXTURE(AlbedoMap, 1);
DECLARE_TEXTURE(NormalMap, 2);
DECLARE_TEXTURE(RMAOMap, 3);
DECLARE_TEXTURE(EmissiveMap, 4);

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float2 scaledUV = input.UV * TextureTiling;
	float4 albedo = SAMPLE_TEXTURE(AlbedoMap, scaledUV);
	float4 rmsao = SAMPLE_TEXTURE(RMAOMap, scaledUV);
	float3 normal = input.WorldNormal;

	// Cutout
	if (Features.z > 0)
		clip(albedo.a <= Cutout ? -1 : 1);

	if (Features.x > 0)
	{
		normal = (2.0 * (SAMPLE_TEXTURE(NormalMap, scaledUV).xyz)) - 1.0;
		normal = normalize(mul(normal, input.WorldToTangentSpace));
	}

	// Shadows
	float shadowTerm = CalcShadow(input.WorldPosition);

	// Emission
	float3 emissive = FLOAT3(0);
	if (Features.y > 0)
		emissive = SAMPLE_TEXTURE(EmissiveMap, scaledUV).rgba;

	// PBR Lighting
	return float4(PBRPixelShader(input.WorldPosition, normal, pow(albedo.rgb, TO_LINEAR) * DiffuseColor, rmsao, emissive, shadowTerm), albedo.a);
}

TECHNIQUE_SM4(PBR, VertexShaderFunction, PixelShaderFunction);
TECHNIQUE_SM4(PBR_Instancing, MainVS_Instancing, PixelShaderFunction);

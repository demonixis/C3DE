#include "PBRForward.fx"
#include "../Common/ShadowMap.fxh"

// Variables
float2 Features;
float2 TextureTiling;

// Textures
DECLARE_TEXTURE(AlbedoMap, 1);
DECLARE_TEXTURE(NormalMap, 2);
DECLARE_TEXTURE(RMAOMap, 3);
DECLARE_TEXTURE(EmissiveMap, 4);

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 albedo = pow(SAMPLE_TEXTURE(AlbedoMap, input.UV * TextureTiling).xyz, TO_LINEAR);
	float4 rmsao = SAMPLE_TEXTURE(RMAOMap, input.UV * TextureTiling);
	float3 normal = input.WorldNormal;

	if (Features.x > 0)
	{
		normal = (2.0 * (SAMPLE_TEXTURE(NormalMap, input.UV * TextureTiling).xyz)) - 1.0;
		normal = normalize(mul(normal, input.WorldToTangentSpace));
	}

	// Shadows
	float shadowTerm = CalcShadow(input.WorldPosition);

	// Emission
	float3 emissive = FLOAT3(0);
	if (Features.y > 0)
		emissive = SAMPLE_TEXTURE(EmissiveMap, input.UV * TextureTiling).rgba;

	// PBR Lighting
	return float4(PBRPixelShader(input.WorldPosition, normal, albedo, rmsao, emissive, shadowTerm), 1);
}

TECHNIQUE_SM4(PBR, VertexShaderFunction, PixelShaderFunction);

#include "PBRBase.fx"
#include "../Common/ShadowMap.fxh"

// Variables
float2 Features;
float2 TextureTiling;
float TotalTime;
float Alpha;

// Textures
DECLARE_TEXTURE(AlbedoMap, 1);
DECLARE_TEXTURE(NormalMap, 2);
DECLARE_TEXTURE(RMAOMap, 3);

VertexShaderOutput VertexShaderWaterFunction(VertexShaderInput input)
{
	input.Position.z += sin((TotalTime * 16.0) + (input.Position.y / 1.0)) / 16.0;
	input.Position.y += sin(1.0 * input.Position.y + (TotalTime * 5.0)) * 0.25;

	return VertexShaderFunction(input);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float2 uv = input.UV;
	uv.x = uv.x * 20.0 + sin(TotalTime * 3.0 + 10.0) / 256.0;
	uv.y = uv.y * 20.0;

	float3 albedo = pow(SAMPLE_TEXTURE(AlbedoMap, uv * TextureTiling).xyz, TO_LINEAR);
	float4 rmsao = SAMPLE_TEXTURE(RMAOMap, uv * TextureTiling);
	float3 normal = input.WorldNormal;

	if (Features.x > 0)
	{
		normal = (2.0 * (SAMPLE_TEXTURE(NormalMap, uv * TextureTiling).xyz)) - 1.0;
		normal = normalize(mul(normal, input.WorldToTangentSpace));
	}

	// Shadows
	float shadowTerm = CalcShadow(input.WorldPosition);

	// Emission
	float3 emissive = FLOAT3(0);

	// PBR Lighting
	return float4(PBRPixelShader(input.WorldPosition, normal, albedo, rmsao, emissive, shadowTerm), Alpha);
}
 
technique Water
{
	pass WaterPass
	{
		AlphaBlendEnable = TRUE;
		DestBlend = INVSRCALPHA;
		SrcBlend = SRCALPHA;
#if SM4
		VertexShader = compile vs_4_0 VertexShaderWaterFunction();
		PixelShader = compile ps_4_0 PixelShaderFunction();
#else
		VertexShader = compile vs_3_0 VertexShaderWaterFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
#endif
	}
}

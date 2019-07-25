#include "StandardBase.fx"

// Misc
float2 Features;
float2 TextureTiling;
float TotalTime;
float3 SpecularColor;
float SpecularIntensity;

DECLARE_TEXTURE(AlbedoMap, 1);
DECLARE_TEXTURE(NormalMap, 2);
DECLARE_TEXTURE(SpecularMap, 3);

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 noise = SAMPLE_TEXTURE(NormalMap, input.UV * TextureTiling);
	float2 T1 = (input.UV + float2(1.5, -1.5) * TotalTime * 0.02) * TextureTiling;
	float2 T2 = (input.UV + float2(-0.5, 2.0) * TotalTime * 0.01) * TextureTiling;

	T1.x += noise.x * 2.0;
	T1.y += noise.y * 2.0;
	T2.x -= noise.y * 0.2;
	T2.y += noise.z * 0.2;

	float p = SAMPLE_TEXTURE(NormalMap, (T1 * 3.0)).a;

	float3 color = SAMPLE_TEXTURE(AlbedoMap, (T2 * 4.0)).xyz;
	float3 albedo = color * (float3(p, p, p) * 2.0) + (color * color - 0.1);

	if (albedo.r > 1.0)
		albedo.bg += clamp(albedo.r - 2.0, 0.0, 100.0);

	if (albedo.g > 1.0)
		albedo.rb += albedo.g - 1.0;

	if (albedo.b > 1.0)
		albedo.rg += albedo.b - 1.0;

	float3 normal = input.WorldNormal;

	if (Features.x > 0)
	{
		normal = 2.0 * (SAMPLE_TEXTURE(NormalMap, (T2 * 4.0))) - 1.0;
		normal = normalize(mul(normal, input.WorldToTangentSpace));
	}

	// Specular
	float specular = SpecularColor * SpecularIntensity;
	if (Features.y > 0)
		specular *= SAMPLE_TEXTURE(SpecularMap, (T2 * 4.0));

	// Base Pixel Shader
	return float4(StandardPixelShader(input.WorldPosition, normal, specular, input.FogDistance, albedo, FLOAT3(0), 1.0, FLOAT4(0)), 1.0);
}

TECHNIQUE_SM4(Lava, VertexShaderFunction, PixelShaderFunction);
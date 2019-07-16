#include "PBRBase.fx"

// Variables
// x: NormalMap Enabled
float2 Features;
float2 TextureTiling;
float TotalTime;
float Metallic;
float Roughness;

// Textures
DECLARE_TEXTURE(AlbedoMap, 1);
DECLARE_TEXTURE(NormalMap, 2);

VertexShaderOutput VertexShaderLavaFunction(VertexShaderInput input)
{
	input.UV *= float2(0.5, 0.5);
	return VertexShaderFunction(input);
}

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
	
    float3 color = pow(SAMPLE_TEXTURE(AlbedoMap, (T2 * 4.0)), TO_LINEAR).xyz;
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

	// PBR Lighting
	return float4(PBRPixelShader(input.WorldPosition, normal, albedo, float3(Roughness, Metallic, 1), FLOAT3(0), 1.0), 1.0);
}
 
TECHNIQUE_SM4(PBR, VertexShaderLavaFunction, PixelShaderFunction);

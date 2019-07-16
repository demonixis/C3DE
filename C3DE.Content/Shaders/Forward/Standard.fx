#include "StandardBase.fx"
#include "../Common/ShadowMap.fxh"

// Variables
float4 Features;

// Material
float3 DiffuseColor;
float2 TextureTiling = float2(1, 1);
float3 EmissiveColor;
float SpecularColor;
float EmissiveIntensity;
float ReflectionIntensity;
float Cutout;

DECLARE_TEXTURE(AlbedoMap, 1);
DECLARE_TEXTURE(NormalMap, 2);
DECLARE_TEXTURE(SpecularMap, 3);
DECLARE_TEXTURE(EmissiveMap, 4);
DECLARE_TEXTURE(ReflectionMap, 5);

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{ 
	float2 scaledUV = input.UV * TextureTiling;

	// Albedo
	float4 albedo = SAMPLE_TEXTURE(AlbedoMap, scaledUV);

	// Cutout
	if (Features.z > 0)
		clip(albedo.a <= Cutout ? -1 : 1);
	
	// Normal
	float3 normal = input.WorldNormal;
    if (Features.x > 0)
    {
        float3 normalMap = (2.0 * (SAMPLE_TEXTURE(NormalMap, scaledUV))) - 1.0;
		normal = normalize(mul(normalMap, input.WorldToTangentSpace));
    }

	// Specular
	float specularTerm = SpecularColor;
	
	if (Features.w > 0)
		specularTerm = SAMPLE_TEXTURE(SpecularMap, scaledUV).r;
	
	// Emissive
	float3 emissive = EmissiveColor * EmissiveIntensity;
    if (Features.y > 0)
		emissive = SAMPLE_TEXTURE(EmissiveMap, scaledUV).xyz * EmissiveIntensity;

	// Shadows
	float shadowTerm = CalcShadow(input.WorldPosition);

	// Reflection
	float4 reflection = float4(0, 0, 0, 0);

	if (ReflectionIntensity > 0)
	{
		float2 projectedUV = float2(input.Reflection.x / input.Reflection.w / 2.0 + 0.5, -input.Reflection.y / input.Reflection.w / 2.0 + 0.5);
		float3 reflectionColor = SAMPLE_TEXTURE(ReflectionMap, projectedUV).xyz;
		reflection = float4(reflectionColor, ReflectionIntensity);
	}

	// Base Pixel Shader
	return float4(StandardPixelShader(input.WorldPosition, normal, specularTerm, input.FogDistance, albedo.rgb * DiffuseColor, emissive, shadowTerm, reflection), albedo.a);
}

TECHNIQUE_SM4(Standard, VertexShaderFunction, PixelShaderFunction);
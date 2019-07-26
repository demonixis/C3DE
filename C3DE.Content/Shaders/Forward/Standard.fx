#include "StandardBase.fx"
#include "../Common/ShadowMap.fxh"

// Variables
float4 Features;

// Material
float3 DiffuseColor;
float2 TextureTiling = float2(1, 1);
float3 EmissiveColor;
float3 SpecularColor;
float EmissiveIntensity;
float SpecularIntensity;
float ReflectionIntensity;
float Cutout;

DECLARE_TEXTURE(AlbedoMap, 1);
DECLARE_TEXTURE(NormalMap, 2);
DECLARE_TEXTURE(SpecularMap, 3);
DECLARE_TEXTURE(EmissiveMap, 4);
DECLARE_CUBEMAP(ReflectionMap, 5);

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
	float3 specular = SpecularColor * SpecularIntensity;
	
	if (Features.w > 0)
		specular *= SAMPLE_TEXTURE(SpecularMap, scaledUV).rgb;
	
	// Emissive
	float3 emissive = float3(0, 0, 0);
	if (EmissiveIntensity > 0)
	{
		emissive = EmissiveColor * EmissiveIntensity;

		if (Features.y > 0)
			emissive *= SAMPLE_TEXTURE(EmissiveMap, scaledUV).xyz;
	}
	
	// Shadows
	float shadowTerm = CalcShadow(input.WorldPosition);

	// Reflection
	float4 reflection = float4(0, 0, 0, 0);
	if (ReflectionIntensity > 0)
	{
		reflection = float4(SAMPLE_CUBEMAP(ReflectionMap, normalize(input.Reflection)).rgb, ReflectionIntensity);

		// FIXME: Temporary hack
		if (ReflectionIntensity > 1)
			return float4(reflection.xyz, 1);
	}
	// Base Pixel Shader
	return float4(StandardPixelShader(input.WorldPosition, normal, specular, input.FogDistance, albedo.rgb * DiffuseColor, emissive, shadowTerm, reflection), albedo.a);
}

// Per Pixel Lighting
TECHNIQUE_SM4(Standard, VertexShaderFunction, PixelShaderFunction);
TECHNIQUE_SM4(Standard_Instancing, MainVS_Instancing, PixelShaderFunction);
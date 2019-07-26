#include "StandardBase.fx"
#include "../Common/ShadowMap.fxh"

// Variables
float3 DiffuseColor;
float3 Features;
float2 TextureTiling;
float TotalTime;
float Alpha;
float3 SpecularColor;
float SpecularIntensity;
float ReflectionIntensity;

// Textures
DECLARE_TEXTURE(AlbedoMap, 1);
DECLARE_TEXTURE(NormalMap, 2);
DECLARE_TEXTURE(SpecularMap, 3);
DECLARE_CUBEMAP(ReflectionMap, 4);

VertexShaderOutput VertexShaderWaterFunction(VertexShaderInput input)
{
	input.Position.z += sin((TotalTime * 16.0) + (input.Position.y / 1.0)) / 16.0;
	input.Position.y += sin(1.0 * input.Position.y + (TotalTime * 5.0)) * 0.25;

	return VertexShaderFunction(input);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	// Custom UV
	float2 uv = input.UV;
	uv.x = uv.x * 20.0 + sin(TotalTime * 3.0 + 10.0) / 256.0;
	uv.y = uv.y * 20.0;

	float2 scaledUV = uv * TextureTiling;

	// Albedo
	float3 albedo = SAMPLE_TEXTURE(AlbedoMap, scaledUV).xyz * DiffuseColor;

	// Normal
	float3 normal = input.WorldNormal;
	if (Features.x > 0)
	{
		normal = (2.0 * (SAMPLE_TEXTURE(NormalMap, scaledUV).xyz)) - 1.0;
		normal = normalize(mul(normal, input.WorldToTangentSpace));
	}

	// Specular
	float specular = SpecularColor * SpecularIntensity;
	if (Features.y > 0)
		specular *= SAMPLE_TEXTURE(SpecularMap, scaledUV).r;

	// Shadows
	float shadowTerm = CalcShadow(input.WorldPosition);

	// Reflection
	// Reflection
	float4 reflection = float4(0, 0, 0, 0);
	if (ReflectionIntensity > 0)
		reflection = float4(SAMPLE_CUBEMAP(ReflectionMap, normalize(input.Reflection)).rgb, ReflectionIntensity);
	
	float3 emission = float3(0, 0, 0);

	// Base Pixel Shader
	return float4(StandardPixelShader(input.WorldPosition, normal, specular, input.FogDistance, albedo, emission, shadowTerm, reflection), Alpha);
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

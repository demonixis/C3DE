#include "StandardBase.fx"

// Variables
float3 DiffuseColor;
float2 TextureTiling;
float3 SpecularColor;
float SpecularIntensity;
float TotalTime;
float Alpha;

// Textures
texture AlbedoMap;
sampler albedoSampler = sampler_state
{
    Texture = (AlbedoMap);
    MAGFILTER = LINEAR;
    MINFILTER = LINEAR;
    MIPFILTER = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

VertexShaderOutput VertexShaderWaterFunction(VertexShaderInput input)
{
	input.Position.z += sin((TotalTime * 16.0) + (input.Position.y / 1.0)) / 16.0;
	input.Position.x += sin(1.0 * input.Position.y + (TotalTime * 5.0)) * 0.25;

	return MainVS(input);
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
	// Custom UV
	float2 uv = input.UV;
	uv.x = uv.x * 20.0 + sin(TotalTime * 3.0 + 10.0) / 256.0;
	uv.y = uv.y * 20.0;

	float2 scaledUV = uv * TextureTiling;

	// Albedo
	float3 albedo = tex2D(albedoSampler, scaledUV).xyz;

	// Normal
	float3 normal = input.WorldNormal;

	// Base Pixel Shader
	return float4(StandardPixelShader(input.WorldPosition, normal, SpecularColor * SpecularIntensity, albedo.rgb * DiffuseColor, float3(0, 0, 0)), Alpha);
}

technique Water
{
	pass WaterPass
	{
		AlphaBlendEnable = TRUE;
		DestBlend = INVSRCALPHA;
		SrcBlend = SRCALPHA;
#if SM4
		VertexShader = compile vs_4_0 MainVS();
		PixelShader = compile ps_4_0 MainPS();
#else
		VertexShader = compile vs_3_0 MainVS();
		PixelShader = compile ps_3_0 MainPS();
#endif
	}
}

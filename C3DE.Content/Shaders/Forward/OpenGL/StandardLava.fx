#include "StandardBase.fx"

// Misc
float2 TextureTiling;
float TotalTime;
float3 SpecularColor;
float SpecularIntensity;

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

texture NormalMap;
sampler normalSampler = sampler_state
{
    Texture = (NormalMap);
    MAGFILTER = LINEAR;
    MINFILTER = LINEAR;
    MIPFILTER = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

float4 MainPS(VertexShaderOutput input) : COLOR0
{
	float3 noise = tex2D(normalSampler, input.UV * TextureTiling);
	float2 T1 = (input.UV + float2(1.5, -1.5) * TotalTime * 0.02) * TextureTiling;
	float2 T2 = (input.UV + float2(-0.5, 2.0) * TotalTime * 0.01) * TextureTiling;

	T1.x += noise.x * 2.0;
	T1.y += noise.y * 2.0;
	T2.x -= noise.y * 0.2;
	T2.y += noise.z * 0.2;

	float p = tex2D(normalSampler, (T1 * 3.0)).a;

	float3 color = tex2D(albedoSampler, (T2 * 4.0)).xyz;
	float3 albedo = color * (float3(p, p, p) * 2.0) + (color * color - 0.1);

	if (albedo.r > 1.0)
		albedo.bg += clamp(albedo.r - 2.0, 0.0, 100.0);

	if (albedo.g > 1.0)
		albedo.rb += albedo.g - 1.0;

	if (albedo.b > 1.0)
		albedo.rg += albedo.b - 1.0;

	float3 normal = input.WorldNormal;

	// Base Pixel Shader
	return float4(StandardPixelShader(input.WorldPosition, normal, SpecularColor * SpecularIntensity, albedo, float3(0, 0, 0)), 1.0);
}

technique StandardLava
{
    pass P0
    {
#if SM4
		VertexShader = compile vs_4_0 MainVS();
		PixelShader = compile ps_4_0 MainPS();
#else
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 MainPS();
#endif
    }
}
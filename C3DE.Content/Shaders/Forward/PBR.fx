#include "../Common/PBR.fxh"
#include "../Common/ShadowMap.fxh"

// Constants
#if SM4
#define MAX_LIGHT_COUNT 64
#else
#define MAX_LIGHT_COUNT 8
#endif

const float GAMMA_CORRECTION = 0.45454545;

int Debug = 0;

// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

// Variables
float3 EyePosition;
float GammaCorrection = 2.2;
float2 Features;
float2 TextureTiling;

// Lighting
float3 LightPosition[MAX_LIGHT_COUNT];
float3 LightColor[MAX_LIGHT_COUNT];
float3 LightData[MAX_LIGHT_COUNT];
int LightCount = 0;

// Textures
texture AlbedoMap;
sampler2D albedoSampler = sampler_state
{
	Texture = (AlbedoMap);
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Point;
	AddressU = Wrap;
	AddressV = Wrap;
};

texture NormalMap;
sampler2D normalSampler = sampler_state
{
	Texture = (NormalMap);
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Point;
	AddressU = Wrap;
	AddressV = Wrap;
};

texture RMSAOMap;
sampler2D rmsaoSampler = sampler_state
{
	Texture = (RMSAOMap);
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Point;
	AddressU = Wrap;
	AddressV = Wrap;
};

texture EmissiveMap;
sampler2D emissiveSampler = sampler_state
{
	Texture = (EmissiveMap);
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Point;
	AddressU = Wrap;
	AddressV = Wrap;
};

texture IrradianceMap;
samplerCUBE irradianceSampler = sampler_state
{
	Texture = <IrradianceMap>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Mirror;
	AddressV = Mirror;
};

struct VertexShaderInput
{
#if SM4
	float4 Position : SV_Position;
#else
	float4 Position : POSITION0;
#endif
	float2 UV : TEXCOORD0;
	float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float4 WorldPosition : TEXCOORD1;
	float3 WorldNormal : TEXCOORD2;
	float3x3 WorldToTangentSpace : TEXCOORD3;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.UV = input.UV;
	output.WorldPosition = worldPosition;
	output.WorldNormal = mul(input.Normal, World);

	float3 c1 = cross(input.Normal, float3(0.0, 0.0, 1.0));
	float3 c2 = cross(input.Normal, float3(0.0, 1.0, 0.0));

	// [0] Tangent / [1] Binormal / [2] Normal
	output.WorldToTangentSpace[0] = length(c1) > length(c2) ? c1 : c2;
	output.WorldToTangentSpace[1] = normalize(output.WorldToTangentSpace[0]);
	output.WorldToTangentSpace[2] = input.Normal;

	return output;
}

float3 NewFloat3(float value)
{
	return float3(value, value, value);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 albedo = pow(tex2D(albedoSampler, input.UV * TextureTiling).xyz, NewFloat3(2.2));
	float4 rmsao = tex2D(rmsaoSampler, input.UV * TextureTiling);
	float roughness = rmsao.r;
	float metallic = rmsao.g;
	float specular = rmsao.b;
	float ao = rmsao.a;

	float3 normal = input.WorldNormal;

	if (Features.x > 0)
	{
		normal = (2.0 * (tex2D(normalSampler, input.UV * TextureTiling).xyz)) - 1.0;
		normal = normalize(mul(normal, input.WorldToTangentSpace));
	}

	float3 N = normal;
	float3 V = normalize(EyePosition - input.WorldPosition.xyz);
	float3 R = reflect(-V, N);

	float3 F0 = NewFloat3(0.04);
	F0 = lerp(F0, albedo, metallic);

	float3 Lo = NewFloat3(0.0);

	// ------
	// Lighting (one light for now)
	//---------
	for (int i = 0; i < LightCount; i++)
	{
		float3 L = normalize(LightPosition[i] - input.WorldPosition.xyz);
		float3 H = normalize(V + L);
		float3 radiance = float3(0, 0, 0);

		// Radiance
		if (LightData[i].x == 0) // Directional
		{
			radiance = LightColor[i] * LightData[i].y;
		}
		else // Point
		{
			float distance = length(LightPosition[i] - input.WorldPosition.xyz);
			float attenuation = 1.0 / distance * distance;
			radiance = LightColor[i] * attenuation * LightData[i].y;
		}

		// Cook-Torrance BRDF
		float NDF = DistributionGGX(N, H, roughness);
		float G = GeometrySmith(N, V, L, roughness);
		float3 F = FresnelSchlick(max(dot(H, V), 0.0), F0);

		float3 nominator = NDF * G * F;
		float denominator = 4.0 * max(dot(V, N), 0.0) * max(dot(L, N), 0.0) + 0.001f; // 0.001 to prevent divide by zero.
		float3 brdf = nominator / denominator;

		float3 kS = F;
		float3 kD = NewFloat3(1.0) - kS;
		kD *= 1.0 - metallic;

		// Scale light 
		float NdotL = max(dot(N, L), 0.0);

		Lo += (kD * albedo / PI + brdf) * radiance * NdotL;
	}

	// ------
	// Shadows
	// ------
	Lo *= CalcShadow(input.WorldPosition);

	// ------
	// Emissive Lighting
	// ------
	if (Features.y > 0)
		Lo += tex2D(emissiveSampler, input.UV * TextureTiling).rgba;

	// ------
	// Ambient Lighting
	//---------
	float3 kS = FresnelSchlick(max(dot(N, V), 0.0), F0);
	float3 kD = 1.0 - kS;
	kD *= 1.0 - metallic;
	float3 irradiance = texCUBE(irradianceSampler, N).xyz;
	float3 diffuse = irradiance * albedo;
	float3 ambient = (kD * diffuse) * ao;
	float3 color = ambient + Lo;

	// HDR + Gamma correction
	color = color / (color + NewFloat3(1.0));
	color = pow(color, NewFloat3(1.0 / GammaCorrection));

	return float4(color, 1.0);
}

technique PBR
{
	pass PBRPass
	{
#if SM4
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 PixelShaderFunction();
#else
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
#endif
	}
}

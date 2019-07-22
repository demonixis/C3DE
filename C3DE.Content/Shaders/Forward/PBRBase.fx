#include "../Common/Macros.fxh"
#include "../Common/PBR.fxh"

// Constants
#if SM4
#define MAX_LIGHT_COUNT 128
#else
#define MAX_LIGHT_COUNT 16
#endif

const float3 TO_GAMMA = float3(0.45454545, 0.45454545, 0.45454545);
const float3 TO_LINEAR = float3(2.2, 2.2, 2.2);

// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

// Variables
float3 EyePosition;

// Lighting
float3 LightPosition[MAX_LIGHT_COUNT];
float3 LightColor[MAX_LIGHT_COUNT];
float4 LightData[MAX_LIGHT_COUNT];
int LightCount = 0;

DECLARE_CUBEMAP(IrradianceMap, 13);

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

VertexShaderOutput CommonVS(VertexShaderInput input, float4x4 instanceTransform)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, instanceTransform);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.UV = input.UV;
	output.WorldPosition = worldPosition;
	output.WorldNormal = mul(input.Normal, instanceTransform);

	float3 c1 = cross(input.Normal, float3(0.0, 0.0, 1.0));
	float3 c2 = cross(input.Normal, float3(0.0, 1.0, 0.0));

	// [0] Tangent / [1] Binormal / [2] Normal
	output.WorldToTangentSpace[0] = length(c1) > length(c2) ? c1 : c2;
	output.WorldToTangentSpace[1] = normalize(output.WorldToTangentSpace[0]);
	output.WorldToTangentSpace[2] = input.Normal;

	return output;
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	return CommonVS(input, World);
}

VertexShaderOutput MainVS_Instancing(VertexShaderInput input, float4x4 instanceTransform : BLENDWEIGHT)
{
	return CommonVS(input, mul(World, transpose(instanceTransform)));
}

float3 PBRPixelShader(float4 worldPosition, float3 normal, float3 albedo, float3 rmao, float3 emissive, float shadowTerm)
{
	float roughness = rmao.r;
	float metallic = rmao.g;
	float ao = rmao.b;

	float3 N = normal;
	float3 V = normalize(EyePosition - worldPosition.xyz);
	float3 R = reflect(-V, N);

	float3 F0 = FLOAT3(0.04);
	F0 = lerp(F0, albedo, metallic);

	float3 Lo = FLOAT3(0.0);

	// ------
	// Lighting
	//---------
	for (int i = 0; i < LightCount; i++)
	{
		float3 L = normalize(LightPosition[i] - worldPosition.xyz);
		float3 H = normalize(V + L);
		float3 radiance = float3(0, 0, 0);

		// Radiance
		if (LightData[i].x == 0) // Directional
		{
			radiance = LightColor[i] * LightData[i].y;
		}
		else // Point
		{
			float distance = length(LightPosition[i] - worldPosition.xyz);
			float attenuation = 1.0 / distance * distance;
			//attenuation = 1.0 / LightData[i].z;
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
		float3 kD = FLOAT3(1.0) - kS;
		kD *= 1.0 - metallic;

		// Scale light 
		float NdotL = max(dot(N, L), 0.0);

		Lo += (kD * albedo / PI + brdf) * radiance * NdotL;
	}

	// ------
	// Shadows
	// ------
	Lo *= shadowTerm;

	// ------
	// Emissive Lighting
	// ------
	Lo += emissive;

	// ------
	// Ambient Lighting
	//---------
	float3 kS = FresnelSchlick(max(dot(N, V), 0.0), F0);
	float3 kD = 1.0 - kS;
	kD *= 1.0 - metallic;
	float3 irradiance = SAMPLE_CUBEMAP(IrradianceMap, N).xyz;
	float3 diffuse = irradiance * albedo;
	float3 ambient = (kD * diffuse) * ao;
	float3 color = ambient + Lo;

	// HDR + Gamma correction
	color = color / (color + FLOAT3(1.0));
	color = pow(color, TO_GAMMA);

	return color;
}
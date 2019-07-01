// Constants
const float PI = 3.14159265359;
const float GAMMA_CORRECTION = 0.45454545;

// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

// Variables
float3 EyePosition;
float GammaCorrection = 2.2;
float2 Features;

// Lighting
float3 LightPosition;
float3 LightColor;

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

float DistributionGGX(float3 N, float3 H, float roughness)
{
	float a = roughness * roughness;
	float a2 = a * a;
	float NdotH = max(dot(N, H), 0.0);
	float NdotH2 = NdotH * NdotH;

	float nom = a2;
	float denom = (NdotH2 * (a2 - 1.0) + 1.0);
	denom = PI * denom * denom;

	return nom / denom;
}

float GeometrySchlickGGX(float NdotV, float roughness)
{
	float r = (roughness + 1.0);
	float k = (r * r) / 8.0;

	float nom = NdotV;
	float denom = NdotV * (1.0 - k) + k;

	return nom / denom;
}

float GeometrySmith(float3 N, float3 V, float3 L, float roughness)
{
	float NdotV = max(dot(N, V), 0.0);
	float NdotL = max(dot(N, L), 0.0);
	float ggx2 = GeometrySchlickGGX(NdotV, roughness);
	float ggx1 = GeometrySchlickGGX(NdotL, roughness);

	return ggx1 * ggx2;
}

float3 FresnelSchlick(float cosTheta, float3 F0)
{
	return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}

float3 NewFloat3(float value)
{
	return float3(value, value, value);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 albedo = pow(tex2D(albedoSampler, input.UV).xyz, NewFloat3(2.2));
	float4 rmsao = tex2D(rmsaoSampler, input.UV);
	float roughness = rmsao.r;
	float metallic = rmsao.g;
	float specular = rmsao.b;
	float ao = rmsao.a;

	float3 normal = input.WorldNormal;
	
	if (Features.x > 0)
	{
		normal = (2.0 * (tex2D(normalSampler, input.UV).xyz)) - 1.0;
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
	{
		// Radiance
		float3 L = normalize(LightPosition - input.WorldPosition.xyz);
		float3 H = normalize(V + L);
		float distance = length(LightPosition - input.WorldPosition.xyz);
		float attenuation = 1.0 / distance * distance;
		float3 radiance = LightColor * attenuation;

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
	// Emissive Lighting
	// ------
	if (Features.y > 0)
		Lo += tex2D(emissiveSampler, input.UV).xyz;

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
		VertexShader = compile vs_4_0_level_9_3 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_3 PixelShaderFunction();
#else
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
#endif
	}
}

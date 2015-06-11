#include "ShadowMap.fxh"
#include "Fog.fxh"

// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

// Material
float3 AmbientColor = float4(0.1, 0.1, 0.1);
float3 DiffuseColor = float4(1.0, 1.0, 1.0);
float3 EmissiveColor = float4(0.0, 0.0, 0.0);
float3 SpecularColor = float4(0.8, 0.8, 0.8);
float Shininess = 200.0;

// Lighting
float3 LightColor;
float3 LightDirection;
float3 LightPosition;
float LightIntensity;
float LightSpotAngle;
float LightRange;
int LightFallOff;
int NbLights;
int LightType = 0;

// Misc
float3 EyePosition = float3(1, 1, 0);
float2 TextureTiling = float2(1, 1);
float2 TextureOffset = float2(0, 0);

texture MainTexture;
sampler2D textureSampler = sampler_state
{
	Texture = (MainTexture);
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

float4 CalcDirectionalLightColor(float3 normal, float4 worldPosition)
{
	float3 diffuse = saturate(dot(normal, LightDirection));
	return float4(diffuse * LightColor * LightIntensity, 1.0);
}

float4 CalcPointLightColor(float3 normal, float4 worldPosition)
{
	float3 lightDirection = normalize(LightPosition - worldPosition);
	float diffuse = saturate(dot(normal, lightDirection));
	float d = distance(LightPosition, worldPosition);
	float attenuation = 1 - pow(clamp(d / LightRange, 0, 1), LightFallOff);

	return float4(diffuse * attenuation * LightColor * LightIntensity, 1.0);
}

float4 CalcSpotLightColor(float3 normal, float4 worldPosition)
{
	float3 lightDirection = normalize(LightPosition - worldPosition);
	float3 diffuse = saturate(dot(normal, lightDirection));

	float d = dot(lightDirection, normalize(LightDirection));
	float a = cos(LightSpotAngle);

	float attenuation = 1.0;

	if (a < d)
		attenuation = 1 - pow(clamp(a / d, 0, 1), LightFallOff);

	return float4(diffuse * attenuation * LightColor * LightIntensity, 1.0);
}

float4 CalcSpecularColor(float3 normal, float4 worldPosition, float4 color, int type)
{
	if (type == 0)
		return float4(0, 0, 0, 1);

	float3 lightDirection = LightDirection;

	//  Point light 
	if (type == 2)
		lightDirection = normalize(LightPosition - worldPosition);

	float3 R = normalize(2 * color.xyz * normal - lightDirection);

	return SpecularColor * pow(saturate(dot(R, normalize(LightPosition - worldPosition))), Shininess);
}

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
	float3 Normal : TEXCOORD1;
	float4 WorldPosition : TEXCOORD2;
	float FogDistance : FOG;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.UV = input.UV;
	output.Normal = mul(input.Normal, World);
	output.WorldPosition = worldPosition;
	output.FogDistance = distance(worldPosition, EyePosition);

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 baseDiffuse = DiffuseColor * tex2D(textureSampler, (input.UV + TextureOffset) * TextureTiling);
	float4 lightFactor = float4(1, 1, 1, 1);
	float3 normal = normalize(input.Normal);
	float shadowTerm = CalcShadow(input.WorldPosition);

	// Apply a light influence.
	if (LightType == 1)
		lightFactor = CalcDirectionalLightColor(normal, input.WorldPosition);
	else if (LightType == 2)
		lightFactor = CalcPointLightColor(normal, input.WorldPosition);
	else if (LightType == 3)
		lightFactor = CalcSpotLightColor(normal, input.WorldPosition);

	float4 finalDiffuse = baseDiffuse * lightFactor * shadowTerm;
	float4 finalSpecular = CalcSpecularColor(normal, input.WorldPosition, finalDiffuse, LightType);
	float4 finalCompose = AmbientColor + finalDiffuse + finalSpecular + EmissiveColor;
	
	return ApplyFog(finalCompose, input.FogDistance);
}

technique Textured
{
	pass Pass1
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
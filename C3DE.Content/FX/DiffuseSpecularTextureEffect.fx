static const float2 poissonDisk[4] = {
	float2(-0.94201624, -0.39906216),
	float2(0.94558609, -0.76890725),
	float2(-0.094184101, -0.92938870),
	float2(0.34495938, 0.29387760)
};

// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

// Renderer
float4 AmbientColor = float4(1.0, 1.0, 1.0, 1.0);
float AmbientIntensity = 0.1;

// Object
float4 EmissiveColor = float4(0.0, 0.0, 0.0, 1.0);

// Light 0
float4x4 LightView0;
float4x4 LightProjection0;
float3 DiffuseLightDirection = float3(1.0, 0.0, 0.0);
float4 DiffuseColor = float4(1.0, 1.0, 1.0, 1.0);
float DiffuseIntensity = 1.0;

// Specular 
float Shininess = 200.0;
float4 SpecularColor = float4(1.0, 1.0, 1.0, 1.0);
float SpecularIntensity = 1.0;
float3 ViewVector = float3(1.0, 0.0, 0.0);

// Shadow map
bool ShadowMapEnabled = true;
float ShadowMapSize = 512;
float ShadowBias = 0.05;
float ShadowStrength = 1.0;
float ShadowSamples = 0;
bool RecieveShadows = true;

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

texture ShadowMap;
sampler2D shadowSampler = sampler_state
{
	Texture = (ShadowMap);
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

struct VertexShaderInput
{
#if SM4
	float4 Position : SV_Position;
#else
	float4 Position : POSITION0;
#endif
	float4 Normal : NORMAL0;
	float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
	float3 Normal : TEXCOORD0;
	float2 TextureCoordinate : TEXCOORD1;
	float4 WorldPosition : TEXCOORD2;
};

float calcShadowPCF(float lightSpaceDepth, float2 shadowCoordinates)
{
	float size = 1.0 / ShadowMapSize;
	float samples[4];
	float gradiant = lightSpaceDepth - ShadowBias;

	samples[0] = (gradiant < tex2D(shadowSampler, shadowCoordinates).r) ? 1.0 : 0.0;
	samples[1] = (gradiant < tex2D(shadowSampler, shadowCoordinates + float2(size, 0)).r) ? 1.0 : 0.0;
	samples[2] = (gradiant < tex2D(shadowSampler, shadowCoordinates + float2(0, size)).r) ? 1.0 : 0.0;
	samples[3] = (gradiant < tex2D(shadowSampler, shadowCoordinates + float2(size, size)).r) ? 1.0 : 0.0;

	return (samples[0] + samples[1] + samples[2] + samples[3]) / 4.0;
}

float shadowPoissonSampling(float lightSpaceDepth, float2 shadowCoordinates)
{
	float shadowTerm = 0;
	float samples = 8;
	float visibility = 1.0f;

	visibility -= (1 / ShadowSamples) * (lightSpaceDepth < tex2D(shadowSampler, shadowCoordinates + poissonDisk[0] / 100).r);
	visibility -= (1 / ShadowSamples) * (lightSpaceDepth < tex2D(shadowSampler, shadowCoordinates + poissonDisk[1] / 100).r);
	visibility -= (1 / ShadowSamples) * (lightSpaceDepth < tex2D(shadowSampler, shadowCoordinates + poissonDisk[2] / 100).r);
	visibility -= (1 / ShadowSamples) * (lightSpaceDepth < tex2D(shadowSampler, shadowCoordinates + poissonDisk[3] / 100).r);

	return 1.0 - visibility;
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	float4 normal = normalize(mul(input.Normal, WorldInverseTranspose));
	float lightIntensity = dot(normal, float4(DiffuseLightDirection, 1.0));

	output.Position = mul(viewPosition, Projection);
	float4 color = DiffuseColor * DiffuseIntensity * lightIntensity;
	
	output.Color = saturate(color);
	output.Normal = float3(normal.x, normal.y, normal.z);
	output.TextureCoordinate = input.TextureCoordinate;
	output.WorldPosition = worldPosition;

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	// Specular
	float3 light = normalize(DiffuseLightDirection);
	float3 normal = normalize(input.Normal);
	float3 r = normalize(2 * dot(light, normal) * normal - light);
	float3 v = (float3)normalize(mul(normalize(float4(ViewVector, 1.0)), World));
	float dotProduct = dot(r, v);
	float4 specular = SpecularIntensity * SpecularColor * max(pow(abs(dotProduct), Shininess), 0) * length(input.Color);

	// Shadow Map.
	float shadowTerm = 1.0;

	if (ShadowMapEnabled == true && RecieveShadows == true)
	{
		// Transform to light space
		float4 lightSpacePosition = mul(mul(input.WorldPosition, LightView0), LightProjection0);
		lightSpacePosition -= ShadowBias;
		lightSpacePosition /= lightSpacePosition.w;

		float2 screenPosition = 0.5 + float2(lightSpacePosition.x, -lightSpacePosition.y) * 0.5;

		float shadow = 1.0;

		if (ShadowSamples > 0)
			 shadow = shadowPoissonSampling(lightSpacePosition.z, screenPosition);
		else
			shadow = calcShadowPCF(lightSpacePosition.z, screenPosition);

		if ((saturate(screenPosition).x == screenPosition.x) && (saturate(screenPosition).y == screenPosition.y))
			shadowTerm = max(ShadowStrength, shadow);
	}

	// Final composition.
	float4 textureColor = tex2D(textureSampler, input.TextureCoordinate);
	textureColor.a = 1;

	return saturate(textureColor * input.Color * shadowTerm + AmbientColor * AmbientIntensity + specular + EmissiveColor);
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
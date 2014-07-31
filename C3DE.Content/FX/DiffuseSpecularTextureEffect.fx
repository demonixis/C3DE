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
float DiffuseOffset = 0.0;

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
	float4 Position : POSITION0;
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

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	float4 normal = normalize(mul(input.Normal, WorldInverseTranspose));
	float lightIntensity = dot(normal, float4(DiffuseLightDirection, 0.0));

	output.Position = mul(viewPosition, Projection);
	float4 color = DiffuseColor * DiffuseIntensity;
	
	if (RecieveShadows == true)
		color *= lightIntensity + DiffuseOffset;
	
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

	if (ShadowMapEnabled == true)
	{
		// Transform to light space
		float4 lightSpacePosition = mul(mul(input.WorldPosition, LightView0), LightProjection0);
		lightSpacePosition -= ShadowBias;
		lightSpacePosition /= lightSpacePosition.w;

		float2 screenPosition = 0.5 + float2(lightSpacePosition.x, -lightSpacePosition.y) * 0.5;

		if ((saturate(screenPosition).x == screenPosition.x) && (saturate(screenPosition).y == screenPosition.y))
			shadowTerm = max(ShadowStrength, calcShadowPCF(lightSpacePosition.z, screenPosition));
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
		VertexShader = compile vs_4_0_level_9_1 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 PixelShaderFunction();
#else
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
#endif
	}
}
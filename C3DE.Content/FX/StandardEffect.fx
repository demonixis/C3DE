float4x4 World;
float4x4 View;
float4x4 Projection;
// Light
float4x4 lightView;
float4x4 lightProjection;
float3 lightPosition;
float3 lightRadius = float3(100.0, 100.0, 100.0);
bool receiveShadow = true;
float4 emissiveColor = float4(0.0, 0.0, 0.0, 1.0);
// Shadow
bool shadowMapEnabled = true;
float shadowMapSize = 512;
float shadowBias = 0.05;
float shadowStrength = 1.0;
// Renderer
float4 ambientColor = float4(1.0, 1.0, 1.0, 1.0);

texture mainTexture;
sampler textureSampler = sampler_state
{
	texture = (mainTexture);
	minfilter = linear;
	magfilter = linear;
	mipfilter = linear;
	addressu = wrap;
	addressv = wrap;
};

texture shadowTexture;
sampler shadowSampler = sampler_state
{
	texture = (shadowTexture);
	minfilter = point;
	magfilter = point;
	mipfilter = point;
	addressu = clamp;
	addressv = clamp;
};

struct VertexShaderInput
{
#if SM4
	float4 Position : SV_Position;
#else
	float4 Position : POSITION0;
#endif
	float2 textureCoords:TEXCOORD0;
	float3 normal:NORMAL0;
};

struct VertexShaderOutput
{
	float4 Position:POSITION0;
	float2 textureCoords:TEXCOORD0;
	float3 normal:TEXCOORD1;
	float4 worldPosition:TEXCOORD2;
};

float calcShadowPCF(float lightSpaceDepth, float2 shadowCoordinates)
{
	float size = 1.0 / shadowMapSize;
	float samples[4];
	float gradiant = lightSpaceDepth - shadowBias;

	samples[0] = (gradiant < tex2D(shadowSampler, shadowCoordinates).r);
	samples[1] = (gradiant < tex2D(shadowSampler, shadowCoordinates + float2(size, 0)).r);
	samples[2] = (gradiant < tex2D(shadowSampler, shadowCoordinates + float2(0, size)).r);
	samples[3] = (gradiant < tex2D(shadowSampler, shadowCoordinates + float2(size, size)).r);

	return (samples[0] + samples[1] + samples[2] + samples[3]) / 4.0;
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	float4 n = float4(input.normal.x, input.normal.y, input.normal.z, 0);

	output.Position = mul(viewPosition, Projection);
	output.textureCoords = input.textureCoords;
	output.normal = (float3)normalize(mul(n, World));
	output.worldPosition = worldPosition;

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input):COLOR0
{
	float4 color = tex2D(textureSampler, input.textureCoords);
	float4 worldPosition = input.worldPosition;
	float3 lightDir = (float3)lightPosition - (float3)worldPosition;
	float inverseLightRadiusSquared = 1.0 / (lightRadius.x * lightRadius.x);
	float lightFactor = 0;
	float attenuation = 0;
	float ndl = 0;

	//transform to light space
	float4 lightSpacePosition = mul(mul(worldPosition, lightView), lightProjection);
	lightSpacePosition -= shadowBias;
	lightSpacePosition /= lightSpacePosition.w;
	float2 screenPosition = 0.5 + float2(lightSpacePosition.x, -lightSpacePosition.y) * 0.5;
	float lightSpaceDepth = lightSpacePosition.z;

	//light influence
	attenuation = 1 - saturate(dot(lightDir, lightDir) * inverseLightRadiusSquared);
	ndl = saturate(dot(input.normal, lightDir));

	if (receiveShadow)
		lightFactor = attenuation * ndl;
	else
		lightFactor = 1.0;

	float shadowTerm = 1.0;

	// Using this hack for now
	if (shadowMapEnabled == true)
	{
		if ((saturate(screenPosition).x == screenPosition.x) && (saturate(screenPosition).y == screenPosition.y))
			shadowTerm = max(shadowStrength, calcShadowPCF(lightSpaceDepth, screenPosition));
	}

	color = clamp(color * ambientColor * lightFactor * shadowTerm + emissiveColor, 0.0, 1.0);

	return color;
}

technique Technique1
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

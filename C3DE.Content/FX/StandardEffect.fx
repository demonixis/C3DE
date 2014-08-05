// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

// Camera
float3 EyePosition = float3(1.0, 0.0, 0.0);

// Material
float3 AmbientColor = float3(1.0, 1.0, 1.0);
float3 DiffuseColor = float3(1.0, 1.0, 1.0);
float3 EmissiveColor = float3(0.0, 0.0, 0.0);
float3 SpecularColor = float3(0.8, 0.8, 0.8);
float Shininess = 200.0;

// Lighting
float4x4 LightView;
float4x4 LightProjection;
float3 LightColor;
float3 LightDirection;
float3 LightPosition;
float LightIntensity;
float LightSpotAngle;
float LightRange;
int LightFallOff;
int NbLights;
int LightType = 0;

// ShadowData [0] => Map size [1] => Bias [2] => Strength
float3 ShadowData = float3(0, 0.05, 1.0);
bool RecieveShadow = false;

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

float CalcShadowPCF(float lightSpaceDepth, float2 shadowCoordinates)
{
	float size = 1.0 / ShadowData[1];
	float samples[4];
	float gradiant = lightSpaceDepth - ShadowData[2];

	samples[0] = (gradiant < tex2D(shadowSampler, shadowCoordinates).r) ? 1.0 : 0.0;
	samples[1] = (gradiant < tex2D(shadowSampler, shadowCoordinates + float2(size, 0)).r) ? 1.0 : 0.0;
	samples[2] = (gradiant < tex2D(shadowSampler, shadowCoordinates + float2(0, size)).r) ? 1.0 : 0.0;
	samples[3] = (gradiant < tex2D(shadowSampler, shadowCoordinates + float2(size, size)).r) ? 1.0 : 0.0;

	return (samples[0] + samples[1] + samples[2] + samples[3]) / 4.0;
}

float4 CalcAmbientLightColor(float2 uv)
{
	return float4(AmbientColor + tex2D(textureSampler, uv) + EmissiveColor, 1.0);
}

float4 CalcDirectionalLightColor(float2 uv, float3 normal, float4 worldPosition)
{
	float3 diffuse = saturate(dot(normal, LightDirection));
	return float4(diffuse * LightColor * LightIntensity, 1.0);
}

float4 CalcPointLightColor(float2 uv, float3 normal, float4 worldPosition)
{
	float3 lightDirection = normalize(LightPosition - worldPosition);
	float diffuse = saturate(dot(normalize(normal), lightDirection));
	float d = distance(LightPosition, worldPosition);
	float attenuation = 1 - pow(clamp(d / LightRange, 0, 1), LightFallOff);

	return float4(diffuse * attenuation * LightColor * LightIntensity, 1.0);
}

float4 CalcSpotLightColor(float2 uv, float3 normal, float4 worldPosition)
{
	float3 lightDirection = normalize(LightPosition - worldPosition);
	float3 diffuse = saturate(dot(normalize(normal), lightDirection));

	float d = dot(-lightDirection, normalize(LightDirection));
	float a = cos(LightSpotAngle);

	float attenuation = 0.0;

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

	float3 light = normalize(lightDirection);
	float3 norm = normalize(normal);
	float3 r = normalize(2 * dot(light, norm) * norm - light);
	float3 v = (float3)normalize(mul(normalize(float4(EyePosition, 1.0)), World));
	float dotProduct = dot(r, v);
	
	return float4(SpecularColor * max(pow(abs(dotProduct), Shininess), 0) * length(color), 1.0);
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

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 finalCompose = CalcAmbientLightColor(input.UV);
	float shadowTerm = 1.0;

	// Apply a light influence.
	if (LightType == 1)
		finalCompose *= CalcDirectionalLightColor(input.UV, input.Normal, input.WorldPosition);
	else if (LightType == 2)
		finalCompose *= CalcPointLightColor(input.UV, input.Normal, input.WorldPosition);
	else if (LightType == 3)
		finalCompose *= CalcSpotLightColor(input.UV, input.Normal, input.WorldPosition);

	// ShadowData [0] => Map size [1] => Bias [2] => Strength
	if (ShadowData[0] > 0 && RecieveShadow == true)
	{
		float4 lightSpacePosition = mul(mul(input.WorldPosition, LightView), LightProjection);
		lightSpacePosition -= ShadowData[1];
		lightSpacePosition /= lightSpacePosition.w;

		float2 screenPosition = 0.5 + float2(lightSpacePosition.x, -lightSpacePosition.y) * 0.5;

		float shadow = CalcShadowPCF(lightSpacePosition.z, screenPosition);

		if ((saturate(screenPosition).x == screenPosition.x) && (saturate(screenPosition).y == screenPosition.y))
			shadowTerm = max(ShadowData[2], shadow);
	}

	return (finalCompose * shadowTerm) + CalcSpecularColor(input.Normal, input.WorldPosition, finalCompose, LightType);
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
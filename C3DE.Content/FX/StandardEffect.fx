// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

// Camera
float3 EyePosition = float3(1.0, 0.0, 0.0);
float3 ViewPosition = float3(1.0, 0.0, 0.0);

// Material
float4 AmbientColor = float4(0.1, 0.1, 0.1, 1.0);
float4 DiffuseColor = float4(1.0, 1.0, 1.0, 1.0);
float4 EmissiveColor = float4(0.0, 0.0, 0.0, 1.0);
float4 SpecularColor = float4(0.8, 0.8, 0.8, 1.0);
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

// ShadowData [0] => Map size [1] => Bias [2] => Strength
float CalcShadowPCF(float lightSpaceDepth, float2 shadowCoordinates)
{
	float size = 1.0 / ShadowData[0];
	float samples[4];
	float gradiant = lightSpaceDepth - ShadowData[1];

	samples[0] = (gradiant < tex2D(shadowSampler, shadowCoordinates).r) ? 1.0 : 0.0;
	samples[1] = (gradiant < tex2D(shadowSampler, shadowCoordinates + float2(size, 0)).r) ? 1.0 : 0.0;
	samples[2] = (gradiant < tex2D(shadowSampler, shadowCoordinates + float2(0, size)).r) ? 1.0 : 0.0;
	samples[3] = (gradiant < tex2D(shadowSampler, shadowCoordinates + float2(size, size)).r) ? 1.0 : 0.0;

	return (samples[0] + samples[1] + samples[2] + samples[3]) / 4.0;
}

float4 CalcDirectionalLightColor(float3 normal, float4 worldPosition)
{
	float3 diffuse = saturate(dot(normal, LightDirection));
	return float4(diffuse * LightColor * LightIntensity, 1.0);
}

float4 CalcPointLightColor(float3 normal, float4 worldPosition)
{
	float3 lightDirection = normalize(LightPosition - worldPosition);
	float diffuse = saturate(dot(normalize(normal), lightDirection));
	float d = distance(LightPosition, worldPosition);
	float attenuation = 1 - pow(clamp(d / LightRange, 0, 1), LightFallOff);

	return float4(diffuse * attenuation * LightColor * LightIntensity, 1.0);
}

float4 CalcSpotLightColor(float3 normal, float4 worldPosition)
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

	float3 R = normalize(2 * normal - lightDirection);

	return SpecularColor * pow(saturate(dot(R, normalize(EyePosition - worldPosition))), Shininess);
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
	float4 baseDiffuse = DiffuseColor * tex2D(textureSampler, input.UV);
	float4 lightFactor = float4(1, 1, 1, 1);
	float3 normal = normalize(input.Normal);
	float shadowTerm = 1;

	// Apply a light influence.
	if (LightType == 1)
		lightFactor = CalcDirectionalLightColor(normal, input.WorldPosition);
	else if (LightType == 2)
		lightFactor = CalcPointLightColor(normal, input.WorldPosition);
	else if (LightType == 3)
		lightFactor = CalcSpotLightColor(normal, input.WorldPosition);

	// ShadowData [0] => Map size [1] => Bias [2] => Strength
	if (ShadowData[0] > 0 && RecieveShadow == true)
	{
		float4 lightSpacePosition = mul(mul(input.WorldPosition, LightView), LightProjection);
		lightSpacePosition -= ShadowData[1];
		lightSpacePosition /= lightSpacePosition.w;

		float2 screenPosition = 0.5 + float2(lightSpacePosition.x, -lightSpacePosition.y) * 0.5;

		if ((saturate(screenPosition).x == screenPosition.x) && (saturate(screenPosition).y == screenPosition.y))
			shadowTerm = max(ShadowData[2], CalcShadowPCF(lightSpacePosition.z, screenPosition));
	}

	float4 finalDiffuse = baseDiffuse * lightFactor * shadowTerm;
	float4 finalSpecular = CalcSpecularColor(normal, input.WorldPosition, finalDiffuse, LightType);

	return AmbientColor + finalDiffuse + finalSpecular + EmissiveColor;
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
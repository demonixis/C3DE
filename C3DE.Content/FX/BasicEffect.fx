// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

// Material
float4 AmbientColor = float4(0.1, 0.1, 0.1, 1.0);
float4 DiffuseColor = float4(1.0, 1.0, 1.0, 1.0);

// Light 0
float4x4 LightView0;
float4x4 LightProjection0;
float3 LightDirection = float3(1.0, 0.0, 0.0);
float4 LightColor = float4(1.0, 1.0, 1.0, 1.0);
float LightIntensity = 1.0;

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
	output.Position = mul(viewPosition, Projection);
	output.UV = input.UV;
	output.Normal = mul(input.Normal, World);

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 baseDiffuse = DiffuseColor * tex2D(textureSampler, input.UV);
	float shadowTerm = 1;
	
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
	
	float3 diffuse = saturate(dot(normal, LightDirection));
	
	return AmbientColor * baseDiffuse * diffuse * shadowTerm;
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
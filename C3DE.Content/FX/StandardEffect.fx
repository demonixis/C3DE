// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

float3 EyePosition = float3(1.0, 0.0, 0.0);
bool RecieveShadows = true;

// Material
float3 AmbientColor = float3(1.0, 1.0, 1.0);
float3 DiffuseColor = float3(1.0, 1.0, 1.0);
float3 EmissiveColor = float3(0.0, 0.0, 0.0);
float3 SpecularColor = float3(0.8, 0.8, 0.8);
float Shininess = 200.0;

// Lighting
float4x4 LightView;
float4x4 LightProjection;
float3 LightDirection = float3(1.0, 0.0, 0.0);
float3 LightPosition = float3(0.0, 0.0, 0.0);
// Data { Intensity / Spot Angle / Range / Nb lights }
float4 LightData = float4(1.0, 30, 5000, 0);

// Shadow map { Map size / Bias / Strength / Nb Samples }
float4 ShadowData = float4(0, 0.05, 1.0, 0);

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
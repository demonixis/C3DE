static const float ToonThresholds[4] = { 0.95, 0.5, 0.2, 0.03 };
static const float ToonBrightnessLevels[5] = { 1.0, 0.8, 0.6, 0.35, 0.2 };

// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

// Material
float3 AmbientColor = float3(0.1, 0.1, 0.1);
float3 DiffuseColor = float3(1.0, 1.0, 1.0);
float3 EmissiveColor = float3(0.0, 0.0, 0.0);

// Misc
float2 TextureTiling = float2(1, 1);
float2 TextureOffset = float2(0, 0);

// Light
float3 LightDirection = float3(0, 1, 1);

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

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.UV = input.UV;
	output.Normal = normalize(mul(input.Normal, World));

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 color = (DiffuseColor * tex2D(textureSampler, (input.UV + TextureOffset) * TextureTiling));
	
	float diffuse = saturate(dot(input.Normal, LightDirection));
	
	if (diffuse > ToonThresholds[0])
		color *= ToonBrightnessLevels[0];
	else if (diffuse > ToonThresholds[1])
		color *= ToonBrightnessLevels[1];
	else if (diffuse > ToonThresholds[2])
		color *= ToonBrightnessLevels[2];
	else if (diffuse > ToonThresholds[3])
		color *= ToonBrightnessLevels[3];
	else
		color *= ToonBrightnessLevels[4];
	
	return float4(AmbientColor + color + EmissiveColor, 1.0);
}

technique Toon
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
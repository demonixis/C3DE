// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

// Material
float3 AmbientColor = float4(1.0, 1.0, 1.0);
float3 DiffuseColor = float4(1.0, 1.0, 1.0);
float3 SpecularColor = float4(0.8, 0.8, 0.8);
float Shininess = 250.0;

// Light
float3 LightDirection = float3(1.0, 1.0, 0.0);
float LightIntensity = 1.0;
float3 LightColor = float4(1, 1, 1);

// Misc
float2 TextureTiling = float2(1, 1);
float2 TextureOffset = float2(0, 0);
float TotalTime = 0.0;
float Alpha = 0.3;

texture2D WaterTexture;
sampler2D WaterMapSampler = sampler_state
{
	Texture = < WaterTexture > ;
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
	float4 WorldPosition : TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	// Wave
	input.Position.z += sin((TotalTime * 16.0) + (input.Position.y / 1.0)) / 16.0;
	input.Position.y += sin(1.0 * input.Position.y + (TotalTime * 5.0)) * 0.25;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.UV = input.UV;
	output.Normal = input.Normal;
	output.WorldPosition = worldPosition;

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	input.UV.x = input.UV.x * 20.0 + sin(TotalTime * 3.0 + 10.0) / 256.0;
	input.UV.y = input.UV.y * 20.0;

	float4 baseColor = tex2D(WaterMapSampler, (input.UV + TextureOffset) * TextureTiling);
	float4 normal = float4(input.Normal, 1.0);
	float4 reflectColor = float4(1, 1, 1, 1);

	float4 diffuse = saturate(dot(LightDirection, normal)) * LightColor * LightIntensity;
	float3 R = normalize(2 * diffuse.xyz * normal - float4(LightDirection, 1.0));
	float4 specular = SpecularColor * pow(saturate(dot(R, LightDirection)), Shininess);
	
	float4 finalColor = AmbientColor + (baseColor * DiffuseColor * diffuse * reflectColor) + specular;
	finalColor.a = Alpha;

	return finalColor;
}

technique Water
{
	pass Pass1
	{
		AlphaBlendEnable = TRUE;
		DestBlend = INVSRCALPHA;
		SrcBlend = SRCALPHA;
#if SM4
		VertexShader = compile vs_4_0_level_9_3 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_3 PixelShaderFunction();
#else
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
#endif
	}
}
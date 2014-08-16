// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

// Material
float4 AmbientColor = float4(1.0, 1.0, 1.0, 1.0);
float4 DiffuseColor = float4(1.0, 1.0, 1.0, 1.0);
float4 SpecularColor = float4(0.8, 0.8, 0.8, 1.0);
float Shininess = 250.0;

// Light
float3 LightDirection = float3(1.0, 1.0, 0.0);
float LightIntensity = 1.0;
float4 LightColor = float4(1, 1, 1, 1);

// Misc
float2 TextureTiling = float2(1, 1);
float2 TextureOffset = float2(0, 0);
float3 EyePosition = float3(0, 0, 1);
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

texture2D NormalTexture;
sampler2D NormalMapSampler = sampler_state
{
	Texture = < NormalTexture >;
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
	float4 WorldPosition : TEXCOORD1;
	float3x3 WorldToTangentSpace : TEXCOORD2;
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

	float3 normal = mul(normalize(input.Normal), World);

	// [0] Tangent / [1] Binormal / [2] Normal
	output.WorldToTangentSpace[0] = cross(normal, float3(-1.0, 0.0, 0.0));
	output.WorldToTangentSpace[1] = cross(output.WorldToTangentSpace[0], normal);
	output.WorldToTangentSpace[2] = normal;
	
	output.WorldPosition = worldPosition;

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	input.UV.x = ((input.UV.x + TextureOffset.x) * TextureTiling.x) * 20.0 + sin(TotalTime * 3.0 + 10.0) / 256.0;
	input.UV.y = ((input.UV.y + TextureOffset.y) * TextureTiling.y) * 20.0;

	float4 color = tex2D(WaterMapSampler, input.UV);

	input.UV.y += (sin(TotalTime * 3.0 + 10.0) / 256) + (TotalTime / 16);
	float3 normalMap = 2.0 * (tex2D(NormalMapSampler, input.UV)) - 1.0;

	input.UV.y -= ((sin(TotalTime * 3.0 + 10) / 256.0) + (TotalTime / 16.0)) * 2.0;
	float3 normalMap2 = (2.0 * (tex2D(NormalMapSampler, input.UV))) - 1.0;

	normalMap = (normalMap + normalMap2) / 2.0;
	normalMap = normalize(mul(normalMap, input.WorldToTangentSpace));
	
	float4 normal = float4(normalMap, 1.0);

	float4 diffuse = saturate(dot(LightDirection, normal)) * LightColor * LightIntensity;
	float3 R = normalize(2 * diffuse.xyz * normal - float4(LightDirection, 1.0));
	float4 specular = SpecularColor * pow(saturate(dot(R, LightDirection)), Shininess);
	
	float4 finalColor = AmbientColor + (color * DiffuseColor * diffuse) + specular;
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
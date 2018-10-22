// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

// Material
float3 AmbientColor;
float3 DiffuseColor;

// Misc
float2 TextureTiling;

texture2D MainTexture;
sampler2D textureSampler = sampler_state
{
	Texture = < MainTexture >;
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
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.UV = input.UV;
	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 diffuse = tex2D(textureSampler, input.UV * TextureTiling);
	float4 finalColor = float4(AmbientColor + DiffuseColor * diffuse.xyz, 1.0);
	
	clip(diffuse.a < 0.1f ? -1 : 1);

	return finalColor;
}

technique Transparent
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
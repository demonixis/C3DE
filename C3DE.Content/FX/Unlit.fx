// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

// Material
float3 AmbientColor = float3(0.1, 0.1, 0.1);
float3 DiffuseColor = float3(1.0, 1.0, 1.0);

// Misc
float2 TextureTiling = float2(1, 1);
float2 TextureOffset = float2(0, 0);

texture MainTexture;
sampler2D textureSampler = sampler_state 
{
	Texture = (MainTexture);
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Point;
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
	return float4(AmbientColor + (DiffuseColor * tex2D(textureSampler, (input.UV + TextureOffset) * TextureTiling)), 1.0);
}

technique TexturedSimple
{
	pass Unlit
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
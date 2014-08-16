// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

// Material
float4 AmbientColor = float4(0.1, 0.1, 0.1, 1.0);
float4 DiffuseColor = float4(1.0, 1.0, 1.0, 1.0);
float4 EmissiveColor = float4(0.0, 0.0, 0.0, 1.0);
float Alpha = 1.0;

// Misc
float2 TextureTiling = float2(1, 1);
float2 TextureOffset = float2(0, 0);

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

float4 PixelShaderFunctionNoAlpha(VertexShaderOutput input) : COLOR0
{
	return AmbientColor + (DiffuseColor * tex2D(textureSampler, (input.UV + TextureOffset) * TextureTiling)) + EmissiveColor;
}

float4 PixelShaderFunctionAlpha(VertexShaderOutput input) : COLOR0
{
	float4 color = PixelShaderFunctionNoAlpha(input);
	color.a = Alpha;
	return color;
}

technique TexturedSimple
{
	pass Alpha
	{
		AlphaBlendEnable = TRUE;
		DestBlend = INVSRCALPHA;
		SrcBlend = SRCALPHA;
#if SM4
		VertexShader = compile vs_4_0_level_9_1 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 PixelShaderFunctionAlpha();
#else
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunctionAlpha();
#endif
	}

	pass NoAlpha
	{
#if SM4
		VertexShader = compile vs_4_0_level_9_3 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_3 PixelShaderFunctionNoAlpha();
#else
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunctionNoAlpha();
#endif
	}
}
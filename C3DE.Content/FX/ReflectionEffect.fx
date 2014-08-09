float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

float4 TintColor = float4(1, 1, 1, 1);
float3 EyePosition;

Texture ReflectiveTexture;
samplerCUBE reflectiveSampler = sampler_state
{
	Texture = <ReflectiveTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Mirror;
	AddressV = Mirror;
};

struct VertexShaderInput
{
#if SM4
	float4 Position : SV_Position;
#else
	float4 Position : POSITION0;
#endif
	float4 Normal : NORMAL0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float3 Reflection : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	
	float3 viewDirection = EyePosition - worldPosition;
	float3 normal = normalize(mul(input.Normal, WorldInverseTranspose));
	output.Reflection = reflect(-normalize(viewDirection), normalize(normal));
	
	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	return TintColor * texCUBE(reflectiveSampler, normalize(input.Reflection));
}

technique Reflection
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
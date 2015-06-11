float4x4 World;
float4x4 View;
float4x4 Projection;

float3 ReflectionColor = float4(1.0, 1.0, 1.0);
float3 EyePosition = float3(0, 0, 1);
bool MainTextureEnabled = false;

// Misc
float2 TextureTiling = float2(1, 1);
float2 TextureOffset = float2(0, 0);

Texture2D MainTexture;
sampler2D mainTextureSampler = sampler_state
{
	Texture = < MainTexture > ;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

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
	float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float3 Reflection : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.UV = input.UV;
	
	float3 viewDirection = EyePosition - worldPosition;
	float3 normal = input.Normal;
	output.Reflection = reflect(-normalize(viewDirection), normalize(normal));
	
	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 baseColor = float4(1.0, 1.0, 1.0, 1.0);
	float4 reflectColor = ReflectionColor * texCUBE(reflectiveSampler, normalize(input.Reflection));
	
	if (MainTextureEnabled == true)
		baseColor = tex2D(mainTextureSampler, (input.UV + TextureOffset) * TextureTiling);
		
	return baseColor * reflectColor;
}

technique ReflectionTexture
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
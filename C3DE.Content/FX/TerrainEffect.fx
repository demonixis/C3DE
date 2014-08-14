// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

// Material
float4 AmbientColor = float4(1.0, 1.0, 1.0, 1.0);
float4 DiffuseColor = float4(1.0, 1.0, 1.0, 1.0);

// Light
float3 LightDirection = float3(1.0, 1.0, 0.0);
float LightIntensity = 1.0;
float4 LightColor = float4(1, 1, 1, 1);

// Misc
float2 TextureTiling = float2(1, 1);

texture2D MainTexture;
sampler2D MainTextureSampler = sampler_state
{
	Texture = < MainTexture > ;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

texture2D SandTexture;
sampler2D SandTextureSampler = sampler_state
{
	Texture = < SandTexture > ;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

texture2D RockTexture;
sampler2D RockTextureSampler = sampler_state
{
	Texture = < RockTexture > ;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

texture2D SnowTexture;
sampler2D SnowTextureSampler = sampler_state
{
	Texture = < SnowTexture > ;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

texture2D WeightMap;
sampler2D WeightMapSampler = sampler_state
{
	Texture = < WeightMap > ;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
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
	float light = dot(normalize(input.Normal), normalize(LightDirection));
	light = saturate(light * LightColor * LightIntensity);
	
	float3 sandTex = tex2D(SandTextureSampler, input.UV * TextureTiling);
	float rockTex = tex2D(RockTextureSampler, input.UV * TextureTiling);
	float3 snowTex = tex2D(SnowTextureSampler, input.UV * TextureTiling);
	float3 mainTex = tex2D(MainTextureSampler, input.UV * TextureTiling);
	float3 weightTex = tex2D(WeightMapSampler, input.UV);
	
	float3 baseCompose = clamp(1.0 - weightTex.r - weightTex.g - weightTex.b, 0, 1);
	baseCompose *= mainTex;
	
	baseCompose += weightTex.r * sandTex + weightTex.g * rockTex + weightTex.b * snowTex;
	
	return AmbientColor + (DiffuseColor * float4(baseCompose, 1.0) * light);
}

technique Terrain
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
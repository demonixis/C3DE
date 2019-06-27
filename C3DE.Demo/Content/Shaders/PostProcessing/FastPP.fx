float BloomSize;
float BloomAmount;
float BloomPower;
float Exposure;
float4 TextureSamplerTexelSize;

Texture TargetTexture;
sampler2D textureSampler = sampler_state
{
	Texture = <TargetTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

struct PixelShaderInput
{
#if SM4
	float4 Position : SV_Position;
#else
	float4 Position : POSITION0;
#endif
	float4 Color : COLOR0;
	float2 UV : TEXCOORD0;
};

float4 FastPPPixelShader(PixelShaderInput input) : COLOR
{
	float2 uv = input.UV;
	float3 col = tex2D(textureSampler, uv).xyz;

	// Bloom
	float size = 1 / BloomSize;
	float4 sum = 0;
	float3 bloom;

	for (int i = -3; i < 3; i++)
	{
		sum += tex2D(textureSampler, uv + float2(-1, i) * size) * BloomAmount;
		sum += tex2D(textureSampler, uv + float2(0, i) * size) * BloomAmount;
		sum += tex2D(textureSampler, uv + float2(1, i) * size) * BloomAmount;
	}

	if (col.r < 0.3 && col.g < 0.3 && col.b < 0.3)
	{
		bloom = sum.rgb * sum.rgb * 0.012 + col;
	}
	else
	{
		if (col.r < 0.5 && col.g < 0.5 && col.b < 0.5)
		{
			bloom = sum.xyz * sum.xyz * 0.009 + col;
		}
		else
		{
			bloom = sum.xyz * sum.xyz * 0.0075 + col;
		}
	}

	col = lerp(col, bloom, BloomPower);
	
	// ACES Tonemapper
	col *= Exposure;
	
	const float3 a = 2.51f;
	const float3 b = 0.03f;
	const float3 c = 2.43f;
	const float3 d = 0.59f;
	const float3 e = 0.14f;
	col = saturate((col * (a * col + b)) / (col * (c * col + d) + e));
	
	// Dithering
	float3 magic = float3(0.06711056, 0.00583715, 52.9829189);
	float gradient = frac(magic.z * frac(dot(uv / TextureSamplerTexelSize, magic.xy))) / 255.0;
	col.rgb -= gradient.xxx;
	
	// Next will follow
	
	return float4(col, 1.0);
}

Technique PostProcess
{
    pass FastPostProcessing
    {
#if SM4
		PixelShader = compile ps_4_0_level_9_3 FastPPPixelShader();
#else
		PixelShader = compile ps_3_0 FastPPPixelShader();
#endif
    }
}
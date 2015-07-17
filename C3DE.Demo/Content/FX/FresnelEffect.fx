// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;
float3 EyePosition = float3(1, 0, 0);

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

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.UV = input.UV;
	output.WorldPosition = worldPosition;
	output.Normal = input.Normal;
	
	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 normal = normalize(mul(input.Normal, World));
	float4 color = float4(1, 1, 1, 1);
	float3 viewDirection = normalize(EyePosition - input.WorldPosition);
	
	// Fresnel
	float term = dot(viewDirection, normal);
	term = saturate(1.0 - term);
	
	float4 finalColor = color * term;
	finalColor.a = 1;
	
	return finalColor;
}

technique Fresnel
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
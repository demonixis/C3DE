// Matrix
float4x4 WorldViewProjection;
float4x4 InvViewProjection;

// Light
float3 LightColor;
float3 LightPosition;
float LightAttenuation;
float LightRange;
float LightIntensity;

float2 Viewport;

Texture2D DepthTexture;
sampler2D depthSampler = sampler_state
{
	Texture = (DepthTexture);
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Point;
};

Texture2D NormalTexture;
sampler2D normalSampler = sampler_state
{
	Texture = (NormalTexture);
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Point;
};

struct VertexShaderInput
{
#if SM4
	float4 Position : SV_Position;
#else
	float4 Position : POSITION0;
#endif
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float4 LightPosition : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	output.Position = mul(input.Position, WorldViewProjection);;
	output.LightPosition = output.Position;
	return output;
}

float2 PostProjectToScreen(float4 position)
{
	float2 sp = position.xy / position.w;
	return 0.5f * (float2(sp.x, -sp.y) + 1);
}

float2 HalfPixel()
{
	return 0.5f / float2(Viewport.x, Viewport.y);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float2 texCoord = PostProjectToScreen(input.LightPosition) + HalfPixel();
	
	float4 depth = tex2D(depthSampler, texCoord);
	
	// Recreate position and UV
	float4 pos;
	pos.x = texCoord.x * 2 - 1;
	pos.y = (1 - texCoord.y) * 2 - 1;
	pos.z = depth.x;
	pos.w = 1.0f;
	
	pos = mul(pos, InvViewProjection);
	pos.xyz /= pos.w;
	
	// Extract normal and range from 0, 1 to -1, 1
	float4 normal = (tex2D(normalSampler, texCoord) - 0.5) * 2;
	
	float3 direction = normalize(LightPosition - pos);
	float light = saturate(dot(normal, direction));
	float d = distance(LightPosition, pos);
	float att = 1 - pow(clamp(d / LightRange, 0, 1), LightAttenuation);
	
	return float4(light * att * LightColor * LightIntensity, 1);
}

technique Basic
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
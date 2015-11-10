static const float FXAA_ReduceMin = 1.0 / 128.0;
static const float FXAA_ReduceMul = 1.0 / 8.0;
static const float FXAA_SpanMax = 1.0 / 8.0;

float2 TexelSize = float2(1, 1);

texture TargetTexture;
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

float4 FXAAPixelShader(PixelShaderInput input) : COLOR
{
	float4 rgbNW = tex2D(textureSampler, input.UV + float2(-1.0, 1.0) * TexelSize);
	float4 rgbNE = tex2D(textureSampler, input.UV + float2(1.0, -1.0) * TexelSize);
	float4 rgbSW = tex2D(textureSampler, input.UV + float2(-1.0, 1.0) * TexelSize);
	float4 rgbSE = tex2D(textureSampler, input.UV + float2(1.0, 1.0) * TexelSize);
	float4 rgbM = tex2D(textureSampler, input.UV);
	float4 luma = float4(0.299, 0.587, 0.114, 1.0);
	float lumaNW = dot(rgbNW, luma);
	float lumaNE = dot(rgbNE, luma);
	float lumaSW = dot(rgbSW, luma);
	float lumaSE = dot(rgbSE, luma);
	float lumaM = dot(rgbM, luma);
	float lumaMin = min(lumaM, min(min(lumaNW, lumaNE), min(lumaSW, lumaSE)));
	float lumaMax = max(lumaM, max(max(lumaNW, lumaNE), max(lumaSW, lumaSE)));
	
	float2 dir = float2(-((lumaNW + lumaNE) - (lumaSW + lumaSE)), ((lumaNW + lumaSW) - (lumaNE + lumaSE)));
	float dirReduce = max((lumaNW + lumaSW + lumaSE) * (0.25 * FXAA_ReduceMul), FXAA_ReduceMin);
	float rcpDirMin = 1.0 / (min(abs(dir.x), abs(dir.y)) + dirReduce);
	dir = min(float2(FXAA_SpanMax, FXAA_SpanMax), max(float2(-FXAA_SpanMax, -FXAA_SpanMax), dir * rcpDirMin)) * TexelSize;
	
	float4 rgbA = 0.5 * (tex2D(textureSampler, input.UV + dir * (1.0 / 3.0 - 0.5)) + tex2D(textureSampler, input.UV + dir * (2.0 / 3.0 - 0.5)));
	
	float4 rgbB = rgbA * 0.5 + 0.25 * (tex2D(textureSampler, input.UV + dir * -0.5) + tex2D(textureSampler, input.UV + dir * 0.5));
	float lumaB = dot(rgbB, luma);
	
	if ((lumaB < lumaMin) || (lumaB > lumaMax))
		return rgbA;
	else
		return rgbB;
}

technique Technique1
{
	pass FXAA
	{
#if SM4
		PixelShader = compile ps_4_0_level_9_1 FXAAPixelShader();
#else
		PixelShader = compile ps_3_0 FXAAPixelShader();
#endif
	}
}
float K1_Red;
float K1_Green;
float K1_Blue;
float2 Center;

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

float2 Distort(float2 p, float k1)
{
	float r2 = p.x * p.x + p.y * p.y;
	float newRadius = (1 + k1 * r2);
	p.x = p.x * newRadius;
	p.y = p.y * newRadius;
	return p;
}

float4 OsvrPixelShaderFunction(PixelShaderInput input) : COLOR
{
	float2 uvRed;
	float2 uvGreen;
	float2 uvBlue;
	float4 red;
	float4 green;
	float4 blue;

	uvRed = Distort(input.UV - Center, K1_Red) + Center;
	uvGreen = Distort(input.UV - Center, K1_Green) + Center;
	uvBlue = Distort(input.UV - Center, K1_Blue) + Center;

	red = tex2D(textureSampler, uvRed);
	green = tex2D(textureSampler, uvGreen);
	blue = tex2D(textureSampler, uvBlue);

	if (((uvRed.x > 0.0) && (uvRed.x < 1.0) && (uvRed.y > 0.0) && (uvRed.y < 1.0)))
		return float4(red.x, green.y, blue.z, 1.0);
	else
		return float4(0.0, 0.0 ,0.0 ,1.0);
}

technique OsvrDistorition  
{
    pass Pass1
    {
#if SM4
		PixelShader = compile ps_4_0_level_9_1 OsvrPixelShaderFunction();
#else
		PixelShader = compile ps_3_0 OsvrPixelShaderFunction();
#endif
    }
}

float BlurDistance;

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

float4 BlurPixelShader(PixelShaderInput input) : COLOR
{
	float4 diffuse = tex2D(textureSampler, float2(input.UV.x + BlurDistance, input.UV.y + BlurDistance));
	diffuse += tex2D(textureSampler, float2(input.UV.x - BlurDistance, input.UV.y - BlurDistance));
	diffuse += tex2D(textureSampler, float2(input.UV.x + BlurDistance, input.UV.y - BlurDistance));
	diffuse += tex2D(textureSampler, float2(input.UV.x - BlurDistance, input.UV.y + BlurDistance));
	return diffuse / 4.0;
}

technique Technique1
{
	pass Blur
	{
#if SM4
		PixelShader = compile ps_4_0_level_9_1 BlurPixelShader();
#else
		PixelShader = compile ps_3_0 BlurPixelShader();
#endif
	}
}
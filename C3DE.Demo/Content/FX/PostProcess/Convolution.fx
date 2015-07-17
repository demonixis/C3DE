float2 ScreenSize;
float Kernel[9];

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

float4 ConvolutionPixelShader(PixelShaderInput input) : COLOR
{
	float2 onePx = float2(1.0, 1.0) / ScreenSize;
	float4 colorSum = tex2D(textureSampler, input.UV + onePx * float2(-1, -1)) * Kernel[0];
	colorSum += tex2D(textureSampler, input.UV + onePx * float2(0, -1)) * Kernel[1];
	colorSum += tex2D(textureSampler, input.UV + onePx * float2(1, -1)) * Kernel[2];
	colorSum += tex2D(textureSampler, input.UV + onePx * float2(-1, 0)) * Kernel[3];
	colorSum += tex2D(textureSampler, input.UV + onePx * float2(0, 0)) * Kernel[4];
	colorSum += tex2D(textureSampler, input.UV + onePx * float2(1, 0)) * Kernel[5];
	colorSum += tex2D(textureSampler, input.UV + onePx * float2(-1, 1)) * Kernel[6];
	colorSum += tex2D(textureSampler, input.UV + onePx * float2(0, 1)) * Kernel[7];
	colorSum += tex2D(textureSampler, input.UV + onePx * float2(1, 1)) * Kernel[8];
	
	float kWeight = Kernel[0];
	kWeight += Kernel[1];
	kWeight += Kernel[2];
	kWeight += Kernel[3];
	kWeight += Kernel[4];
	kWeight += Kernel[5];
	kWeight += Kernel[6];
	kWeight += Kernel[7];
	kWeight += Kernel[8];
	
	if (kWeight <= 0.0)
		kWeight = 1.0;
		
	return float4((colorSum / kWeight).xyz, 1);
}

technique Technique1
{
    pass Convolution
    {
#if SM4
		PixelShader = compile ps_4_0_level_9_1 ConvolutionPixelShader();
#else
		PixelShader = compile ps_3_0 ConvolutionPixelShader();
#endif
    }
}
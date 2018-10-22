float Time;
bool GrayScaleEnabled;
float NoiseIntensity; // 0 to 1
float ScanlineIntensity; // 0 to 1
float ScanlineCount; // 0 to 4069

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

float4 FilmPixelShader(PixelShaderInput input) : COLOR
{
	float4 color = tex2D(textureSampler, input.UV);
	float3 targetColor = color.xyz;
	
	// Noise
	float x = input.UV.x * input.UV.y * Time * 1000.0;
	x = fmod(x, 13.0) * fmod(x, 123.0);
	float dx = fmod(x, 0.01);
	float3 result = targetColor + targetColor * saturate(0.1 + dx * 100.0);
	float2 sinCos = float2(sin(input.UV.y * ScanlineCount), cos(input.UV.y * ScanlineCount));
	result += targetColor * float3(sinCos.x, sinCos.y, sinCos.x) * ScanlineIntensity;
	
	// Interpolation between source and result by intensity.
	result = targetColor + saturate(NoiseIntensity) * (result - targetColor);
	
	if (GrayScaleEnabled == true)
	{
		float grayFactor = result.x * 0.3 + result.y * 0.59 + result.z * 0.11;
		result = float3(grayFactor, grayFactor, grayFactor);
	}
	
	return float4(result, color.w);
}

technique Technique1
{
	pass Film
	{
#if SM4
		PixelShader = compile ps_4_0_level_9_1 FilmPixelShader();
#else
		PixelShader = compile ps_3_0 FilmPixelShader();
#endif
	}
}
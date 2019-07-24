int Amount;
float4 MainTextureTexelSize;

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
	float2 UV : TEXCOORD;
};

float Luminance(float3 rgb)
{
    return dot(rgb, float3(0.22, 0.707, 0.071));
}

float4 MainPS(PixelShaderInput input) : COLOR
{
	float4 main = tex2D(textureSampler, input.UV);
	float lum = Luminance(main.rgb);
	lum = Amount - (lum * (1.0 / pow(lum, 2.2)));
	lum = smoothstep(Amount * -1.0, 3.0, lum);
	float curve = (0.75 * pow(lum, 3.0)) + 0.25;
	curve += 0.075;
	main.rgb *= curve;
	
	return main;
}

technique SSAO
{
	pass P0
	{
#if SM4
		PixelShader = compile ps_4_0 MainPS();
#else
		PixelShader = compile ps_3_0 MainPS();
#endif
	}
}
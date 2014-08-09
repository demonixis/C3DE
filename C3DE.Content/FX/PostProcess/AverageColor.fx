float blurDistance;

texture TargetTexture;
sampler2D textureSampler = sampler_state
{
	Texture = (MainTexture);
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

#if SM4
float4 AverageColorPixelShader(float4 position : SV_Position, float4 color : COLOR0, float2 UV : TEXCOORD0) : COLOR
#else
float4 AverageColorPixelShader(float4 position : POSITION0, float4 color : COLOR0, float2 UV : TEXCOORD0) : COLOR
#endif
{
	float4 diffuse = tex2D(textureSampler, UV);
	diffuse.rgb = (diffuse.r + diffuse.g + diffuse.b) / 3.0;
	diffuse.a = 1.0f;
	return diffuse;
}

technique Technique1
{
    pass AverageColor
    {
#if SM4
		PixelShader = compile ps_4_0_level_9_1 AverageColorPixelShader();
#else
		PixelShader = compile ps_3_0 AverageColorPixelShader();
#endif
    }
}
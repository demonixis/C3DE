float2 LensCenter;
float2 Scale;
float2 ScaleIn;
float4 HmdWarpParam;

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

float2 HmdWarp(float2 in01)
{
	float2 theta = (in01 - LensCenter) * ScaleIn; // Scales to [-1, 1]
	float rSq = theta.x * theta.x + theta.y * theta.y;
	float2 rvector = theta * (HmdWarpParam.x + HmdWarpParam.y * rSq + HmdWarpParam.z * rSq * rSq + HmdWarpParam.w * rSq * rSq * rSq);
	return LensCenter + Scale * rvector;
}

float4 OculusPixelShaderFunction(PixelShaderInput input) : COLOR
{
	float2 tc = HmdWarp(input.UV);
	
	if (tc.x < 0.0 || tc.x > 1.0 || tc.y < 0.0 || tc.y > 1.0)
		return float4(0.0, 0.0, 0.0, 1.0);
	else
		return float4(tex2D(textureSampler, tc));
}

technique Technique1  
{
    pass Pass1
    {
	#if SM4
		PixelShader = compile ps_4_0_level_9_1 OculusPixelShaderFunction();
#else
		PixelShader = compile ps_3_0 OculusPixelShaderFunction();
#endif
    }
}

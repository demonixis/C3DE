#define FXAA_PC 1
#define FXAA_HLSL_3 1
#define FXAA_QUALITY__PRESET 12
#define FXAA_GREEN_AS_LUMA 1

#include "Fxaa3_11.fxh"

float4 PixelSize;

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
    float2 UV : TEXCOORD0;
};

float4 PixelShaderFunction(PixelShaderInput input) : COLOR0
{
    return FxaaPixelShader( 
        input.UV,                 //pos 
        0,                  //fxaaConsolePosPos (not used) 
        textureSampler,     //tex 
        textureSampler,     //fxaaConsole360TexExpBiasNegOne (not used)
        textureSampler,     //fxaaConsole360TexExpBiasNegTwo (not used)
        PixelSize.xy,       //fxaaQualityRcpFrame 
        0,                  //fxaaConsoleRcpFrameOpt (not used) 
        0,                  //fxaaConsoleRcpFrameOpt2 (not used)
        0,                  //fxaaConsole360RcpFrameOpt2 (not used)
        0.6,                //fxaaQualitySubpix, 
        0.166,              //fxaaQualityEdgeThreshold, 
        0.0625,             //fxaaQualityEdgeThresholdMin, 
        8.0,                //fxaaConsoleEdgeSharpness (not used)
        0.125,              //fxaaConsoleEdgeThreshold (not used)
        0.05,               //fxaaConsoleEdgeThresholdMin (not used)
        0                   //fxaaConsole360ConstDir (not used)
    );
} 

technique Default
{
    pass Pass1
    {
#if SM4
		PixelShader = compile ps_4_0_level_9_3 PixelShaderFunction();
#else
        PixelShader = compile ps_3_0 PixelShaderFunction();
#endif
    }
}
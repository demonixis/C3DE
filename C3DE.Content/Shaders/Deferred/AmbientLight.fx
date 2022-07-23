float4x4 World;
float3 Color;
float Intensity;

texture ColorMap;
sampler colorSampler = sampler_state
{
    Texture = (ColorMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

texture DepthMap;
sampler depthSampler = sampler_state
{
    Texture = (DepthMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

struct VertexShaderInput
{
    float3 Position : POSITION0;
    float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
    float4 WorldPosition : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    output.Position = float4(input.Position, 1);
    output.WorldPosition = mul(input.Position, World);
    output.UV = input.UV;
    return output;
}

float4 PixelShaderAmbient(VertexShaderOutput input) : COLOR0
{
    float4 color = tex2D(colorSampler, input.UV);

    //read depth
    float4 depth = tex2D(depthSampler, input.UV);
    float depthVal = depth.r;

    // Unlit case: If all depth values are 9 we just draw the color on the lightmap.
    if (depth.r == 0)
        return float4(color.rgb, 0);
    
    return float4(color + Color, 0);
}

technique AmbientLightTechnique
{
	pass Pass0
	{
#if SM4
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 PixelShaderAmbient();
#else
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderAmbient();
#endif
	}
}
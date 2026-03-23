float4 MainTextureTexelSize;
float4 AOParams;

texture DepthTexture;
sampler2D depthSampler = sampler_state
{
    Texture = <DepthTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VertexShaderInput
{
#if SM4
    float4 Position : SV_Position;
#else
    float4 Position : POSITION0;
#endif
    float2 UV : TEXCOORD0;
};

struct PixelShaderInput
{
    float4 Position : SV_POSITION;
    float2 UV : TEXCOORD0;
};

PixelShaderInput VS_Main(VertexShaderInput input)
{
    PixelShaderInput output;
    output.Position = input.Position;
    output.UV = input.UV;
    return output;
}

float SampleDepth(float2 uv)
{
    return tex2D(depthSampler, uv).r;
}

float4 PS_Main(PixelShaderInput input) : COLOR0
{
    float2 texel = MainTextureTexelSize.xy * AOParams.y;
    float depth = SampleDepth(input.UV);
    float ao = 0.0;

    static const float2 offsets[8] =
    {
        float2(1, 0), float2(-1, 0), float2(0, 1), float2(0, -1),
        float2(1, 1), float2(-1, 1), float2(1, -1), float2(-1, -1)
    };

    [unroll]
    for (int i = 0; i < 8; i++)
    {
        float2 sampleUv = input.UV + offsets[i] * texel;
        float sampleDepth = SampleDepth(sampleUv);
        float depthDelta = saturate((depth - sampleDepth) * 64.0 - AOParams.z * 64.0);
        ao += depthDelta;
    }

    ao = saturate(1.0 - (ao / 8.0) * AOParams.x);
    return float4(ao, ao, ao, 1.0);
}

technique Technique0
{
    pass AmbientOcclusion
    {
#if SM4
        VertexShader = compile vs_4_0 VS_Main();
        PixelShader = compile ps_4_0 PS_Main();
#else
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_Main();
#endif
    }
}

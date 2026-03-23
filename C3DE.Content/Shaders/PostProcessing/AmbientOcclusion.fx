float4 MainTextureTexelSize;
float4 AOParams;
float2 BlurDirection;

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

texture AOTexture;
sampler2D aoSampler = sampler_state
{
    Texture = <AOTexture>;
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

float ComputeRawAO(float2 uv)
{
    float2 texel = MainTextureTexelSize.xy * AOParams.y;
    float depth = SampleDepth(uv);
    float ao = 0.0;

    static const float2 offsets[12] =
    {
        float2(1.0, 0.0), float2(-1.0, 0.0), float2(0.0, 1.0), float2(0.0, -1.0),
        float2(0.707, 0.707), float2(-0.707, 0.707), float2(0.707, -0.707), float2(-0.707, -0.707),
        float2(2.0, 0.5), float2(-2.0, 0.5), float2(0.5, 2.0), float2(0.5, -2.0)
    };

    [unroll]
    for (int i = 0; i < 12; i++)
    {
        float2 sampleUv = uv + offsets[i] * texel;
        float sampleDepth = SampleDepth(sampleUv);
        float depthDelta = saturate((depth - sampleDepth - AOParams.z) * 72.0);
        ao += depthDelta;
    }

    ao = saturate(1.0 - (ao / 12.0) * AOParams.x);
    return ao;
}

float BlurAO(float2 uv)
{
    float centerDepth = SampleDepth(uv);
    float center = tex2D(aoSampler, uv).r;
    float2 texel = MainTextureTexelSize.xy * BlurDirection;

    float accumulated = center * 0.36;
    float weight = 0.36;

    static const float weights[2] = { 0.24, 0.08 };

    [unroll]
    for (int i = 0; i < 2; i++)
    {
        float scale = i + 1.0;
        float2 offset = texel * scale;
        float2 uvA = uv + offset;
        float2 uvB = uv - offset;

        float depthA = SampleDepth(uvA);
        float depthB = SampleDepth(uvB);
        float aoA = tex2D(aoSampler, uvA).r;
        float aoB = tex2D(aoSampler, uvB).r;

        float depthWeightA = saturate(1.0 - abs(depthA - centerDepth) * AOParams.w * 32.0);
        float depthWeightB = saturate(1.0 - abs(depthB - centerDepth) * AOParams.w * 32.0);
        float sampleWeightA = weights[i] * depthWeightA;
        float sampleWeightB = weights[i] * depthWeightB;

        accumulated += aoA * sampleWeightA;
        accumulated += aoB * sampleWeightB;
        weight += sampleWeightA + sampleWeightB;
    }

    return accumulated / max(weight, 0.0001);
}

float4 PS_AmbientOcclusion(PixelShaderInput input) : COLOR0
{
    float ao = ComputeRawAO(input.UV);
    return float4(ao, ao, ao, 1.0);
}

float4 PS_Blur(PixelShaderInput input) : COLOR0
{
    float ao = BlurAO(input.UV);
    return float4(ao, ao, ao, 1.0);
}

technique Technique0
{
    pass AmbientOcclusion
    {
#if SM4
        VertexShader = compile vs_4_0 VS_Main();
        PixelShader = compile ps_4_0 PS_AmbientOcclusion();
#else
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_AmbientOcclusion();
#endif
    }

    pass BlurHorizontal
    {
#if SM4
        VertexShader = compile vs_4_0 VS_Main();
        PixelShader = compile ps_4_0 PS_Blur();
#else
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_Blur();
#endif
    }

    pass BlurVertical
    {
#if SM4
        VertexShader = compile vs_4_0 VS_Main();
        PixelShader = compile ps_4_0 PS_Blur();
#else
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_Blur();
#endif
    }
}

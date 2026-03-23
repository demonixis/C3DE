float4 MainTextureTexelSize;
float4 TonemapParams;
float4 ColorAdjustments;
float4 WhiteBalance;
float4 Lift;
float4 Gamma;
float4 Gain;
float4 VignetteParams;
float4 VignetteColor;
float4 FxaaParams;
float4 SunFlareParams;
float4 SunFlareColor;
float4 SunFlarePosition;
float4 EffectToggles;
float4 DebugParams;

texture MainTexture;
sampler2D sceneSampler = sampler_state
{
    Texture = <MainTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture BloomTexture;
sampler2D bloomSampler = sampler_state
{
    Texture = <BloomTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture AmbientOcclusionTexture;
sampler2D aoSampler = sampler_state
{
    Texture = <AmbientOcclusionTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

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

float Luminance(float3 rgb)
{
    return dot(rgb, float3(0.299, 0.587, 0.114));
}

float3 ApplyTonemapping(float3 color)
{
    color *= TonemapParams.x;
    const float3 a = 2.51f;
    const float3 b = 0.03f;
    const float3 c = 2.43f;
    const float3 d = 0.59f;
    const float3 e = 0.14f;
    return saturate((color * (a * color + b)) / (color * (c * color + d) + e));
}

float3 ApplyWhiteBalance(float3 color)
{
    float3 balance = float3(
        1.0 + WhiteBalance.x - WhiteBalance.y * 0.5,
        1.0,
        1.0 - WhiteBalance.x + WhiteBalance.y * 0.5);
    return color * balance;
}

float3 ApplyColorAdjustments(float3 color)
{
    color = ApplyWhiteBalance(color);
    color = max(color + Lift.xyz, 0.0);
    color *= Gain.xyz;
    color = pow(max(color, 0.0001), 1.0 / max(Gamma.xyz, 0.0001));

    float luma = Luminance(color);
    color = lerp(luma.xxx, color, ColorAdjustments.y);
    color = ((color - 0.5) * ColorAdjustments.x) + 0.5;
    return saturate(color);
}

float3 ApplySharpen(float2 uv, float3 center)
{
    if (EffectToggles.x <= 0.0)
        return center;

    float2 texel = MainTextureTexelSize.xy;
    float3 north = tex2D(sceneSampler, uv + float2(0.0, -texel.y)).rgb;
    float3 south = tex2D(sceneSampler, uv + float2(0.0, texel.y)).rgb;
    float3 east = tex2D(sceneSampler, uv + float2(texel.x, 0.0)).rgb;
    float3 west = tex2D(sceneSampler, uv + float2(-texel.x, 0.0)).rgb;
    return saturate(center * (1.0 + 4.0 * EffectToggles.x) - (north + south + east + west) * EffectToggles.x);
}

float3 ApplyFxaa(float2 uv, float3 color)
{
    if (EffectToggles.z <= 0.0)
        return color;

    float2 texel = MainTextureTexelSize.xy;
    float3 rgbNW = tex2D(sceneSampler, uv + float2(-texel.x, -texel.y)).rgb;
    float3 rgbNE = tex2D(sceneSampler, uv + float2(texel.x, -texel.y)).rgb;
    float3 rgbSW = tex2D(sceneSampler, uv + float2(-texel.x, texel.y)).rgb;
    float3 rgbSE = tex2D(sceneSampler, uv + float2(texel.x, texel.y)).rgb;
    float lumaNW = Luminance(rgbNW);
    float lumaNE = Luminance(rgbNE);
    float lumaSW = Luminance(rgbSW);
    float lumaSE = Luminance(rgbSE);
    float lumaM = Luminance(color);

    float lumaMin = min(lumaM, min(min(lumaNW, lumaNE), min(lumaSW, lumaSE)));
    float lumaMax = max(lumaM, max(max(lumaNW, lumaNE), max(lumaSW, lumaSE)));

    float2 dir;
    dir.x = -((lumaNW + lumaNE) - (lumaSW + lumaSE));
    dir.y = ((lumaNW + lumaSW) - (lumaNE + lumaSE));

    float dirReduce = max((lumaNW + lumaNE + lumaSW + lumaSE) * (0.25 * FxaaParams.y), FxaaParams.x);
    float inverseDirAdjustment = 1.0 / (min(abs(dir.x), abs(dir.y)) + dirReduce);
    dir = saturate(abs(dir) * inverseDirAdjustment / FxaaParams.z) * sign(dir);
    dir *= texel;

    float3 rgbA = 0.5 * (
        tex2D(sceneSampler, uv + dir * (1.0 / 3.0 - 0.5)).rgb +
        tex2D(sceneSampler, uv + dir * (2.0 / 3.0 - 0.5)).rgb);

    float3 rgbB = rgbA * 0.5 + 0.25 * (
        tex2D(sceneSampler, uv + dir * -0.5).rgb +
        tex2D(sceneSampler, uv + dir * 0.5).rgb);

    float lumaB = Luminance(rgbB);
    if (lumaB < lumaMin || lumaB > lumaMax)
        return rgbA;

    return rgbB;
}

float3 ComputeSunFlare(float2 uv)
{
    if (EffectToggles.w <= 0.0 || SunFlarePosition.z <= 0.0)
        return 0.0.xxx;

    float2 sunUv = SunFlarePosition.xy;
    float2 screenDelta = uv - sunUv;
    float distanceToSun = length(screenDelta);
    float sunDisk = smoothstep(SunFlareParams.y * 0.35, 0.0, distanceToSun);
    float sunHalo = smoothstep(SunFlareParams.y * 1.75, 0.0, distanceToSun);
    float edgeFade = saturate(1.0 - max(abs(sunUv.x - 0.5), abs(sunUv.y - 0.5)) / max(0.0001, 0.5 - SunFlareParams.w));
    float sceneDepthAtSun = tex2D(depthSampler, saturate(sunUv)).r;
    float depthMask = sceneDepthAtSun <= 0.0001 ? 1.0 : smoothstep(SunFlareParams.z - 0.15, SunFlareParams.z, sceneDepthAtSun);
    depthMask = max(depthMask, 0.2);

    float2 ghostUv = lerp(uv, 1.0 - uv, 0.35);
    float ghost = smoothstep(SunFlareParams.y * 0.85, 0.0, length(ghostUv - sunUv)) * SunFlareParams.x;
    float streakA = saturate(1.0 - abs(screenDelta.x + screenDelta.y) / max(SunFlareParams.y * 2.0, 0.0001));
    float streakB = saturate(1.0 - abs(screenDelta.x - screenDelta.y) / max(SunFlareParams.y * 2.0, 0.0001));

    float flare = (sunDisk * 2.4 + sunHalo * 0.9 + ghost * (SunFlareParams.x * 1.35) + (streakA + streakB) * 0.14) * edgeFade * depthMask * EffectToggles.w;
    return SunFlareColor.rgb * flare;
}

float4 PS_Main(PixelShaderInput input) : COLOR0
{
    float3 rawSceneColor = tex2D(sceneSampler, input.UV).rgb;
    float3 bloomColor = tex2D(bloomSampler, input.UV).rgb;
    float ao = tex2D(aoSampler, input.UV).r;

    float3 sceneColor = rawSceneColor;
    sceneColor = ApplyFxaa(input.UV, sceneColor);
    sceneColor = ApplySharpen(input.UV, sceneColor);

    if (EffectToggles.y > 0.0)
        sceneColor *= ao.xxx;

    if (TonemapParams.y > 0.0)
        sceneColor += bloomColor * TonemapParams.y;

    if (TonemapParams.w > 0.0)
        sceneColor = ApplyTonemapping(sceneColor);

    if (ColorAdjustments.w > 0.0)
        sceneColor = ApplyColorAdjustments(sceneColor);

    if (TonemapParams.z > 0.0)
        sceneColor += ComputeSunFlare(input.UV);

    if (VignetteParams.x > 0.0)
    {
        float2 centered = abs(input.UV * 2.0 - 1.0);
        float vignette = pow(saturate(1.0 - dot(centered * VignetteParams.z, centered * VignetteParams.z)), max(0.0001, VignetteParams.y));
        sceneColor = lerp(VignetteColor.rgb, sceneColor, lerp(1.0, vignette, VignetteParams.x));
    }

    if (DebugParams.x > 0.5)
    {
        if (DebugParams.x < 1.5)
            return float4(saturate(rawSceneColor), 1.0);

        if (DebugParams.x < 2.5)
            return float4(saturate(bloomColor), 1.0);

        if (DebugParams.x < 3.5)
            return float4(ao.xxx, 1.0);
    }

    return float4(saturate(sceneColor), 1.0);
}

technique Technique0
{
    pass FinalComposite
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

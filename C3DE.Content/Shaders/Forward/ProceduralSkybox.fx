float4x4 World;
float4x4 View;
float4x4 Projection;
float3 EyePosition;

float3 SunDirection;
float3 MoonDirection;
float4 SkyParams;
float4 CloudParams;
float4 DayTopColor;
float4 DayHorizonColor;
float4 NightTopColor;
float4 NightHorizonColor;
float4 NightTint;

texture CloudNoiseTexture;
sampler2D cloudSampler = sampler_state
{
    Texture = <CloudNoiseTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture StarNoiseTexture;
sampler2D starSampler = sampler_state
{
    Texture = <StarNoiseTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VertexShaderInput
{
#if SM4
    float4 Position : SV_Position;
#else
    float4 Position : POSITION0;
#endif
    float3 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float3 UV : TEXCOORD0;
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.UV = normalize(worldPosition.xyz - EyePosition);
    return output;
}

float2 DirectionToUV(float3 dir)
{
    float2 uv;
    uv.x = atan2(dir.x, dir.z) / 6.2831853 + 0.5;
    uv.y = dir.y * 0.5 + 0.5;
    return uv;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    float3 dir = normalize(input.UV);
    float2 uv = DirectionToUV(dir);
    float upFactor = saturate(dir.y * 0.5 + 0.5);
    float dayFactor = saturate(SkyParams.y);
    float nightFactor = 1.0 - dayFactor;

    float3 daySky = lerp(DayHorizonColor.rgb, DayTopColor.rgb, pow(upFactor, 0.55));
    float3 nightSky = lerp(NightHorizonColor.rgb, NightTopColor.rgb, pow(upFactor, 0.8)) * NightTint.rgb;
    float3 skyColor = lerp(nightSky, daySky, dayFactor);

    float twilight = saturate(1.0 - abs(SunDirection.y) * 5.0) * saturate(1.0 - abs(dir.y) * 1.8);
    float3 twilightColor = lerp(float3(0.95, 0.45, 0.2), float3(0.9, 0.7, 0.45), saturate(upFactor));
    skyColor += twilightColor * twilight * 0.45;

    float sunDot = saturate(dot(dir, normalize(SunDirection)));
    float sunDisk = smoothstep(0.99955, 0.99992, sunDot);
    float sunHalo = pow(sunDot, 64.0);

    float moonDot = saturate(dot(dir, normalize(MoonDirection)));
    float moonDisk = smoothstep(0.9992, 0.99985, moonDot);
    float moonHalo = pow(moonDot, 24.0);

    float2 cloudUvA = uv * float2(SkyParams.w, SkyParams.w * 0.85) + float2(SkyParams.x * CloudParams.y, 0.0);
    float2 cloudUvB = uv * float2(SkyParams.w * 1.8, SkyParams.w * 1.35) - float2(SkyParams.x * CloudParams.y * 1.6, SkyParams.x * CloudParams.y * 0.35);
    float cloudA = tex2D(cloudSampler, cloudUvA).r;
    float cloudB = tex2D(cloudSampler, cloudUvB).r;
    float cloudNoise = saturate(cloudA * 0.7 + cloudB * 0.5);
    float skyCloudBand = smoothstep(0.08, 0.28, dir.y);
    skyCloudBand *= 1.0 - smoothstep(0.82, 1.0, dir.y) * 0.2;
    float cloudMask = smoothstep(1.0 - CloudParams.x, 1.0, cloudNoise) * skyCloudBand;
    float silverLining = pow(sunDot, 18.0) * cloudMask * dayFactor;
    float3 cloudColor = lerp(float3(0.12, 0.14, 0.2), float3(1.0, 0.98, 0.94), dayFactor);
    cloudColor += silverLining.xxx * 0.85;
    skyColor = lerp(skyColor, cloudColor, cloudMask * 0.75);

    float2 starUvA = uv * float2(32.0, 18.0) + float2(SkyParams.x * 0.01, 0.0);
    float2 starUvB = uv * float2(61.0, 35.0) - float2(SkyParams.x * 0.006, 0.0);
    float starA = tex2D(starSampler, starUvA).r;
    float starB = tex2D(starSampler, starUvB).r;
    float stars = step(0.985, starA) * 0.7 + step(0.992, starB) * 0.9;
    stars *= nightFactor * SkyParams.z * saturate(upFactor * 1.1) * (1.0 - cloudMask * 0.75);
    skyColor += stars.xxx;

    skyColor += float3(1.0, 0.92, 0.78) * (sunDisk * CloudParams.z * 1.6 + sunHalo * CloudParams.z * 0.35 * dayFactor);
    skyColor += float3(0.78, 0.84, 1.0) * (moonDisk * CloudParams.w * 1.6 + moonHalo * CloudParams.w * 0.2 * nightFactor);

    return float4(saturate(skyColor), 1.0);
}

technique Skybox
{
    pass AmbientPass
    {
#if SM4
        VertexShader = compile vs_4_0_level_9_1 MainVS();
        PixelShader = compile ps_4_0_level_9_1 MainPS();
#else
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 MainPS();
#endif
    }
}

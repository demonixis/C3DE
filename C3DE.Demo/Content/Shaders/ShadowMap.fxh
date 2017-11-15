float4x4 LightView;
float4x4 LightProjection;

// ShadowData [0] => Map size [1] => Bias [2] => Strength
float3 ShadowData = float3(0, 0.05, 1.0);
bool RecieveShadow = false;

texture ShadowMap;
sampler2D shadowSampler = sampler_state
{
    Texture = (ShadowMap);
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};

float CalcShadowPCF(float lightSpaceDepth, float2 shadowCoordinates)
{
    float size = 1.0 / ShadowData[0];
    float samples[4];
    float gradiant = lightSpaceDepth - ShadowData[1];

    samples[0] = (gradiant < tex2D(shadowSampler, shadowCoordinates).r) ? 1.0 : 0.0;
    samples[1] = (gradiant < tex2D(shadowSampler, shadowCoordinates + float2(size, 0)).r) ? 1.0 : 0.0;
    samples[2] = (gradiant < tex2D(shadowSampler, shadowCoordinates + float2(0, size)).r) ? 1.0 : 0.0;
    samples[3] = (gradiant < tex2D(shadowSampler, shadowCoordinates + float2(size, size)).r) ? 1.0 : 0.0;

    return (samples[0] + samples[1] + samples[2] + samples[3]) / 4.0;
}

float CalcShadow(float4 worldPosition)
{
    float shadowTerm = 0;
	
	// ShadowData [0] => Map size [1] => Bias [2] => Strength
    if (ShadowData[0] > 0 && RecieveShadow == true)
    {
        float4 lightSpacePosition = mul(mul(worldPosition, LightView), LightProjection);
        lightSpacePosition -= ShadowData[1];
        lightSpacePosition /= lightSpacePosition.w;

        float2 screenPosition = 0.5 + float2(lightSpacePosition.x, -lightSpacePosition.y) * 0.5;

        if ((saturate(screenPosition).x == screenPosition.x) && (saturate(screenPosition).y == screenPosition.y))
            shadowTerm = CalcShadowPCF(lightSpacePosition.z, screenPosition);
    }
	
    return shadowTerm;
}

float CalcShadow2(float4 WorldPos)
{
    float4x4 LightViewProj = mul(LightView, LightProjection);
    float4 lightingPosition = mul(WorldPos, LightViewProj);
    
    // Find the position in the shadow map for this pixel
    float2 ShadowTexCoord = 0.5 * lightingPosition.xy / lightingPosition.w + float2(0.5, 0.5);
    ShadowTexCoord.y = 1.0f - ShadowTexCoord.y;

    // Get the current depth stored in the shadow map
    float shadowdepth = tex2D(shadowSampler, ShadowTexCoord).r;
    float ourdepth = (lightingPosition.z / lightingPosition.w) - ShadowData[1];
    
    if (shadowdepth < ourdepth)
        return 0.5f / ShadowData[2];
    
    return 1.0f;
}